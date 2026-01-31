using Assets._Scripts.Model.Instructions.Comparators;
using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class EyeColorInstruction : InstructionBase<Color> {
		public EyeColorInstruction(Color expectedValue, IComparator<Color> comparator)
			: base(expectedValue, comparator) { }

		public override Color GetValue(Profile profile) {
			return profile.PictureElements.EyeColor;
		}
	}
}
