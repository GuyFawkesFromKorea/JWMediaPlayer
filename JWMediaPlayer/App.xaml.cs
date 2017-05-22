using JWLibrary.Core.ExecuteLocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JWMediaPlayer {
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup (StartupEventArgs e) {
			#region profile optimized 
			ProfileOptimization.SetProfileRoot(PathInfo.GetApplicationPath());
			ProfileOptimization.StartProfile("Startup.Profile");
			#endregion

			LanguageChange(null);
			base.OnStartup(e);
			if (e.Args.Length > 0) {
				if (File.Exists(e.Args[0])) {
					JWMediaPlayer.Properties.Settings.Default.LastPlayFileName = e.Args[0];
					JWMediaPlayer.Properties.Settings.Default.IsAutoPlay = true;
				}
			}
		}

		public static void LanguageChange (string type) {
			CultureInfo gInfo = null;

			if (type == null) {
				gInfo = new CultureInfo(CultureInfo.CurrentCulture.ToString());
			}
			else {
				gInfo = new CultureInfo(type);
			}

			Thread.CurrentThread.CurrentCulture = gInfo;
			Thread.CurrentThread.CurrentUICulture = gInfo;
			Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() {
				Source = new Uri(string.Format("./Localization/StringResource.{0}.xaml", gInfo.ToString()), UriKind.Relative)
			});
			
		}
	}


}
