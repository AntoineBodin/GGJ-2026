using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers {
	[Serializable]
	public class Damage {
		[field: SerializeField]
		public Sprite FrameImage { get; private set; }
		[field: SerializeField]
		public Sprite ScreenImage { get; private set; }
		[field: SerializeField]
		public Sprite CrackImage { get; private set; }
		[field: SerializeField]
		public Sprite ShineImage { get; private set; }
	}

	internal class TabletManager : SingletonBehaviour<TabletManager> {
		[field: SerializeField]
		public TextMeshProUGUI ProfileIndexText { get; private set; }

		[field: SerializeField]
		public Image Frame { get; private set; }
		[field: SerializeField]
		public Image Screen { get; private set; }
		[field: SerializeField]
		public Image Crack { get; private set; }
		[field: SerializeField]
		public Image Shine { get; private set; }
		[field: SerializeField]
		public List<Damage> Damages { get; private set; }

		private void Start() {
			// StartCoroutine(IntervalRoutine());

			GameManager.OnLiveChange += changeDamage;
			RoundManager.OnIndexChange += changeProfile;
		}

		// IEnumerator IntervalRoutine() {
		// uint test = 3;
		// 	while (true) {
		// 		yield return new WaitForSeconds(5);
		// changeDamage(test);
		// test--;
		// 		if (test > 4) {
		// 			test = 3;
		// 		}
		// }

		private void changeDamage(uint lives) {
			Debug.Log("Lives:" + lives);
			if (lives >= GameManager.Instance.GlobalSettings.LivesCount) {
				Frame.enabled = false;
				Screen.enabled = false;
				Crack.enabled = false;
				Shine.enabled = false;
			} else {
				Frame.enabled = true;
				Screen.enabled = true;
				Crack.enabled = true;
				Shine.enabled = true;

				Damage damge = Damages[(int)(Damages.Count - lives) - 1];
				Frame.sprite = damge.FrameImage;
				Screen.sprite = damge.ScreenImage;
				Crack.sprite = damge.CrackImage;
				Shine.sprite = damge.ShineImage;
			}
		}

		private void changeProfile(int profileNumber) {
			ProfileIndexText.text = (1 + profileNumber) + "/" + GameManager.Instance.GlobalSettings.ProfilesPerDay;
		}
	}
}
