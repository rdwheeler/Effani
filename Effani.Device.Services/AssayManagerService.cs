using Effani.Device.DataCollection;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Flurl.Http;
using Flurl;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Effani.Device.Services
{
	public class AssayManagerService : IAssayManagerService, INotifyPropertyChanged
	{
		#region Constructors
		public AssayManagerService(EfContext efContext, ISettingsService settingsService)
		{
			_settingsService = settingsService;
			_efContext = efContext;
			foreach(var a in _efContext.AssayData)
				AssayData.Add(a);
		}
		#endregion

		#region Public methods
		public void DeleteAssay(Guid id)
		{
			var item = _efContext.AssayData.Find(id);
			_efContext.AssayData.Remove(item);
			_efContext.SaveChanges();
		}
		public async Task<string> SaveAssay(Assay assay)
		{
			var assayData = new AssayData()
			{
				AssayJson = JsonSerializer.Serialize(assay),
				AssayName = GetAssayName(assay),
				AssayId = assay.Id,
				CreatedOn = assay.CreatedOn
			};
			AssayData.Add(assayData);
			_efContext.AssayData.Add(assayData);
			await _efContext.SaveChangesAsync();
			if (_settingsService.SaveAssayFiles)
				WriteAssayFile(assay);

			return assayData.AssayName;
		}
		public async Task UploadAssays()
		{
			await SyncAll();
		}
		#endregion

		#region Private methods
		private string GetAssayName(Assay assay)
		{
			return $"Assay Results {assay.AssayDescription} {GetShortNow(assay.CreatedOn)}";
		}
		private string GetShortNow(DateTime time)
		{
			return $"{time.ToString("yyyy-dd-M--HH-mm-ss")}";
		}
		private async Task SyncAll()
		{
			Activity.Clear();

			if (String.IsNullOrWhiteSpace(_settingsService.EffaniUsername) ||
				String.IsNullOrWhiteSpace(_settingsService.EffaniPassword))
			{
				Activity.Add($"Missing Effani username or password.");
				return;
			}

			var toUpload = _efContext.AssayData;

			foreach (var a in toUpload)
			{
				Activity.Add($"Upoading {a.AssayName} ({a.AssayId})");
				try
				{
					var result = await _settingsService.AssayUploadUrl
						.AppendPathSegment("EffaniAssay")
						.AppendPathSegment("Upload")
						.WithBasicAuth(_settingsService.EffaniUsername, _settingsService.EffaniPassword)
						.PostJsonAsync(a.AssayJson, cancellationToken: CancellationToken.None);
					if(result.StatusCode == 200)
					{
						a.UploadedOn = DateTime.Now;
						await _efContext.SaveChangesAsync();
					}
				}
				catch (FlurlHttpException ex)
				{
					Activity.Add($"* Failed: {ex.Call.Exception}");
					continue;
				}
			}
		}
		private string WriteAssayFile(Assay assay)
		{
			var results = new StringBuilder();
			results.AppendLine(assay.AssayDescription);
			results.AppendLine($"Low NADH:{assay.Low_NADH}");
			results.AppendLine($"High NADH:{assay.High_NADH}");
			results.AppendLine($"1 µg DNA:{assay.DNA_1mcg}");
			results.AppendLine($"0.25 µg DNA:{assay.DNA_025mcg}");
			results.AppendLine($"0.0625 µg DNA:{assay.DNA_0625mcg}");
			results.AppendLine($"0 µg DNA:{assay.DNA_0mcg}");
			foreach (var well in assay.TestWells.Skip(assay.StandardsWellCount).Where(w => !w.Canceled))
				results.AppendLine(
					$"{well.SampleId},{well.Label},DNA:{well.DnaValue},{String.Join(',', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
			var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Effani");
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			var fullname = GetAssayName(assay);
			var filePath = Path.Combine(
				folder,
				$"{fullname}.{assayFileExtension}");
			File.WriteAllText(filePath, results.ToString());
			return fullname;
		}
		#endregion

		#region Properties
		public BindingList<string> Activity { get; set; } = new();
		public BindingList<AssayData> AssayData { get; set; } = new();
		#endregion

		#region Events
		public event PropertyChangedEventHandler? PropertyChanged;
		#endregion

		#region Fields
		readonly EfContext _efContext;
		readonly ISettingsService _settingsService;
		string _uploading;

		const string assayFileExtension = "effani";
		#endregion
	}

	public interface IAssayManagerService : INotifyPropertyChanged
	{
		#region Public methods
		void DeleteAssay(Guid id);
		Task<string> SaveAssay(Assay assay);
		Task UploadAssays();
		#endregion

		#region Properties
		BindingList<string> Activity { get; set; }
		BindingList<AssayData> AssayData { get; set; }
		#endregion
	}
}
