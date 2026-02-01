using System.Collections.Generic;

namespace Assets._Scripts.Model.Instructions {
	internal abstract class CompositeInstruction : IInstruction {
		public List<IInstruction> Instructions { get; private set; }

		protected CompositeInstruction(List<IInstruction> instructions) {
			Instructions = instructions;
		}

		public abstract bool IsValid(Profile profile);
		public abstract string Describe();

		public abstract void ApplyAlienFeature(Profile profile);
		
	}
}
