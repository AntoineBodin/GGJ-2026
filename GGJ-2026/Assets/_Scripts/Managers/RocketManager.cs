using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers {
	internal class RocketManager : MonoBehaviour {

		private void Start() {
			LaunchAnimation();
		}

		[SerializeField]
		private Image rocketImage;

		[SerializeField]
		private Animator rocketAnimator;

		[SerializeField]
		private float messageDelay = 1.33f; // Temps avant d'afficher le message

		[SerializeField]
		private float animationDuration = 2f;

		private const string ROCKET_LAUNCH_ANIM = "RocketLaunch";

		public void LaunchAnimation() {
			rocketImage.gameObject.SetActive(true);
			rocketAnimator.Play(ROCKET_LAUNCH_ANIM);

			StartCoroutine(LogMessageAfterDelay());
			StartCoroutine(HideAfterAnimation());
		}

		private IEnumerator LogMessageAfterDelay() {
			yield return new WaitForSeconds(messageDelay);
			Debug.Log("Rocket launch - Message triggered!");
		}

		private IEnumerator HideAfterAnimation() {
			yield return new WaitForSeconds(animationDuration);
			rocketImage.gameObject.SetActive(false);
		}
	}
}
