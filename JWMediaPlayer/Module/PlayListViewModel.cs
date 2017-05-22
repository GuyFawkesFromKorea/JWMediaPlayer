using JWMediaPlayer.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JWMediaPlayer.Module {
	public class PlayListViewModel : ModelBase {
		private BindingList<PlayListModel> _items;
		public BindingList<PlayListModel> Items { get
			{
				return _items;
			}
			set
			{
				_items = value;
				OnPropertyChanged("Items");
			}
		}

		private PlayListModel _selectedItem;
		public PlayListModel SelectedItem { get
			{
				return _selectedItem;
			}
			set
			{
				_selectedItem = value;
				OnPropertyChanged("SelectedItem");
			}
		}

		public PlayListViewModel() {
			Items = new BindingList<PlayListModel>();
		}

		public void Add(PlayListModel model) {
			var exist = (from m in Items
						 where m.FileName == model.FileName
						 select m).FirstOrDefault();

			if (exist != null) return;

			Items.Add(model);
			Items = new BindingList<PlayListModel>((from m in Items
					 orderby m.DisplayName ascending
					 select m).ToList());
		}

		public void Remove(string fileName) {
			var exist = Items.Where(m => m.FileName == fileName).First();

			if (exist == null) return;

			Items.Remove(exist);
		}

		public void Save(string saveFileName) {
			Playlist plist = new Playlist();
			plist.PlayListModels = new PlayListModel[Items.Count];

			for(int i=0; i<Items.Count; i++) {
				plist.PlayListModels[i] = new PlayListModel {
					FileName = Items[i].FileName,
					DisplayName = Items[i].DisplayName
				};
			}
			Playlist.Save2Xml(saveFileName, plist);
		}

		public void Load(string loadFileName) {
			Items.Clear();
			Playlist list = Playlist.LoadFromXml(loadFileName);
			if (list.PlayListModels == null) return;

			if (list.PlayListModels.Length > 0) {
				IList<PlayListModel> templist = new List<PlayListModel>();
				foreach (var item in list.PlayListModels) {
					templist.Add(new PlayListModel { FileName = item.FileName, DisplayName = item.DisplayName });
				}

				Items = new BindingList<PlayListModel>(templist.OrderBy(e => e.DisplayName).ToList());

				if (Items.Count > 0) this.SelectedItem = Items[0];
			}
		}
	}

	public class PlayListModel : ModelBase{
		private string _displayName;
		public string DisplayName
		{
			get { return _displayName; }
			set { _displayName = value; OnPropertyChanged("DisplayName"); }
		}

		private string _fileName;
		public string FileName { get { return _fileName; } set { _fileName = value; OnPropertyChanged("FileName"); } }

		private string _playTime;
		public string PlayTime { get { return _playTime; } set { _playTime = value; OnPropertyChanged("PlayTime"); } }

	}

	public class Playlist : JWLibrary.Parser.Xml.XmlLoader<Playlist> {
		public PlayListModel[] PlayListModels { get; set; }
	}
}
