using System.Collections.Generic;
using System.Linq;

using Assets._Scripts.Model;
using Assets._Scripts.Model.Instructions;
using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class ProfileGenerator : SingletonBehaviour<ProfileGenerator> {
		public ProfileGenerationSettings ProfileGenerationSettings;
		public ProfilePictureGenerationSettings ProfilePictureGenerationSettings;

		private Profile _lastGeneratedProfile; // TODO : make sure to not generate exactly the same profile twice in a row
		private GeneratedRule _currentRule;

		protected override void Awake() {
			base.Awake();
			for (int i = 0; i < 25; i++) {
				GenerateProfile();
			}
		}


		private void Start() {
			GameManager.OnNewRound += OnNewRound;

			if (Application.isEditor) {
				ProfileManager.Instance.DisplayProfile(GenerateProfile());
			}
		}

		private void OnDestroy() {
			GameManager.OnNewRound -= OnNewRound;
		}

		private void OnNewRound(GeneratedRule rule) {
			_currentRule = rule;
		}

		/// <summary>
		/// Generates a random profile (legacy method, use GenerateAlienProfile or GenerateHumanProfile instead)
		/// </summary>
		public Profile GenerateProfile() {
			Profile profile = new();

			profile.Gender = GenerateGender();
			profile.Name = GenerateName(profile.Gender);
			profile.Interests = GenerateInterests();
			profile.Age = GenerateAge();
			profile.PictureElements = GeneratePictureElements();
			profile.Height = GenerateHeight();

			_lastGeneratedProfile = profile;
			return profile;
		}

		/// <summary>
		/// Generates an alien profile that matches the current detection rules
		/// </summary>
		public Profile GenerateAlienProfile() {
			if (_currentRule == null) {
				Debug.LogWarning("No current rule set, generating random profile");
				return GenerateProfile();
			}

			Profile profile = GenerateProfile();

			// Override values to match alien detection rules
			foreach (var instruction in _currentRule.Instructions) {
				ApplyAlienValue(profile, instruction);
			}

			Debug.Log($"Generated ALIEN: Age={profile.Age}, Height={profile.Height:F2}, Interests={profile.Interests.Count}");
			return profile;
		}

		/// <summary>
		/// Generates a human profile that does NOT match the detection rules
		/// </summary>
		public Profile GenerateHumanProfile() {
			if (_currentRule == null) {
				Debug.LogWarning("No current rule set, generating random profile");
				return GenerateProfile();
			}

			Profile profile = GenerateProfile();

			// Override values to ensure they're in human range (not matching alien rules)
			foreach (var instruction in _currentRule.Instructions) {
				ApplyHumanValue(profile, instruction);
			}

			// Verify the profile doesn't match the rule
			if (_currentRule.CombinedRule.IsValid(profile)) {
				// If it still matches (edge case), force one value to be clearly human
				var firstInstruction = _currentRule.Instructions[0];
				ForceHumanValue(profile, firstInstruction);
			}

			Debug.Log($"Generated HUMAN: Age={profile.Age}, Height={profile.Height:F2}, Interests={profile.Interests.Count}");
			return profile;
		}

		private void ApplyAlienValue(Profile profile, GeneratedInstruction instruction) {
			var config = instruction.NumericConfig;
			var colorConfig = instruction.ColorConfig;

			switch (instruction.AttributeType) {
				case AttributeType.Age:
					profile.Age = Mathf.RoundToInt(Random.Range(config.AlienMin, config.AlienMax + 1));
					break;
				case AttributeType.Height:
					profile.Height = Random.Range(config.AlienMin, config.AlienMax);
					break;
				case AttributeType.InterestCount:
					int targetCount = Mathf.RoundToInt(Random.Range(config.AlienMin, config.AlienMax + 1));
					AdjustInterestCount(profile, targetCount);
					break;
				case AttributeType.EyeColor:
					if (colorConfig.AlienColors.Count > 0) {
						profile.PictureElements.EyeColor = colorConfig.AlienColors[Random.Range(0, colorConfig.AlienColors.Count)];
					}
					break;
				case AttributeType.HairColor:
					if (colorConfig.AlienColors.Count > 0) {
						profile.PictureElements.HairColor = colorConfig.AlienColors[Random.Range(0, colorConfig.AlienColors.Count)];
					}
					break;
				case AttributeType.FaceColor:
					if (colorConfig.AlienColors.Count > 0) {
						profile.PictureElements.FaceColor = colorConfig.AlienColors[Random.Range(0, colorConfig.AlienColors.Count)];
					}
					break;
			}
		}

		private void ApplyHumanValue(Profile profile, GeneratedInstruction instruction) {
			var config = instruction.NumericConfig;
			var colorConfig = instruction.ColorConfig;

			switch (instruction.AttributeType) {
				case AttributeType.Age:
					profile.Age = GenerateHumanAge(config);
					break;
				case AttributeType.Height:
					profile.Height = GenerateHumanHeight(config);
					break;
				case AttributeType.InterestCount:
					int targetCount = GenerateHumanInterestCount(config);
					AdjustInterestCount(profile, targetCount);
					break;
				case AttributeType.EyeColor:
					if (colorConfig.HumanColors.Count > 0) {
						profile.PictureElements.EyeColor = colorConfig.HumanColors[Random.Range(0, colorConfig.HumanColors.Count)];
					}
					break;
				case AttributeType.HairColor:
					if (colorConfig.HumanColors.Count > 0) {
						profile.PictureElements.HairColor = colorConfig.HumanColors[Random.Range(0, colorConfig.HumanColors.Count)];
					}
					break;
				case AttributeType.FaceColor:
					if (colorConfig.HumanColors.Count > 0) {
						profile.PictureElements.FaceColor = colorConfig.HumanColors[Random.Range(0, colorConfig.HumanColors.Count)];
					}
					break;
			}
		}

		private void ForceHumanValue(Profile profile, GeneratedInstruction instruction) {
			var config = instruction.NumericConfig;

			switch (instruction.AttributeType) {
				case AttributeType.Age:
					// Force age to be clearly outside alien range
					profile.Age = Mathf.RoundToInt(config.HumanMin);
					break;
				case AttributeType.Height:
					profile.Height = config.HumanMin;
					break;
				case AttributeType.InterestCount:
					AdjustInterestCount(profile, Mathf.RoundToInt(config.HumanMax));
					break;
			}
		}

		private int GenerateHumanAge(NumericAttributeConfig config) {
			int humanMin = Mathf.RoundToInt(config.HumanMin);
			int humanMax = Mathf.RoundToInt(config.HumanMax);
			int alienMin = Mathf.RoundToInt(config.AlienMin);
			int alienMax = Mathf.RoundToInt(config.AlienMax);

			// Generate age in human range but outside alien range
			List<int> validAges = new();
			for (int age = humanMin; age <= humanMax; age++) {
				if (age < alienMin || age > alienMax) {
					validAges.Add(age);
				}
			}

			if (validAges.Count > 0) {
				return validAges[Random.Range(0, validAges.Count)];
			}
			return humanMin; // Fallback
		}

		private float GenerateHumanHeight(NumericAttributeConfig config) {
			// Generate height in human range but outside alien range
			if (config.HumanMax <= config.AlienMin) {
				return Random.Range(config.HumanMin, config.HumanMax);
			} else if (config.HumanMin >= config.AlienMax) {
				return Random.Range(config.HumanMin, config.HumanMax);
			} else {
				// Ranges overlap, pick from human-only zone
				if (config.HumanMin < config.AlienMin) {
					return Random.Range(config.HumanMin, config.AlienMin);
				} else {
					return Random.Range(config.AlienMax, config.HumanMax);
				}
			}
		}

		private int GenerateHumanInterestCount(NumericAttributeConfig config) {
			int humanMin = Mathf.RoundToInt(config.HumanMin);
			int humanMax = Mathf.RoundToInt(config.HumanMax);
			int alienMin = Mathf.RoundToInt(config.AlienMin);
			int alienMax = Mathf.RoundToInt(config.AlienMax);

			List<int> validCounts = new();
			for (int count = humanMin; count <= humanMax; count++) {
				if (count < alienMin || count > alienMax) {
					validCounts.Add(count);
				}
			}

			if (validCounts.Count > 0) {
				return validCounts[Random.Range(0, validCounts.Count)];
			}
			return humanMax; // Fallback
		}

		private void AdjustInterestCount(Profile profile, int targetCount) {
			targetCount = Mathf.Clamp(targetCount, 0, ProfileGenerationSettings.Interests.Count);

			while (profile.Interests.Count > targetCount) {
				profile.Interests.RemoveAt(profile.Interests.Count - 1);
			}

			while (profile.Interests.Count < targetCount) {
				var available = ProfileGenerationSettings.Interests
					.Where(i => !profile.Interests.Contains(i))
					.ToList();
				if (available.Count == 0) break;
				profile.Interests.Add(available[Random.Range(0, available.Count)]);
			}
		}

		private Gender GenerateGender() {
			float t = Random.value;
			if (t < 0.33f) {
				return Gender.Male;
			} else if (t < 0.66f) {
				return Gender.Female;
			} else {
				return Gender.Other;
			}
		}

		private string GenerateName(Gender gender) {
			bool firstNameGenderIsMale =
				gender == Gender.Male || (gender == Gender.Other && Random.value > 0.5f);
			string firstName;
			string lastName = ProfileGenerationSettings.LastNames[
				Random.Range(0, ProfileGenerationSettings.LastNames.Count)
			];

			if (firstNameGenderIsMale) {
				firstName = ProfileGenerationSettings.FirstNamesMale[
					Random.Range(0, ProfileGenerationSettings.FirstNamesMale.Count)
				];
			} else {
				firstName = ProfileGenerationSettings.FirstNamesFemale[
					Random.Range(0, ProfileGenerationSettings.FirstNamesFemale.Count)
				];
			}
			return $"{firstName} {lastName}";
		}

		private List<string> GenerateInterests() {
			int maxInterestCount = Mathf.Min(
				ProfileGenerationSettings.MaxInterests,
				ProfileGenerationSettings.Interests.Count
			);

			return ProfileGenerationSettings
				.Interests.OrderBy(x => Random.value)
				.Take(Random.Range(ProfileGenerationSettings.MinInterests, maxInterestCount + 1))
				.ToList();
		}

		private int GenerateAge() {
			float ageT = Random.value;
			float ageValue = ProfileGenerationSettings.AgeRandomFunction.Evaluate(ageT);
			return Mathf.RoundToInt(
				Mathf.Lerp(ProfileGenerationSettings.MinAge, ProfileGenerationSettings.MaxAge, ageValue)
			);
		}

		public ProfilePictureElements GeneratePictureElements() {
			ProfilePictureElements result = new() {
				FaceSprite = ProfilePictureGenerationSettings.Faces[
					Random.Range(0, ProfilePictureGenerationSettings.Faces.Count)
				],
				FaceColor = ProfilePictureGenerationSettings.FaceColors.Colors[
					Random.Range(0, ProfilePictureGenerationSettings.FaceColors.Colors.Count)
				].Color,
				BackgroundColor = ProfilePictureGenerationSettings.BackgroundColors.Colors[
					Random.Range(0, ProfilePictureGenerationSettings.BackgroundColors.Colors.Count)
				].Color,
				EyeShapeSprite = ProfilePictureGenerationSettings.EyeShapes[
					Random.Range(0, ProfilePictureGenerationSettings.EyeShapes.Count)
				],
				EyeColor = ProfilePictureGenerationSettings.EyeColors.Colors[
					Random.Range(0, ProfilePictureGenerationSettings.EyeColors.Colors.Count)
				].Color,
				HairstyleSprite = ProfilePictureGenerationSettings.Hairstyles[
					Random.Range(0, ProfilePictureGenerationSettings.Hairstyles.Count)
				],
				HairColor = ProfilePictureGenerationSettings.HairColors.Colors[
					Random.Range(0, ProfilePictureGenerationSettings.HairColors.Colors.Count)
				].Color,
				MouthShapeSprite = ProfilePictureGenerationSettings.MouthShapes[
					Random.Range(0, ProfilePictureGenerationSettings.MouthShapes.Count)
				],
				NoseShapeSprite = ProfilePictureGenerationSettings.NoseShapes[
					Random.Range(0, ProfilePictureGenerationSettings.NoseShapes.Count)
					]
			};

			var t = Random.value;
			if (t < 0.33f) {
				result.AccessorySprite = ProfilePictureGenerationSettings.Accessories[
					Random.Range(0, ProfilePictureGenerationSettings.Accessories.Count)
				];
			} else {
				result.AccessorySprite = null;
			}
			return result;
		}

		private float GenerateHeight() {
			float t = Random.value;
			float heightValue = ProfileGenerationSettings.HeightRandomFunction.Evaluate(t);
			return Mathf.Lerp(
				ProfileGenerationSettings.MinHeight,
				ProfileGenerationSettings.MaxHeight,
				heightValue
			);
		}
	}
}
