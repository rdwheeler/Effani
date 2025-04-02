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
		public void EmitFluid(AxisSpec axisSpec, int microLiters, int pauseDwell)
		{
			if(!(axisSpec is FluidAxis))
				return;

			var distance = CalcFluidTravelMmForMl(microLiters);
			MoveRelative(axisSpec.Axis, distance, Configuration.FluidSyringeSpeed);
			Pause(pauseDwell);
		}
		public void RemoveKoh(int microLiters)
		{
			MoveAbsolute(Axis.AspirateKohZ, (Configuration.AspirateKohActuatorStartPosition + Configuration.AspirateKohDistanceToPlate), Configuration.AspiratePositioningSpeed);
			var distance = CalcAspirateTravelMmForMl(microLiters);
			DoStir(Axes[Axis.AspirateKohZ], distance);
			MoveAbsolute(Axis.AspirateKohZ, Configuration.AspirateKohActuatorStartPosition);
		}
		public void RemoveFluid(int microLiters)
		{
			MoveAbsolute(Axis.AspirateFluidZ, (Configuration.AspirateFluidActuatorStartPosition + Configuration.AspirateFluidDistanceToPlate), Configuration.AspiratePositioningSpeed);
			var distance = CalcAspirateTravelMmForMl(microLiters);
			DoStir(Axes[Axis.AspirateFluidZ], distance);
			MoveAbsolute(Axis.AspirateFluidZ, Configuration.AspirateFluidActuatorStartPosition);
		}

		private float CalcFluidTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.FluidTravelPerMicroLiter;
			var ret = microLitersPerMm * microLiters;
			return ret;
		}
		private float CalcAspirateTravelMmForMl(int microLiters)
		{
			var ret = (Configuration.AspirateTravelPerMicroLiter * microLiters) * -1;
			return ret;
		}
		private void DoStir(AxisSpec axis, float distance)
		{
			var repetitions = 1;
			var descentFactor = distance / (repetitions * 4);
			var proximityFactor = (Configuration.WellWidth / 2) * 0.9f;
			MoveRelative(Axis.TableY, proximityFactor, Configuration.AspirateStirSpeed); // move near well edge
			while(repetitions > 0)
			{
				//arc to right edge and down
				var rightMultiAxis = new List<MoveSpec>();
				rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor));
				rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
				rightMultiAxis.Add(new MoveSpec(axis, descentFactor));
				MoveRelativeClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
				//arc to bottom edge and down
				var bottomMultiAxis = new List<MoveSpec>();
				bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
				bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
				bottomMultiAxis.Add(new MoveSpec(axis, descentFactor));
				MoveRelativeClockwiseArc(bottomMultiAxis, Configuration.AspirateStirSpeed);
				//arc to left edge and down
				var leftMultiAxis = new List<MoveSpec>();
				leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
				leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor));
				leftMultiAxis.Add(new MoveSpec(axis, descentFactor));
				MoveRelativeClockwiseArc(leftMultiAxis, Configuration.AspirateStirSpeed);
				//arc to top edge and down
				var topMultiAxis = new List<MoveSpec>();
				topMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor));
				topMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor));
				topMultiAxis.Add(new MoveSpec(axis, descentFactor));
				MoveRelativeClockwiseArc(topMultiAxis, Configuration.AspirateStirSpeed);
				repetitions--;
			}
			//arc back
			var backMultiAxis = new List<MoveSpec>();
			backMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], proximityFactor * -1));
			backMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], proximityFactor * -1));
			MoveRelativeCounterClockwiseArc(backMultiAxis, Configuration.AspirateStirSpeed);
		}

		public Dictionary<Axis, AxisSpec> Axes { get; private set; }
		public List<AxisSpec> FluidAxes => Axes.Values.Where(x => x is FluidAxis).ToList();
		public List<AxisSpec> ZAxes => Axes.Values.Where(x => x is BiopsyZAxis).ToList();
	}
}
