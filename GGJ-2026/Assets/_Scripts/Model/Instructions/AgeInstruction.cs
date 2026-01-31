namespace Assets._Scripts.Model.Instructions {
	internal class AgeInstruction : InstructionBase<int> {
		public override int GetValue(Profile profile) {
			return profile.Age;
		}
	}
}
