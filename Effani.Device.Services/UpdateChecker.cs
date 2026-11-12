using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Effani.Device.Services
{
	/// <summary>The outcome of <see cref="UpdateChecker.CheckAsync"/>.</summary>
	/// <param name="Succeeded">False when the check could not be completed (see <paramref name="Message"/>).</param>
	/// <param name="IsUpdateAvailable">True when the published version is newer than the running one.</param>
	/// <param name="LatestVersion">The newest version published in the update manifest, if it could be read.</param>
	/// <param name="IsCritical">True when the newest version is flagged as a critical update.</param>
	/// <param name="Message">A sentence describing the outcome, suitable for showing to the user.</param>
	public record UpdateCheckResult(bool Succeeded, bool IsUpdateAvailable, Version? LatestVersion, bool IsCritical, string Message);

	/// <summary>
	/// Checks for new software versions published with Advanced Installer. The installer puts the Advanced Installer updater
	/// (<c>Effani Updater.exe</c>) and its configuration (<c>Effani Updater.ini</c>) next to the program. The configuration holds the
	/// address of the update manifest (the <c>EffaniUpdates.txt</c> built by the updates project), which this class downloads and
	/// compares with the running version. Installing an update is left to the updater itself (see <see cref="StartUpdater"/>).
	/// </summary>
	public class UpdateChecker
	{
		/// <summary>True when this copy of the program was installed with the updater, which is needed to check for and install updates.</summary>
		public static bool IsUpdaterInstalled => File.Exists(UpdaterIniPath) && File.Exists(UpdaterExePath);
		public static string UpdaterExePath => Path.Combine(AppContext.BaseDirectory, "Effani Updater.exe");
		public static string UpdaterIniPath => Path.Combine(AppContext.BaseDirectory, "Effani Updater.ini");

		/// <summary>Downloads the update manifest and reports whether it publishes a version newer than <paramref name="current"/>.</summary>
		public async Task<UpdateCheckResult> CheckAsync(Version current, CancellationToken cancellationToken = default)
		{
			try
			{
				var manifestUrl = ReadManifestUrl(UpdaterIniPath);
				if (manifestUrl == null)
					return Failed("Update checking is only available in an installed copy of the program.");

				string manifest;
				using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeout.CancelAfter(TimeSpan.FromSeconds(20));
					manifest = await _http.GetStringAsync(manifestUrl, timeout.Token);
				}

				var updates = ParseManifest(manifest);
				if (updates.Count == 0)
					return Failed("The update information could not be read.");

				var latest = updates.OrderByDescending(u => u.Version).First();
				var available = Normalize(latest.Version) > Normalize(current);
				var message = available
					? $"Version {Format(latest.Version)} is available (this is {Format(current)})."
					: $"This is the latest version ({Format(current)}).";
				return new UpdateCheckResult(true, available, latest.Version, latest.IsCritical, message);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				return Failed("Timed out contacting the update server.");
			}
			catch (HttpRequestException ex)
			{
				return Failed($"Could not contact the update server: {ex.Message}");
			}
			catch (Exception ex)
			{
				return Failed($"Could not check for updates: {ex.Message}");
			}
		}
		/// <summary>Starts the Advanced Installer updater, which checks again and offers to download and install the new version.</summary>
		public static void StartUpdater()
		{
			Process.Start(new ProcessStartInfo(UpdaterExePath, "/checknow") { UseShellExecute = true });
		}

		/// <summary>Reads the manifest address (the URL key of the General section) from the updater's configuration file.</summary>
		internal static string? ReadManifestUrl(string iniPath)
		{
			if (!File.Exists(iniPath))
				return null;

			var inGeneral = false;
			foreach (var raw in File.ReadLines(iniPath))
			{
				var line = raw.Trim();
				if (line.StartsWith('['))
				{
					inGeneral = line.Equals("[General]", StringComparison.OrdinalIgnoreCase);
					continue;
				}
				if (!inGeneral)
					continue;
				var separator = line.IndexOf('=');
				if (separator > 0 && line[..separator].Trim().Equals("URL", StringComparison.OrdinalIgnoreCase))
					return ToDirectLink(line[(separator + 1)..].Trim());
			}
			return null;
		}
		/// <summary>A Dropbox share link ending in dl=0 opens a web page; dl=1 downloads the file itself.</summary>
		internal static string ToDirectLink(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Host.EndsWith("dropbox.com", StringComparison.OrdinalIgnoreCase))
				return url.Replace("dl=0", "dl=1", StringComparison.OrdinalIgnoreCase);
			return url;
		}
		/// <summary>Parses an Advanced Installer update manifest (an ini file with one [Update] section per published version).</summary>
		internal static List<PublishedUpdate> ParseManifest(string manifest)
		{
			var ret = new List<PublishedUpdate>();
			Dictionary<string, string>? section = null;
			var sections = new List<Dictionary<string, string>>();
			foreach (var raw in manifest.Split('\n'))
			{
				var line = raw.Trim().TrimStart('﻿');
				if (line.Length == 0 || line.StartsWith(';'))
					continue;
				if (line.StartsWith('['))
				{
					section = line.StartsWith("[Update", StringComparison.OrdinalIgnoreCase) ? new(StringComparer.OrdinalIgnoreCase) : null;
					if (section != null)
						sections.Add(section);
					continue;
				}
				var separator = line.IndexOf('=');
				if (section != null && separator > 0)
					section[line[..separator].Trim()] = line[(separator + 1)..].Trim();
			}

			foreach (var s in sections)
			{
				if (!(s.TryGetValue("Version", out var text) || s.TryGetValue("ProductVersion", out text)) || !Version.TryParse(text, out var version))
					continue;
				var critical = s.TryGetValue("Flags", out var flags) && flags.Split('|').Any(f => f.Trim().Equals("Critical", StringComparison.OrdinalIgnoreCase));
				ret.Add(new PublishedUpdate(version, critical));
			}
			return ret;
		}
		internal record PublishedUpdate(Version Version, bool IsCritical);

		/// <summary>Gives a version all four parts so that 4.0.1 and 4.0.1.0 compare as equal.</summary>
		private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0), Math.Max(v.Revision, 0));
		private static string Format(Version v) => Normalize(v).Revision == 0 ? Normalize(v).ToString(3) : Normalize(v).ToString();
		private static UpdateCheckResult Failed(string message) => new(false, false, null, false, message);

		private static readonly HttpClient _http = new();
	}
}
