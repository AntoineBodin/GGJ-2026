using System;

namespace Assets._Scripts.Model.Instructions.Comparators {
	public class GreaterComparator<T> : IComparator<T> where T : IComparable<T> {
		public bool Compare(T actualValue, T expectedValue) {
			return actualValue.CompareTo(expectedValue) > 0;
		}

		public string Describe() {
			return "is greater than";
		}
	}
}
