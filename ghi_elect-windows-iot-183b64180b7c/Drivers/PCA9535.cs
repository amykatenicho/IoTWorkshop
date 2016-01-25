using System;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation;

namespace GHIElectronics.UAP.Drivers {
    public class PCA9535 {
        public class PinChangedEventArgs : EventArgs {
            public int Pin { get; }
            public bool NewState { get; }

            public PinChangedEventArgs(int pin, bool newState) {
                this.Pin = pin;
                this.NewState = newState;
            }
        }

        private I2cDevice device;
        private GpioPin interrupt;
        private byte[] write2;
        private byte[] write1;
        private byte[] read1;
        private bool disposed;
        private byte in0;
        private byte in1;
        private byte out0;
        private byte out1;
        private byte config0;
        private byte config1;

        private enum Register {
            InputPort0 = 0x00,
            InputPort1 = 0x01,
            OutputPort0 = 0x02,
            OutputPort1 = 0x03,
            ConfigurationPort0 = 0x06,
            ConfigurationPort1 = 0x07,
        }

        public static byte GetAddress(bool a0, bool a1, bool a2) => (byte)(0x20 | (a0 ? 1 : 0) | (a1 ? 2 : 0) | (a2 ? 4 : 0));

        public event TypedEventHandler<PCA9535, PinChangedEventArgs> PinChanged;

        public void Dispose() => this.Dispose(true);

        public PCA9535(I2cDevice device, GpioPin interrupt) {
            this.write2 = new byte[2];
            this.write1 = new byte[1];
            this.read1 = new byte[1];
            this.disposed = false;

            this.out0 = 0xFF;
            this.out1 = 0xFF;
            this.config0 = 0xFF;
            this.config1 = 0xFF;

            this.device = device;
            this.interrupt = interrupt;

            this.interrupt.SetDriveMode(GpioPinDriveMode.Input);
            this.interrupt.ValueChanged += this.OnInterruptValueChanged;
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    this.device.Dispose();
                    this.interrupt.Dispose();
                }

                this.disposed = true;
            }
        }

        private void OnInterruptValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args) {
            var in0 = this.ReadRegister(Register.InputPort0);
            var in1 = this.ReadRegister(Register.InputPort1);

            for (var i = 0; i < 8; i++)
                if ((in0 & (1 << i)) != (this.in0 & (1 << i)))
                    this.OnPinValueChanged(i, (in0 & (1 << i)) != 0);

            for (var i = 0; i < 8; i++)
                if ((in1 & (1 << i)) != (this.in1 & (1 << i)))
                    this.OnPinValueChanged(i + 10, (in1 & (1 << i)) != 0);

            this.in0 = in0;
            this.in1 = in1;
        }

        private void OnPinValueChanged(int pin, bool newState) => this.PinChanged?.Invoke(this, new PinChangedEventArgs(pin, newState));

        public bool Read(int pin) {
            if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
            if (!((pin >= 0 && pin <= 7) || (pin >= 10 && pin <= 17))) throw new ArgumentOutOfRangeException(nameof(pin));

            if (pin < 8) {
                return (this.ReadRegister(Register.InputPort0) & (1 << pin)) != 0;
            }
            else {
                return (this.ReadRegister(Register.InputPort1) & (1 << (pin - 10))) != 0;
            }
        }

        public void Write(int pin, bool state) {
            if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
            if (!((pin >= 0 && pin <= 7) || (pin >= 10 && pin <= 17))) throw new ArgumentOutOfRangeException(nameof(pin));

            if (state) {
                if (pin < 8) {
                    this.out0 |= (byte)(1 << pin);

                    this.WriteRegister(Register.OutputPort0, this.out0);
                }
                else {
                    this.out1 |= (byte)(1 << (pin - 10));

                    this.WriteRegister(Register.OutputPort1, this.out1);
                }
            }
            else {
                if (pin < 8) {
                    this.out0 &= (byte)~(1 << pin);

                    this.WriteRegister(Register.OutputPort0, this.out0);
                }
                else {
                    this.out1 &= (byte)~(1 << (pin - 10));

                    this.WriteRegister(Register.OutputPort1, this.out1);
                }
            }
        }

        public void SetDriveMode(int pin, GpioPinDriveMode driveMode) {
            if (this.disposed) throw new ObjectDisposedException(nameof(PCA9685));
            if (!((pin >= 0 && pin <= 7) || (pin >= 10 && pin <= 17))) throw new ArgumentOutOfRangeException(nameof(pin));

            if (driveMode == GpioPinDriveMode.Input) {
                if (pin < 8) {
                    this.config0 |= (byte)(1 << pin);

                    this.WriteRegister(Register.ConfigurationPort0, this.config0);
                }
                else {
                    this.config1 |= (byte)(1 << (pin - 10));

                    this.WriteRegister(Register.ConfigurationPort1, this.config1);
                }
            }
            else {
                if (pin < 8) {
                    this.config0 &= (byte)~(1 << pin);

                    this.WriteRegister(Register.ConfigurationPort0, this.config0);
                }
                else {
                    this.config1 &= (byte)~(1 << (pin - 10));

                    this.WriteRegister(Register.ConfigurationPort1, this.config1);
                }
            }
        }

        private void WriteRegister(Register register, int value) {
            this.write2[0] = (byte)register;
            this.write2[1] = (byte)value;

            this.device.Write(this.write2);
        }

        private byte ReadRegister(Register register) {
            this.write1[0] = (byte)register;

            this.device.WriteRead(this.write1, this.read1);

            return this.read1[0];
        }
    }
}