using System.ComponentModel;
using System.Text;

namespace Effani.Device.Assay
{
	public class AssayManagerService : IAssayManagerService
	{
		public AssayManagerService()
		{
			dataFolder = GetDataFolder();
			if (!Directory.Exists(dataFolder))
				Directory.CreateDirectory(dataFolder);
			foreach (var file in Directory.GetFiles(dataFolder, $"*.{assayFileExtension}"))
			{
				var assayFile = new AssayFile { FilePath = file };
				AssayFiles.Add(assayFile);
			}
		}

		public void SaveAssay(Assay assay)
		{
			var results = new StringBuilder();
			results.AppendLine(assay.AssayDescription);
			results.AppendLine($"Low NADH:{assay.LowNadh}");
			results.AppendLine($"High NADH:{assay.HighNadh}");
			results.AppendLine($"1 µg DNA:{assay.Microgram1Dna}");
			results.AppendLine($"0.25 µg DNA:{assay.MicrogramDna25Dna}");
			results.AppendLine($"0.0625 µg DNA:{assay.MicrogramDna0625Dna}");
			results.AppendLine($"0 µg DNA:{assay.Microgram0Dna}");
			foreach (var well in assay.TestWells.Skip(assay.StandardsWellCount).Where(w => !w.Canceled))
				results.AppendLine(
					$"{well.SampleId},{well.Label},DNA:{well.DnaValue},{String.Join(',', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
			var filePath = Path.Combine(GetDataFolder(), $"Assay Results {assay.AssayDescription} {GetShortNow(assay.CreatedOn)}.{assayFileExtension}");
			File.WriteAllText(filePath, results.ToString());
			AssayFiles.Add(new AssayFile { FilePath = filePath });
		}
		public string GetDataFolder()
		{
			return Path.Combine(Environment.SpecialFolder.LocalApplicationData.ToString(), "Effani");
		}
		private string GetShortNow(DateTime time)
		{
			return $"{time.ToString("yyyy-dd-M--HH-mm-ss")}";
		}

		public BindingList<AssayFile> AssayFiles { get; set; } = new();

		string dataFolder;
		const string assayFileExtension = "effani";
	}

	public interface IAssayManagerService
	{
		void SaveAssay(Assay assay);
	}
}
