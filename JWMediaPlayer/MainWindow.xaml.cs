//#define __CSHARP6__

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Meta.Vlc;
using JWMediaPlayer.Models;
using JWMediaPlayer.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaState = Meta.Vlc.Interop.Media.MediaState;

namespace JWMediaPlayer {

    /// <summary>
    ///     MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged {
		#region variable

		private Timer _timer;
		private Timer _videoTimer;
		private CustomDialog _customDialog;
		private FileSearchDlg _fileSearchWindowDlg;
		private Settings _settingsDlg;

		private MainWnidowEventHelper _mainWindowEventHelper;
        
        bool _isClose;
        #endregion

        #region property
        private List<AspectRatioModel> _aspectRatioList;
		public List<AspectRatioModel> AspectRatioList
		{
			get { return _aspectRatioList; }
			set { _aspectRatioList = value; RaisePropertyChanged("AspectRatioList"); }
		}

		private Visibility _controlLayoutVisible;
		public Visibility ControlLayoutVisible {
			get { return _controlLayoutVisible; }
			set { _controlLayoutVisible = value; RaisePropertyChanged("ControlLayoutVisible"); }
		}

		private static MainWindow _instance;
		public static MainWindow Instance
		{
			get
			{
				return _instance;
			}
		}

		public string PlayingFileName
		{
			get {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastPlayFileName))
                {
                    FileInfo fInfo = new FileInfo(Properties.Settings.Default.LastPlayFileName);

                    var fileName = fInfo.Name;
                    if (fileName.Length > 60) fileName = fileName.Substring(0, 60) + "...";
                    return fileName;
                }

