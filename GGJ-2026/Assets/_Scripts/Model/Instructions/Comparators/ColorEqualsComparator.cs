using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions.Comparators {
	public class ColorEqualsComparator : IComparator<NamedColor> {
		public bool Compare(NamedColor value, NamedColor expected) {
			return value.Name == expected.Name;
		}

		public string Describe() {
			return "are";
		}
	}
}
