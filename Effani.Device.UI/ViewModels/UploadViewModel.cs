using Effani.Device.Assay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.UI
{
	public class UploadViewModel
	{
		public UploadViewModel(IAssayManagerService assayManagerService)
		{
			_assayManagerService = assayManagerService;
		}
		public BindingList<AssayFile> AssayFiles { get; set; } = new();

		readonly IAssayManagerService _assayManagerService;
	}
}
