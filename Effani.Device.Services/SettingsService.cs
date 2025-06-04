using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Services
{
	public class SettingsService : ISettingsService
	{
		public SettingsService()
		{
			AssayUploadUrl = ConfigurationManager.AppSettings["EffaniApiUrl"];
		}
		public static string DataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Effani");

		public string AssayUploadUrl { get; set; } = "https://www.effani.com";
		public string EffaniUsername { get; set; }
		public string EffaniPassword { get; set; }

		public bool SaveAssayFiles { get; set; } = true;
	}
	public interface ISettingsService
	{
		bool SaveAssayFiles { get; set; }
		string EffaniPassword { get; set; }
		string EffaniUsername { get; set; }
		string AssayUploadUrl { get; }
		static string DataFolder { get; }
	}
}
