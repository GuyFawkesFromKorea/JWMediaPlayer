using Meta.Vlc.Wpf;
using System.Collections.Generic;

namespace JWMediaPlayer.Models {
	public class AspectRatioViewModel {
		public List<AspectRatioModel> Items { get; private set; }
		public AspectRatioViewModel () {
			Items = new List<AspectRatioModel>();
			Items.Add(new AspectRatioModel { AspectRatioName = "Defalut", AspectRatioValue = AspectRatio.Default });
			Items.Add(new AspectRatioModel { AspectRatioName = "4:3", AspectRatioValue = AspectRatio._4_3 });
			Items.Add(new AspectRatioModel { AspectRatioName = "16:9", AspectRatioValue = AspectRatio._16_9 });
		}
		
	}

	public class AspectRatioModel : ModelBase {
		private string _aspectRatioName;

		public string AspectRatioName
		{
			get
			{
				return _aspectRatioName;
			}

			set
			{
				_aspectRatioName = value;
				OnPropertyChanged("AspectRatioName");
			}
		}

		private AspectRatio _aspectRatioValue;
		public AspectRatio AspectRatioValue
		{
			get
			{
				return _aspectRatioValue;
			}

			set
			{
				_aspectRatioValue = value;
				OnPropertyChanged("AspectRatioValue");
			}
		}

		

	}
}
