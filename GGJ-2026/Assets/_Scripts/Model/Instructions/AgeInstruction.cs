using Assets._Scripts.Model.Instructions.Comparators;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class AgeInstruction : InstructionBase<int> {
		public AgeInstruction(int expectedValue, IComparator<int> comparator)
			: base(expectedValue, comparator) { }

		public override int GetValue(Profile profile) {
			return profile.Age;
		}

		public override string Describe() {
			return $"Aliens are {Comparator.Describe()} {ExpectedValue} years old.";
		}

		public override void ApplyAlienFeature(Profile profile) {
			if (Comparator is GreaterComparator<int> || Comparator is GreaterOrEqualsComparator<int>) {
				profile.Age = Random.Range(ExpectedValue + 1, 99);
			} else if (Comparator is LowerComparator<int> || Comparator is LowerOrEqualsComparator<int>) {
				profile.Age = Random.Range(18, ExpectedValue);
			} else if (Comparator is EqualsComparator<int>) {
				profile.Age = ExpectedValue;
			}
		}
	}
}
