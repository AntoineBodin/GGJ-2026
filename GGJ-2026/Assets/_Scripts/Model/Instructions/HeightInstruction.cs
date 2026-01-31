namespace Assets._Scripts.Model.Instructions {
	internal class HeightInstruction : InstructionBase<float> {
		public override float GetValue(Profile profile) {
			return profile.Height;
		}
	}
}
