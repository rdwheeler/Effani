using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Effani.Device.Control
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
			Pause(Configuration.SensorReadDwell);
			return ReadSensor(0);
		}
		public string ReadNadh()
		{
			Pause(Configuration.SensorReadDwell);
			return ReadSensor(1);
		}

		public void PlateUvOn()
		{
			AccOn(0);
		}
		public void PlateUvOff()
		{
			AccOff(0);
		}
		public void BiopsyUvOn()
		{
			AccOn(1);
		}
		public void BiopsyUvOff()
		{
			AccOff(1);
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
			MoveAbsolute(Axis.AspirateKohZ, Configuration.AspirateTopZTravel, 100);
			DoStir(Axes[Axis.AspirateKohZ]);

			var distance = CalcKohAspirateTravelMmForMl(microLiters);
			MoveRelative(Axis.AspirateKoh, distance, Configuration.AspirateSyringeSpeed);
			Pause(1000);
			MoveAbsolute(Axis.AspirateKohZ, Configuration.AspirateTopZTravel * -1, 100);
		}
		public void RemoveFluid(int microLiters)
		{
			MoveAbsolute(Axis.AspirateFluidZ, Configuration.AspirateTopZTravel, 10);
			DoStir(Axes[Axis.AspirateFluidZ]);

			var distance = CalcFluidAspirateTravelMmForMl(microLiters);
			MoveRelative(Axis.AspirateFluid, distance, Configuration.AspirateSyringeSpeed);
			Pause(5000);
			MoveAbsolute(Axis.AspirateFluidZ, Configuration.AspirateTopZTravel * -1, 10);
		}

		private float CalcFluidTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.FluidSyringeVolumeMl / Configuration.FluidSyringeLength / 1000 * 2.5f;
			var ret = microLitersPerMm * microLiters;
			return ret;
		}
		private float CalcFluidAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.AspirateSyringeVolumeMl / Configuration.AspirateSyringeTravelMm / 1000;
			var ret = (microLitersPerMm * microLiters) * -1;
			return ret;
		}
		private float CalcKohAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = 0.06f;
			var ret = (microLitersPerMm * microLiters) * -1;
			return ret;
		}
		private void DoStir(AxisSpec axis)
		{
			MoveRelative(Axis.TableY, Configuration.WellWidth * -0.8f, 100); // move near well edge
			MoveRelative(axis.Axis, Configuration.WellDepth * 0.8f, 100);
			//         int count = 0;
			//while(count <= 5)
			//{
			//	//arc to right edge
			//	var rightMultiAxis = new List<MoveSpec>();
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f) * -1));
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	rightMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to bottom edge
			//	var bottomMultiAxis = new List<MoveSpec>();
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f)));
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	bottomMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to left edge
			//	var leftMultiAxis = new List<MoveSpec>();
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f)));
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f)));
			//	leftMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.2f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to top edge
			//	var topMultiAxis = new List<MoveSpec>();
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f) * -1));
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	topMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	count++;
			//}
		}

		public Dictionary<Axis, AxisSpec> Axes { get; private set; }
		public List<AxisSpec> FluidAxes => Axes.Values.Where(x => x is FluidAxis).ToList();
		public List<AxisSpec> ZAxes => Axes.Values.Where(x => x is BiopsyZAxis).ToList();
	}
}
