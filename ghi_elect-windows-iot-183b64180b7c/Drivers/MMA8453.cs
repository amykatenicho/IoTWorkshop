using System;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Drivers {
	public class MMA8453 {
        private I2cDevice device;
        private byte[] write;
        private byte[] read;
        private bool disposed;

        public static byte GetAddress(bool a0) => (byte)(0x1C | (a0 ? 1 : 0));

        public void Dispose() => this.Dispose(true);

        public MMA8453(I2cDevice device) {
            this.device = device;
            this.write = new byte[1] { 0x01 };
            this.read = new byte[6];
            this.disposed = false;

            this.device.Write(new byte[] { 0x2A, 0x01 });
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    this.device.Dispose();
                }

                this.disposed = true;
            }
        }

        public void GetAcceleration(out double x, out double y, out double z) {
            if (this.disposed) throw new ObjectDisposedException(nameof(MMA8453));

            this.device.WriteRead(this.write, this.read);

            x = this.Normalize(0);
            y = this.Normalize(2);
            z = this.Normalize(4);
        }

        private double Normalize(int offset) {
            double value = (this.read[offset] << 2) | (this.read[offset + 1] >> 6);

            if (value > 511.0)
                value = value - 1024.0;

            value /= 512.0;

            return value;
        }
    }
}