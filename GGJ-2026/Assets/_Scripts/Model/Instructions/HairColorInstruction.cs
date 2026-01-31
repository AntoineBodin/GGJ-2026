using Assets._Scripts.Model.Instructions.Comparators;
using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class HairColorInstruction : InstructionBase<Color> {
		public HairColorInstruction(Color expectedValue, IComparator<Color> comparator)
			: base(expectedValue, comparator) { }

		public override Color GetValue(Profile profile) {
			return profile.PictureElements.HairColor;
		}
	}
}
