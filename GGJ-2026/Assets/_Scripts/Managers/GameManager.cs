using System;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class GameManager : SingletonBehaviour<GameManager> {
		[field: SerializeField]
		public GlobalSettings globalSettings { get; private set; }

		public uint RemainingLives { get; private set; }
		public float DaySecondsPassed { get; private set; }

		public GameState CurrentGameState { get; private set; }
		public static Action<GameState> OnGameStateChanged { get; set; }
		public static event Action<int> OnResetGame;
		public static event Action OnStartGame;

		private void Start() {
			TutorialManager.Instance.OnTutorialEnded += StartGame;
		}

		private void Update() {
			if (CurrentGameState == GameState.Playing)
				DaySecondsPassed += Time.deltaTime;
		}

		private void StartGame() {
			UpdateGameState(GameState.Playing);
			OnStartGame?.Invoke();
			RemainingLives = globalSettings.LivesCount;
		}

		public void UpdateGameState(GameState newGameState) {
			CurrentGameState = newGameState;

			switch (newGameState) {
				case GameState.Playing:
					break;
				case GameState.Upgrade:
					break;
				case GameState.None:
					break;
				case GameState.Trading:
					break;
				case GameState.WinningScreen:
					break;
			}

			OnGameStateChanged?.Invoke(newGameState);
		}

		public void StartNewRound() {
			// Generate new tree
			// Generate Alien profiles
			// Generate Humans profiles
			// Generate human readable instructions
			// Show first profile

			if (globalSettings.ResetLivesAfterDay)
				RemainingLives = globalSettings.LivesCount;
			DaySecondsPassed = 0;
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
