using System;
using System.Collections.Generic;
using Assets._Scripts.Model.Instructions;
using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[Serializable]
	public class ColorAttributeConfig {
		public AttributeType AttributeType;
		public bool Enabled = true;

		[Range(0f, 1f)]
		[Tooltip("Chance to be picked when generating round rules")]
		public float Weight = 1f;

		[Header("Human Colors")]
		public List<Color> HumanColors = new();

		[Header("Alien Colors")]
		public List<Color> AlienColors = new();
	}
}
