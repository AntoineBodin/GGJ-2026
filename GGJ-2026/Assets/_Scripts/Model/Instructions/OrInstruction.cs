using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets._Scripts.Model.Instructions {
	internal class OrInstruction : CompositeInstruction {
		public OrInstruction(List<IInstruction> instructions) : base(instructions) {
		}

		public override string Describe() {
			return string.Join(" OR \n", Instructions.Select(instruction => instruction.Describe()));
		}

		public override bool IsValid(Profile profile) {
			return Instructions.Any(instruction => instruction.IsValid(profile));
		}

		public override void ApplyAlienFeature(Profile profile) {
			int featuresToApply = Random.Range(1, Instructions.Count - 1);
			for (int i = 0; i < featuresToApply; i++) {
				int instructionIndex = Random.Range(0, Instructions.Count);
				Instructions[instructionIndex].ApplyAlienFeature(profile);
			}


			}
	}
}
