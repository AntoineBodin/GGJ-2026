namespace Assets._Scripts.Model.Instructions.Comparators {
	public interface IComparator<T> {
		bool Compare(T actualValue, T expectedValue);
		string Describe();
	}
}
