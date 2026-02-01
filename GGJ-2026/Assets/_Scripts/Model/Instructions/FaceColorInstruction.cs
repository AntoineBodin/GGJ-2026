using Assets._Scripts.Model.Instructions.Comparators;
using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class FaceColorInstruction : InstructionBase<NamedColor> {
		public FaceColorInstruction(NamedColor expectedValue, IComparator<NamedColor> comparator)
			: base(expectedValue, comparator) { }

		public override void ApplyAlienFeature(Profile profile) {
			profile.PictureElements.FaceColor = ExpectedValue;
		}

		public override string Describe() {
			return $"Aliens have a {ExpectedValue} Face.";
		}

		public override NamedColor GetValue(Profile profile) {
			return profile.PictureElements.FaceColor;
		}
	}
}
