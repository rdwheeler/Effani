using Drivetrain.Robotics.Control;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class AbalDeviceControl : DeviceControl
	{
		public AbalDeviceControl()
		{
			Axes = Configuration.DeviceSpecification.Axes.ToDictionary(k => k.Axis, v => v);
		}

		public void MoveAbsolute(Axis axis, float location, float? speed = null)
		{
			if(!Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {axis} in {nameof(MoveAbsolute)}");

			MoveAbsolute(axisSpec, location, speed);
		}

		public void MoveRelative(Axis axis, float distance, float? speed = null)
		{
			if (!Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {axis} in {nameof(MoveRelative)}");

			MoveRelative(axisSpec, distance, speed);
		}
		public void TableGoTo(Coordinate location, float? speed = null)
		{
			TableGoTo(location.X, location.Y, speed);
		}
		public void TableGoTo(float x, float y, float? speed = null)
		{
			var xaxis = Axes[Axis.TableX];
			var yaxis = Axes[Axis.TableY];

			if (!speed.HasValue)
				speed = Math.Min(xaxis.MaxSpeed, yaxis.MaxSpeed);

			if (x < xaxis.Minimum || x > xaxis.Maximum || y < yaxis.Minimum || y > yaxis.Maximum)
				return;

			MoveAbsolute([ new MoveSpec(xaxis, x), new MoveSpec(yaxis, y)], speed.Value);
		}
		public void Home(Axis axis)
		{
			if (!Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {axis} in {nameof(MoveRelative)}");

			Home(axisSpec);
		}
		public string ReadDna()
		{
			return ReadSensor(0, Configuration.SensorReadDwell);
		}
		public string ReadNadh()
		{
			return ReadSensor(1, Configuration.SensorReadDwell);
		}
		public void LaserDnaOn()
		{
			AccOn(3);
		}
		public void LaserDnaOff()
		{
			AccOff(3);
		}
		public void LaserNadhOn()
		{
			AccOn(2);
		}
		public void LaserNadhOff()
		{
			AccOff(2);
		}
		public void EmitFluid(AxisSpec axisSpec, int microLiters)
		{
			if(!(axisSpec is FluidAxis))
				return;

			var distance = CalcFluidTravelMmForMl(microLiters, axisSpec);
			MoveRelative(axisSpec.Axis, distance, Configuration.FluidSyringeSpeed);
			Pause(Configuration.FluidEmissionDwell);
		}
		public void AspirateKoh(int microLiters)
		{
			MoveAbsolute(Axis.AspirateKohZ, (Configuration.AspirateKohActuatorStartPosition + Configuration.AspirateKohDistanceToPlate), Configuration.AspiratePositioningSpeed);
			var distance = CalcAspirateTravelMmForMl(microLiters);
			DoStirringDescent(Axes[Axis.AspirateKohZ], distance);
			MoveAbsolute(Axis.AspirateKohZ, Configuration.AspirateKohActuatorStartPosition);
		}
		public void AspirationPressureOn()
		{
			AccOn(4);
		}
		public void AspirationPressureOff()
		{
			AccOff(4);
		}
		public void AspirationSuctionOn()
		{
			AccOn(5);
		}
		public void AspirationSuctionOff()
		{
			AccOff(5);
		}
		public void AspirateFluid(int microLiters)
		{
			// turn suction on
			AspirationSuctionOn();
			// lower fluid aspirator needle to top of the plate
			MoveAbsolute(
				Axis.AspirateFluidZ,
				(Configuration.AspirateFluidActuatorStartPosition + Configuration.AspirateFluidDistanceToPlate),
				Configuration.AspiratePositioningSpeed);
			// lower aspirator needle calculated distance per ml to extract
			var distance = CalcAspirateTravelMmForMl(microLiters);
			MoveRelative(Axis.AspirateFluidZ, distance, Configuration.AspirateStirSpeed);
			// wait for extraction to complete
			Pause(Configuration.AspirationDwell);
			// in case the sample got in the way of removing as much liquid as planned, raise with suction off and lower with suction on again
			AspirationSuctionOff();
			MoveRelative(Axis.AspirateFluidZ, -3, Configuration.AspirateStirSpeed);
			AspirationSuctionOn();
			MoveRelative(Axis.AspirateFluidZ, 3, Configuration.AspirateStirSpeed);
			// turn suction off
			AspirationSuctionOff();
			// raise aspiration needle to sample ejection position
			MoveRelative(Axis.AspirateFluidZ, Configuration.AspirationPressureAscension * -1);
			// turn pressure on
			AspirationPressureOn();
			// wait for sample to be ejected
			Pause(Configuration.AspirationPressureDwell);
			// turn pressure off
			AspirationPressureOff();
			// raise aspiration needle to its starting location
			MoveAbsolute(Axis.AspirateFluidZ, Configuration.AspirateFluidActuatorStartPosition);
		}

		private float CalcFluidTravelMmForMl(int microLiters, AxisSpec axisSpec)
		{
			var microLitersPerMm = axisSpec.Axis switch
			{
				Axis.Basal1 => Configuration.BasalTravelPerMicroLiter,
				Axis.Nadh1 => Configuration.NadhTravelPerMicroLiter,
				Axis.Koh1 => Configuration.KohTravelPerMicroLiter,
				Axis.DnaReagent1 => Configuration.DnaTravelPerMicroLiter,
				_ => throw new ApplicationException($"Unknown axis {axisSpec.Axis} in {nameof(CalcFluidTravelMmForMl)}")
			};
			var ret = microLitersPerMm * microLiters;
			return ret;
		}
		private float CalcAspirateTravelMmForMl(int microLiters)
		{
			var ret = (Configuration.AspirateTravelPerMicroLiter * microLiters);
			return ret;
		}
		private void DoStirringDescent(AxisSpec axis, float distance)
		{
			MoveRelative(axis, distance, Configuration.AspirateStirSpeed);
			Pause(Configuration.AspirationDwell);
			//var repetitions = 1;
			//var descentFactor = distance / (repetitions * 4);
			//var proximityFactor = (Configuration.WellWidth / 2) * 0.9f;
			//MoveRelative(Axis.TableY, proximityFactor, Configuration.AspirateStirSpeed); // move near well edge
			//while(repetitions > 0)
			//{
			//	//arc to right edge and down
			//	var rightMultiAxis = new List<MoveSpec>();
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor));
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
			//	rightMultiAxis.Add(new MoveSpec(axis, descentFactor));
			//	MoveRelativeClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to bottom edge and down
			//	var bottomMultiAxis = new List<MoveSpec>();
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
			//	bottomMultiAxis.Add(new MoveSpec(axis, descentFactor));
			//	MoveRelativeClockwiseArc(bottomMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to left edge and down
			//	var leftMultiAxis = new List<MoveSpec>();
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor));
			//	leftMultiAxis.Add(new MoveSpec(axis, descentFactor));
			//	MoveRelativeClockwiseArc(leftMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to top edge and down
			//	var topMultiAxis = new List<MoveSpec>();
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor));
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor));
			//	topMultiAxis.Add(new MoveSpec(axis, descentFactor));
			//	MoveRelativeClockwiseArc(topMultiAxis, Configuration.AspirateStirSpeed);
			//	repetitions--;
			//}
			////arc back
			//var backMultiAxis = new List<MoveSpec>();
			//backMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
			//backMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
			//MoveRelativeCounterClockwiseArc(backMultiAxis, Configuration.AspirateStirSpeed);
		}

		public Dictionary<Axis, AxisSpec> Axes { get; private set; }
		public List<AxisSpec> FluidAxes => Axes.Values.Where(x => x is FluidAxis).ToList();
		public List<AxisSpec> ZAxes => Axes.Values.Where(x => x is BiopsyZAxis).ToList();
	}
}
