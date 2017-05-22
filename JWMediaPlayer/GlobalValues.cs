namespace JWMediaPlayer {
	public class GlobalValues {
        public const float SEEK_TIME = 5.0f;
        public static float Seek5Second = 0.0f;
        public static readonly int VolumeUpValue = 1;
        public static readonly int VolumeDownValue = -1;
        public static readonly string[] SubTitleExtensions = new string[3]
        {
      ".smi",
      ".srt",
      ".ass"
        };
        public static readonly string[] AllowStreamingUrls = new string[3]
        {
      "http",
      "rtsp",
      "rtmp"
        };
    }
}
