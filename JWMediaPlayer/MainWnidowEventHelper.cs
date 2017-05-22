//#define __TOUCH_DEBUG__

using MahApps.Metro.Controls;
using JWMediaPlayer.Models;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JWMediaPlayer {
	public class MainWnidowEventHelper : IDisposable{
		private MainWindow window;
		private bool _isFullScreen;
		private bool _sliderDown;
		private Timer _toastVideoTimer;
		bool _isTouchDown;
		Point _startTouchPoint;
        
		public MainWnidowEventHelper(MainWindow window) {
			this.window = window;
		}

		~MainWnidowEventHelper() {
			Dispose(false);
		}

		public void ControlLock() {
			//this.window.KeyUp -= Window_KeyUp;
			//this.window.PlayerLayout.MouseUp -= Window_MouseUp;
			//this.window.MouseDoubleClick -= Window_MouseDoubleClick;

			this.window.ProgressSlider.PreviewMouseDown -= ProgressSlider_MouseDown;
			this.window.ProgressSlider.PreviewMouseMove -= ProgressSlider_MouseMove;
			this.window.ProgressSlider.PreviewMouseUp -= ProgressSlider_MouseUp;


			//this.window.TouchDown += MainWindow_TouchDown;
			//this.window.TouchMove += MainWindow_TouchMove;
			//this.window.TouchUp += MainWindow_TouchUp;

			this.window.PlayerLayout.TouchDown -= MainWindow_TouchDown;
			this.window.PlayerLayout.TouchMove -= MainWindow_TouchMove;
			this.window.PlayerLayout.TouchUp -= MainWindow_TouchUp;

			this.window.MouseDoubleClick -= Window_MouseDoubleClick;
		}

		public void ControlUnLock() {
			//this.window.KeyUp += Window_KeyUp;
			//this.window.PlayerLayout.MouseUp += Window_MouseUp;
			//this.window.MouseDoubleClick += Window_MouseDoubleClick;

			this.window.ProgressSlider.PreviewMouseDown += ProgressSlider_MouseDown;
			this.window.ProgressSlider.PreviewMouseMove += ProgressSlider_MouseMove;
			this.window.ProgressSlider.PreviewMouseUp += ProgressSlider_MouseUp;

			//this.window.TouchDown += MainWindow_TouchDown;
			//this.window.TouchMove += MainWindow_TouchMove;
			//this.window.TouchUp += MainWindow_TouchUp;

			this.window.PlayerLayout.TouchDown += MainWindow_TouchDown;
			this.window.PlayerLayout.TouchMove += MainWindow_TouchMove;
			this.window.PlayerLayout.TouchUp += MainWindow_TouchUp;

			this.window.MouseDoubleClick += Window_MouseDoubleClick;
		}

		public void Build() {
			AspectRatioViewModel viewModel = new AspectRatioViewModel();

			this.window.AspectRatioList = viewModel.Items;
			this.window.TopMenus.Visibility = Visibility.Hidden;

			this.window.KeyUp += Window_KeyUp;
			this.window.PlayerLayout.MouseUp += Window_MouseUp;
			this.window.MouseDoubleClick += Window_MouseDoubleClick;

			this.window.ProgressSlider.PreviewMouseDown += ProgressSlider_MouseDown;
			this.window.ProgressSlider.PreviewMouseMove += ProgressSlider_MouseMove;
			this.window.ProgressSlider.PreviewMouseUp += ProgressSlider_MouseUp;

			this.window.btnPlayList.Click += BtnPlayList_Click;
			//this.window.TouchDown += MainWindow_TouchDown;
			//this.window.TouchMove += MainWindow_TouchMove;
			//this.window.TouchUp += MainWindow_TouchUp;

			this.window.PlayerLayout.TouchDown += MainWindow_TouchDown;
			this.window.PlayerLayout.TouchMove += MainWindow_TouchMove;
			this.window.PlayerLayout.TouchUp += MainWindow_TouchUp;

			this.window.btnAdjust.Click += (s, e) =>
			{
				Nullable<bool> check = this.window.btnAdjust.IsChecked;
				if (check == true) {
					this.window.AdjustLayout.Visibility = Visibility.Visible;
				}
				else {
					this.window.AdjustLayout.Visibility = Visibility.Hidden;
				}
			};

			this.window.btnCapture.Click += (s, e) =>
			{
				this.window.Capture();
			};

			this.window.BtnTopMost.Click += (s, e) =>
			{
				if ((bool)this.window.BtnTopMost.IsChecked) {
					this.window.Topmost = true;
					this.window.BtnTopMost.Foreground = new SolidColorBrush(Colors.GreenYellow);
				}
				else {
					this.window.Topmost = false;
					this.window.BtnTopMost.Foreground = new SolidColorBrush(Colors.White);
				}
			};

			this.window.NowTime.Click += (s, e) =>
			{
				if ((bool)this.window.NowTime.IsChecked)
					this.window.TimeText.Visibility = Visibility.Visible;
				else
					this.window.TimeText.Visibility = Visibility.Hidden;
			};

			Task.Factory.StartNew(() =>
		   {
			   while (true) {
				   System.Threading.Thread.Sleep(1000);
				   this.window.Dispatcher.Invoke(() =>
				  {
					  this.window.TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
				  });
			   }
		   });



#if __TOUCH_DEBUG__
			this.Log1.Visibility = Visibility.Visible;
			this.Log2.Visibility = Visibility.Visible;
			this.Log3.Visibility = Visibility.Visible;
			this.Log4.Visibility = Visibility.Visible;
#else
			this.window.Log1.Visibility = Visibility.Collapsed;
			this.window.Log2.Visibility = Visibility.Collapsed;
			this.window.Log3.Visibility = Visibility.Collapsed;
			this.window.Log4.Visibility = Visibility.Collapsed;
#endif
		}

		private void BtnPlayList_Click (object sender, RoutedEventArgs e) {
			var flyout = this.window.Flyouts.Items[0] as Flyout;

			if (flyout == null) return;
			flyout.IsOpen = !flyout.IsOpen;
		}

		private void Window_KeyUp (object sender, System.Windows.Input.KeyEventArgs e) {
			if ((bool)this.window.Lock.IsChecked) return;
			var metroWindow = (MetroWindow)sender;

			if (metroWindow != null) {
				if (Keyboard.IsKeyUp(e.Key) && e.Key == Key.Enter) {
					if (!_isFullScreen) {
						metroWindow.WindowState = WindowState.Maximized;
						metroWindow.UseNoneWindowStyle = true;
						metroWindow.IgnoreTaskbarOnMaximize = true;
						metroWindow.ShowTitleBar = false;
						_isFullScreen = true;
					}
					else {
						metroWindow.WindowState = WindowState.Normal;
						metroWindow.UseNoneWindowStyle = false;
						metroWindow.ShowTitleBar = true; // <-- this must be set to true
						metroWindow.IgnoreTaskbarOnMaximize = false;
						_isFullScreen = false;
					}
				}
				else if (e.Key == Key.Escape) {
					SetControlPanelVisible(true);
				}
			}
		}

		private void Window_MouseUp (object sender, MouseButtonEventArgs e) {
			if (_isTouchMove) {
				_isTouchMove = false;
				return;
			}

			if (e.ChangedButton == MouseButton.Left) {
					SetControlPanelVisible(this.window.ControlLayoutVisible == Visibility.Visible);
			}
		}

		private void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				var metroWindow = (MetroWindow)sender;

				if (e.RightButton == MouseButtonState.Released) {
					if (!_isFullScreen) {
						metroWindow.WindowState = WindowState.Maximized;
						metroWindow.UseNoneWindowStyle = true;
						metroWindow.IgnoreTaskbarOnMaximize = true;
						_isFullScreen = true;
					}
					else {
						metroWindow.WindowState = WindowState.Normal;
						metroWindow.UseNoneWindowStyle = false;
						metroWindow.ShowTitleBar = true; // <-- this must be set to true
						metroWindow.IgnoreTaskbarOnMaximize = false;
						_isFullScreen = false;
					}
				}
			}
		}

		private void ProgressSlider_MouseDown (object sender, MouseButtonEventArgs e) {
			_sliderDown = true;
			ToastVideoTime();
		}

		private void ProgressSlider_MouseUp (object sender, MouseButtonEventArgs e) {
			if (_sliderDown) {
				ToastVideoTime();
			}
		}

		private void ProgressSlider_MouseMove (object sender, MouseEventArgs e) {
			if (_sliderDown) {
				ToastVideoTime();
				_sliderDown = false;
			}
		}

		private void SetControlPanelVisible (bool isVisible) {
			if (isVisible) {
				this.window.ControlLayoutVisible = Visibility.Hidden;
				this.window.AspectRatioSplitButton.Visibility = Visibility.Hidden;
			}
			else {
				this.window.ControlLayoutVisible = Visibility.Visible;
				this.window.AspectRatioSplitButton.Visibility = Visibility.Visible;
			}
		}

		private void ToastVideoTime () {
			this.window.VideoTime.Visibility = Visibility.Visible;
			if (_toastVideoTimer == null) {
				_toastVideoTimer = new Timer();
				_toastVideoTimer.Elapsed += _toastVideoTimer_Elapsed;
				_toastVideoTimer.Interval = 5 * 1000;
				_toastVideoTimer.Start();
			}
		}

		private void _toastVideoTimer_Elapsed (object sender, ElapsedEventArgs e) {
			if (_toastVideoTimer != null) {
				_toastVideoTimer.Stop();
				_toastVideoTimer.Elapsed -= _toastVideoTimer_Elapsed;
				_toastVideoTimer.Dispose();
				_toastVideoTimer = null;
				this.window.Dispatcher.Invoke(() =>
				{
					this.window.VideoTime.Visibility = Visibility.Hidden;
				});
			}
		}

		public void Dispose () {
			Dispose(true);
		}

		protected virtual void Dispose (bool bDisposing) {
			if (bDisposing) {
				// dispose managed resource (종결자를 가진 객체의 자원 해제)
				if (_toastVideoTimer != null) {
					_toastVideoTimer.Dispose();
					_toastVideoTimer.Elapsed -= _toastVideoTimer_Elapsed;
					_toastVideoTimer = null;
				}
			}

			// do releasing unmanaged resource (종결자가 없는 객체의 자원 해제)
			// i.e. close file handle of operating systems
			this.window.KeyUp -= Window_KeyUp;
			this.window.MouseUp -= Window_MouseUp;
			this.window.MouseDoubleClick -= Window_MouseDoubleClick;

			this.window.ProgressSlider.PreviewMouseDown -= ProgressSlider_MouseDown;
			this.window.ProgressSlider.PreviewMouseMove -= ProgressSlider_MouseMove;
			this.window.ProgressSlider.PreviewMouseUp -= ProgressSlider_MouseUp;

			this.window.btnPlayList.Click -= BtnPlayList_Click;
			//this.window.TouchDown += MainWindow_TouchDown;
			//this.window.TouchMove += MainWindow_TouchMove;
			//this.window.TouchUp += MainWindow_TouchUp;

			this.window.PlayerLayout.TouchDown -= MainWindow_TouchDown;
			this.window.PlayerLayout.TouchMove -= MainWindow_TouchMove;
			this.window.PlayerLayout.TouchUp -= MainWindow_TouchUp;

			// suppress calling of Finalizer
			GC.SuppressFinalize(this);
		}

		private void MainWindow_TouchUp (object sender, TouchEventArgs e) {
			if (_isTouchDown) {
				_isTouchDown = false;
				_startTouchPoint = new Point(0, 0);
			}
		}

		private void MainWindow_TouchDown (object sender, TouchEventArgs e) {
			_isTouchDown = true;
			_startTouchPoint = e.GetTouchPoint(this.window).Position;

#if __TOUCH_DEBUG__
			this.window.Log1.Text = $"START_POS X : {_startTouchPoint.X} Y : {_startTouchPoint.Y}";
#endif
		}

		private bool _isTouchMove;
		private void MainWindow_TouchMove (object sender, TouchEventArgs e) {

			if (_isTouchDown) {
				var movePoint = e.GetTouchPoint(this.window).Position;

#if __TOUCH_DEBUG__
				this.window.Log2.Text = $"MOVE_POS X : {movePoint.X} Y : {movePoint.Y}";
#endif
				var resultX = _startTouchPoint.X - movePoint.X;
				var resultY = _startTouchPoint.Y - movePoint.Y;

#if __TOUCH_DEBUG__
				this.window.Log3.Text = $"RESULT X : {resultX} Y : {resultY}";
#endif
				if (Math.Abs(resultX) > 100.0d) {
					_isTouchMove = true;
					if ((int)resultX > 0) {
						this.window.SetVideoSeek(GlobalValues.Seek5Second);
						ToastVideoTime();
					}
					else {
						this.window.SetVideoSeek(GlobalValues.Seek5Second);
						ToastVideoTime();
					}
					ToastVideoTime();
				}

				if (Math.Abs(resultY) > 100.0d) {
					_isTouchMove = true;
					if ((int)resultY > 0) {
						this.window.SetVolume(GlobalValues.VolumeUpValue);
						this.window.ToastVolumeMark();
					}
					else {
						this.window.SetVolume(GlobalValues.VolumeDownValue);
						this.window.ToastVolumeMark();
					}
				}
			}
		}
	}
}
