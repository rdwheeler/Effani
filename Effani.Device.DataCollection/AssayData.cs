using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.DataCollection
{
	public class AssayData
	{
		#region Properties
		[Key]
		public Guid AssayId { get; set; }
		public string AssayJson { get; set; }
		public string AssayName { get; set; }
		public DateTime? UploadedOn { get; set; }
		public DateTime? CreatedOn { get; set; }
		#endregion
	}
}
