using System;
using Assets._Scripts.Model.Instructions;
using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[Serializable]
	public class NumericAttributeConfig {
		public AttributeType AttributeType;
		public bool Enabled = true;

		[Range(0f, 1f)]
		[Tooltip("Chance to be picked when generating round rules")]
		public float Weight = 1f;

		[Header("Human Range")]
		public float HumanMin;
		public float HumanMax;

		[Header("Alien Range")]
		public float AlienMin;
		public float AlienMax;
	}
}
