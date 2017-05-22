using Meta.Vlc.Wpf.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JWMediaPlayer.Models {
	public class ModelBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
