using JWLibrary.Core.ExecuteLocation;
using Meta.Vlc.Wpf.Annotations;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace JWMediaPlayer {
	/// <summary>
	/// Settings.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Settings : UserControl, INotifyPropertyChanged {
		public event Action<object, EventArgs> OKButtonClicked;
		public event Action<object, EventArgs> CancelButtonClicked;

		public string SnapShotPath
		{
			get
			{
				var path = System.IO.Path.Combine(PathInfo.GetApplicationPath(), "./Snapshot");
				return Properties.Settings.Default.SnapShotPath ?? path;
			}
			set
			{
				if(Directory.Exists(value)) {
					Properties.Settings.Default.SnapShotPath = value;
					Properties.Settings.Default.Save();
				}
				else {
					Properties.Settings.Default.SnapShotPath = @"C:\";
					Properties.Settings.Default.Save();
				}
				OnPropertyChanged("SnapShotPath");
			}
		}

		public Settings () {
			this.DataContext = this;
			InitializeComponent();

			this.btnPath.Click += BtnPath_Click;
			this.btnOK.Click += BtnOK_Click;
			this.btnCancel.Click += BtnCancel_Click;
		}

		void BtnCancel_Click (object sender, RoutedEventArgs e) {
			if (CancelButtonClicked != null) {
				CancelButtonClicked(sender, e);
			}
		}

		void BtnOK_Click (object sender, RoutedEventArgs e) {
			if (OKButtonClicked != null) {
				OKButtonClicked(sender, e);
			}
		}

		private void BtnPath_Click (object sender, RoutedEventArgs e) {
			Gat.Controls.OpenDialogView openDialog = new Gat.Controls.OpenDialogView();
			Gat.Controls.OpenDialogViewModel vm = (Gat.Controls.OpenDialogViewModel)openDialog.DataContext;
			vm.IsDirectoryChooser = true;
			vm.Show();

			if (vm.SelectedFilePath == null) return;
			SnapShotPath = vm.SelectedFilePath.ToString();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
