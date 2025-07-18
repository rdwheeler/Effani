using System;
using System.ComponentModel;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Coordinate(decimal X, decimal Y) : INotifyPropertyChanged
	{
		decimal _x = X;
		public decimal X
		{
			get
			{
				return _x;
			}
			set
			{
				if (_x == value)
					return;
				_x = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
			}
		}
		decimal _y = Y;
		public decimal Y
		{
			get
			{
				return _y;
			}
			set
			{
				if (_y == value)
					return;
				_y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
			}
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}
