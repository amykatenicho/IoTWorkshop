using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class MaxO : Module {
		private SpiDevice spi;
		private DigitalIO enable;
		private DigitalIO clr;
		private byte[] data;
		private int boards;

		public override string Name => "MaxO";
		public override string Manufacturer => "GHI Electronics, LLC";

		public MaxO() {

		}

		protected async override Task Initialize(ISocket parentSocket) {
			this.boards = 0;
			this.data = null;

			this.spi = await parentSocket.CreateSpiDeviceAsync(new Windows.Devices.Spi.SpiConnectionSettings(0) { Mode = Windows.Devices.Spi.SpiMode.Mode0, ClockFrequency = 1000 });
			this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three, false);
			this.clr = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, true);
		}

		public int Boards {
			get {
				return this.boards;
			}
			set {
				if (this.data != null) throw new InvalidOperationException("You may only set boards once.");
				if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value must be positive.");

				this.boards = value;
				this.data = new byte[value * 4];
			}
		}

		public int ArraySize {
			get {
				return data.Length;
			}
		}

		public bool OutputEnabled {
			get {
				return !this.enable.Read();
			}
			set {
				this.enable.Write(!value);
			}
		}

		public void Clear() {
			if (this.data == null) throw new InvalidOperationException("You must set Boards first.");

			this.enable.Write(true);
			this.clr.Write(false);

			Task.Delay(10).Wait();

			this.spi.Write(new byte[] { 0 });

			this.clr.Write(true);
			this.enable.Write(false);

			for (int i = 0; i < this.data.Length; i++)
				this.data[i] = 0x0;
		}

		public void Write(byte[] buffer) {
			if (this.data == null) throw new InvalidOperationException("You must set Boards first.");
			if (buffer.Length != this.data.Length) throw new ArgumentException("array", "array.Length must be the same size as ArraySize.");

			this.enable.Write(true);

			var reversed = new byte[buffer.Length];
			for (int i = 0; i < reversed.Length; i++)
				reversed[i] = buffer[reversed.Length - i - 1];

			this.spi.Write(reversed);

			if (this.data != buffer)
				Array.Copy(buffer, this.data, buffer.Length);

			this.enable.Write(false);
		}

		public void SetPin(int board, int pin, bool value) {
			if (this.data == null) throw new InvalidOperationException("You must set Boards first.");
			if (board * 4 > data.Length) throw new ArgumentException("board", "The board is out of range.");

			var index = (board - 1) * 4 + pin / 8;

			if (value) {
				this.data[index] = (byte)(this.data[index] | (1 << (pin % 8)));
			}
			else {
				this.data[index] = (byte)(this.data[index] & ~(1 << (pin % 8)));
			}

			this.Write(this.data);
		}

		public byte[] Read() {
			if (this.data == null) throw new InvalidOperationException("You must set Boards first.");

			var buffer = new byte[this.data.Length];

			Array.Copy(this.data, buffer, this.data.Length);

			return this.data;
		}
	}
}