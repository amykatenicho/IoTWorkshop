using System;
using System.Threading.Tasks;
using GHIElectronics.UAP.Drivers;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Shields {
	/// <summary>
	/// A helper class for the FEZ Utility.
	/// </summary>
	public class FEZUtility : IDisposable {
		private bool disposed;
		private PCA9685 pwm;
		private PCA9535 gpio;
		private ADS7830 analog;

		/// <summary>
		/// The frequency that the onboard PWM controller outputs. All PWM pins use the same frequency, only the duty cycle is controllable.
		/// </summary>
		/// <remarks>
		/// The range and granularity of the frequency are limited. The value of this property is closest to the value that the chip is actually capable of generating. It might not be exactly what you set.
		/// </remarks>
		public int PwmFrequency {
			get {
				return this.pwm.Frequency;
			}
			set {
				this.pwm.Frequency = value;
			}
		}

		/// <summary>
		/// Disposes of the object releasing control the pins.
		/// </summary>
		public void Dispose() => this.Dispose(true);

		private FEZUtility() {
			this.disposed = false;
		}

		/// <summary>
		/// Disposes of the object releasing control the pins.
		/// </summary>
		/// <param name="disposing">Whether or not this method is called from Dispose().</param>
		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					this.pwm.Dispose();
					this.analog.Dispose();
					this.gpio.Dispose();
				}

				this.disposed = true;
			}
		}

		/// <summary>
		/// Creates a new instance of the FEZ Utility.
		/// </summary>
		/// <returns>The new instance.</returns>
		public static async Task<FEZUtility> CreateAsync() {
			var gpioController = GpioController.GetDefault();
			var i2cController = (await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1")))[0];
			var hat = new FEZUtility();

			hat.analog = new ADS7830(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(ADS7830.GetAddress(false, false))));
			hat.pwm = new PCA9685(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(PCA9685.GetAddress(true, true, true, true, true, true))));
			hat.gpio = new PCA9535(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(PCA9535.GetAddress(true, true, false))), gpioController.OpenPin(22));

			hat.gpio.Write((int)Led.Led1, false);
			hat.gpio.Write((int)Led.Led2, false);
			hat.gpio.SetDriveMode((int)Led.Led1, GpioPinDriveMode.Output);
			hat.gpio.SetDriveMode((int)Led.Led2, GpioPinDriveMode.Output);

			return hat;
		}

		/// <summary>
		/// Sets the duty cycle of the given pwm pin.
		/// </summary>
		/// <param name="pin">The pin to set the duty cycle for.</param>
		/// <param name="value">The new duty cycle between 0 (off) and 1 (on).</param>
		public void SetPwmDutyCycle(PwmPin pin, double value) {
			if (value < 0.0 || value > 1.0) throw new ArgumentOutOfRangeException(nameof(value));
			if (!Enum.IsDefined(typeof(PwmPin), pin)) throw new ArgumentException(nameof(pin));

			this.pwm.SetDutyCycle((int)pin, value);
		}

		/// <summary>
		/// Sets the drive mode of the given pin.
		/// </summary>
		/// <param name="pin">The pin to set.</param>
		/// <param name="driveMode">The new drive mode of the pin.</param>
		public void SetDigitalDriveMode(DigitalPin pin, GpioPinDriveMode driveMode) {
			if (!Enum.IsDefined(typeof(DigitalPin), pin)) throw new ArgumentException(nameof(pin));

			this.gpio.SetDriveMode((int)pin, driveMode);
		}

		/// <summary>
		/// Write the given value to the given pin.
		/// </summary>
		/// <param name="pin">The pin to set.</param>
		/// <param name="state">The new state of the pin.</param>
		public void WriteDigital(DigitalPin pin, bool state) {
			if (!Enum.IsDefined(typeof(DigitalPin), pin)) throw new ArgumentException(nameof(pin));

			this.gpio.Write((int)pin, state);
		}

		/// <summary>
		/// Reads the current state of the given pin.
		/// </summary>
		/// <param name="pin">The pin to read.</param>
		/// <returns>True if high, false is low.</returns>
		public bool ReadDigital(DigitalPin pin) {
			if (!Enum.IsDefined(typeof(DigitalPin), pin)) throw new ArgumentException(nameof(pin));

			return this.gpio.Read((int)pin);
		}

		/// <summary>
		/// Sets the state of the given onboard LED.
		/// </summary>
		/// <param name="led">The LED to set.</param>
		/// <param name="state">The new state of the LED.</param>
		public void SetLedState(Led led, bool state) {
			if (!Enum.IsDefined(typeof(Led), led)) throw new ArgumentException(nameof(led));

			if (led == Led.Led1 || led == Led.Led2) {
				this.gpio.Write((int)led, state);
			}
			else {
				this.pwm.SetDutyCycle((int)led, state ? 1.00 : 0.00);
			}
		}

		/// <summary>
		/// Reads the current voltage on the given pin.
		/// </summary>
		/// <param name="pin">The pin to read.</param>
		/// <returns>The voltage between 0 (0V) and 1 (3.3V).</returns>
		public double ReadAnalog(AnalogPin pin) {
			if (!Enum.IsDefined(typeof(AnalogPin), pin)) throw new ArgumentException(nameof(pin));

			return this.analog.Read((byte)pin);
		}

		/// <summary>
		/// The possible analog pins.
		/// </summary>
		public enum AnalogPin {
			/// <summary>An analog pin.</summary>
			A0 = 0,
			/// <summary>An analog pin.</summary>
			A1 = 1,
			/// <summary>An analog pin.</summary>
			A2 = 2,
			/// <summary>An analog pin.</summary>
			A3 = 3,
			/// <summary>An analog pin.</summary>
			A4 = 4,
			/// <summary>An analog pin.</summary>
			A5 = 5,
			/// <summary>An analog pin.</summary>
			A6 = 6,
			/// <summary>An analog pin.</summary>
			A7 = 7,
		}

		/// <summary>
		/// The possible pwm pins.
		/// </summary>
		public enum PwmPin {
			/// <summary>A pwm pin.</summary>
			P0 = 0,
			/// <summary>A pwm pin.</summary>
			P1 = 1,
			/// <summary>A pwm pin.</summary>
			P2 = 2,
			/// <summary>A pwm pin.</summary>
			P3 = 3,
			/// <summary>A pwm pin.</summary>
			P4 = 4,
			/// <summary>A pwm pin.</summary>
			P5 = 5,
			/// <summary>A pwm pin.</summary>
			P6 = 6,
			/// <summary>A pwm pin.</summary>
			P7 = 7,
			/// <summary>A pwm pin.</summary>
			P8 = 8,
			/// <summary>A pwm pin.</summary>
			P9 = 9,
			/// <summary>A pwm pin.</summary>
			P10 = 10,
			/// <summary>A pwm pin.</summary>
			P11 = 11,
			/// <summary>A pwm pin.</summary>
			P12 = 12,
			/// <summary>A pwm pin.</summary>
			P13 = 13,
		}

		/// <summary>
		/// The possible digital pins.
		/// </summary>
		public enum DigitalPin {
			/// <summary>A digital pin.</summary>
			V00 = 0,
			/// <summary>A digital pin.</summary>
			V01 = 1,
			/// <summary>A digital pin.</summary>
			V02 = 2,
			/// <summary>A digital pin.</summary>
			V03 = 3,
			/// <summary>A digital pin.</summary>
			V04 = 4,
			/// <summary>A digital pin.</summary>
			V05 = 5,
			/// <summary>A digital pin.</summary>
			V06 = 6,
			/// <summary>A digital pin.</summary>
			V07 = 7,
			/// <summary>A digital pin.</summary>
			V10 = 10,
			/// <summary>A digital pin.</summary>
			V11 = 11,
			/// <summary>A digital pin.</summary>
			V12 = 12,
			/// <summary>A digital pin.</summary>
			V13 = 13,
			/// <summary>A digital pin.</summary>
			V14 = 14,
			/// <summary>A digital pin.</summary>
			V15 = 15,
		}

		/// <summary>
		/// The possible LEDs.
		/// </summary>
		public enum Led {
			/// <summary>A digital pin.</summary>
			Led1 = 16,
			/// <summary>A digital pin.</summary>
			Led2 = 17,
			/// <summary>A digital pin.</summary>
			Led3 = 14,
			/// <summary>A digital pin.</summary>
			Led4 = 15,
		}
	}
}