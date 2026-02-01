using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
	public abstract class InstructionBase<T> : IInstruction {
		public T ExpectedValue { get; private set; }
		public IComparator<T> Comparator { get; private set; }

		protected InstructionBase(T expectedValue, IComparator<T> comparator) {
			ExpectedValue = expectedValue;
			Comparator = comparator;
		}

		public abstract T GetValue(Profile profile);
		public abstract string Describe();
		public bool Compare(T value) {
			return Comparator.Compare(value, ExpectedValue);
		}

		public bool IsValid(Profile profile) {
			return Compare(GetValue(profile));
		}

		public abstract void ApplyAlienFeature(Profile profile);
	}
}
