using System;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class GameManager : SingletonBehaviour<GameManager> {
		[field: SerializeField]
		public GlobalSettings GlobalSettings { get; private set; }

		public uint RemainingLives { get; private set; }

		public GameState CurrentGameState { get; private set; }
		public static Action<GameState> OnGameStateChanged { get; set; }
		public static event Action<int> OnResetGame;
		public static event Action OnStartGame;
		public static event Action OnStartDay;

		private void Start() {
			TutorialManager.Instance.OnTutorialEnded += StartGame;
		}

		private void StartGame() {
			UpdateGameState(GameState.Playing);
			OnStartGame?.Invoke();
			StartNewRound();
		}

		public void UpdateGameState(GameState newGameState) {
			CurrentGameState = newGameState;

			switch (newGameState) {
				case GameState.Playing:
					break;
				case GameState.None:
					break;
				case GameState.EndingScreen:
					break;
			}

			OnGameStateChanged?.Invoke(newGameState);
		}

		public void StartNewRound() {
			OnStartDay?.Invoke();
			// Generate new tree
			// Generate Alien profiles
			// Generate Humans profiles
			// Generate human readable instructions
			// Show first profile

			if (GlobalSettings.ResetLivesAfterDay)
				RemainingLives = GlobalSettings.LivesCount;
		}

		public void OnError() {
			RemainingLives--;
			if (RemainingLives == 0) {
				UpdateGameState(GameState.None);
				Debug.Log("Game Over");
				// TODO : Show Game Over Screen
			}
		}

		public void OnNewRound() {

		}
	}
}
