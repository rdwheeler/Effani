using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
    public class UvBiopsyOn(DeviceControl device) : Command(device)
    {
        protected override string Run()
        {
            Result = Device.BiopsyUvOn();
            return Result;
        }

        public override string Result { get; protected set; } = $"UV Biopsy off";
    }
}
