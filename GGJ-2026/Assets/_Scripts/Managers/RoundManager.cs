using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets._Scripts.Model;

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
		private Profile currentProfile;
		private bool timeIsUp = false;

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
			Index = 0;
			currentProfile = roundProfiles[Index];
			OnNewProfileLoaded?.Invoke(currentProfile, false, false, MoveDirection.None);
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
				currentProfile = roundProfiles[Index];
				OnNewProfileLoaded?.Invoke(currentProfile, hasWon, hasDeported, MoveDirection.Right);
			} else {
				// End of round
			}
		}

	}
}
