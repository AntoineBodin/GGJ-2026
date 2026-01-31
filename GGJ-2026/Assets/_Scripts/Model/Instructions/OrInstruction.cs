using System.Collections.Generic;
using System.Linq;

namespace Assets._Scripts.Model.Instructions {
	internal class OrInstruction : CompositeInstruction {
		public OrInstruction(List<IInstruction> instructions) : base(instructions) {
		}

		public override bool IsValid(Profile profile) {
			return Instructions.Any(instruction => instruction.IsValid(profile));
		}
	}
}
