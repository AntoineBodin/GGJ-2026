using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public class HeightInstruction : InstructionBase<float> {
		public HeightInstruction(float expectedValue, IComparator<float> comparator)
			: base(expectedValue, comparator) { }

		public override float GetValue(Profile profile) {
			return profile.Height;
		}
	}
}
