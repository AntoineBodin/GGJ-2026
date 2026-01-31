using System.Collections.Generic;

using UnityEngine;

namespace Assets._Scripts.ScriptableObjects {
	[CreateAssetMenu(fileName = "ProfilePictureGenerationSettings", menuName = "Scriptable Objects/ProfilePictureGenerationSettings")]
	internal class ProfilePictureGenerationSettings : ScriptableObject {

		[SerializeField]
		private List<Sprite> _faces;
		public List<Sprite> Faces => _faces;

		[SerializeField]
		private GameColors _backgroundColors;
		public GameColors BackgroundColors => _backgroundColors;

		[SerializeField]
		private GameColors _faceColors;
		public GameColors FaceColors => _faceColors;

		[SerializeField]
		private List<Sprite> _eyeShapes;
		public List<Sprite> EyeShapes => _eyeShapes;

		[SerializeField]
		private GameColors _eyeColors;
		public GameColors EyeColors => _eyeColors;

		[SerializeField]
		private List<Sprite> _hairstyles;
		public List<Sprite> Hairstyles => _hairstyles;

		[SerializeField]
		private GameColors _hairColors;
		public GameColors HairColors => _hairColors;
	}
}
