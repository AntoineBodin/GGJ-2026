using System.Collections.Generic;

namespace Assets._Scripts.Model {
	public class Profile {
		public string Name;
		public ProfilePictureElements PictureElements;
		public int Age;
		public string Bio;
		public Gender Gender;
		public float Height; //in cm
		public List<string> Interests;
		public bool IsAlien;

	}
}
