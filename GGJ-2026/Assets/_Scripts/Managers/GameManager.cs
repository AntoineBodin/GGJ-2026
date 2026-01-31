using System;

namespace Assets._Scripts.Managers {
	internal class GameManager : SingletonBehaviour<GameManager> {
		private GameState _gameState;

		public static Action<GameState> OnGameStateChanged { get; set; }
		public GameState CurrentGameState => _gameState;

		public static event Action<int> OnResetGame;
		public static event Action OnStartGame;

		private int prestigeLevel = 0;

		private void Start() {
			TutorialManager.Instance.OnTutorialEnded += StartGame;
		}

		private void StartGame() {
			UpdateGameState(GameState.Playing);
			OnStartGame?.Invoke();
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

		internal void RestartGame() {
			prestigeLevel++;
			OnResetGame?.Invoke(prestigeLevel);
		}
	}
}
