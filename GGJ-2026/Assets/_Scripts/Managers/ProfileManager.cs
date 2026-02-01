using Assets._Scripts.Model;

using TMPro;

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
		[field: SerializeField]
		public TextMeshProUGUI Text { get; private set; }


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

			Text.text = $@"<b>Name:</b> {profile.Name}
<b>Age:</b>  {profile.Age}Y
<b>Gender:</b>  {profile.Gender.ToString()}
<b>Height:</b>  {profile.Height.ToString("F2")}m

<b>Interests:</b>
- {string.Join("\n- ", profile.Interests)}";
			LayoutRebuilder.ForceRebuildLayoutImmediate(
					Text.rectTransform
			);
		}
	}
}
