using Assets._Scripts.Model.Instructions.Comparators;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class HeightInstruction : InstructionBase<float> {
		public HeightInstruction(float expectedValue, IComparator<float> comparator)
			: base(expectedValue, comparator) { }

		public override void ApplyAlienFeature(Profile profile) {
			if (Comparator is GreaterComparator<int> || Comparator is GreaterOrEqualsComparator<int>) {
				profile.Height = Random.Range(ExpectedValue + 0.01f , 2.4f);
			} else if (Comparator is LowerComparator<int> || Comparator is LowerOrEqualsComparator<int>) {
				profile.Height = Random.Range(1.4f, ExpectedValue);
			} else if (Comparator is EqualsComparator<int>) {
				profile.Height = ExpectedValue;
			}
		}

		public override string Describe() {
			return $"Aliens are {Comparator.Describe()} {ExpectedValue} meters tall.";
		}

		public override float GetValue(Profile profile) {
			return profile.Height;
		}
	}
}
