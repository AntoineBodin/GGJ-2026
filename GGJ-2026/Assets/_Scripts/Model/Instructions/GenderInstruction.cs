using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public class GenderInstruction : InstructionBase<Gender> {
		public GenderInstruction(Gender expectedValue, IComparator<Gender> comparator)
			: base(expectedValue, comparator) { }

		public override Gender GetValue(Profile profile) {
			return profile.Gender;
		}

		public override string Describe() {
			return $"Aliens are {Comparator.Describe()} {ExpectedValue}.";
		}

		public override void ApplyAlienFeature(Profile profile) {
			profile.Gender = ExpectedValue;
		}
	}
}
