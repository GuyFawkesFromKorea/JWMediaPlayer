using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace JWMediaPlayer.Module {
	/// <summary>
	/// PlayListControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PlayListControl : MahApps.Metro.Controls.Flyout {
		public PlayListViewModel ViewModel = new PlayListViewModel();

		public PlayListModel SelectedItem
		{
			get
			{
				return this.ViewModel.SelectedItem;
			}
		}

		public List<PlayListModel> Items
		{
			get
			{
				return this.ViewModel.Items.ToList();
			}
		}


		public PlayListControl () {
			this.DataContext = ViewModel;
			InitializeComponent();
			this.btnOK.Click += BtnOK_Click;
			this.btnLoad.Click += BtnLoad_Click;
			this.btnSave.Click += BtnSave_Click;
			this.btnAdd.Click += BtnAdd_Click;
			this.btnRemove.Click += BtnRemove_Click;
		}

		private void BtnRemove_Click (object sender, RoutedEventArgs e) {
			ViewModel.Remove(ViewModel.SelectedItem.FileName);
		}

		private void BtnAdd_Click (object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			var filters = AVFileFormat.GetFormatFilters();
			string filterString = string.Empty;
			foreach (var item in filters) {
				filterString += item.Item1 + "|" + item.Item2;
			}
			// Set filter for file extension and default file extension
			dlg.DefaultExt = "*.*";
			dlg.Filter = filterString;
			var result = dlg.ShowDialog();
			if ((bool)result) {
				ViewModel.Add(new PlayListModel { FileName = dlg.FileName, DisplayName = Path.GetFileName(dlg.FileName) });
			}
		}

		private void BtnSave_Click (object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "*.xml|*.xml";
			var result = dlg.ShowDialog();
			if((bool)result) {
				ViewModel.Save(dlg.FileName);
			}
		}

		private void BtnLoad_Click (object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "*.xml|*.xml";
			var result = dlg.ShowDialog();
			if ((bool)result) {
				ViewModel.Load(dlg.FileName);
			}

		}

		private void BtnOK_Click (object sender, RoutedEventArgs e) {
			MainWindow.Instance.VideoPlay(ViewModel.SelectedItem.FileName);
			this.IsOpen = false;
		}
	}
}
