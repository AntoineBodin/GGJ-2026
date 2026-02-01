using System;

using TMPro;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class TimeManager : SingletonBehaviour<TimeManager> {
		[field: SerializeField]
		protected GameObject Sun { get; private set; }
		[field: SerializeField]
		protected TextMeshProUGUI Text { get; private set; }

		public float DaySecondsPassed { get; private set; }
		public static event Action OnTimePassed;

		protected override void Awake() {
			GameManager.OnDayChange += (_) => {
				DaySecondsPassed = 0;
			};
		}

		private void Update() {
			DaySecondsPassed += Time.deltaTime;
			if (DaySecondsPassed > GameManager.Instance.GlobalSettings.DayDurationSecond) {
				OnTimePassed?.Invoke();
				GameManager.Instance.UpdateGameState(GameState.EndingScreen);
				return;
			}

			float percentage = DaySecondsPassed / GameManager.Instance.GlobalSettings.DayDurationSecond;
			float angle = Mathf.Lerp(16f, -62f, percentage);
			RectTransform rect = (RectTransform)Sun.transform;
			rect.localRotation = Quaternion.Euler(0f, 0f, angle);

			TimeSpan workDayStart = new TimeSpan(9, 0, 0);  // 09:00
			TimeSpan workDayEnd = new TimeSpan(17, 0, 0); // 17:00
			TimeSpan workDayDuration = workDayEnd - workDayStart;
			TimeSpan currentTime = workDayStart + TimeSpan.FromTicks((long)(workDayDuration.Ticks * percentage));
			Text.text = currentTime.ToString(@"hh\:mm");
		}
	}
}
