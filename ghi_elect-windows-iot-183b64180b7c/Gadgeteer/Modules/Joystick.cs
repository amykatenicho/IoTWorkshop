using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class Joystick : Module {
		private double offsetX;
		private double offsetY;
		private AnalogIO x;
		private AnalogIO y;
		private DigitalIO input;

		public override string Name => "Joystick";
		public override string Manufacturer => "GHI Electronics, LLC";

		public int SampleCount { get; set; } = 5;

		protected async override Task Initialize(ISocket parentSocket) {
			this.x = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Four);
			this.y = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Five);
			this.input = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three);
		}

		public bool IsPressed {
			get {
				return !this.input.Read();
			}
		}

		public double X {
			get {
				return this.Read(this.x) * 2.0 - 1.0 - this.offsetX;
			}
		}

		public double Y {
			get {
				return (1.0 - this.Read(this.y)) * 2.0 - 1.0 - this.offsetX;
			}
		}

		public Position GetPosition() {
			return new Position() { X = this.X, Y = this.Y };
		}

		public void Calibrate() {
			this.offsetX = this.X;
			this.offsetY = this.Y;
		}

		public struct Position {
			public double X { get; set; }
			public double Y { get; set; }

			public override string ToString() {
				return $"({this.X:N2}, {this.Y:N2})";
			}
		}

		private double Read(AnalogIO input) {
			var total = 0.0;

			for (var i = 0; i < this.SampleCount; i++)
				total += input.ReadProportion();

			return total / this.SampleCount;
		}
	}
}