using System.Collections.Generic;
using Assets._Scripts.Managers;
using Assets._Scripts.Model;
using Assets._Scripts.Model.Instructions;
using Assets._Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets._Scripts.Testing {
	/// <summary>
	/// Test script for the instruction system.
	/// Attach to any GameObject and assign a RoundGenerationConfig to test.
	/// </summary>
	public class InstructionSystemTester : MonoBehaviour {
		[Header("Config")]
		public RoundGenerationConfig Config;

		[Header("Test Settings")]
		public int ProfilesToGenerate = 5;

		[Header("Controls")]
		[Tooltip("Press this key to generate new rules")]
		public KeyCode GenerateRulesKey = KeyCode.G;

		[Tooltip("Press this key to generate test profiles")]
		public KeyCode GenerateProfilesKey = KeyCode.P;

		[Tooltip("Press this key to run full test")]
		public KeyCode FullTestKey = KeyCode.T;

		private RoundRuleGenerator _generator;
		private GeneratedRule _currentRule;

		private void Start() {
			if (Config == null) {
				Debug.LogError("InstructionSystemTester: No config assigned!");
				return;
			}

			_generator = new RoundRuleGenerator(Config);
			Debug.Log("=== INSTRUCTION SYSTEM TESTER ===");
			Debug.Log($"Press [{GenerateRulesKey}] to generate new rules");
			Debug.Log($"Press [{GenerateProfilesKey}] to generate test profiles");
			Debug.Log($"Press [{FullTestKey}] to run full test");
		}

		private void Update() {
			if (Input.GetKeyDown(GenerateRulesKey)) {
				TestRuleGeneration();
			}
			if (Input.GetKeyDown(GenerateProfilesKey)) {
				TestProfileGeneration();
			}
			if (Input.GetKeyDown(FullTestKey)) {
				RunFullTest();
			}
		}

		[ContextMenu("Test Rule Generation")]
		public void TestRuleGeneration() {
			if (_generator == null) {
				Debug.LogError("Generator not initialized! Make sure to assign Config in Inspector.");
				return;
			}

			Debug.Log("\n========== GENERATING NEW RULES ==========");

			_currentRule = _generator.GenerateRule();

			if (_currentRule == null) {
				Debug.LogError("Failed to generate rule!");
				return;
			}

			Debug.Log($"Logic Type: {_currentRule.LogicType}");
			Debug.Log($"Number of conditions: {_currentRule.Instructions.Count}");
			Debug.Log("--- Detection Rules ---");

			foreach (var instruction in _currentRule.Instructions) {
				//Debug.Log($"  - {instruction.Description}");
				Debug.Log($"  - {instruction.HumanReadableDescription}");
				Debug.Log($"    Attribute: {instruction.AttributeType}");
				if (instruction.NumericConfig != null) {
					Debug.Log($"    Human range: {instruction.NumericConfig.HumanMin} - {instruction.NumericConfig.HumanMax}");
					Debug.Log($"    Alien range: {instruction.NumericConfig.AlienMin} - {instruction.NumericConfig.AlienMax}");
				}
			}

			Debug.Log("==========================================\n");
		}

		[ContextMenu("Test Profile Generation")]
		public void TestProfileGeneration() {
			if (_currentRule == null) {
				Debug.Log("No rules generated yet. Generating rules first...");
				TestRuleGeneration();
			}

			Debug.Log("\n========== TESTING PROFILE GENERATION ==========");
			Debug.Log("Current rules:");
			foreach (var instr in _currentRule.Instructions) {
				Debug.Log($"  - {instr.HumanReadableDescription}");
				//Debug.Log($"  - {instr.Description}");
			}

			Debug.Log("\n--- Generating Aliens ---");
			for (int i = 0; i < ProfilesToGenerate; i++) {
				Profile alien = GenerateTestAlienProfile();
				bool detected = _currentRule.CombinedRule.IsValid(alien);
				Debug.Log($"Alien {i + 1}: Age={alien.Age}, Height={alien.Height:F2}, Interests={alien.Interests.Count} | Detected: {(detected ? "YES" : "NO - FAIL")}");
			}

			Debug.Log("\n--- Generating Humans ---");
			for (int i = 0; i < ProfilesToGenerate; i++) {
				Profile human = GenerateTestHumanProfile();
				bool detected = _currentRule.CombinedRule.IsValid(human);
				Debug.Log($"Human {i + 1}: Age={human.Age}, Height={human.Height:F2}, Interests={human.Interests.Count} | Detected: {(detected ? "YES - FALSE POSITIVE" : "NO")}");
			}

			Debug.Log("=================================================\n");
		}

		[ContextMenu("Run Full Test")]
		public void RunFullTest() {
			if (Config == null) {
				Debug.LogError("Config not assigned! Assign RoundGenerationConfig in Inspector.");
				return;
			}

			Debug.Log("\n############### FULL SYSTEM TEST ###############\n");

			var originalLogicType = Config.LogicType;

			TestLogicType(LogicType.And);
			TestLogicType(LogicType.Or);
			TestLogicType(LogicType.Tree);

			Config.LogicType = originalLogicType;

			Debug.Log("############### TEST COMPLETE ###############\n");
		}

		private void TestLogicType(LogicType logicType) {
			Debug.Log($"\n===== Testing LogicType: {logicType} =====");
			Config.LogicType = logicType;
			_generator = new RoundRuleGenerator(Config);

			_currentRule = _generator.GenerateRule();

			Debug.Log("Rules:");
			foreach (var instr in _currentRule.Instructions) {
				Debug.Log($"  - {instr.HumanReadableDescription}");
				//Debug.Log($"  - {instr.Description}");
			}

			int alienCorrect = 0;
			int humanCorrect = 0;
			int testCount = 10;

			for (int i = 0; i < testCount; i++) {
				Profile alien = GenerateTestAlienProfile();
				if (_currentRule.CombinedRule.IsValid(alien)) alienCorrect++;

				Profile human = GenerateTestHumanProfile();
				if (!_currentRule.CombinedRule.IsValid(human)) humanCorrect++;
			}

			Debug.Log($"Results: Aliens detected {alienCorrect}/{testCount}, Humans passed {humanCorrect}/{testCount}");
		}

		private Profile GenerateTestAlienProfile() {
			Profile profile = CreateBaseProfile();

			foreach (var instruction in _currentRule.Instructions) {
				ApplyAlienValueToProfile(profile, instruction);
			}

			return profile;
		}

		private Profile GenerateTestHumanProfile() {
			Profile profile = CreateBaseProfile();

			foreach (var instruction in _currentRule.Instructions) {
				ApplyHumanValueToProfile(profile, instruction);
			}

			return profile;
		}

		private Profile CreateBaseProfile() {
			return new Profile {
				Name = "Test Person",
				Age = Random.Range(18, 60),
				Height = Random.Range(1.5f, 1.9f),
				Gender = Gender.Other,
				Bio = "Test bio",
				Interests = new List<string> { "Interest1", "Interest2", "Interest3", "Interest4" },
				PictureElements = new ProfilePictureElements()
			};
		}

		private void ApplyAlienValueToProfile(Profile profile, GeneratedInstruction instruction) {
			var config = instruction.NumericConfig;
			if (config == null) return;

			switch (instruction.AttributeType) {
				case AttributeType.Age:
					int alienMinAge = Mathf.RoundToInt(config.AlienMin);
					int alienMaxAge = Mathf.RoundToInt(config.AlienMax);
					profile.Age = Random.Range(alienMinAge, alienMaxAge + 1);
					break;
				case AttributeType.Height:
					profile.Height = Random.Range(config.AlienMin, config.AlienMax);
					break;
				case AttributeType.InterestCount:
					int alienMinCount = Mathf.RoundToInt(config.AlienMin);
					int alienMaxCount = Mathf.RoundToInt(config.AlienMax);
					int alienCount = Random.Range(alienMinCount, alienMaxCount + 1);
					while (profile.Interests.Count > alienCount) profile.Interests.RemoveAt(0);
					while (profile.Interests.Count < alienCount) profile.Interests.Add($"Interest{profile.Interests.Count}");
					break;
			}
		}

		private void ApplyHumanValueToProfile(Profile profile, GeneratedInstruction instruction) {
			var config = instruction.NumericConfig;
			if (config == null) return;

			int humanMin, humanMax, alienMin, alienMax;

			switch (instruction.AttributeType) {
				case AttributeType.Age:
					humanMin = Mathf.RoundToInt(config.HumanMin);
					humanMax = Mathf.RoundToInt(config.HumanMax);
					alienMin = Mathf.RoundToInt(config.AlienMin);
					alienMax = Mathf.RoundToInt(config.AlienMax);

					if (humanMin < alienMin) {
						profile.Age = Random.Range(humanMin, alienMin);
					} else {
						profile.Age = Random.Range(alienMax + 1, humanMax + 1);
					}
					break;
				case AttributeType.Height:
					if (config.HumanMin < config.AlienMin) {
						profile.Height = Random.Range(config.HumanMin, config.AlienMin);
					} else {
						profile.Height = Random.Range(config.AlienMax, config.HumanMax);
					}
					break;
				case AttributeType.InterestCount:
					humanMin = Mathf.RoundToInt(config.HumanMin);
					humanMax = Mathf.RoundToInt(config.HumanMax);
					alienMin = Mathf.RoundToInt(config.AlienMin);
					alienMax = Mathf.RoundToInt(config.AlienMax);

					int humanCount = Random.Range(humanMin, humanMax + 1);
					if (humanCount >= alienMin && humanCount <= alienMax) {
						humanCount = humanMax;
					}
					while (profile.Interests.Count > humanCount) profile.Interests.RemoveAt(0);
					while (profile.Interests.Count < humanCount) profile.Interests.Add($"Interest{profile.Interests.Count}");
					break;
			}
		}
	}
}
