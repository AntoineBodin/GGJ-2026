using UnityEngine;

namespace Assets._Scripts.Model.Instructions.Comparators {
	public class ColorEqualsComparator : IComparator<Color> {
		public bool Compare(Color value, Color expected) {
			return value == expected;
		}
	}
}
