using System;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Drivers {
	public class ADS7830 {
		private I2cDevice device;
		private bool disposed;
		private byte[] read;
		private byte[] write;

		public static byte GetAddress(bool a0, bool a1) => (byte)(0x48 | (a0 ? 1 : 0) | (a1 ? 2 : 0));

		public void Dispose() => this.Dispose(true);

		public ADS7830(I2cDevice device) {
			this.device = device;
			this.disposed = false;
			this.read = new byte[1];
			this.write = new byte[1];
		}

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					this.device.Dispose();
				}

				this.disposed = true;
			}
		}

		public double Read(int channel) {
			if (this.disposed) throw new ObjectDisposedException(nameof(ADS7830));
			if (channel > 8 || channel < 0) throw new ArgumentOutOfRangeException(nameof(channel));

			this.write[0] =(byte)( 0x84 | ((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4));

			this.device.WriteRead(this.write, this.read);

			return this.read[0] / 255.0;
		}
	}
}