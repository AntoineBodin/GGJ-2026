using System;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class TimeManager : SingletonBehaviour<TimeManager> {
		[field: SerializeField]
		protected GameObject Sun { get; private set; }

		public float DaySecondsPassed { get; private set; }
		public static event Action OnTimePassed;

		protected override void Awake() {
			GameManager.OnStartDay += () => {
				DaySecondsPassed = 0;
			};
		}

		private void Update() {

			DaySecondsPassed += Time.deltaTime;
			if (DaySecondsPassed >= GameManager.Instance.GlobalSettings.DayDurationSecond) {
				OnTimePassed?.Invoke();
				GameManager.Instance.UpdateGameState(GameState.EndingScreen);
			}

			float percentage = DaySecondsPassed / GameManager.Instance.GlobalSettings.DayDurationSecond;
			float angle = Mathf.Lerp(16f, -62f, percentage);
			RectTransform rect = (RectTransform)Sun.transform;
			rect.localRotation = Quaternion.Euler(0f, 0f, angle);
		}
	}
}
