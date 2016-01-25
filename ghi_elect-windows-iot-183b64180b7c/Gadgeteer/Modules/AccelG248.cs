using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class AccelG248 : Module {
		private static byte I2cAddress => 0x1C;

		private I2cDevice i2c;
		private byte[] buffer;

		public override string Name => "AccelG248";
		public override string Manufacturer => "GHI Electronics, LLC";

		public AccelG248() {
			this.buffer = new byte[1];
		}

		protected async override Task Initialize(ISocket parentSocket) {
			this.i2c = await parentSocket.CreateI2cDeviceAsync(new Windows.Devices.I2c.I2cConnectionSettings(AccelG248.I2cAddress));
			this.i2c.WriteRegister(0x2A, 0x01);
		}

		private double Read(byte register) {
			var data = this.i2c.ReadRegisters(register, 2);

			return this.Normalize(data, 0);
		}

		private double Normalize(byte[] data, int offset) {
			double value = (data[offset] << 2) | (data[offset + 1] >> 6);

			if (value > 511.0)
				value = value - 1024.0;

			value /= 512.0;

			return value;
		}

		public double GetX() {
			return this.Read(0x01);
		}

		public double GetY() {
			return this.Read(0x03);
		}

		public double GetZ() {
			return this.Read(0x05);
		}

		public void GetXYZ(out double x, out double y, out double z) {
			var data = this.i2c.ReadRegisters(0x01, 6);

			x = this.Normalize(data, 0);
			y = this.Normalize(data, 2);
			z = this.Normalize(data, 4);
		}

		public Acceleration GetAcceleration() {
			double x, y, z;

			this.GetXYZ(out x, out y, out z);

			return new Acceleration { X = x, Y = y, Z = z };
		}

		public struct Acceleration {
			public double X { get; set; }
			public double Y { get; set; }
			public double Z { get; set; }

			public override string ToString() {
				return $"{this.X:F2}, {this.Y:F2}, {this.Z:F2}";
			}
		}
	}
}