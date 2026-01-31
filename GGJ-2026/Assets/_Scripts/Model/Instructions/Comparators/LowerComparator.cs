using System;

namespace Assets._Scripts.Model.Instructions.Comparators {
	internal class LowerComparator<T> : IComparator<T> where T : IComparable<T> {
		public bool Compare(T actualValue, T expectedValue) {
			return actualValue.CompareTo(expectedValue) < 0;
		}
	}
}
