using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public class AgeInstruction : InstructionBase<int> {
		public AgeInstruction(int expectedValue, IComparator<int> comparator)
			: base(expectedValue, comparator) { }

		public override int GetValue(Profile profile) {
			return profile.Age;
		}
	}
}
