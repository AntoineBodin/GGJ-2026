using System.Collections.Generic;

using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {

	[CreateAssetMenu(fileName = "ProfileGenerationSettings", menuName = "Scriptable Objects/ProfileGenerationSettings")]
	internal class ProfileGenerationSettings : ScriptableObject {
		public List<string> FirstNamesMale;
		public List<string> FirstNamesFemale;
		public List<string> LastNames;
		public List<string> Interests;
		public List<string> Bios;

		[SerializeField]
		private int _minAge = 18;
		public int MinAge => _minAge;

		[SerializeField]
		private int _maxAge = 99;
		public int MaxAge => _maxAge;

		[SerializeField]
		private AnimationCurve _ageRandomFunction;
		public AnimationCurve AgeRandomFunction => _ageRandomFunction;

		[SerializeField]
		private int _minInterests = 3;
		public int MinInterests => _minInterests;

		[SerializeField]
		private int _maxInterests = 10;
		public int MaxInterests => _maxInterests;

		[SerializeField]
		private float minHeight = 1.4f;
		public float MinHeight => minHeight;

		[SerializeField]
		private float maxHeight = 2.4f;
		public float MaxHeight => maxHeight;

		[SerializeField]
		private AnimationCurve _heightRandomFunction;
		public AnimationCurve HeightRandomFunction => _heightRandomFunction;
	}
}
