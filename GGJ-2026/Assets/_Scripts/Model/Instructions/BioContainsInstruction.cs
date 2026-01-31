namespace Assets._Scripts.Model.Instructions {
	public class BioContainsInstruction : IInstruction {
		public string Text { get; private set; }
		public bool ShouldContain { get; private set; }

		public BioContainsInstruction(string text, bool shouldContain = true) {
			Text = text;
			ShouldContain = shouldContain;
		}

		public bool IsValid(Profile profile) {
			bool contains = profile.Bio.Contains(Text);
			return contains == ShouldContain;
		}
	}
}
