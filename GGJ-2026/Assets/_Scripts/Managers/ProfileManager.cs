using Assets._Scripts.Model;

using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers {
	internal class ProfileManager : SingletonBehaviour<ProfileManager> {
		[field: SerializeField]
		protected Image Background { get; private set; }
		[field: SerializeField]
		protected Image Hair { get; private set; }
		[field: SerializeField]
		protected Image Face { get; private set; }
		[field: SerializeField]
		protected Image Eyes { get; private set; }

		public void DisplayProfile(Profile profile) {
			Background.color = profile.PictureElements.BackgroundColor;
			Hair.sprite = profile.PictureElements.HairstyleSprite;
			Hair.color = profile.PictureElements.HairColor;
			Face.sprite = profile.PictureElements.FaceSprite;
			Face.color = profile.PictureElements.FaceColor;
			Eyes.sprite = profile.PictureElements.EyeShapeSprite;
			Eyes.color = profile.PictureElements.EyeColor;
		}
	}
}
