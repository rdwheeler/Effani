using Effani.Device.DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using Effani.Device.Services;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Xpf;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.Control;
using System.Collections.ObjectModel;
using DevExpress.Xpf.WindowsUI;
using DevExpress.Xpf.Editors;

namespace Effani.Device.UI
{
	public partial class UtilitiesViewModel : ViewModelBase, IDisposable
	{
		#region Constructors
		public UtilitiesViewModel(ISettingsService settingsService, ProcessEngine processEngine, EffaniDeviceControl device, ObservableCollection<string> consoleMessages)
		{
			ConsoleMessages = consoleMessages;
			_device = device;
			_processEngine = processEngine;
			_settingsService = settingsService;

			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
			Sensors = new ObservableCollection<SensorSpec>(_device.Configuration.DeviceSpecification.Sensors);
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
		}
		#endregion

		#region Properties
		public ObservableCollection<string> ConsoleMessages { get; init; }
		/// <summary>The version of the running software (major.minor.build).</summary>
		public string SoftwareVersion { get; } = typeof(UtilitiesViewModel).Assembly.GetName().Version?.ToString(3) ?? "unknown";
		/// <summary>The outcome of the last check for a new software version, or the progress of a check under way.</summary>
		public string UpdateStatus
		{
			get => _updateStatus;
			private set
			{
				_updateStatus = value;
				RaisePropertyChanged(nameof(UpdateStatus));
			}
		}
		/// <summary>True when an assay is running; settings the device control uses are locked while it is.</summary>
		public bool IsAssayRunning { get; init; }
		public Well CurrentWell
		{
			get
			{
				if (_currentWell == null && _device is not null)
					_currentWell = _device.Configuration.DeviceSpecification.Plates.FirstOrDefault()?.Wells.FirstOrDefault();
				return _currentWell;
			}
			set
			{
				_currentWell = value;
				RaisePropertyChanged(nameof(CurrentWell));
			}
		}
		public int EmitFluidVolume
		{
			get => _emitFluidVolume;
			set
			{
				if (_emitFluidVolume == value)
				{
					return;
				}

				_emitFluidVolume = value;
				RaisePropertyChanged(nameof(EmitFluidVolume));
			}
		}
		public string Password
		{
			get
			{
				return _settingsService.Settings.EffaniPassword;
			}
			set
			{
				if (_settingsService.Settings.EffaniPassword == value)
					return;
				_settingsService.Settings.EffaniPassword = value;
				RaisePropertyChanged(nameof(Password));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public string SendCommandText
		{
			get => _sendCommandText;
			set
			{
				if (_sendCommandText == value)
				{
					return;
				}

				_sendCommandText = value;
				RaisePropertyChanged(nameof(SendCommandText));
			}
		}
		public ObservableCollection<string> SettingsServiceActivity => _settingsService.Activity;
		public string Username
		{
			get
			{
				return _settingsService.Settings.EffaniUsername;
			}
			set
			{
				if (_settingsService.Settings.EffaniUsername == value)
					return;
				_settingsService.Settings.EffaniUsername = value;
				RaisePropertyChanged(nameof(Username));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public async void ActivateSettingsVersion(SettingsVersion settingsVersion)
		{
			if (settingsVersion == null)
				return;

			_settingsService.ActivateSettingsVersion(settingsVersion.Id);

			RaisePropertyChanged(nameof(Settings));
			RaisePropertyChanged(nameof(ActiveSettingsVersion));
		}
		[Command]
		public async void DeleteSettings(object parameter)
		{
			if(parameter is SettingsVersion settingsVersion)
			{
				SettingsVersions.Remove(settingsVersion);
				_settingsService.DeleteSettings(settingsVersion.Id);
			}
		}
		[Command]
		public async void SaveActiveSettingsAsNewVersion()
		{
			if(DialogService.ShowDialog(MessageBoxButton.OKCancel, "Save Settings As...", this) == MessageBoxResult.Cancel)
				return;

			_settingsService.SaveAcitveSettingsAsNew(NewSettingsDescription);
			NewSettingsDescription = null;

			RaisePropertyChanged(nameof(Settings));
			RaisePropertyChanged(nameof(ActiveSettingsVersion));
        }
        [Command]
        public async void ResetActiveSettings()
        {
            if (DevExpress.Xpf.Core.DXMessageBox.Show("Reset all settings to their default values? Any changes to the active settings will be lost.",
                    "Reset Settings", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;

            _settingsService.ResetSettings();

            RaisePropertyChanged(nameof(Settings));
            RaisePropertyChanged(nameof(ActiveSettingsVersion));
        }

        public string NewSettingsDescription { get; set; }
		public ObservableCollection<SettingsVersion> SelectedSettingsVersions { get; set; } = new();
		public ObservableCollection<SettingsVersion> SettingsVersions => _settingsService.SettingsVersions;
		public Settings Settings => _settingsService.Settings;
		public SettingsVersion ActiveSettingsVersion => _settingsService.ActiveSettingsVersion;

		public bool UsernameAndPasswordComplete => !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password);
		public ObservableCollection<AxisSpec> Axes { get; set; }
		public ObservableCollection<SensorSpec> Sensors { get; set; }
		protected IDialogService DialogService { get { return GetService<IDialogService>(); } }
		#endregion

		#region Fields
		DeviceConfiguration _config => _device.Configuration;
		Well _currentWell;
		readonly EffaniDeviceControl _device;
		int _emitFluidVolume;
		readonly ProcessEngine _processEngine;
		string _sendCommandText;
		string _updateStatus = "";
		bool _checkingForUpdates;
		readonly UpdateChecker _updateChecker = new();
		readonly ISettingsService _settingsService;
		#endregion
	}
}
