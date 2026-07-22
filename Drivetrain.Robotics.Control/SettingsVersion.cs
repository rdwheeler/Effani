using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
    public class SettingsVersion
    {
        #region Properties
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public Settings DeviceSettings { get; set; } = new Settings();
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsActive { get; set; }
        #endregion
    }
}
