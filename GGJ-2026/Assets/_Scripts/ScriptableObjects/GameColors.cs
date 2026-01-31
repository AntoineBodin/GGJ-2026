using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[CustomPropertyDrawer(typeof(NamedColor))]
	public class NamedColorDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var nameProp = property.FindPropertyRelative("_name");
			var colorProp = property.FindPropertyRelative("_color");

			label.text = string.IsNullOrEmpty(nameProp.stringValue)
					? "Color"
					: nameProp.stringValue;

			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.PropertyField(position, property, label, true);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return property.isExpanded
					? EditorGUIUtility.singleLineHeight * 3
					: EditorGUIUtility.singleLineHeight;
		}
	}

	[Serializable]
	public class NamedColor {
		[SerializeField]
		private string _name;
		public string Name => _name;

		[SerializeField]
		private Color _color;
		public Color Color => _color;
	}

	[CreateAssetMenu(fileName = "GameColors", menuName = "Scriptable Objects/GameColors")]
	public class GameColors : ScriptableObject {
		[SerializeField]
		private List<NamedColor> _colors;
		public List<NamedColor> Colors => _colors;
	}
}
