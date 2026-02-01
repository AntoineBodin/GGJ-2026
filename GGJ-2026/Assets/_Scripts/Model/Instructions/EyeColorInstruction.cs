using Assets._Scripts.Model.Instructions.Comparators;
using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class EyeColorInstruction : InstructionBase<NamedColor> {
		public EyeColorInstruction(NamedColor expectedValue, IComparator<NamedColor> comparator)
			: base(expectedValue, comparator) { }

		public override void ApplyAlienFeature(Profile profile) {
			profile.PictureElements.EyeColor = ExpectedValue;
		}

		public override string Describe() {
			return $"Aliens eyes are {ExpectedValue}";
		}

		public override NamedColor GetValue(Profile profile) {
			return profile.PictureElements.EyeColor;
		}
	}
}
