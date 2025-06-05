using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.DataCollection
{
	public class AssayData : INotifyPropertyChanged
	{
		Guid _assayId;
		#region Properties
		[Key]
		public Guid AssayId
		{
			get
			{
				return _assayId;
			}
			set
			{
				if (_assayId == value)
					return;
				_assayId = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssayId)));
			}
		}
		string _assayJson;
		public string AssayJson
		{
			get
			{
				return _assayJson;
			}
			set
			{
				if (_assayJson == value)
					return;
				_assayJson = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssayJson)));
			}
		}
		string _assayName;
		public string AssayName
		{
			get
			{
				return _assayName;
			}
			set
			{
				if (_assayName == value)
					return;
				_assayName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssayName)));
			}
		}
		DateTime? _uploadedOn;
		public DateTime? UploadedOn
		{
			get
			{
				return _uploadedOn;
			}
			set
			{
				if (_uploadedOn == value)
					return;
				_uploadedOn = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UploadedOn)));
			}
		}
		DateTime? _createdOn;
		public DateTime? CreatedOn
		{
			get
			{
				return _createdOn;
			}
			set
			{
				if (_createdOn == value)
					return;
				_createdOn = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedOn)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		#endregion
	}
}
