using Drivetrain.Robotics.Control;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Effani.Device.Services
{
	public class SettingsService : ISettingsService
	{
		#region Constructors
		public SettingsService(EfContext efContext)
		{
			_efContext = efContext;
			ActiveSettingsVersion = _efContext.SettingsVersions.FirstOrDefault(x => x.IsActive);
			if (ActiveSettingsVersion == null)
			{
				ActiveSettingsVersion = _efContext.SettingsVersions.Add(new SettingsVersion() { Description = "Default Settings", IsActive = true }).Entity;
				_efContext.SaveChanges();
			}
			ActiveSettingsVersion.DeviceSettings.PropertyChanged += DeviceSettings_PropertyChanged;
			SettingsVersions = new(_efContext.SettingsVersions.ToList());
		}
		#endregion

		#region Public methods
		public void ActivateSettingsVersion(string id)
		{
			ActivateSettingsVersion(_efContext.SettingsVersions.Find(id));
		}
		public void SaveAcitveSettingsAsNew(string description)
		{
			var newSettings = _efContext.SettingsVersions.Add(new SettingsVersion() { Description = description }).Entity;
			newSettings.DeviceSettings = ActiveSettingsVersion.DeviceSettings;
			SettingsVersions.Add(newSettings);
			_efContext.SaveChanges();
			ActivateSettingsVersion(newSettings);
		}
		public void DeleteSettings(string id)
		{
			var settings = _efContext.SettingsVersions.Find(id);
			if(settings != null)
			{
				_efContext.SettingsVersions.Remove(settings);
				SettingsVersions.Remove(settings);
			}
			_efContext.SaveChanges();
		}
		public async Task UploadSettings()
		{
			Activity.Clear();

			if (String.IsNullOrWhiteSpace(Settings.EffaniUsername) ||
				String.IsNullOrWhiteSpace(Settings.EffaniPassword))
			{
				Activity.Add($"Missing Effani username or password.");
				return;
			}

			var toUpload = _efContext.SettingsVersions;

			foreach (var a in toUpload)
			{
				Activity.Add($"Upoading settings from {a.CreatedOn}.");
				try
				{
					var result = await Settings.EffaniApiUrl
						.AppendPathSegment("Document")
						.AppendPathSegment("Upload")
						.AppendQueryParam("documentTypeName", "EffaniSettings")
						.WithBasicAuth(Settings.EffaniUsername, Settings.EffaniPassword)
						.PostJsonAsync(JsonSerializer.Serialize(ActiveSettingsVersion), cancellationToken: CancellationToken.None);
				}
				catch (FlurlHttpException ex)
				{
					Activity.Add($"* Failed: {ex.Call.Exception}");
					continue;
				}
			}
		}
		#endregion

		#region Private methods
		private void ActivateSettingsVersion(SettingsVersion nextSettings)
		{
			if (nextSettings == null)
				throw new ArgumentNullException(nameof(nextSettings));

			ActiveSettingsVersion.DeviceSettings.PropertyChanged -= DeviceSettings_PropertyChanged;
			ActiveSettingsVersion.IsActive = false;
			nextSettings.IsActive = true;
			nextSettings.DeviceSettings.PropertyChanged += DeviceSettings_PropertyChanged;
			ActiveSettingsVersion = nextSettings;
			_efContext.SaveChanges();
		}
		private void DeviceSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			_efContext.SaveChanges();
		}
		#endregion

		#region Properties
		public ObservableCollection<string> Activity { get; init; } = new();
		public static string DataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Effani");
		public Settings Settings => ActiveSettingsVersion.DeviceSettings;
		public ObservableCollection<SettingsVersion> SettingsVersions { get; init; }
		#endregion

		#region Fields
		public SettingsVersion ActiveSettingsVersion { get; private set; }
		readonly EfContext _efContext;
		#endregion
	}

	public interface ISettingsService
	{
		void DeleteSettings(string id);
		#region Public methods
		void ActivateSettingsVersion(string id);
		void SaveAcitveSettingsAsNew(string description);
		Task UploadSettings();
		#endregion

		#region Properties
		ObservableCollection<string> Activity { get; }
		static string DataFolder { get; }
		Settings Settings { get; }
		SettingsVersion ActiveSettingsVersion { get; }
		ObservableCollection<SettingsVersion> SettingsVersions { get; }
		#endregion
	}
}
