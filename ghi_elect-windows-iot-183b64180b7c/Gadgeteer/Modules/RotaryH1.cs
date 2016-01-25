using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class RotaryH1 : Module {
		private byte[] write1;
		private byte[] write2;
		private byte[] read4;

		private SpiDevice spi;
		private DigitalIO enable;
		private CountMode mode;

		protected ISocket socket;

		public override string Name => "RotaryH1";
		public override string Manufacturer => "GHI Electronics, LLC";

		protected async override Task Initialize(ISocket parentSocket) {
			this.socket = parentSocket;

			this.write1 = new byte[1];
			this.write2 = new byte[2];
			this.read4 = new byte[4];

			this.spi = await this.socket.CreateSpiDeviceAsync(new Windows.Devices.Spi.SpiConnectionSettings(parentSocket.NativeSpiChipSelectPin) { Mode = Windows.Devices.Spi.SpiMode.Mode0, ClockFrequency = 1000000 });
			this.enable = await this.socket.CreateDigitalIOAsync(SocketPinNumber.Five, true);

			this.Write(Command.Clear, Register.Mode0);
			this.Write(Command.Clear, Register.Mode1);
			this.Write(Command.Clear, Register.Status);
			this.Write(Command.Clear, Register.Counter);
			this.Write(Command.Load, Register.Output);

			this.Mode = CountMode.Quad1;

			this.Write(Command.Write, Register.Mode1, Mode1.FourByte | Mode1.EnableCount);
		}

		public int GetCount() {
			this.Write(Command.Load, Register.Output);

			return this.Read(Command.Read, Register.Output);
		}

		public void ResetCount() {
			this.Write(Command.Clear, Register.Counter);
		}

		public CountMode Mode {
			get {
				return this.mode;
			}
			set {
				if (this.mode == value)
					return;

				this.mode = value;

				var command = Mode0.FreeRunning | Mode0.DisableIndex | Mode0.FilterClockDivisionTwo;

				switch (this.mode) {
					case CountMode.None: command |= Mode0.Quad0; break;
					case CountMode.Quad1: command |= Mode0.Quad1; break;
					case CountMode.Quad2: command |= Mode0.Quad2; break;
					case CountMode.Quad4: command |= Mode0.Quad4; break;
				}

				this.Write(Command.Write, Register.Mode0, command);
			}
		}

		private int Read(Command command, Register register) {
			this.write1[0] = (byte)((byte)command | (byte)register);

			this.spi.WriteThenRead(this.write1, this.read4);

			return (this.read4[0] << 24) + (this.read4[1] << 16) + (this.read4[2] << 8) + this.read4[3];
		}

		private void Write(Command command, Register register) {
			this.write1[0] = (byte)((byte)command | (byte)register);

			this.spi.Write(this.write1);
		}

		private void Write(Command command, Register register, Mode0 mode) {
			this.write2[0] = (byte)((byte)command | (byte)register);
			this.write2[1] = (byte)mode;

			this.spi.Write(this.write2);
		}

		private void Write(Command command, Register register, Mode1 mode) {
			this.write2[0] = (byte)((byte)command | (byte)register);
			this.write2[1] = (byte)mode;

			this.spi.Write(this.write2);
		}

		public enum Direction : byte {
			CounterClockwise,
			Clockwise
		}

		private enum Command {
			Clear = 0x00,
			Read = 0x40,
			Write = 0x80,
			Load = 0xC0,
		}

		private enum Register {
			Mode0 = 0x08,
			Mode1 = 0x10,
			Input = 0x18,
			Counter = 0x20,
			Output = 0x28,
			Status = 0x30,
		}

		[Flags]
		private enum Mode0 {
			Quad0 = 0x00,
			Quad1 = 0x01,
			Quad2 = 0x02,
			Quad4 = 0x03,
			FreeRunning = 0x00,
			SingleCycleCount = 0x04,
			Range = 0x08,
			ModuloN = 0x0C,
			DisableIndex = 0x00,
			IndexAsLoadCount = 0x10,
			IndexAsResetCount = 0x20,
			IndexAsLoadOutput = 0x30,
			AsynchronousIndex = 0x00,
			SynchronousIndex = 0x40,
			FilterClockDivisionOne = 0x00,
			FilterClockDivisionTwo = 0x80,
		}

		[Flags]
		private enum Mode1 {
			FourByte = 0x00,
			ThreeByte = 0x01,
			TwoByte = 0x02,
			OneByte = 0x03,
			EnableCount = 0x00,
			DisableCount = 0x04,
			FlagIndex = 0x10,
			FlagCompare = 0x20,
			FlagBorrow = 0x40,
			FlagCarry = 0x80
		}

		public enum CountMode {
			None,
			Quad1,
			Quad2,
			Quad4
		}
	}
}