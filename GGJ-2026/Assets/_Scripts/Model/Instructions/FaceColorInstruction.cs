using Assets._Scripts.Model.Instructions.Comparators;
using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class FaceColorInstruction : InstructionBase<Color> {
		public FaceColorInstruction(Color expectedValue, IComparator<Color> comparator)
			: base(expectedValue, comparator) { }

		public override Color GetValue(Profile profile) {
			return profile.PictureElements.FaceColor;
		}
	}
}
