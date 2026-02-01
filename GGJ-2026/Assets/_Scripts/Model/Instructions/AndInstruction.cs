using System.Collections.Generic;
using System.Linq;

namespace Assets._Scripts.Model.Instructions {
	internal class AndInstruction : CompositeInstruction {
		public AndInstruction(List<IInstruction> instructions) : base(instructions) {
		}

		public override bool IsValid(Profile profile) {
			return Instructions.All(instruction => instruction.IsValid(profile));
		}

		public override string Describe() {
			return string.Join(" AND", Instructions.Select(instruction => instruction.Describe()));
		}

		public override void ApplyAlienFeature(Profile profile) {
			foreach (var instruction in Instructions) {
				instruction.ApplyAlienFeature(profile);
			}
		}
	}
}

