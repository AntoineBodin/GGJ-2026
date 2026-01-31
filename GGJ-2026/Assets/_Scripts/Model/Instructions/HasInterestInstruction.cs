using System.Linq;

namespace Assets._Scripts.Model.Instructions {
	public class HasInterestInstruction : IInstruction {
		public string Interest { get; private set; }
		public bool ShouldHave { get; private set; }

		public HasInterestInstruction(string interest, bool shouldHave = true) {
			Interest = interest;
			ShouldHave = shouldHave;
		}

		public bool IsValid(Profile profile) {
			bool hasInterest = profile.Interests.Contains(Interest);
			return hasInterest == ShouldHave;
		}
	}
}
