using System;
using System.Collections.Generic;

namespace Assets._Scripts.Model.Instructions.Comparators {
	// Checks if at least one expected element is contained in the actual collection
	internal class ContainsOneComparator<T> : IComparator<ICollection<T>> {
		public bool Compare(ICollection<T> actualValue, ICollection<T> expectedValue) {
			if (actualValue == null || expectedValue == null) {
				return false;
			}

			foreach (var expected in expectedValue) {
				if (actualValue.Contains(expected)) {
					return true;
				}
			}

			return false;
		}
	}
}
