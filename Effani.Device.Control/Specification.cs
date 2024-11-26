using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Control
{
	public class Specification
	{
		public Specification() { }
		public DeviceSpecification GetDeviceSpecification(string name)
		{
			var ret = new DeviceSpecification("Effani Device");
			ret.Axes.Add(new TableAxis("X", 0, 0, 314, EndstopLocation.LowEnd, direction: Direction.Backwards));
			ret.Axes.Add(new TableAxis("Y", 0, 1, 399, EndstopLocation.HighEnd, direction: Direction.Backwards));
			ret.Axes.AddRange(GetBioposyAxes());
			ret.Axes.AddRange(GetAspirateAxes());
			ret.Axes.Add(new FluidAxis(1, "DNA 2", "C", 1, 3));
			ret.Axes.Add(new FluidAxis(2, "DNA 1", "D", 1, 4));
			ret.Axes.Add(new FluidAxis(3, "KOH 2", "'a", 2, 0));
			ret.Axes.Add(new FluidAxis(4, "KOH 1", "'b", 2, 1));
			ret.Axes.Add(new FluidAxis(5, "NADH 2", "'c", 2, 3));
			ret.Axes.Add(new FluidAxis(6, "NADH 1", "'d", 2, 4));
			ret.Axes.Add(new FluidAxis(7, "BASAL 2", "'e", 3, 0));
			ret.Axes.Add(new FluidAxis(8, "BASAL 1", "'f", 3, 1));

			return ret;
		}

		private List<AxisSpec> GetBioposyAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new BiopsyAxis("Z", 0, 2));
			ret.Add(new ShortZAxis("Biopsy", "U", 0, 3));

			return ret;
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new BiopsyAxis("V", 0, 4));
			ret.Add(new ShortZAxis("Aspirate", "W", 1, 0));
			ret.Add(new BiopsyAxis("A", 1, 1));
			ret.Add(new ShortZAxis("Aspirate", "B", 1, 2));

			return ret;
		}
	}

	public class TableAxis : AxisSpec
	{
		public TableAxis(string orientation, int board, int port, int maximum, EndstopLocation endstopLocation, Direction direction = Direction.Forwards)
			: base ($"Table {orientation}", orientation, board, port)
		{ 
			Maximum = maximum;
			StepsPerMillimeter = 400;
			MaxSpeed = 30000;
			Driver = new DriverSpec(Label)
			{
				Direction = direction,
				Endstop = new EndstopSpec($"{Label} Endstop")
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = endstopLocation,
					FirstHomingSpeed = 1800
				}
			};
		}
	}
	public class ShortZAxis : AxisSpec
	{
		public ShortZAxis(string labelPrefix, string designation, int board, int port)
			: base($"{labelPrefix} Z", designation, board, port)
		{
			Maximum = 43;
			StepsPerMillimeter = 533.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 600
				}
			};
		}
	}
	public class BiopsyAxis : AxisSpec
	{
		public BiopsyAxis(string designation, int board, int port)
			: base($"Biopsy", designation, board, port)
		{
			Maximum = 150;
			StepsPerMillimeter = 533.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 10
				}
			};
		}
	}
	public class AspirateAxis : AxisSpec
	{
		public AspirateAxis(int labelNum, string designation, int board, int port)
			: base($"Aspirate {labelNum}", designation, board, port)
		{
			Maximum = 150;
			StepsPerMillimeter = 2133.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 10
				}
			};
		}
	}
	public class FluidAxis : AxisSpec
	{
		public FluidAxis(int labelNum, string fuildName, string designation, int board, int port)
			: base($"Fluid {labelNum} - {fuildName}", designation, board, port)
		{
			Maximum = 150;
			StepsPerMillimeter = 533.33f;
			MaxSpeed = 6000;
			Driver = new DriverSpec(Label)
			{
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.High,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 10
				}
			};
		}
	}
	public class DeviceSpecification : Spec
	{
		public DeviceSpecification(string label) : base(label)
		{
			DeviceName = label;
		}

		public string DeviceName { get; set; }
		public List<AxisSpec> Axes { get; set; }
	}

	public class AxisSpec : Spec
	{
		public AxisSpec(string label, string designation, int board, int port)
			: base (label)
		{
			DriverPort = $"{board}.{port}";
			Designation = designation;
		}
		public string Designation { get; set; }
		public string DriverPort { get; set; }
		public int Minimum { get; set; }
		public int Maximum { get; set; }
		public float StepsPerMillimeter { get; set; }

		public int MaxSpeed { get; set; } = 500;

		public int Acceleration { get; set; } = 500;
		public DriverSpec Driver { get; set; }
	}
	public class DriverSpec : Spec
	{
		public DriverSpec(string label) : base(label)
		{

		}

		public int Current { get; set; } = 800; // milli amps

		public Direction Direction { get; set; } = Direction.Forwards;
		public EndstopSpec Endstop { get; set; }
	}
	public class EndstopSpec : Spec
	{
		public EndstopSpec(string driverLabel)
			: base($"{driverLabel} Endstop")
		{
		}
		public string SensorPort { get; set; }
		public PinSignal Signal { get; set; }
		public EndstopLocation Location { get; set; }

		public int FirstHomingSpeed { get; set; } = 600;

		public int SecondHomingSpeed { get; set; } = 300;
	}

	public class Spec
	{
		public Spec(string label)
		{
			Label = label;
		}
		public string Label { get; set; }
	}

	public enum Direction
	{
		Forwards,
		Backwards
	}
	public enum PinSignal
	{
		High,
		Low
	}
	public enum EndstopLocation
	{
		HighEnd,
		LowEnd
	}
}
