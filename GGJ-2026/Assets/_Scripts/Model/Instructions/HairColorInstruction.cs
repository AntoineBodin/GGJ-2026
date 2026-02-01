using Assets._Scripts.Model.Instructions.Comparators;
using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class HairColorInstruction : InstructionBase<NamedColor> {
		public HairColorInstruction(NamedColor expectedValue, IComparator<NamedColor> comparator)
			: base(expectedValue, comparator) { }

		public override string Describe() {
			return $"Aliens hair is {ExpectedValue}.";
		}

		public override NamedColor GetValue(Profile profile) {
			return profile.PictureElements.HairColor;
		}

		public override void ApplyAlienFeature(Profile profile) {
			profile.PictureElements.HairColor = ExpectedValue;
		}
	}
}
