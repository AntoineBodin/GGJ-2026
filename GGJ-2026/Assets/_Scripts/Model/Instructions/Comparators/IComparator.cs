namespace Assets._Scripts.Model.Instructions {
	public interface IComparator<T> {
		bool Compare(T actualValue, T	expectedValue);
	}
}
