using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Model;
using Assets._Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class ProfileGenerator : SingletonBehaviour<ProfileGenerator> {

		public ProfileGenerationSettings ProfileGenerationSettings;
		public ProfilePictureGenerationSettings ProfilePictureGenerationSettings;

		private void Awake() {
			for (int i = 0; i < 25; i++) {
				GenerateProfile();
			}
		}


		private Profile _lastGeneratedProfile; // TODO : make sure to not generate exactly the same profile twice in a row
		public Profile GenerateProfile() {
			Profile profile = new();

			profile.Gender = GenerateGender();
			profile.Name = GenerateName(profile.Gender);
			profile.Bio = GenerateBio();
			profile.Interests = GenerateInterests();
			profile.Age = GenerateAge();
			profile.PictureElements = GeneratePictureElements();
			profile.Height = GenerateHeight();
			Debug.Log($"heightValue: {profile.Height}, age : {profile.Age}");

			_lastGeneratedProfile = profile;
			return profile;
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
			bool firstNameGenderIsMale = gender == Gender.Male || (gender == Gender.Other && Random.value > 0.5f);
			string firstName;
			string lastName = ProfileGenerationSettings.LastNames[Random.Range(0, ProfileGenerationSettings.LastNames.Count)];

			if (firstNameGenderIsMale) {
				firstName = ProfileGenerationSettings.FirstNamesMale[Random.Range(0, ProfileGenerationSettings.FirstNamesMale.Count)];
			} else {
				firstName = ProfileGenerationSettings.FirstNamesFemale[Random.Range(0, ProfileGenerationSettings.FirstNamesFemale.Count)];
			}
			return $"{firstName} {lastName}";
		}

		private string GenerateBio() {
			return ProfileGenerationSettings.Bios[Random.Range(0, ProfileGenerationSettings.Bios.Count)];
		}

		private List<string> GenerateInterests() {
			int maxInterestCount = Mathf.Min(ProfileGenerationSettings.MaxInterests, ProfileGenerationSettings.Interests.Count);

			return ProfileGenerationSettings.Interests
				.OrderBy(x => Random.value)
				.Take(Random.Range(ProfileGenerationSettings.MinInterests, maxInterestCount + 1))
				.ToList();
		}

		private int GenerateAge() {
			float ageT = Random.value;
			float ageValue = ProfileGenerationSettings.AgeRandomFunction.Evaluate(ageT);
			return Mathf.RoundToInt(Mathf.Lerp(ProfileGenerationSettings.MinAge, ProfileGenerationSettings.MaxAge, ageValue));
		}

		public ProfilePictureElements GeneratePictureElements() {
			return new() {
				FaceSprite = ProfilePictureGenerationSettings.Faces[Random.Range(0, ProfilePictureGenerationSettings.Faces.Count)],
				FaceColor = ProfilePictureGenerationSettings.FaceColors[Random.Range(0, ProfilePictureGenerationSettings.FaceColors.Count)],
				BackgroundColor = ProfilePictureGenerationSettings.BackgroundColors[Random.Range(0, ProfilePictureGenerationSettings.BackgroundColors.Count)],
				EyeShapeSprite = ProfilePictureGenerationSettings.EyeShapes[Random.Range(0, ProfilePictureGenerationSettings.EyeShapes.Count)],
				EyeColor = ProfilePictureGenerationSettings.EyeColors[Random.Range(0, ProfilePictureGenerationSettings.EyeColors.Count)],
				HairstyleSprite = ProfilePictureGenerationSettings.Hairstyles[Random.Range(0, ProfilePictureGenerationSettings.Hairstyles.Count)],
				HairColor = ProfilePictureGenerationSettings.HairColors[Random.Range(0, ProfilePictureGenerationSettings.HairColors.Count)]
			};
		}

		private float GenerateHeight() {
			float t = Random.value;
			float heightValue = ProfileGenerationSettings.HeightRandomFunction.Evaluate(t);
			return Mathf.Lerp(ProfileGenerationSettings.MinHeight, ProfileGenerationSettings.MaxHeight, heightValue);
		}

	}
}
