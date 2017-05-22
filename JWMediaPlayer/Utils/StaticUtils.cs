namespace JWMediaPlayer.Utils {
	public class StaticUtils {
		public static bool isContainHangul (string s) {
			char[] charArr = s.ToCharArray();

			foreach (char c in charArr) {
				if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter) {
					return true;
				}
			}

			return false;
		}

	}
}
