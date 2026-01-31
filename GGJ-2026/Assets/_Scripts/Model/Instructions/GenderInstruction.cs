using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public class GenderInstruction : InstructionBase<Gender> {
		public GenderInstruction(Gender expectedValue, IComparator<Gender> comparator)
			: base(expectedValue, comparator) { }

		public override Gender GetValue(Profile profile) {
			return profile.Gender;
		}
	}
}
