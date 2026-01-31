using System.Collections.Generic;
using Assets._Scripts.Model.Instructions;
using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[CreateAssetMenu(fileName = "RoundGenerationConfig", menuName = "Scriptable Objects/RoundGenerationConfig")]
	public class RoundGenerationConfig : ScriptableObject {

		[Header("Logic Settings")]
		public LogicType LogicType = LogicType.And;

		[Min(1)]
		public int MinConditions = 1;

		[Min(1)]
		public int MaxConditions = 3;

		[Header("Tree Settings (only used if LogicType is Tree)")]
		[Min(1)]
		public int MinDepth = 1;

		[Min(1)]
		public int MaxDepth = 3;

		[Min(1)]
		public int MinWidth = 1;

		[Min(2)]
		public int MaxWidth = 2;

		[Header("Numeric Attributes")]
		public List<NumericAttributeConfig> NumericAttributes = new() {
			new NumericAttributeConfig {
				AttributeType = AttributeType.Age,
				Enabled = true,
				Weight = 1f,
				HumanMin = 18,
				HumanMax = 60,
				AlienMin = 28,
				AlienMax = 32
			},
			new NumericAttributeConfig {
				AttributeType = AttributeType.Height,
				Enabled = true,
				Weight = 1f,
				HumanMin = 1.5f,
				HumanMax = 1.9f,
				AlienMin = 1.9f,
				AlienMax = 2.2f
			},
			new NumericAttributeConfig {
				AttributeType = AttributeType.InterestCount,
				Enabled = true,
				Weight = 1f,
				HumanMin = 3,
				HumanMax = 8,
				AlienMin = 1,
				AlienMax = 2
			}
		};

		[Header("Color Attributes")]
		public List<ColorAttributeConfig> ColorAttributes = new() {
			new ColorAttributeConfig {
				AttributeType = AttributeType.EyeColor,
				Enabled = false,
				Weight = 0.5f
			},
			new ColorAttributeConfig {
				AttributeType = AttributeType.HairColor,
				Enabled = false,
				Weight = 0.5f
			},
			new ColorAttributeConfig {
				AttributeType = AttributeType.FaceColor,
				Enabled = false,
				Weight = 0.5f
			}
		};

		[Header("Difficulty")]
		[Range(0f, 1f)]
		[Tooltip("Chance for human values to be close to alien range (suspicious humans)")]
		public float SuspiciousHumanChance = 0.1f;

		[Range(0f, 1f)]
		[Tooltip("Chance for alien values to be at edge of alien range (hard to detect)")]
		public float SubtleAlienChance = 0.1f;
	}
}
