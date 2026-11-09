namespace Drivetrain.Robotics.Control
{
	/// <summary>
	/// A connection point on a controller board. Board 0 is the main board; expansion boards
	/// are addressed by their (CAN) board number.
	/// </summary>
	public readonly record struct BoardPort(int Board, int Port)
	{
		private string? BoardPrefix => Board > 0 ? $"{Board}." : null;

		/// <summary>Motor driver designation, e.g. "1.4".</summary>
		public string DriverDesignation => $"{Board}.{Port}";

		/// <summary>Digital input pin name, e.g. "io4.in" or "1.io4.in".</summary>
		public string InputDesignation => $"{BoardPrefix}io{Port}.in";

		/// <summary>Temperature (thermistor) input pin name, e.g. "temp0" or "1.temp0".</summary>
		public string TempDesignation => $"{BoardPrefix}temp{Port}";

		/// <summary>Output pin name, e.g. "out5" or "2.out5".</summary>
		public string OutputDesignation => $"{BoardPrefix}out{Port}";
	}
}
