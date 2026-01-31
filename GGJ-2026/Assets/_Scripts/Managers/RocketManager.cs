using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers {
	internal class RocketManager : MonoBehaviour {

		private void Start() {
			LaunchAnimation(true);
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
		[SerializeField]
		private GameObject alienPrefab;

		[SerializeField]
		private GameObject humanPrefab;

		[SerializeField]
		private GameObject passengerParent;

		public void LaunchAnimation(bool isAlien) {
			rocketImage.gameObject.SetActive(true);
			rocketAnimator.Play(ROCKET_LAUNCH_ANIM);

			StartCoroutine(LaunchPassenger(isAlien));
			StartCoroutine(HideAfterAnimation());
		}

		private IEnumerator LaunchPassenger(bool isAlien) {
			yield return new WaitForSeconds(messageDelay);
			if (isAlien)
				Instantiate(alienPrefab, passengerParent.transform.position, Quaternion.identity);
			else
				Instantiate(humanPrefab, passengerParent.transform.position, Quaternion.identity);
		}

		private IEnumerator HideAfterAnimation() {
			yield return new WaitForSeconds(animationDuration);
			rocketImage.gameObject.SetActive(false);
		}
	}
}
