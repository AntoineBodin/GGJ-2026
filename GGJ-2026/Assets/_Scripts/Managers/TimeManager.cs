using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class TimeManager : SingletonBehaviour<TimeManager> {
		[field: SerializeField]
		protected GameObject Sun { get; private set; }

		private GameManager gameManager;

		protected override void Awake() {
			gameManager = GameManager.Instance;
		}

		private void Update() {
			float percentage = gameManager.DaySecondsPassed / gameManager.globalSettings.DayDurationSecond;
			float angle = Mathf.Lerp(0f, -93f, percentage);
			RectTransform rect = (RectTransform)Sun.transform;
			rect.localRotation = Quaternion.Euler(0f, 0f, angle);
		}
	}
}
