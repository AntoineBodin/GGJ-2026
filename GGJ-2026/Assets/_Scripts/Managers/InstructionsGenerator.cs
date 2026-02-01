using System.Collections.Generic;
using System.Globalization;

using Assets._Scripts.Model;
using Assets._Scripts.Model.Instructions;
using Assets._Scripts.Model.Instructions.Comparators;
using Assets._Scripts.ScriptableObjects;

using NUnit.Framework;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class InstructionsGenerator : SingletonBehaviour<InstructionsGenerator> {

		[SerializeField]
		private ProfilePictureGenerationSettings profilePictureSettings;

		[SerializeField]
		private ProfileGenerationSettings profileSettings;

		public IInstruction GenerateInstructions(int dayIndex) {
			switch (dayIndex) {
				case 0:
					int hairColorIndex = Random.Range(0, profilePictureSettings.HairColors.Colors.Count);
					NamedColor namedColor = profilePictureSettings.HairColors.Colors[hairColorIndex];

					return new HairColorInstruction(namedColor, new EqualsComparator<NamedColor>());
				case 1:
					return new OrInstruction(new() { GetRandomFacialInstruction(), GetRandomInfoInstruction() });
				case 2:
					return new AndInstruction(new() { GetRandomFacialInstruction(), GetRandomInfoInstruction() });
				case 3:
					return new OrInstruction(new() { GetRandomFacialInstruction(), GetRandomInfoInstruction(), GetRandomInfoInstruction() });
				case 4:
					return new AndInstruction(new() { GetRandomFacialInstruction(), GetRandomInfoInstruction(), GetRandomInfoInstruction() });
				default:
					return new OrInstruction(new()
					{ GetRandomFacialInstruction(),
						GetRandomInfoInstruction(),
						new AndInstruction(new()
							{ GetRandomFacialInstruction(), GetRandomInfoInstruction()
					}),
						new AndInstruction(new()
							{ GetRandomInfoInstruction(), GetRandomInfoInstruction()
					})});
			}

		}

		private IInstruction GetRandomFacialInstruction() {
			List<string> facialFeatures = new List<string> { "EyeColor", "FaceColor", "HairColor" };
			int randomFacialFeatureIndex = Random.Range(0, facialFeatures.Count);
			NamedColor randomColorValue;

			switch (randomFacialFeatureIndex) {
				case 0:
					int colorListLength = profilePictureSettings.EyeColors.Colors.Count;
					int randomColorIndex = Random.Range(0, colorListLength);
					randomColorValue = profilePictureSettings.EyeColors.Colors[randomColorIndex];
					return new EyeColorInstruction(randomColorValue, new ColorEqualsComparator());
				case 1:
					colorListLength = profilePictureSettings.FaceColors.Colors.Count;
					randomColorIndex = Random.Range(0, colorListLength);
					randomColorValue = profilePictureSettings.FaceColors.Colors[randomColorIndex];
					return new FaceColorInstruction(randomColorValue, new ColorEqualsComparator());
				case 2:
					colorListLength = profilePictureSettings.HairColors.Colors.Count;
					randomColorIndex = Random.Range(0, colorListLength);
					randomColorValue = profilePictureSettings.HairColors.Colors[randomColorIndex];
					return new HairColorInstruction(randomColorValue, new ColorEqualsComparator());
			}
			return null;
		}

		private IInstruction GetRandomInfoInstruction() {
			List<string> infoFeatures = new List<string> { "Age", "Height", "Gender", "Interests" };
			int randomInfoFeaturesIndex = Random.Range(0, infoFeatures.Count);

			switch (randomInfoFeaturesIndex) {
				case 0:
					int randomAge = Random.Range(profileSettings.MinAge + 5, profileSettings.MaxAge - 5);
					var randomAgeComparator = GetRandomIntComparator();
					return new AgeInstruction(randomAge, randomAgeComparator);
				case 1:
					float randomHeight = Random.Range(profileSettings.MinHeight + 0.2f, profileSettings.MaxHeight - 0.2f);
					var randomHeightComparator = GetRandomFloatComparator();
					return new HeightInstruction(randomHeight, randomHeightComparator);
				case 2:
					Gender gender = GetRandomGender();
					var genderComparator = new EqualsComparator<Gender>();
					return new GenderInstruction(gender, genderComparator);
				case 3:
					int randomInterestsCount = Random.Range(0, 4);
					var randomInterestCOuntComparator = GetRandomIntComparator();
					return new InterestCountInstruction(randomInterestsCount, randomInterestCOuntComparator);
			}
			return null;
		}

		IComparator<float> GetRandomFloatComparator() {
			List<string> comparatorsList = new List<string> { "Greater", "Lower", "GreaterOrEquals", "LowerOrEquals", "Equals" };
			int randomComparatorIndex = Random.Range(0, comparatorsList.Count);

			switch (randomComparatorIndex) {
				case 0:
					return new GreaterComparator<float>();
				case 1:
					return new LowerComparator<float>();
				case 2:
					return new GreaterOrEqualsComparator<float>();
				case 3:
					return new LowerOrEqualsComparator<float>();
				case 4:
					return new EqualsComparator<float>();
			}
			return null;
		}

		IComparator<int> GetRandomIntComparator() {
			List<string> comparatorsList = new List<string> { "Greater", "Lower", "GreaterOrEquals", "LowerOrEquals", "Equals" };
			int randomComparatorIndex = Random.Range(0, comparatorsList.Count);

			switch (randomComparatorIndex) {
				case 0:
					return new GreaterComparator<int>();
				case 1:
					return new LowerComparator<int>();
				case 2:
					return new GreaterOrEqualsComparator<int>();
				case 3:
					return new LowerOrEqualsComparator<int>();
				case 4:
					return new EqualsComparator<int>();
			}
			return null;
		}

		private Gender GetRandomGender() {
			int randomGenderIndex = Random.Range(0, 2);
			switch (randomGenderIndex) {
				case 0:
					return Gender.Male;
				case 1:
					return Gender.Female;
				default:
					return Gender.Other;
			}
		}
	}
}
