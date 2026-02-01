using System.Collections.Generic;

namespace Assets._Scripts.Model.Instructions.Comparators {
	public class EqualsComparator<T> : IComparator<T> {
		public bool Compare(T actualValue, T expectedValue) {
			return EqualityComparer<T>.Default.Equals(actualValue, expectedValue);
		}

		public string Describe() {
			return "equals to";
		}
	}
}
