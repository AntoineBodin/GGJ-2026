using System;

using Assets._Scripts.ScriptableObjects;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class GameManager : SingletonBehaviour<GameManager> {
		[field: SerializeField]
		public GlobalSettings GlobalSettings { get; private set; }
		[field: SerializeField]
		public RoundGenerationConfig Config { get; private set; }

		private uint _remainingLives;
		public uint RemainingLives {
			get => _remainingLives; private set {
				_remainingLives = value;
				OnLiveChange?.Invoke(_remainingLives);
			}
		}

		public GameState CurrentGameState { get; private set; }
		public static Action<GameState> OnGameStateChanged { get; set; }
		public static event Action<int> OnResetGame;
		public static event Action OnStartGame;
		public static event Action OnStartDay;
		public static event Action<GeneratedRule> OnNewRound;
		public static event Action<uint> OnLiveChange;

		private int prestigeLevel = 0;
		private RoundRuleGenerator _ruleGenerator;
		private GeneratedRule _currentRule;

		public GeneratedRule CurrentRule => _currentRule;

		private void Start() {
			TutorialManager.Instance.OnTutorialEnded += StartGame;
			_ruleGenerator = new RoundRuleGenerator(Config);
			StartNewRound();
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
			_currentRule = _ruleGenerator.GenerateRule();
			OnNewRound?.Invoke(_currentRule);
			Debug.Log($"New round started with {_currentRule.Instructions.Count} detection rules:");
			foreach (var instruction in _currentRule.Instructions) {
				Debug.Log($"  - {instruction.Description}");
			}

			if (GlobalSettings.ResetLivesAfterDay)
				RemainingLives = GlobalSettings.LivesCount;
		}

		public void OnError() {
			RemainingLives--;
			if (RemainingLives <= 0) {
				UpdateGameState(GameState.None);
				Debug.Log("Game Over");
				// TODO : Show Game Over Screen
			}
		}
	}
}
