using System.Collections.Generic;

namespace Assets._Scripts.Model.Instructions.Comparators {
	// Checks if actual collection contains all expected elements
	internal class ContainsAllComparator<T> : IComparator<ICollection<T>> {
		public bool Compare(ICollection<T> actualValue, ICollection<T> expectedValue) {
			if (actualValue == null || expectedValue == null) {
				return false;
			}

			foreach (var expected in expectedValue) {
				if (!actualValue.Contains(expected)) {
					return false;
				}
			}

			return true;
		}

		public string Describe() {
			throw new System.NotImplementedException();
		}
	}
}
