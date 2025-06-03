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

namespace Effani.Device.UI
{
	public class UploadViewModel : ViewModelBase, IDisposable
	{
		public UploadViewModel(IAssayManagerService assayManagerService, ISettingsService settingsService)
		{
			_settingsService = settingsService;
			_assayManagerService = assayManagerService;
		}

		public void Dispose()
		{
		}
		[Command]
		public void ValidateRowDeletion(ValidateRowDeletionArgs args)
		{
			_assayManagerService.DeleteAssay(((AssayData)args.Items.First()).AssayId);
		}

		public BindingList<AssayData> AssayData => _assayManagerService.AssayData;
		public BindingList<string> Activity => _assayManagerService.Activity;
		public string Username
		{
			get
			{
				return _settingsService.EffaniUsername;
			}
			set
			{
				if (_settingsService.EffaniUsername == value)
					return;
				_settingsService.EffaniUsername = value;
				RaisePropertyChanged(nameof(Username));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public string Password
		{
			get
			{
				return _settingsService.EffaniPassword;
			}
			set
			{
				if (_settingsService.EffaniPassword == value)
					return;
				_settingsService.EffaniPassword = value;
				RaisePropertyChanged(nameof(Password));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public bool UsernameAndPasswordComplete => !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password);

		readonly IAssayManagerService _assayManagerService;
		readonly ISettingsService _settingsService;
	}
}
