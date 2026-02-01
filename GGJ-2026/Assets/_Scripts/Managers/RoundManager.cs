using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets._Scripts.Model;
using Assets._Scripts.Model.Instructions;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets._Scripts.Managers {
	internal class RoundManager : SingletonBehaviour<RoundManager> {

		private List<Profile> roundProfiles;
		private int _index;
		public int Index {
			get => _index; private set {
				_index = value;
				OnIndexChange?.Invoke(_index);
			}
		}
		private int dayIndex = 0;
		private Profile currentProfile => roundProfiles[Index];
		private bool timeIsUp = false;

		[SerializeField]
		private GlobalSettings globalSettings;

		public event Action<Profile, bool, bool, MoveDirection> OnNewProfileLoaded;
		public static event Action<int> OnIndexChange;

		private void Start() {
			UIManager.Instance.OnLikeButtonClicked += HandleLikeButtonClicked;
			UIManager.Instance.OnDeportButtonClicked += HandleDeportButtonClicked;
			TimeManager.OnTimePassed += () => timeIsUp = true;

			var firstAlienProfile = ProfileGenerator.Instance.GenerateProfile();
			firstAlienProfile.IsAlien = true;
			roundProfiles = new List<Profile> { firstAlienProfile };
			for (int i = 0; i < 10; i++) {
				roundProfiles.Add(ProfileGenerator.Instance.GenerateProfile());
			}
			GameManager.OnStartGame += StartRound;
		}

		private IEnumerator StartRoundAfter3Seconds() {
			yield return new WaitForSeconds(3);
			StartRound();
		}

		public void SetProfiles(List<Profile> roundProfiles) {
			this.roundProfiles = roundProfiles;
		}

		public void StartRound() {
			IInstruction instruction = InstructionsGenerator.Instance.GenerateInstructions(dayIndex);
			string descr = instruction.Describe();
			Debug.Log(descr);
			GenerateProfileList(instruction);
			roundProfiles.OrderBy(profile => Guid.NewGuid());
			Index = 0;
			// currentProfile = roundProfiles[Index];
			OnNewProfileLoaded?.Invoke(currentProfile, false, false, MoveDirection.None);
			dayIndex++;
		}

		private void GenerateProfileList(IInstruction instruction) {
			roundProfiles = new List<Profile>();
			uint numberTotalProfiles = globalSettings.ProfilesPerDay;
			float alienPercentage = UnityEngine.Random.Range(globalSettings.AlienPercentagePerDayMin, globalSettings.AlienPercentagePerDayMax);

			int numberOfAliens = Mathf.CeilToInt(numberTotalProfiles * alienPercentage);
			int remainingHumanProfiles = (int)numberTotalProfiles - numberOfAliens;

			int i = 0;

			while (i < numberOfAliens) {
				Profile alienProfile = ProfileGenerator.Instance.GenerateAlienProfile(instruction);
				roundProfiles.Add(alienProfile);
				i++;
			}

			int j = 0;
			while (j < remainingHumanProfiles) {
				Profile humanProfile = ProfileGenerator.Instance.GenerateProfile();
				if (instruction.IsValid(humanProfile)) {
					humanProfile.IsAlien = true;
				}
				j++;
				roundProfiles.Add(humanProfile);
			}

		}
		private void HandleDeportButtonClicked() {
			if (currentProfile.IsAlien) {
				// Correct decision
			} else {
				// Wrong decision
			}

			TryGetNextProfile(currentProfile.IsAlien, true);
		}
		private void HandleLikeButtonClicked() {
			if (!currentProfile.IsAlien) {
				// Correct decision
			} else {
				// Wrong decision
			}

			TryGetNextProfile(!currentProfile.IsAlien, false);
		}

		private void TryGetNextProfile(bool hasWon, bool hasDeported) {
			Index++;
			if (Index < roundProfiles.Count && !timeIsUp) {
				// currentProfile = roundProfiles[Index];
				OnNewProfileLoaded?.Invoke(currentProfile, hasWon, hasDeported, hasDeported? MoveDirection.Left : MoveDirection.Right);
			} else {
				// End of round
			}
		}

	}
}
