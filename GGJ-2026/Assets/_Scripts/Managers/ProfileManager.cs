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
		[field: SerializeField]
		protected Image Nose { get; private set; }
		[field: SerializeField]
		protected Image Mouth { get; private set; }
		[field: SerializeField]
		protected Image Accessory { get; private set; }


		public void DisplayProfile(Profile profile) {
			Background.color = profile.PictureElements.BackgroundColor.Color;
			Hair.sprite = profile.PictureElements.HairstyleSprite;
			Hair.color = profile.PictureElements.HairColor.Color;
			Face.sprite = profile.PictureElements.FaceSprite;
			Face.color = profile.PictureElements.FaceColor.Color;
			Eyes.sprite = profile.PictureElements.EyeShapeSprite;
			Eyes.color = profile.PictureElements.EyeColor.Color;
			Nose.sprite = profile.PictureElements.NoseShapeSprite;
			Mouth.sprite = profile.PictureElements.MouthShapeSprite;
			if (profile.PictureElements.AccessorySprite != null) {
				Accessory.sprite = profile.PictureElements.AccessorySprite;
				Accessory.enabled = true;
			} else {
				Accessory.enabled = false;
			}
		}
	}
}