                return string.Empty;
			}
			set {
				Properties.Settings.Default.LastPlayFileName = value;
				Properties.Settings.Default.Save();
				RaisePropertyChanged("PlayingFileName");
			}
		}

		public float Brightness
		{
			get { return MetaVlcPlayer.VlcMediaPlayer.Brightness; }
			set
			{
				MetaVlcPlayer.VlcMediaPlayer.IsAdjustEnable = true;
				MetaVlcPlayer.VlcMediaPlayer.Brightness = value;
				RaisePropertyChanged("Brightness");
			}
		}

		public float Gamma
		{
			get { return MetaVlcPlayer.VlcMediaPlayer.Gamma; }
			set
			{
				MetaVlcPlayer.VlcMediaPlayer.IsAdjustEnable = true;
				MetaVlcPlayer.VlcMediaPlayer.Gamma = value;
				RaisePropertyChanged("Gamma");
			}
		}

		public float Contrast
		{
			get { return MetaVlcPlayer.VlcMediaPlayer.Contrast; }
			set
			{
				MetaVlcPlayer.VlcMediaPlayer.IsAdjustEnable = true;
				MetaVlcPlayer.VlcMediaPlayer.Contrast = value;
				RaisePropertyChanged("Contrast");
			}
		}
        #endregion

        GlobalHotKey.HotKeyManager mgr = new GlobalHotKey.HotKeyManager();

        #region constructor
        public MainWindow () {
			DataContext = this;

			InitializeComponent();

			_instance = this;

			_mainWindowEventHelper = new MainWnidowEventHelper(this);
			_mainWindowEventHelper.Build();

			Init();

			if(string.IsNullOrEmpty(Properties.Settings.Default.SnapShotPath)) {
				var t = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var t1 = Path.Combine(t, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

				Properties.Settings.Default.SnapShotPath = t1;
			}

            
            mgr.Register(Key.Left, ModifierKeys.None);
            mgr.Register(Key.Right, ModifierKeys.None);
            mgr.Register(Key.Up, ModifierKeys.None);
            mgr.Register(Key.Down, ModifierKeys.None);
            mgr.KeyPressed += Mgr_KeyPressed;
        }

        private void Mgr_KeyPressed(object sender, GlobalHotKey.KeyPressedEventArgs e)
        {
            if ((bool)this.Lock.IsChecked) return;
            
            if (_isActivate)
            {
                if (e.HotKey.Key == Key.Left)
                {
                    this.SetVideoSeek(-GlobalValues.Seek5Second);
                    ToastVideoTime();
                }
                else if (e.HotKey.Key == Key.Right)
                {
                    this.SetVideoSeek(GlobalValues.Seek5Second);
                    ToastVideoTime();
                }
                else if (e.HotKey.Key == Key.Up)
                {
                    SetVolume(GlobalValues.VolumeUpValue);
                    ToastVolumeMark();
                }
                else if (e.HotKey.Key == Key.Down)
                {
                    SetVolume(GlobalValues.VolumeDownValue);
                    ToastVolumeMark();
                }
            }
        }
        #endregion

        bool _isActivate;
        protected override void OnActivated(EventArgs e)
        {
            _isActivate = true;
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            _isActivate = false;
            base.OnDeactivated(e);
        }

        private void Init () {
			this.KeyDown += MainWindow_KeyDown;
			this.MouseWheel += MainWindow_MouseWheel;

			MetaVlcPlayer.VolumeChanged += XZuneMetaVlcPlayer_VolumeChanged;
			MetaVlcPlayer.LengthChanged += XZuneMetaVlcPlayer_LengthChanged;
			MetaVlcPlayer.TimeChanged += XZuneMetaVlcPlayer_TimeChanged;
			MetaVlcPlayer.StateChanged += xZuneMetaVlcPlayer_StateChanged;

			this.Loaded += (s, e) =>
			{
				if (JWMediaPlayer.Properties.Settings.Default.IsAutoPlay) {
					JWMediaPlayer.Properties.Settings.Default.IsAutoPlay = false;
					
					VideoPlay();
				}
			};
		}

		#region events
		private void Lock_Click (object sender, RoutedEventArgs e) {
			if ((bool)this.Lock.IsChecked) {
				LockBrush.Visual = (Visual)FindResource("appbar_lock");
				_mainWindowEventHelper.ControlLock();
				this.ProgressSlider.IsEnabled = false;
			}
			else {
				LockBrush.Visual = (Visual)FindResource("appbar_unlock");
				_mainWindowEventHelper.ControlUnLock();
				this.ProgressSlider.IsEnabled = true;
			}
		}

		private void MainWindow_KeyDown (object sender, KeyEventArgs e) {

		}

		private void MainWindow_MouseWheel (object sender, MouseWheelEventArgs e) {
			SetVolume(e.Delta);
			ToastVolumeMark();
		}

		private void xZuneMetaVlcPlayer_StateChanged (object sender, ObjectEventArgs<MediaState> e) {
			if (e.Value == MediaState.Ended) {
				Play.Visibility = Visibility.Visible;
				Pause.Visibility = Visibility.Collapsed;
				ProgressSlider.Value = 0;
				MetaVlcPlayer.Stop();
				var items = UcPlaylist.Items;

				for (int i = 0; i < items.Count; i++) {
					if(items[i].FileName == Properties.Settings.Default.LastPlayFileName) {
						i++;
						if (i > items.Count - 1) return;

						PlayingFileName = items[i].FileName;

						Button_Click(this.Open, null);
						break;
					}
				}
			}
            else if(e.Value == MediaState.Playing) {
                Pause.Visibility = Visibility.Visible;
                Play.Visibility = Visibility.Collapsed;
            }
            else if(e.Value == MediaState.Paused) {
                Pause.Visibility = Visibility.Collapsed;
                Play.Visibility = Visibility.Visible;
            }
		}

		private void XZuneMetaVlcPlayer_TimeChanged (object sender, EventArgs e) {
			var dt = new DateTime(2012, 01, 01);
			var ts = MetaVlcPlayer.Time;
			dt = dt + ts;
#if __CSHARP6__
			PlayTime.Text = $"{dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}";
#else
			PlayTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second);
#endif
		}

		private void ToastVideoTime() {
			this.VideoTime.Visibility = Visibility.Visible;
			if (_videoTimer == null) {
				_videoTimer = new Timer();
				_videoTimer.Elapsed += _videoTimer_Elapsed;
				_videoTimer.Interval = 5 * 1000;
				_videoTimer.Start();
			}
		}

		private void _videoTimer_Elapsed (object sender, ElapsedEventArgs e) {
			if (_videoTimer != null) {
				_videoTimer.Stop();
				_videoTimer.Elapsed -= _videoTimer_Elapsed;
				_videoTimer.Dispose();
				_videoTimer = null;
				this.Dispatcher.Invoke(() =>
				{
					this.VideoTime.Visibility = Visibility.Hidden;
				});
			}
		}
        
		private void XZuneMetaVlcPlayer_LengthChanged (object sender, EventArgs e) {
            DateTime dateTime = new DateTime(2012, 1, 1) + this.MetaVlcPlayer.Length;
            GlobalValues.Seek5Second = (float)(GlobalValues.SEEK_TIME / this.MetaVlcPlayer.Length.TotalSeconds);
#if __CSHARP6__
			PlayTime.Text = $"{dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}";
#else
            this.PlayLength.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", dateTime.Hour, dateTime.Minute, dateTime.Second);
#endif
        }

		private void XZuneMetaVlcPlayer_VolumeChanged (object sender, EventArgs e) {
			if (MetaVlcPlayer.Volume == 0)
				VolumeIcon.Visual = (Visual)FindResource("appbar_sound_0");
			else if (MetaVlcPlayer.Volume <= 40)
				VolumeIcon.Visual = (Visual)FindResource("appbar_sound_1");
			else if (MetaVlcPlayer.Volume <= 70)
				VolumeIcon.Visual = (Visual)FindResource("appbar_sound_2");
			else
				VolumeIcon.Visual = (Visual)FindResource("appbar_sound_3");

			ToastVolumeMark();
		}

		private void Button_Click (object sender, RoutedEventArgs e) {
			var btn = sender as Button;

			if (btn == null)
				return;

			if ((bool)this.Lock.IsChecked) {
				if (btn.Equals(Sound) || btn.Equals(SoundIcon)) {
					SetVolumeMute();
				}
				else if(btn.Equals(Menus)) {
					SetTopMenuVisible();
				}
				else if (btn.Equals(Open)) {
					VideoStop();
					VideoPlay();
				}
				return;
			}

			if (btn.Equals(Sound) || btn.Equals(SoundIcon)) {
				SetVolumeMute();
			}
			else if (btn.Equals(Play)) {
				VideoPlay();
			}
			else if (btn.Equals(Stop)) {
				VideoStop();
			}
			else if (btn.Equals(Forward)) {
				SetVideoSeek(+0.002f);
				ToastVideoTime();
			}
			else if (btn.Equals(Rewind)) {
				SetVideoSeek(-0.002f);
				ToastVideoTime();
			}
			else if (btn.Equals(Open)) {
				VideoStop();
				VideoPlay();
			}
			else if (btn.Equals(this.Menus)) {
				SetTopMenuVisible();
			}
			else if(btn.Equals(this.Pause)) {
				VideoPlay();
			}
            else if (btn.Equals(this.PlayPrevious)) {
                var items = UcPlaylist.Items;

                for (int i = 0; i < items.Count; i++) {
                    if (items[i].FileName == Properties.Settings.Default.LastPlayFileName) {
                        i--;
                        if (i <= 0) { i = 0; return; }
                        if (i > items.Count - 1) return;

                        PlayingFileName = items[i].FileName;

                        Button_Click(this.Open, null);
                        break;
                    }
                }
            }
            else if(btn.Equals(this.PlayNext)) {
                var items = UcPlaylist.Items;

                for (int i = 0; i < items.Count; i++) {
                    if (items[i].FileName == Properties.Settings.Default.LastPlayFileName) {
                        i++;
                        if (i > items.Count - 1) return;

                        PlayingFileName = items[i].FileName;

                        Button_Click(this.Open, null);
                        break;
                    }
                }
            }
            else if (btn.Equals(this.btnPlayRate)) {
                if (this.MetaVlcPlayer.Rate == 1f) {
                    this.MetaVlcPlayer.Rate = 1.2f;
                }
                else if (this.MetaVlcPlayer.Rate == 1.2f) {
                    this.MetaVlcPlayer.Rate = 1.5f;
                }
                else if (this.MetaVlcPlayer.Rate == 1.5f) {
                    this.MetaVlcPlayer.Rate = 2.0f;
                }
                else if (this.MetaVlcPlayer.Rate == 2.0f) {
                    this.MetaVlcPlayer.Rate = 2.5f;
                }
                else if (this.MetaVlcPlayer.Rate == 2.5f) {
                    this.MetaVlcPlayer.Rate = 3.0f;
                }
                else if (this.MetaVlcPlayer.Rate == 3.0f) {
                    this.MetaVlcPlayer.Rate = 1.0f;
                }

                this.btnPlayRate.Content = string.Format("{0:#.#}x", this.MetaVlcPlayer.Rate);
            }
		}

		private void MainWindow_Closing (object sender, CancelEventArgs e) {
            if (!_isClose)
            {
                e.Cancel = true;
                ShowCloseMessage();
            }
		}

		private void ProgressSlider_ValueChanged (object sender, RoutedPropertyChangedEventArgs<double> e) {
			//var value = (float)(e.NewValue / slider.ActualWidth);
			//slider.Value = value;
			//Debug.WriteLine("slider : {0}", slider.Value);
			//Debug.WriteLine("Position : {0}", xZuneMetaVlcPlayer.Position);
		}

		private void AspectRatioSplitButton_SelectionChanged (object sender, SelectionChangedEventArgs e) {
			var model = this.AspectRatioSplitButton.SelectedItem as AspectRatioModel;

			if (model != null) {
				this.MetaVlcPlayer.AspectRatio = model.AspectRatioValue;
			}
		}

		private async void SearchPath_Click (object sender, RoutedEventArgs e) {
			if ((bool)this.Lock.IsChecked) return;
			_customDialog = new CustomDialog();
			//var mySettings = new MetroDialogSettings() {
			//	AffirmativeButtonText = Resources["AffirmativeButtonText"].ToString(),
			//	AnimateShow = true,
			//	NegativeButtonText = Resources["NegativeButtonText"].ToString(),
			//	FirstAuxiliaryButtonText = Resources["FirstAuxiliaryButtonText"].ToString(),
			//};
			_fileSearchWindowDlg = new FileSearchDlg();
			_fileSearchWindowDlg.CancelButtonClicked += _fileSearchWindowDlg_CancelButtonClicked;
			_fileSearchWindowDlg.OKButtonClicked += _fileSearchWindowDlg_OKButtonClicked;
			_customDialog.Content = _fileSearchWindowDlg;
			await this.ShowMetroDialogAsync(_customDialog);
		}

		private async void Setting_Click (object sender, RoutedEventArgs e) {
			if ((bool)this.Lock.IsChecked) return;
			_customDialog = new CustomDialog();
			//var mySettings = new MetroDialogSettings() {
			//	AffirmativeButtonText = Resources["AffirmativeButtonText"].ToString(),
			//	AnimateShow = true,
			//	NegativeButtonText = Resources["NegativeButtonText"].ToString(),
			//	FirstAuxiliaryButtonText = Resources["FirstAuxiliaryButtonText"].ToString(),
			//};
			if (_settingsDlg == null)
				_settingsDlg = new Settings();

			_settingsDlg.SnapShotPath = Properties.Settings.Default.SnapShotPath;
			_settingsDlg.CancelButtonClicked += _settingsDlg_CancelButtonClicked;
			_settingsDlg.OKButtonClicked += _settingsDlg_OKButtonClicked;
			_customDialog.Content = _settingsDlg;
			await this.ShowMetroDialogAsync(_customDialog);
		}

		void _fileSearchWindowDlg_OKButtonClicked (object arg1, EventArgs arg2) {
			this.HideMetroDialogAsync(_customDialog);
			PlayingFileName = _fileSearchWindowDlg.FileUrlPath;
			Button_Click(this.Open, null);
		}

		public void VideoPlay(string fileName) {
			PlayingFileName = fileName;
			Button_Click(this.Open, null);
		}

		void _fileSearchWindowDlg_CancelButtonClicked (object arg1, EventArgs arg2) {
			this.HideMetroDialogAsync(_customDialog);
		}

		private void _settingsDlg_OKButtonClicked (object arg1, EventArgs arg2) {
			this.HideMetroDialogAsync(_customDialog);
		}

		private void _settingsDlg_CancelButtonClicked (object arg1, EventArgs arg2) {
			this.HideMetroDialogAsync(_customDialog);
		}
		#endregion

		#region functions
		public void SetVolume(int n) {
			if (n > 0) {
				if (this.MetaVlcPlayer.Volume >= 100)
					this.MetaVlcPlayer.Volume = 100;
				else
					this.MetaVlcPlayer.Volume++;
			}
			else {
				if (this.MetaVlcPlayer.Volume <= 0)
					this.MetaVlcPlayer.Volume = 0;
				else
					this.MetaVlcPlayer.Volume--;
			}
		}

		public void ToastVolumeMark() {
			this.SoundBlock.Visibility = Visibility.Visible;
			this.SoundIcon.Visibility = Visibility.Visible;

			if (_timer == null) {
				_timer = new Timer();
				_timer.Elapsed += Timer_Elapsed;
				_timer.Interval = 5 * 1000;
				_timer.Start();
			}
		}

		private void Timer_Elapsed (object sender, ElapsedEventArgs e) {
			if (_timer != null) {
				_timer.Stop();
				_timer.Elapsed -= Timer_Elapsed;
				_timer.Dispose();
				_timer = null;
				this.Dispatcher.Invoke(() =>
				{
					this.SoundBlock.Visibility = Visibility.Hidden;
					this.SoundIcon.Visibility = Visibility.Hidden;

				});
			}
		}

		private void SetVolumeMute () {
			if (!MetaVlcPlayer.IsMute) {
				VolumeIcon.Visual = (Visual)FindResource("appbar_sound_mute");
				MetaVlcPlayer.IsMute = true;
			}
			else {
				MetaVlcPlayer.IsMute = false;
				XZuneMetaVlcPlayer_VolumeChanged(null, null);
			}
		}

		private void VideoStop () {
			MetaVlcPlayer.Stop();
			ProgressSlider.Value = 0;
		}

		private void SetTopMenuVisible () {
			if (this.TopMenus.Visibility == Visibility.Hidden) {
				this.TopMenus.Visibility = Visibility.Visible;
			}
			else {
				this.TopMenus.Visibility = Visibility.Hidden;
			}
		}

		public void SetVideoSeek (float f) {
			MetaVlcPlayer.Position += f;
		}

		private async void VideoPlay () {
            bool isHttp = false;
            if (!File.Exists(Properties.Settings.Default.LastPlayFileName)) {
#if __CSHARP6__
            
            if (Properties.Settings.Default.LastPlayFileName.Contains("http")) { isHttp = true; }

			if (!isHttp && !File.Exists(Properties.Settings.Default.LastPlayFileName)) {
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["NoSearchFile"].ToString() + $"({Properties.Settings.Default.LastPlayFileName})",
					MessageDialogStyle.Affirmative);
#else
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["NoSearchFile"].ToString() + string.Format("({0})", Properties.Settings.Default.LastPlayFileName),
					MessageDialogStyle.Affirmative);
#endif

				return;
			}


			if (!IsAvFileCheck()) {
#if __CSHARP6__
			if (!isHttp && !IsAvFileCheck()) {
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["NoSearchFile"].ToString() + $"({Properties.Settings.Default.LastPlayFileName})",
					MessageDialogStyle.Affirmative);
#else
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["NoSearchFile"].ToString() + string.Format("({0})", Properties.Settings.Default.LastPlayFileName),
					MessageDialogStyle.Affirmative);
