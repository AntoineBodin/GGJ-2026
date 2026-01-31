using System.Collections.Generic;

using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[CreateAssetMenu(fileName = "ProfilePictureGenerationSettings", menuName = "Scriptable Objects/ProfilePictureGenerationSettings")]
	internal class ProfilePictureGenerationSettings : ScriptableObject {

		[SerializeField]
		private List<Sprite> _faces;
		public List<Sprite> Faces => _faces;

		[SerializeField]
		private List<Color> _backgroundColors;
		public List<Color> BackgroundColors => _backgroundColors;

		[SerializeField]
		private List<Color> _faceColors;
		public List<Color> FaceColors => _faceColors;

		[SerializeField]
		private List<Sprite> _eyeShapes;
		public List<Sprite> EyeShapes => _eyeShapes;

		[SerializeField]
		private List<Color> _eyeColors;
		public List<Color> EyeColors => _eyeColors;

		[SerializeField]
		private List<Sprite> _hairstyles;
		public List<Sprite> Hairstyles => _hairstyles;

		[SerializeField]
		private List<Color> _hairColors;
		public List<Color> HairColors => _hairColors;
	}
}
