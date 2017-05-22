using Meta.Vlc.Wpf.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace JWMediaPlayer {
	/// <summary>
	/// FileSearchDlg.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FileSearchDlg : UserControl, INotifyPropertyChanged {
		public event Action<object, EventArgs> OKButtonClicked;
		public event Action<object, EventArgs> CancelButtonClicked;
		private string fileUrlPath;
		public string FileUrlPath
		{
			get { return fileUrlPath; }
			set {
				fileUrlPath = value;
				OnPropertyChanged("FileUrlPath");
			}
		}

		public FileSearchDlg() {

			InitializeComponent();

			DataContext = this;

			this.btnOK.Click += btnOK_Click;
			this.btnCancel.Click += btnCancel_Click;
		}

		void btnCancel_Click(object sender, RoutedEventArgs e) {
			if (CancelButtonClicked != null) {
				CancelButtonClicked(sender, e);
			}
		}

		void btnOK_Click(object sender, RoutedEventArgs e) {
			if (OKButtonClicked != null) {
				OKButtonClicked(sender, e);
			}
		}

		private void btnPath_Click(object sender, RoutedEventArgs e) {
			// Create OpenFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			string filterString = string.Empty;
			var filters = AVFileFormat.GetFormatFilters();
			foreach (var item in filters) {
				filterString += item.Item1 + "|" + item.Item2;
			}
			// Set filter for file extension and default file extension
			dlg.DefaultExt = "*.*";
			dlg.Filter = filterString;

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true) {
				// Open document
				this.FileUrlPath = dlg.FileName;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class AVFileFormat {
		public static List<Tuple<string, string>> GetFormatFilters () {
			List<Tuple<string, string>> formats = new List<Tuple<string, string>>();
			formats.Add(new Tuple<string, string>("*.* (All Files)", "*.*|"));
			formats.Add(new Tuple<string, string>("*.mp4 (MP4 Files)", "*.mp4|"));
			formats.Add(new Tuple<string, string>("*.avi (AVI Files)", "*.avi|"));
			formats.Add(new Tuple<string, string>("*.mpeg (MPEG Files)", "*.mpeg|"));
			formats.Add(new Tuple<string, string>("*.wmv (WMV Files)", "*.wmv|"));
			formats.Add(new Tuple<string, string>("*.flv (FLV Files)", "*.flv|"));
			formats.Add(new Tuple<string, string>("*.mkv (MKV Files)", "*.mkv"));
			return formats;
		}
		public static List<string> GetFormats() {
			List<string> formats = new List<string>();
			formats.Add("mp4");
			formats.Add("avi");
			formats.Add("mpeg");
			formats.Add("wmv");
			formats.Add("flv");
			formats.Add("mkv");
			return formats;
		}
	}
}
