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
		private int dayIndex = 0;
		private int index = 0;
		private Profile currentProfile;
		private bool timeIsUp = false;

		[SerializeField]
		private GlobalSettings globalSettings;

		public event Action<Profile, bool, bool, MoveDirection> OnNewProfileLoaded;

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
			StartCoroutine(StartRoundAfter3Seconds());
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
			GenerateProfileList(instruction);
			roundProfiles.OrderBy(profile => Guid.NewGuid());
			index = 0;
			currentProfile = roundProfiles[index];
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
			index++;
			if (index < roundProfiles.Count && !timeIsUp) {
				currentProfile = roundProfiles[index];
				OnNewProfileLoaded?.Invoke(currentProfile, hasWon, hasDeported, MoveDirection.Right);
			} else {
				// End of round
			}
		}

	}
}
