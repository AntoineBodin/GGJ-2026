namespace Assets._Scripts.Model.Instructions {
	public abstract class InstructionBase<T> : IInstruction {
		public T ExpectedValue { get; private set; }
		public IComparator<T> Comparator { get; private set; }
		public abstract T GetValue(Profile profile);
		public bool Compare(T value) {
			return Comparator.Compare(value, ExpectedValue);
		}

		public bool IsValid(Profile profile) {
			return Compare(GetValue(profile));
		}
	}
}