#endif

				return;
			}

            string subTitleFileName = string.Empty;
			bool isSubTitleOk = false;
            if(!isHttp)
			    SubTitleCheck(out subTitleFileName, out isSubTitleOk);

			if (MetaVlcPlayer.State == MediaState.Playing) {
				MetaVlcPlayer.Pause();
				Pause.Visibility = Visibility.Collapsed;
				Play.Visibility = Visibility.Visible;
				return;
			}
			if (MetaVlcPlayer.State == MediaState.Buffering) {
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["LoadingVideo"].ToString(),
					MessageDialogStyle.Affirmative);
				return;
			}
			if (MetaVlcPlayer.State == MediaState.Error) {
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["UnknownError"].ToString(), MessageDialogStyle.Affirmative);
				return;
			}
			if (MetaVlcPlayer.State == MediaState.Opening) {
				await this.ShowMessageAsync(Application.Current.Resources["MessageTitleByInfo"].ToString(),
					Application.Current.Resources["LoadingVideo"].ToString(),
					MessageDialogStyle.Affirmative);
			}
			if (MetaVlcPlayer.State == MediaState.Paused) {
				Pause.Visibility = Visibility.Visible;
				Play.Visibility = Visibility.Collapsed;
				MetaVlcPlayer.Resume();
				return;
			}

			if (!isHttp && isSubTitleOk) {
				MetaVlcPlayer.LoadMediaWithOptions(Properties.Settings.Default.LastPlayFileName, new[] { "sub-file=" + subTitleFileName });
			}
			else {
				MetaVlcPlayer.LoadMedia(new Uri(Properties.Settings.Default.LastPlayFileName));
			}

			var control = this.flyoutsControl.Items[0] as PlayListControl;
			if (control != null) {
				var displayName = Path.GetFileName(Properties.Settings.Default.LastPlayFileName);
				control.ViewModel.Add(new PlayListModel { FileName = Properties.Settings.Default.LastPlayFileName, PlayTime = "", DisplayName = displayName });
				var path = Path.GetDirectoryName(Properties.Settings.Default.LastPlayFileName);
				var extension = Path.GetExtension(Properties.Settings.Default.LastPlayFileName);
				var files = Directory.GetFiles(path, "*" + extension);

				foreach(var file in files) {
					displayName = Path.GetFileName(file);
					control.ViewModel.Add(new PlayListModel { FileName = file, PlayTime = "", DisplayName = displayName });
				}
			}

            Pause.Visibility = Visibility.Visible;
            Play.Visibility = Visibility.Collapsed;

            MetaVlcPlayer.Play();
		}

		private bool IsAvFileCheck () {
			bool isAvFile = false;
			var formats = AVFileFormat.GetFormats();

			foreach (var item in formats) {
				if (Properties.Settings.Default.LastPlayFileName.Contains(item)) {
					isAvFile = true;
					break;
				}
			}

			return isAvFile;
		}

		private void SubTitleCheck (out string subTitleFileName, out bool isSubTitleOk) {
			FileInfo fInfo = new FileInfo(Properties.Settings.Default.LastPlayFileName);
			subTitleFileName = string.Empty;
			isSubTitleOk = false;

			if (fInfo.Exists) {
				foreach (var extension in GlobalValues.SubTitleExtensions) {
					var fileNameWithoutExtension = fInfo.FullName.Replace(fInfo.Extension, "");
					subTitleFileName = fileNameWithoutExtension + extension;

					if (File.Exists(subTitleFileName)) {
						isSubTitleOk = ConvertSubTitleToUTF8(subTitleFileName);
						return;
					}
				}
			}
		}

		private static bool ConvertSubTitleToUTF8 (string subTitleFileName) {
			bool isRet = false;

			try {
				Encoding encodingType = SimpleHelpers.FileEncoding.DetectFileEncoding(subTitleFileName);

				if (encodingType == Encoding.UTF8) return true;

				if (encodingType != Encoding.UTF8) {
					string fileContent = string.Empty;
					using (StreamReader sr = new StreamReader(subTitleFileName, Encoding.GetEncoding("ks_c_5601-1987"))) {
						fileContent = sr.ReadToEnd();
						sr.Close();
					}

					if (!string.IsNullOrEmpty(fileContent)) {
						Encoding utf8 = Encoding.UTF8;
						//var asciiString = EncondigConvert.ConvertAsciiToUTF8(File.ReadAllText(subTitleFileName));
						byte[] byteArray = utf8.GetBytes(fileContent);

						using (FileStream fStream = new FileStream(subTitleFileName, FileMode.Create)) {
							fStream.Write(byteArray, 0, byteArray.Length);
							fStream.Close();
						}

						isRet = true;
					}
				}
			}
			catch (Exception e) {
				MessageBox.Show(e.Message);
			}

			return isRet;
		}

		public void Capture() {
			if (MetaVlcPlayer.VideoSource == null) return;

			MetaVlcPlayer.VideoSource.Dispatcher.Invoke(() =>
			{
				var png = new PngBitmapEncoder();
				png.Frames.Add(BitmapFrame.Create(MetaVlcPlayer.VideoSource));
				string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				using (Stream fileStream = File.Create(Path.Combine(Properties.Settings.Default.SnapShotPath, "./" + dateString + ".png"))) {
					png.Save(fileStream);
				}
			});
		}

		private async void ShowCloseMessage () {
			var result =
				await this.ShowMessageAsync(Application.Current.Resources["Close"].ToString(),
				Application.Current.Resources["CloseMessage"].ToString(),
				MessageDialogStyle.AffirmativeAndNegative);

			if (result == MessageDialogResult.Affirmative) {
                _isClose = true;

				MetaVlcPlayer.Stop();

				if (_mainWindowEventHelper != null) {
					_mainWindowEventHelper.Dispose();
					_mainWindowEventHelper = null;
				}

				if(_timer != null) {
					_timer.Dispose();
					_timer = null;
				}

				if(_videoTimer != null) {
					_videoTimer.Dispose();
					_videoTimer = null;
				}

				if(_customDialog != null) {
					_customDialog = null;
				}

				if(_fileSearchWindowDlg != null) {
					_fileSearchWindowDlg = null;
				}

				if(_settingsDlg != null) {
					_settingsDlg = null;
				}

                if(mgr != null)
                {
                    mgr.KeyPressed -= Mgr_KeyPressed;
                    mgr.Unregister(Key.Left, ModifierKeys.None);
                    mgr.Unregister(Key.Right, ModifierKeys.None);
                    mgr.Unregister(Key.Down, ModifierKeys.None);
                    mgr.Unregister(Key.Up, ModifierKeys.None);
                    mgr.Dispose();
                }

				MetaVlcPlayer.Dispose();

				GC.Collect();
				Application.Current.Shutdown();
			}
		}
		#endregion

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged (string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		#endregion
	}
}