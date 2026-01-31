using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public class InterestCountInstruction : InstructionBase<int> {
		public InterestCountInstruction(int expectedValue, IComparator<int> comparator)
			: base(expectedValue, comparator) { }

		public override int GetValue(Profile profile) {
			return profile.Interests.Count;
		}
	}
}
