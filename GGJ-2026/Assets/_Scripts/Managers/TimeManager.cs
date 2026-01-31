using System;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class TimeManager : SingletonBehaviour<TimeManager> {
		[field: SerializeField]
		protected GameObject Sun { get; private set; }

		public float DaySecondsPassed { get; private set; }
		public static event Action OnTimePassed;

		private GameManager gameManager;

		protected override void Awake() {
			gameManager = GameManager.Instance;
			GameManager.OnStartDay += () => {
				DaySecondsPassed = 0;
			};
		}

		private void Update() {

			DaySecondsPassed += Time.deltaTime;
			if (DaySecondsPassed >= gameManager.GlobalSettings.DayDurationSecond) {
				OnTimePassed?.Invoke();
				gameManager.UpdateGameState(GameState.EndingScreen);
			}

			float percentage = DaySecondsPassed / gameManager.GlobalSettings.DayDurationSecond;
			float angle = Mathf.Lerp(16f, -62f, percentage);
			RectTransform rect = (RectTransform)Sun.transform;
			rect.localRotation = Quaternion.Euler(0f, 0f, angle);
		}
	}
}
