using System;
using System.Collections.Generic;
using Assets._Scripts.Model.Instructions.Comparators;

using NUnit.Framework;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	public class InterestCountInstruction : InstructionBase<int> {
		public InterestCountInstruction(int expectedValue, IComparator<int> comparator)
			: base(expectedValue, comparator) { }
		public override void ApplyAlienFeature(Profile profile) {
			int targetInterestCount = 0;

			if (Comparator is GreaterComparator<int> || Comparator is GreaterOrEqualsComparator<int>) {
				targetInterestCount = ExpectedValue + 2;
			} else if (Comparator is LowerComparator<int> || Comparator is LowerOrEqualsComparator<int>) {
				targetInterestCount = Math.Max(1, ExpectedValue - 2);
			} else if (Comparator is EqualsComparator<int>) {
				targetInterestCount = ExpectedValue;
			}
			List<string> bonusInterests = new() { "Astronomy", "Quantum Physics", "Intergalactic Travel", "Alien Cultures", "Space Exploration" };
			if (profile.Interests.Count < targetInterestCount) {
				// Add interests
				while (profile.Interests.Count < targetInterestCount) {
					profile.Interests.Add(bonusInterests[UnityEngine.Random.Range(0, bonusInterests.Count)]);
				}
			} else if (profile.Interests.Count > targetInterestCount) {
				// Remove interests
				profile.Interests.RemoveRange(targetInterestCount, profile.Interests.Count - targetInterestCount);
			}
		}

		public override string Describe() {
			return $"Aliens have {Comparator.Describe()} {ExpectedValue} interests.";
		}

		public override int GetValue(Profile profile) {
			return profile.Interests.Count;
		}
	}
}
