using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			return new GeneratedRule(combinedRule, instructions, _config.LogicType);
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
			ComparatorType comparatorType;

			bool alienIsHigher = alienMin >= humanMax;
			bool alienIsLower = alienMax <= humanMin;

			if (alienIsHigher) {
				alienValue = Random.Range(alienMin, alienMax + 1);
				instruction = CreateIntInstruction(config.AttributeType, alienMin, new GreaterOrEqualsComparator<int>());
				description = $"{config.AttributeType} >= {alienMin}";
				comparatorType = ComparatorType.GreaterOrEquals;
			} else if (alienIsLower) {
				alienValue = Random.Range(alienMin, alienMax + 1);
				instruction = CreateIntInstruction(config.AttributeType, alienMax, new LowerOrEqualsComparator<int>());
				description = $"{config.AttributeType} <= {alienMax}";
				comparatorType = ComparatorType.LowerOrEquals;
			} else {
				// Ranges overlap
				int alienOnlyMin = Mathf.Max(alienMin, humanMax + 1);
				int alienOnlyMax = alienMax;

				if (alienOnlyMax >= alienOnlyMin) {
					alienValue = Random.Range(alienOnlyMin, alienOnlyMax + 1);
					instruction = CreateIntInstruction(config.AttributeType, alienOnlyMin, new GreaterOrEqualsComparator<int>());
					description = $"{config.AttributeType} >= {alienOnlyMin}";
					comparatorType = ComparatorType.GreaterOrEquals;
				} else {
					alienOnlyMin = alienMin;
					alienOnlyMax = Mathf.Min(alienMax, humanMin - 1);

					if (alienOnlyMax >= alienOnlyMin) {
						alienValue = Random.Range(alienOnlyMin, alienOnlyMax + 1);
						instruction = CreateIntInstruction(config.AttributeType, alienOnlyMax, new LowerOrEqualsComparator<int>());
						description = $"{config.AttributeType} <= {alienOnlyMax}";
						comparatorType = ComparatorType.LowerOrEquals;
					} else {
						// Complete overlap - need BOTH boundaries to define alien range
						alienValue = Random.Range(alienMin, alienMax + 1);

						var lowerBound = CreateIntInstruction(config.AttributeType, alienMin, new GreaterOrEqualsComparator<int>());
						var upperBound = CreateIntInstruction(config.AttributeType, alienMax, new LowerOrEqualsComparator<int>());
						instruction = new AndInstruction(new List<IInstruction> { lowerBound, upperBound });
						description = $"{config.AttributeType} >= {alienMin} AND <= {alienMax}";
						comparatorType = ComparatorType.Range;
					}
				}
			}

			string humanReadableDescription = DescriptionBuilder.BuildHumanReadableDescription(config.AttributeType, comparatorType, alienValue);

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				HumanReadableDescription = humanReadableDescription,
				AlienValue = alienValue,
				NumericConfig = config
			};
		}

		private GeneratedInstruction GenerateFloatInstruction(NumericAttributeConfig config) {
			float alienValue;
			IInstruction instruction;
			string description;
			ComparatorType comparatorType;

			bool alienIsHigher = config.AlienMin >= config.HumanMax;
			bool alienIsLower = config.AlienMax <= config.HumanMin;

			if (alienIsHigher) {
				alienValue = Random.Range(config.AlienMin, config.AlienMax);
				instruction = CreateFloatInstruction(config.AttributeType, config.AlienMin, new GreaterOrEqualsComparator<float>());
				description = $"{config.AttributeType} >= {config.AlienMin:F2}";
				comparatorType = ComparatorType.GreaterOrEquals;
			} else if (alienIsLower) {
				alienValue = Random.Range(config.AlienMin, config.AlienMax);
				instruction = CreateFloatInstruction(config.AttributeType, config.AlienMax, new LowerOrEqualsComparator<float>());
				description = $"{config.AttributeType} <= {config.AlienMax:F2}";
				comparatorType = ComparatorType.LowerOrEquals;
			} else {
				float alienOnlyMin = Mathf.Max(config.AlienMin, config.HumanMax);
				float alienOnlyMax = config.AlienMax;

				if (alienOnlyMax > alienOnlyMin) {
					alienValue = Random.Range(alienOnlyMin, alienOnlyMax);
					instruction = CreateFloatInstruction(config.AttributeType, alienOnlyMin, new GreaterOrEqualsComparator<float>());
					description = $"{config.AttributeType} >= {alienOnlyMin:F2}";
					comparatorType = ComparatorType.GreaterOrEquals;
				} else {
					alienOnlyMin = config.AlienMin;
					alienOnlyMax = Mathf.Min(config.AlienMax, config.HumanMin);

					if (alienOnlyMax > alienOnlyMin) {
						alienValue = Random.Range(alienOnlyMin, alienOnlyMax);
						instruction = CreateFloatInstruction(config.AttributeType, alienOnlyMax, new LowerOrEqualsComparator<float>());
						description = $"{config.AttributeType} <= {alienOnlyMax:F2}";
						comparatorType = ComparatorType.LowerOrEquals;
					} else {
						// Complete overlap - need BOTH boundaries to define alien range
						alienValue = Random.Range(config.AlienMin, config.AlienMax);

						var lowerBound = CreateFloatInstruction(config.AttributeType, config.AlienMin, new GreaterOrEqualsComparator<float>());
						var upperBound = CreateFloatInstruction(config.AttributeType, config.AlienMax, new LowerOrEqualsComparator<float>());
						instruction = new AndInstruction(new List<IInstruction> { lowerBound, upperBound });
						description = $"{config.AttributeType} >= {config.AlienMin:F2} AND <= {config.AlienMax:F2}";
						comparatorType = ComparatorType.Range;
					}
				}
			}

			string humanReadableDescription = DescriptionBuilder.BuildHumanReadableDescription(config.AttributeType, comparatorType, alienValue);

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				HumanReadableDescription = humanReadableDescription,
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

			string humanReadableDescription = DescriptionBuilder.BuildHumanReadableDescription(config.AttributeType, ComparatorType.Equals, alienColor);

			return new GeneratedInstruction {
				Instruction = instruction,
				AttributeType = config.AttributeType,
				Description = description,
				HumanReadableDescription = humanReadableDescription,
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

	public enum ComparatorType {
		Equals,
		GreaterOrEquals,
		LowerOrEquals,
		Greater,
		Lower,
		Range
	}

	public static class DescriptionBuilder {
		public static string BuildHumanReadableDescription(AttributeType attributeType, ComparatorType comparatorType, object value) {
			string comparatorText = comparatorType switch {
				ComparatorType.GreaterOrEquals => "at least",
				ComparatorType.LowerOrEquals => "at most",
				ComparatorType.Greater => "more than",
				ComparatorType.Lower => "less than",
				ComparatorType.Range => "between",
				ComparatorType.Equals => "",
				_ => ""
			};

			return attributeType switch {
				// Numeric attributes
				AttributeType.Age => $"The aliens are {comparatorText} {value} years old.",
				AttributeType.Height => $"The aliens are {comparatorText} {value:F2} meters tall.",
				AttributeType.InterestCount => $"The aliens have a list of interests that is {comparatorText} {value} items long.",
				
				// Color attributes
				AttributeType.EyeColor => $"The aliens masks eyes are {value}.",
				AttributeType.HairColor => $"The aliens masks hair is {value}.",
				AttributeType.FaceColor => $"The aliens masks skin is {value}.",
				
				_ => $"{attributeType}: {comparatorText} {value}"
			};
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

		public GeneratedRule(IInstruction combinedRule, List<GeneratedInstruction> instructions, LogicType logicType) {
			CombinedRule = combinedRule;
			Instructions = instructions;
			LogicType = logicType;
			HumanReadableDescription = BuildHumanReadableDescription();
		}

		public IInstruction CombinedRule;
		public List<GeneratedInstruction> Instructions;
		public LogicType LogicType;
		public string HumanReadableDescription;

		private string BuildHumanReadableDescription() {
			if (Instructions == null || Instructions.Count == 0) {
				return "";
			}

			switch (LogicType) {
				case LogicType.And:
					return string.Join("\nAND ", Instructions.Select(i => i.HumanReadableDescription));

				case LogicType.Or:
					return string.Join("\nOR ", Instructions.Select(i => i.HumanReadableDescription));

				case LogicType.Tree:
					return BuildTreeDescription(CombinedRule);

				default:
					return string.Join("\n", Instructions.Select(i => i.HumanReadableDescription));
			}
		}

		private string BuildTreeDescription(IInstruction instruction) {
			var paths = new List<string>();
			CollectPaths(instruction, new List<string>(), paths);
			return string.Join("\n", paths);
		}

		private void CollectPaths(IInstruction instruction, List<string> currentPath, List<string> allPaths) {
			if (instruction is AndInstruction andInstr) {
				// AND: all children must be satisfied, combine them in the same path
				var childDescriptions = new List<string>();
				foreach (var child in andInstr.Instructions) {
					if (child is CompositeInstruction) {
						// Recurse into composite, but wrap in parentheses
						var subPaths = new List<string>();
						CollectPaths(child, new List<string>(), subPaths);
						if (subPaths.Count == 1) {
							childDescriptions.Add(subPaths[0]);
						} else {
							childDescriptions.Add("(" + string.Join(" OR ", subPaths) + ")");
						}
					} else {
						childDescriptions.Add(GetLeafDescription(child));
					}
				}
				string combined = string.Join(" AND ", childDescriptions);
				if (currentPath.Count > 0) {
					allPaths.Add(string.Join(" AND ", currentPath) + " AND " + combined);
				} else {
					allPaths.Add(combined);
				}
			} else if (instruction is OrInstruction orInstr) {
				// OR: each child represents an alternative path
				foreach (var child in orInstr.Instructions) {
					if (child is CompositeInstruction) {
						CollectPaths(child, new List<string>(currentPath), allPaths);
					} else {
						var newPath = new List<string>(currentPath) { GetLeafDescription(child) };
						allPaths.Add(string.Join(" AND ", newPath));
					}
				}
			} else {
				// Leaf instruction
				var newPath = new List<string>(currentPath) { GetLeafDescription(instruction) };
				allPaths.Add(string.Join(" AND ", newPath));
			}
		}

		private string GetLeafDescription(IInstruction instruction) {
			// Try to find matching GeneratedInstruction
			var match = Instructions.FirstOrDefault(i => i.Instruction == instruction);
			if (match != null) {
				return match.HumanReadableDescription;
			}

			// Fallback: check if it's an AndInstruction with nested simple instructions (range case)
			if (instruction is AndInstruction andInstr) {
				var descriptions = new List<string>();
				foreach (var child in andInstr.Instructions) {
					descriptions.Add(GetLeafDescription(child));
				}
				if (descriptions.Count > 0) {
					return string.Join(" AND ", descriptions);
				}
			}

			// Fallback for known instruction types not in the Instructions list
			// This happens when instructions are created internally (e.g., range bounds)
			if (instruction is AgeInstruction ageInstr) {
				var comparatorType = GetComparatorType(ageInstr.Comparator);
				return DescriptionBuilder.BuildHumanReadableDescription(AttributeType.Age, comparatorType, ageInstr.ExpectedValue);
			}
			if (instruction is HeightInstruction heightInstr) {
				var comparatorType = GetComparatorType(heightInstr.Comparator);
				return DescriptionBuilder.BuildHumanReadableDescription(AttributeType.Height, comparatorType, heightInstr.ExpectedValue);
			}
			if (instruction is InterestCountInstruction interestInstr) {
				var comparatorType = GetComparatorType(interestInstr.Comparator);
				return DescriptionBuilder.BuildHumanReadableDescription(AttributeType.InterestCount, comparatorType, interestInstr.ExpectedValue);
			}

			return instruction.ToString();
		}

		private ComparatorType GetComparatorType(object comparator) {
			string typeName = comparator.GetType().Name;
			
			if (typeName.StartsWith("GreaterOrEqualsComparator")) return ComparatorType.GreaterOrEquals;
			if (typeName.StartsWith("LowerOrEqualsComparator")) return ComparatorType.LowerOrEquals;
			if (typeName.StartsWith("GreaterComparator")) return ComparatorType.Greater;
			if (typeName.StartsWith("LowerComparator")) return ComparatorType.Lower;
			
			return ComparatorType.Equals;
		}
	}

	// Individual generated instruction with metadata
	public class GeneratedInstruction {
		public IInstruction Instruction;
		public AttributeType AttributeType;
		public string Description;
		public string HumanReadableDescription;

		// Values for profile generation
		public float AlienValue;
		public Color ColorValue;
		public NumericAttributeConfig NumericConfig;
		public ColorAttributeConfig ColorConfig;
	}
}
