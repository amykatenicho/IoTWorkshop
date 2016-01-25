using System;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Drivers {
	public class PCA9685 {
		private I2cDevice device;
		private GpioPin outputEnable;
		private byte[] write5;
		private byte[] write2;
		private byte[] write1;
		private byte[] read1;
		private bool disposed;

		private enum Register {
			Mode1 = 0x00,
			Mode2 = 0x01,
			Led0OnLow = 0x06,
			Prescale = 0xFE
		}

		public static byte GetAddress(bool a0, bool a1, bool a2, bool a3, bool a4, bool a5) => (byte)(0x40 | (a0 ? 1 : 0) | (a1 ? 2 : 0) | (a2 ? 4 : 0) | (a3 ? 8 : 0) | (a4 ? 16 : 0) | (a5 ? 32 : 0));

		public void Dispose() => this.Dispose(true);

		public PCA9685(I2cDevice device) : this(device, null) {

		}

		public PCA9685(I2cDevice device, GpioPin outputEnable) {
			this.write5 = new byte[5];
			this.write2 = new byte[2];
			this.write1 = new byte[1];
			this.read1 = new byte[1];
			this.disposed = false;

			this.device = device;
			this.outputEnable = outputEnable;

			if (this.outputEnable != null) {
				this.outputEnable.SetDriveMode(GpioPinDriveMode.Output);
				this.outputEnable.Write(GpioPinValue.Low);
			}

			this.WriteRegister(Register.Mode1, 0x20);
			this.WriteRegister(Register.Mode2, 0x06);
		}

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					this.device.Dispose();
					this.outputEnable?.Dispose();
				}

				this.disposed = true;
			}
		}

		public int Frequency {
			get {
				if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));

				return (int)(25000000 / (4096 * (this.ReadRegister(Register.Prescale) + 1)) / 0.9);
			}
			set {
				if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
				if (value < 40 || value > 1500) throw new ArgumentOutOfRangeException(nameof(value), "Valid range is 40 to 1500.");

				value *= 10;
				value /= 9;

				var mode = this.ReadRegister(Register.Mode1);

				this.WriteRegister(Register.Mode1, (byte)(mode | 0x10));

				this.WriteRegister(Register.Prescale, (byte)(25000000 / (4096 * value) - 1));

				this.WriteRegister(Register.Mode1, mode);
			}
		}

		public bool OutputEnabled {
			get {
				return this.outputEnable?.Read() == GpioPinValue.Low;
			}
			set {
				if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
				if (this.outputEnable == null) throw new NotSupportedException();

				this.outputEnable.Write(value ? GpioPinValue.Low : GpioPinValue.High);
			}
		}

		public void TurnOn(int channel) {
			this.SetChannel(channel, 0x1000, 0x0000);
		}

		public void TurnOff(int channel) {
			this.SetChannel(channel, 0x0000, 0x1000);
		}

		public void SetDutyCycle(int channel, double dutyCycle) {
			if (dutyCycle < 0.0 || dutyCycle > 1.0) throw new ArgumentOutOfRangeException(nameof(dutyCycle));

			if (dutyCycle == 1.0) {
				this.TurnOn(channel);
			}
			else if (dutyCycle == 0.0) {
				this.TurnOff(channel);
			}
			else {
				this.SetChannel(channel, 0x0000, (ushort)(4096 * dutyCycle));
			}
		}

		public void SetChannel(int channel, ushort on, ushort off) {
			if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
			if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));
			if (on > 4096) throw new ArgumentOutOfRangeException(nameof(on));
			if (off > 4096) throw new ArgumentOutOfRangeException(nameof(off));

			this.write5[0] = (byte)((byte)Register.Led0OnLow + (byte)channel * 4);
			this.write5[1] = (byte)on;
			this.write5[2] = (byte)(on >> 8);
			this.write5[3] = (byte)off;
			this.write5[4] = (byte)(off >> 8);

			this.device.Write(this.write5);
		}

		private void WriteRegister(Register register, byte value) {
			this.write2[0] = (byte)register;
			this.write2[1] = value;

			this.device.Write(this.write2);
		}

		private byte ReadRegister(Register register) {
			this.write1[0] = (byte)register;

			this.device.WriteRead(this.write1, this.read1);

			return this.read1[0];
		}
	}
}