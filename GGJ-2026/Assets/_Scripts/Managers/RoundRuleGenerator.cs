using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Model;
using Assets._Scripts.Model.Instructions;
using Assets._Scripts.Model.Instructions.Comparators;
using Assets._Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets._Scripts.Managers {
	public class RoundRuleGenerator {
		private readonly RoundGenerationConfig _config;

		public RoundRuleGenerator(RoundGenerationConfig config) {
			_config = config;
		}

		/// <summary>
		/// Generates a detection rule for the current round.
		/// Returns the combined instruction and the list of individual instructions for reference.
		/// </summary>
		public GeneratedRule GenerateRule() {
			int conditionCount = Random.Range(_config.MinConditions, _config.MaxConditions + 1);

			// Gather all enabled attributes
			var availableAttributes = GetAvailableAttributes();

			if (availableAttributes.Count == 0) {
				Debug.LogError("No enabled attributes in config!");
				return null;
			}

			// Pick random attributes based on weights
			var pickedAttributes = PickRandomAttributes(availableAttributes, conditionCount);

			// Generate instructions for each picked attribute
			var instructions = new List<GeneratedInstruction>();
			foreach (var attr in pickedAttributes) {
				var instruction = GenerateInstructionForAttribute(attr);
				if (instruction != null) {
					instructions.Add(instruction);
				}
			}

			if (instructions.Count == 0) {
				Debug.LogError("Failed to generate any instructions!");
				return null;
			}

			// Combine instructions based on logic type
			IInstruction combinedRule = CombineInstructions(instructions.Select(i => i.Instruction).ToList());

			return new GeneratedRule {
				CombinedRule = combinedRule,
				Instructions = instructions,
				LogicType = _config.LogicType
			};
		}

		private List<AttributeConfigBase> GetAvailableAttributes() {
			var available = new List<AttributeConfigBase>();

			foreach (var attr in _config.NumericAttributes) {
				if (attr.Enabled) {
					available.Add(new AttributeConfigBase {
						AttributeType = attr.AttributeType,
						Weight = attr.Weight,
						NumericConfig = attr
					});
				}
			}

			foreach (var attr in _config.ColorAttributes) {
				if (attr.Enabled && attr.AlienColors.Count > 0) {
					available.Add(new AttributeConfigBase {
						AttributeType = attr.AttributeType,
						Weight = attr.Weight,
						ColorConfig = attr
					});
				}
			}

			return available;
		}

		private List<AttributeConfigBase> PickRandomAttributes(List<AttributeConfigBase> available, int count) {
			count = Mathf.Min(count, available.Count);

			// Weighted random selection without replacement
			var picked = new List<AttributeConfigBase>();
			var remaining = new List<AttributeConfigBase>(available);

			for (int i = 0; i < count; i++) {
				float totalWeight = remaining.Sum(a => a.Weight);
				float randomValue = Random.Range(0f, totalWeight);

				float cumulative = 0f;
				for (int j = 0; j < remaining.Count; j++) {
					cumulative += remaining[j].Weight;
					if (randomValue <= cumulative) {
						picked.Add(remaining[j]);
						remaining.RemoveAt(j);
						break;
					}
				}
			}

			return picked;
		}

		private GeneratedInstruction GenerateInstructionForAttribute(AttributeConfigBase attr) {
			if (attr.NumericConfig != null) {
				return GenerateNumericInstruction(attr.NumericConfig);
			} else if (attr.ColorConfig != null) {
				return GenerateColorInstruction(attr.ColorConfig);
			}
			return null;
		}

		private GeneratedInstruction GenerateNumericInstruction(NumericAttributeConfig config) {
			bool isIntType = config.AttributeType == AttributeType.Age ||
			                 config.AttributeType == AttributeType.InterestCount;

			if (isIntType) {
				return GenerateIntInstruction(config);
			} else {
				return GenerateFloatInstruction(config);
			}
		}

		private GeneratedInstruction GenerateIntInstruction(NumericAttributeConfig config) {
			int alienMin = Mathf.RoundToInt(config.AlienMin);
			int alienMax = Mathf.RoundToInt(config.AlienMax);
			int humanMin = Mathf.RoundToInt(config.HumanMin);
			int humanMax = Mathf.RoundToInt(config.HumanMax);

			int alienValue;
			IInstruction instruction;
			string description;

			bool alienIsHigher = alienMin >= humanMax;
			bool alienIsLower = alienMax <= humanMin;

			if (alienIsHigher) {
				alienValue = Random.Range(alienMin, alienMax + 1);
				instruction = CreateIntInstruction(config.AttributeType, alienMin, new GreaterOrEqualsComparator<int>());
				description = $"{config.AttributeType} >= {alienMin}";
			} else if (alienIsLower) {
				alienValue = Random.Range(alienMin, alienMax + 1);
				instruction = CreateIntInstruction(config.AttributeType, alienMax, new LowerOrEqualsComparator<int>());
				description = $"{config.AttributeType} <= {alienMax}";
			} else {
				// Ranges overlap
				int alienOnlyMin = Mathf.Max(alienMin, humanMax + 1);
				int alienOnlyMax = alienMax;

				if (alienOnlyMax >= alienOnlyMin) {
					alienValue = Random.Range(alienOnlyMin, alienOnlyMax + 1);
					instruction = CreateIntInstruction(config.AttributeType, alienOnlyMin, new GreaterOrEqualsComparator<int>());
					description = $"{config.AttributeType} >= {alienOnlyMin}";
				} else {
					alienOnlyMin = alienMin;
					alienOnlyMax = Mathf.Min(alienMax, humanMin - 1);

					if (alienOnlyMax >= alienOnlyMin) {
						alienValue = Random.Range(alienOnlyMin, alienOnlyMax + 1);
						instruction = CreateIntInstruction(config.AttributeType, alienOnlyMax, new LowerOrEqualsComparator<int>());
						description = $"{config.AttributeType} <= {alienOnlyMax}";
					} else {
						// Complete overlap - need BOTH boundaries to define alien range
						alienValue = Random.Range(alienMin, alienMax + 1);

						var lowerBound = CreateIntInstruction(config.AttributeType, alienMin, new GreaterOrEqualsComparator<int>());
						var upperBound = CreateIntInstruction(config.AttributeType, alienMax, new LowerOrEqualsComparator<int>());
						instruction = new AndInstruction(new List<IInstruction> { lowerBound, upperBound });
						description = $"{config.AttributeType} >= {alienMin} AND <= {alienMax}";
					}
				}
			}

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				AlienValue = alienValue,
				NumericConfig = config
			};
		}

		private GeneratedInstruction GenerateFloatInstruction(NumericAttributeConfig config) {
			float alienValue;
			IInstruction instruction;
			string description;

			bool alienIsHigher = config.AlienMin >= config.HumanMax;
			bool alienIsLower = config.AlienMax <= config.HumanMin;

			if (alienIsHigher) {
				alienValue = Random.Range(config.AlienMin, config.AlienMax);
				instruction = CreateFloatInstruction(config.AttributeType, config.AlienMin, new GreaterOrEqualsComparator<float>());
				description = $"{config.AttributeType} >= {config.AlienMin:F2}";
			} else if (alienIsLower) {
				alienValue = Random.Range(config.AlienMin, config.AlienMax);
				instruction = CreateFloatInstruction(config.AttributeType, config.AlienMax, new LowerOrEqualsComparator<float>());
				description = $"{config.AttributeType} <= {config.AlienMax:F2}";
			} else {
				float alienOnlyMin = Mathf.Max(config.AlienMin, config.HumanMax);
				float alienOnlyMax = config.AlienMax;

				if (alienOnlyMax > alienOnlyMin) {
					alienValue = Random.Range(alienOnlyMin, alienOnlyMax);
					instruction = CreateFloatInstruction(config.AttributeType, alienOnlyMin, new GreaterOrEqualsComparator<float>());
					description = $"{config.AttributeType} >= {alienOnlyMin:F2}";
				} else {
					alienOnlyMin = config.AlienMin;
					alienOnlyMax = Mathf.Min(config.AlienMax, config.HumanMin);

					if (alienOnlyMax > alienOnlyMin) {
						alienValue = Random.Range(alienOnlyMin, alienOnlyMax);
						instruction = CreateFloatInstruction(config.AttributeType, alienOnlyMax, new LowerOrEqualsComparator<float>());
						description = $"{config.AttributeType} <= {alienOnlyMax:F2}";
					} else {
						// Complete overlap - need BOTH boundaries to define alien range
						alienValue = Random.Range(config.AlienMin, config.AlienMax);

						var lowerBound = CreateFloatInstruction(config.AttributeType, config.AlienMin, new GreaterOrEqualsComparator<float>());
						var upperBound = CreateFloatInstruction(config.AttributeType, config.AlienMax, new LowerOrEqualsComparator<float>());
						instruction = new AndInstruction(new List<IInstruction> { lowerBound, upperBound });
						description = $"{config.AttributeType} >= {config.AlienMin:F2} AND <= {config.AlienMax:F2}";
					}
				}
			}

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				AlienValue = alienValue,
				NumericConfig = config
			};
		}

		private IInstruction CreateFloatInstruction(AttributeType type, float value, IComparator<float> comparator) {
			switch (type) {
				case AttributeType.Height:
					return new HeightInstruction(value, comparator);
				default:
					return null;
			}
		}

		private IInstruction CreateIntInstruction(AttributeType type, int value, IComparator<int> comparator) {
			switch (type) {
				case AttributeType.Age:
					return new AgeInstruction(value, comparator);
				case AttributeType.InterestCount:
					return new InterestCountInstruction(value, comparator);
				default:
					return null;
			}
		}

		private GeneratedInstruction GenerateColorInstruction(ColorAttributeConfig config) {
			// Pick a random alien color
			Color alienColor = config.AlienColors[Random.Range(0, config.AlienColors.Count)];

			IInstruction instruction = CreateColorInstruction(config.AttributeType, alienColor);
			string description = $"{config.AttributeType} == {alienColor}";

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				ColorValue = alienColor,
				ColorConfig = config
			};
		}

		private IInstruction CreateColorInstruction(AttributeType type, Color color) {
			var comparator = new ColorEqualsComparator();
			switch (type) {
				case AttributeType.EyeColor:
					return new EyeColorInstruction(color, comparator);
				case AttributeType.HairColor:
					return new HairColorInstruction(color, comparator);
				case AttributeType.FaceColor:
					return new FaceColorInstruction(color, comparator);
				default:
					return null;
			}
		}

		private IInstruction CombineInstructions(List<IInstruction> instructions) {
			if (instructions.Count == 1) {
				return instructions[0];
			}

			switch (_config.LogicType) {
				case LogicType.And:
					return new AndInstruction(instructions);
				case LogicType.Or:
					return new OrInstruction(instructions);
				case LogicType.Tree:
					return BuildTree(instructions);
				default:
					return new AndInstruction(instructions);
			}
		}

		private IInstruction BuildTree(List<IInstruction> instructions) {
			if (instructions.Count == 1) {
				return instructions[0];
			}

			if (instructions.Count == 2) {
				// With 2 instructions, randomly choose AND or OR
				if (Random.value > 0.5f) {
					return new AndInstruction(instructions);
				}
				return new OrInstruction(instructions);
			}

			// Split instructions into groups, then: (group1) AND (group2) AND ...
			// Where each group is: (A OR B OR ...)
			// Result: must match at least one from each group
			int groupCount = Mathf.Min(_config.MaxDepth, instructions.Count);
			groupCount = Mathf.Max(2, groupCount); // At least 2 groups

			var groups = new List<List<IInstruction>>();
			for (int i = 0; i < groupCount; i++) {
				groups.Add(new List<IInstruction>());
			}

			// Distribute instructions across groups
			for (int i = 0; i < instructions.Count; i++) {
				groups[i % groupCount].Add(instructions[i]);
			}

			// Build each group as OR, then combine with AND
			var groupInstructions = new List<IInstruction>();
			foreach (var group in groups) {
				if (group.Count == 0) continue;
				if (group.Count == 1) {
					groupInstructions.Add(group[0]);
				} else {
					groupInstructions.Add(new OrInstruction(group));
				}
			}

			if (groupInstructions.Count == 1) {
				return groupInstructions[0];
			}

			return new AndInstruction(groupInstructions);
		}
	}

	// Helper class to hold attribute config info
	public class AttributeConfigBase {
		public AttributeType AttributeType;
		public float Weight;
		public NumericAttributeConfig NumericConfig;
		public ColorAttributeConfig ColorConfig;
	}

	// Result of rule generation
	public class GeneratedRule {
		public IInstruction CombinedRule;
		public List<GeneratedInstruction> Instructions;
		public LogicType LogicType;
	}

	// Individual generated instruction with metadata
	public class GeneratedInstruction {
		public IInstruction Instruction;
		public AttributeType AttributeType;
		public string Description;

		// Values for profile generation
		public float AlienValue;
		public Color ColorValue;
		public NumericAttributeConfig NumericConfig;
		public ColorAttributeConfig ColorConfig;
	}
}
