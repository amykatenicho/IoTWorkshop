using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Gadgeteer {
	public class CY8C9560A {
		private enum Register : byte {
			InputPort0 = 0x00,
			OutputPort0 = 0x08,
			InterruptPort0 = 0x10,
			PortSelect = 0x18,
			InterruptMask = 0x19,
			SelectPwmForOutput = 0x1A,
			PinDirection = 0x1C,
			PinPullUp = 0x1D,
			PinPullDown = 0x1E,
			PinStrongDrive = 0x21,
			PinHighImpedance = 0x23,
			PwmSelect = 0x28,
			ConfigPwm = 0x29,
			PeriodPwm = 0x2A,
			PulseWidthPwm = 0x2B
		}

		private enum PwmClockSource {
			KHz32 = 0x00,
			MHz24 = 0x01,
			MHz1 = 0x02,
			KHz94 = 0x03,
			Hz367 = 0x04,
		}

		public struct Pin {
			public byte Port { get; set; }
			public byte PinNumber { get; set; }
			public byte Mask { get { return (byte)(1 << this.PinNumber); } }

			public Pin(byte port, byte pinNumber) {
				this.Port = port;
				this.PinNumber = pinNumber;
			}

			public static bool operator ==(Pin left, Pin right) {
				return left.PinNumber == right.PinNumber && left.Port == right.Port;
			}

			public static bool operator !=(Pin left, Pin right) {
				return !(left == right);
			}

			public override bool Equals(object o) {
				return o is Pin && this == (Pin)o;
			}

			public override int GetHashCode() {
				return (this.Port << 8) | this.PinNumber;
			}
		}

		private I2cDevice device;
		private GpioPin interrupt;
		private Dictionary<Pin, Action<bool>> interruptRegistrations;
		//private byte[] shadowRegisters;
        private bool disposed;

        public static byte GetAddress(bool a0, bool a1, bool a2, bool a3, bool a4, bool a5, bool a6) => (byte)((a0 ? 1 : 0) | (a1 ? 2 : 0) | (a2 ? 4 : 0) | (a3 ? 8 : 0) | (a4 ? 16 : 0) | (a5 ? 32 : 0) | (a6 ? 64 : 0));

        public void Dispose() => this.Dispose(true);

        public CY8C9560A(I2cDevice device, GpioPin interrupt) {
            this.device = device;
            this.interrupt = interrupt;
            this.disposed = false;

            this.interruptRegistrations = new Dictionary<Pin, Action<bool>>();
            //this.shadowRegisters = new byte[0x2D - 0x08];

            this.interrupt.ValueChanged += this.OnInterrupt;

            //this.i2c.ReadRegisters(0x08, this.shadowRegisters);
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

        public void SetPwm(Pin pin, double frequency, double dutyCycle) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));
            if (dutyCycle < 0.0 || dutyCycle > 1.0) throw new ArgumentOutOfRangeException(nameof(dutyCycle));

            this.WriteRegister(Register.PortSelect, pin.Port);

			this.SetBits(Register.PinStrongDrive, pin.Mask);
			this.SetBits(Register.InterruptMask, pin.Mask);
			this.SetBits(Register.SelectPwmForOutput, pin.Mask);

			this.WriteDigital(pin, true);

			double period;
			PwmClockSource clockSource;

			if (frequency <= 1.45) {
				throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency is too low.");
			}
			else if (frequency <= 125.5) {
				period = 367.6 / frequency;
				clockSource = PwmClockSource.Hz367;
			}
			else if (frequency <= 367.7) {
				period = 32000.0 / frequency;
				clockSource = PwmClockSource.KHz32;
			}
			else if (frequency <= 5882.4) {
				period = 93750.0 / frequency;
				clockSource = PwmClockSource.KHz94;
			}
			else if (frequency <= 94117.7) {
				period = 1500000.0 / frequency;
				clockSource = PwmClockSource.MHz1;
			}
			else if (frequency <= 12000000.0) {
				period = 24000000.0 / frequency;
				clockSource = PwmClockSource.MHz24;
			}
			else {
				throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency is too high.");
			}

			this.WriteRegister(Register.PwmSelect, (byte)(pin.PinNumber + (pin.Port == 7 ? 8 : 0)));
			this.WriteRegister(Register.ConfigPwm, (byte)clockSource);
			this.WriteRegister(Register.PeriodPwm, (byte)period);
			this.WriteRegister(Register.PulseWidthPwm, (byte)(period * dutyCycle));
		}

		public void SetOutput(Pin pin) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));

            this.WriteRegister(Register.PortSelect, pin.Port);

			this.ClearBits(Register.SelectPwmForOutput, pin.Mask);
			this.ClearBits(Register.PinDirection, pin.Mask);
			this.SetBits(Register.PinStrongDrive, pin.Mask);
			this.SetBits(Register.InterruptMask, pin.Mask);
		}

		public void SetInput(Pin pin) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));

            this.WriteRegister(Register.PortSelect, pin.Port);

			this.ClearBits(Register.SelectPwmForOutput, pin.Mask);
			this.SetBits(Register.PinDirection, pin.Mask);
			this.SetBits(Register.PinHighImpedance, pin.Mask);
			this.SetBits(Register.InterruptMask, pin.Mask);

			if (this.interruptRegistrations.ContainsKey(pin))
				this.interruptRegistrations.Remove(pin);
		}

		public void SetInterrupt(Pin pin, Action<bool> onInterrupt) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));

            this.SetInput(pin);

			this.ClearBits(Register.InterruptMask, pin.Mask);

			this.interruptRegistrations[pin] = onInterrupt;
		}

		public bool ReadDigital(Pin pin) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));

            return (this.ReadRegister(Register.InputPort0 + pin.Port) & pin.Mask) != 0;
		}

		public void WriteDigital(Pin pin, bool value) {
            if (this.disposed) throw new ObjectDisposedException(nameof(CY8C9560A));

            if (value) {
				this.SetBits(Register.OutputPort0 + pin.Port, pin.Mask);
			}
			else {
				this.ClearBits(Register.OutputPort0 + pin.Port, pin.Mask);
			}
		}

		private void OnInterrupt(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge != GpioPinEdge.RisingEdge)
                return;

			var ports = this.ReadRegisters(Register.InterruptPort0, 8);

			for (byte i = 0; i < 8; i++) {
				for (byte j = 0; j < 8; j++) {
					var pin = new Pin(i, j);

					if (this.interruptRegistrations.ContainsKey(pin) && (ports[i] & (1 << j)) != 0)
						this.interruptRegistrations[pin](this.ReadDigital(pin));
				}
			}
		}

		private bool IsShadowed(Register register) {
			return false; // ((int)register >= 0x08 && (int)register < 0x10) || ((int)register >= 0x18 && (int)register < 0x2D);
        }

		private byte ReadRegister(Register register) {
            return this.ReadRegisters(register, 1)[0];
        }

        private byte[] ReadRegisters(Register register, int count) {
            //if (this.IsShadowed(register))
            //    return this.shadowRegisters[(int)register - 0x08];

            var result = new byte[count];

            this.device.WriteRead(new byte[] { (byte)register }, result);

            return result;
        }

        private void WriteRegister(Register register, int value) {
            //if (this.IsShadowed(register))
            //	this.shadowRegisters[(int)register - 0x08] = (byte)value;

            this.device.WriteRead(new byte[] { (byte)register }, new byte[] { (byte)value });
		}

		private void SetBits(Register register, int mask) {
			this.WriteRegister(register, (byte)(this.ReadRegister(register) | mask));
		}

		private void ClearBits(Register register, int mask) {
			this.WriteRegister(register, (byte)(this.ReadRegister(register) & ~mask));
		}
	}
}