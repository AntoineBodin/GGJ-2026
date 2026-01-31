using System.Collections.Generic;
using System.Linq;

namespace Assets._Scripts.Model.Instructions {
	internal class AndInstruction : CompositeInstruction {
		public AndInstruction(List<IInstruction> instructions) : base(instructions) {
		}

		public override bool IsValid(Profile profile) {
			return Instructions.All(instruction => instruction.IsValid(profile));
		}
	}
}
