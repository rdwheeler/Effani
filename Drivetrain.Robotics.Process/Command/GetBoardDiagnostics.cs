using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class GetBoardDiagnostics(int boardNumber, DeviceControl device) : Command(device)
	{

		protected override void Run()
		{
			Device.GetBoardDiagnostics(_boardNumber);
		}

		public override string Description => $"Get diagnostics for board {_boardNumber}.";
		private readonly int _boardNumber = boardNumber;
	}
}
