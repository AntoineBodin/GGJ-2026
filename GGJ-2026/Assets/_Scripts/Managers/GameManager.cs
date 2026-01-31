using System;

using Unity.VisualScripting;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class GameManager : SingletonBehaviour<GameManager> {
		private GameState _gameState;
		public static Action<GameState> OnGameStateChanged { get; set; }
		public GameState CurrentGameState => _gameState;
		public static event Action<int> OnResetGame;
		public static event Action OnStartGame;

		private uint remainingLives;
		[SerializeField]
		private GlobalSettings globalSettings;

		private void Start() {
			TutorialManager.Instance.OnTutorialEnded += StartGame;
		}

		private void StartGame() {
			UpdateGameState(GameState.Playing);
			OnStartGame?.Invoke();
			remainingLives = globalSettings.LivesCount;
		}

		public void UpdateGameState(GameState newGameState) {
			_gameState = newGameState;

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

		public void StartNewRound()
		{
			// Generate new tree
			// Generate Alien profiles
			// Generate Humans profiles
			// Generate human readable instructions
			// Show first profile
			// Reset lives?

			if (globalSettings.ResetLivesAfterDay)
				remainingLives = globalSettings.LivesCount;
		}

		public void OnError() {
			remainingLives--;
			if (remainingLives == 0) {
				UpdateGameState(GameState.None);
				Debug.Log("Game Over");
				// TODO : Show Game Over Screen
			}
		}

		public void OnNewRound() {
			
		}
	}
}
