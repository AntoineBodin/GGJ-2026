using System;
using System.Collections;
using Assets._Scripts.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets._Scripts.Managers {
	public class UIManager : SingletonBehaviour<UIManager> {
		[SerializeField]
		private Button deportButton;
		[SerializeField]
		private Button likeButton;

		[SerializeField]
		private Animator scrollViewAnimator;
		[SerializeField]
		private float AnimationHalfDuration = 0.5f;

		public event Action OnLikeButtonClicked;
		public event Action OnDeportButtonClicked;

		private void Start() {
			deportButton.onClick.AddListener(() => OnDeportButtonClicked?.Invoke());
			likeButton.onClick.AddListener(() => OnLikeButtonClicked?.Invoke());
			RoundManager.Instance.OnNewProfileLoaded += HandleNewProfileLoaded;
		}

		private void HandleNewProfileLoaded(Profile profile, bool hasWon, bool hasDeported, MoveDirection direction) {
			if (direction == MoveDirection.Left) {
				scrollViewAnimator.Play("SwipeLeft");
			} else if (direction == MoveDirection.Right) {
				scrollViewAnimator.Play("SwipeRight");
			} else {
				ProfileManager.Instance.DisplayProfile(profile);
			}
				StartCoroutine(WaitForAnimationToChangeProfile(profile));

			if (hasDeported) {
				RocketManager.Instance.LaunchAnimation(hasWon);

				if (!hasWon) {
					GameManager.Instance.OnError();
				}
			}
		}

		private IEnumerator WaitForAnimationToChangeProfile(Profile profile) {
			yield return new WaitForSeconds(AnimationHalfDuration);
			ProfileManager.Instance.DisplayProfile(profile);
		}
	}
}
