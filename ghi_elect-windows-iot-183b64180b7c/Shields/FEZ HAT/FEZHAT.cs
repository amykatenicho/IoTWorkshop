using GHIElectronics.UAP.Drivers;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace GHIElectronics.UAP.Shields {
    /// <summary>
    /// A helper class for the FEZ HAT.
    /// </summary>
    public class FEZHAT : IDisposable {
        private bool disposed;
        private PCA9685 pwm;
        private ADS7830 analog;
        private MMA8453 accelerometer;
        private GpioPin motorEnable;
        private GpioPin dio16;
        private GpioPin dio26;
        private GpioPin dio24;
        private GpioPin dio18;
        private GpioPin dio22;
        
        /// <summary>
        /// The chip select line exposed on the header used for SPI devices.
        /// </summary>
        public static int SpiChipSelectLine => 0;

        /// <summary>
        /// The SPI device name exposed on the header.
        /// </summary>
        public static string SpiDeviceName => "SPI0";

        /// <summary>
        /// The I2C device name exposed on the header.
        /// </summary>
        public static string I2cDeviceName => "I2C1";

        /// <summary>
        /// The frequency that the onboard PWM controller outputs. All PWM pins use the same frequency, only the duty cycle is controllable.
        /// </summary>
        /// <remarks>
        /// Care needs to be taken when using the exposed PWM pins, motors, or servos. Motors generally require a high frequency while servos require a specific low frequency, usually 50Hz.
        /// If you set the frequency to a certain value, you may impair the ability of another part of the board to function.
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
        /// The object used to control the motor terminal labeled A.
        /// </summary>
        public Motor MotorA { get; private set; }

        /// <summary>
        /// The object used to control the motor terminal labeled A.
        /// </summary>
        public Motor MotorB { get; private set; }

        /// <summary>
        /// The object used to control the RGB led labeled D2.
        /// </summary>
        public RgbLed D2 { get; private set; }

        /// <summary>
        /// The object used to control the RGB led labeled D3.
        /// </summary>
        public RgbLed D3 { get; private set; }

        /// <summary>
        /// The object used to control the servo header labeled S1.
        /// </summary>
        public Servo S1 { get; private set; }

        /// <summary>
        /// The object used to control the servo header labeled S2.
        /// </summary>
        public Servo S2 { get; private set; }

        /// <summary>
        /// Whether or not the DIO24 led is on or off.
        /// </summary>
        public bool DIO24On {
            get {
                return this.dio24.Read() == GpioPinValue.High;
            }
            set {
                this.dio24.Write(value ? GpioPinValue.High : GpioPinValue.Low);
            }
        }

        /// <summary>
        /// Whether or not the button labeled DIO18 is pressed.
        /// </summary>
        /// <returns>The pressed state.</returns>
        public bool IsDIO18Pressed() => this.dio18.Read() == GpioPinValue.Low;

        /// <summary>
        /// Whether or not the button labeled DIO18 is pressed.
        /// </summary>
        /// <returns>The pressed state.</returns>
        public bool IsDIO22Pressed() => this.dio22.Read() == GpioPinValue.Low;

        /// <summary>
        /// Gets the light level from the onboard sensor.
        /// </summary>
        /// <returns>The light level between 0 (low) and 1 (high).</returns>
        public double GetLightLevel() => this.analog.Read(5);

        /// <summary>
        /// Gets the temperature in celsius from the onboard sensor.
        /// </summary>
        /// <returns>The temperature.</returns>
        public double GetTemperature() => (this.analog.Read(4) * 3300.0 - 450.0) / 19.5;

        /// <summary>
        /// Gets the acceleration in G's for each axis from the onboard sensor.
        /// </summary>
        /// <param name="x">The current X-axis acceleration.</param>
        /// <param name="y">The current Y-axis acceleration.</param>
        /// <param name="z">The current Z-axis acceleration.</param>
        public void GetAcceleration(out double x, out double y, out double z) => this.accelerometer.GetAcceleration(out x, out y, out z);

        /// <summary>
        /// Disposes of the object releasing control the pins.
        /// </summary>
        public void Dispose() => this.Dispose(true);

        private FEZHAT() {
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
                    this.accelerometer.Dispose();
                    this.motorEnable.Dispose();
                    this.dio16.Dispose();
                    this.dio26.Dispose();
                    this.dio24.Dispose();
                    this.dio18.Dispose();
                    this.dio22.Dispose();

                    this.MotorA.Dispose();
                    this.MotorB.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Creates a new instance of the FEZ HAT.
        /// </summary>
        /// <returns>The new instance.</returns>
        public static async Task<FEZHAT> CreateAsync() {
            var gpioController = GpioController.GetDefault();
            var i2cController = (await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector(FEZHAT.I2cDeviceName)))[0];
            var hat = new FEZHAT();

            hat.accelerometer = new MMA8453(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(MMA8453.GetAddress(false))));
            hat.analog = new ADS7830(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(ADS7830.GetAddress(false, false))));

            hat.pwm = new PCA9685(await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(PCA9685.GetAddress(true, true, true, true, true, true))), gpioController.OpenPin(13));
            hat.pwm.OutputEnabled = true;
            hat.pwm.Frequency = 1500;

            hat.dio16 = gpioController.OpenPin(16);
            hat.dio26 = gpioController.OpenPin(26);
            hat.dio24 = gpioController.OpenPin(24);
            hat.dio18 = gpioController.OpenPin(18);
            hat.dio22 = gpioController.OpenPin(22);

            hat.dio24.SetDriveMode(GpioPinDriveMode.Output);
            hat.dio18.SetDriveMode(GpioPinDriveMode.Input);
            hat.dio22.SetDriveMode(GpioPinDriveMode.Input);

            hat.motorEnable = gpioController.OpenPin(12);
            hat.motorEnable.SetDriveMode(GpioPinDriveMode.Output);
            hat.motorEnable.Write(GpioPinValue.High);

            hat.MotorA = new Motor(hat.pwm, 14, 27, 23);
            hat.MotorB = new Motor(hat.pwm, 13, 6, 5);

            hat.D2 = new RgbLed(hat.pwm, 1, 0, 2);
            hat.D3 = new RgbLed(hat.pwm, 4, 3, 15);

            hat.S1 = new Servo(hat.pwm, 9);
            hat.S2 = new Servo(hat.pwm, 10);

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
        /// Write the given value to the given pin.
        /// </summary>
        /// <param name="pin">The pin to set.</param>
        /// <param name="state">The new state of the pin.</param>
        public void WriteDigital(DigitalPin pin, bool state) {
            if (!Enum.IsDefined(typeof(DigitalPin), pin)) throw new ArgumentException(nameof(pin));

            var gpioPin = pin == DigitalPin.DIO16 ? this.dio16 : this.dio26;

            if (gpioPin.GetDriveMode() != GpioPinDriveMode.Output)
                gpioPin.SetDriveMode(GpioPinDriveMode.Output);

            gpioPin.Write(state ? GpioPinValue.High : GpioPinValue.Low);
        }

        /// <summary>
        /// Reads the current state of the given pin.
        /// </summary>
        /// <param name="pin">The pin to read.</param>
        /// <returns>True if high, false is low.</returns>
        public bool ReadDigital(DigitalPin pin) {
            if (!Enum.IsDefined(typeof(DigitalPin), pin)) throw new ArgumentException(nameof(pin));

            var gpioPin = pin == DigitalPin.DIO16 ? this.dio16 : this.dio26;

            if (gpioPin.GetDriveMode() != GpioPinDriveMode.Input)
                gpioPin.SetDriveMode(GpioPinDriveMode.Input);

            return gpioPin.Read() == GpioPinValue.High;
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
            Ain1 = 1,
            /// <summary>An analog pin.</summary>
            Ain2 = 2,
            /// <summary>An analog pin.</summary>
            Ain3 = 3,
            /// <summary>An analog pin.</summary>
            Ain6 = 6,
            /// <summary>An analog pin.</summary>
            Ain7 = 7,
        }

        /// <summary>
        /// The possible pwm pins.
        /// </summary>
        public enum PwmPin {
            /// <summary>A pwm pin.</summary>
            Pwm5 = 5,
            /// <summary>A pwm pin.</summary>
            Pwm6 = 6,
            /// <summary>A pwm pin.</summary>
            Pwm7 = 7,
            /// <summary>A pwm pin.</summary>
            Pwm11 = 11,
            /// <summary>A pwm pin.</summary>
            Pwm12 = 12,
        }

        /// <summary>
        /// The possible digital pins.
        /// </summary>
        public enum DigitalPin {
            /// <summary>A digital pin.</summary>
            DIO16,
            /// <summary>A digital pin.</summary>
            DIO26
        }

        /// <summary>
        /// Represents a color of the onboard LEDs.
        /// </summary>
        public class Color {
            /// <summary>
            /// The red channel intensity.
            /// </summary>
            public byte R { get; }
            /// <summary>
            /// The green channel intensity.
            /// </summary>
            public byte G { get; }
            /// <summary>
            /// The blue channel intensity.
            /// </summary>
            public byte B { get; }

            /// <summary>
            /// Constructs a new color.
            /// </summary>
            /// <param name="red">The red channel intensity.</param>
            /// <param name="green">The green channel intensity.</param>
            /// <param name="blue">The blue channel intensity.</param>
            public Color(byte red, byte green, byte blue) {
                this.R = red;
                this.G = green;
                this.B = blue;
            }

            /// <summary>
            /// A predefined red color.
            /// </summary>
            public static Color Red => new Color(255, 0, 0);

            /// <summary>
            /// A predefined green color.
            /// </summary>
            public static Color Green => new Color(0, 255, 0);

            /// <summary>
            /// A predefined blue color.
            /// </summary>
            public static Color Blue => new Color(0, 0, 255);

            /// <summary>
            /// A predefined cyan color.
            /// </summary>
            public static Color Cyan => new Color(0, 255, 255);

            /// <summary>
            /// A predefined magneta color.
            /// </summary>
            public static Color Magneta => new Color(255, 0, 255);

            /// <summary>
            /// A predefined yellow color.
            /// </summary>
            public static Color Yellow => new Color(255, 255, 0);

            /// <summary>
            /// A predefined white color.
            /// </summary>
            public static Color White => new Color(255, 255, 255);

            /// <summary>
            /// A predefined black color.
            /// </summary>
            public static Color Black => new Color(0, 0, 0);
        }

        /// <summary>
        /// Represents an onboard RGB led.
        /// </summary>
        public class RgbLed {
            private PCA9685 pwm;
            private Color color;
            private int redChannel;
            private int greenChannel;
            private int blueChannel;

            /// <summary>
            /// The current color of the LED.
            /// </summary>
            public Color Color {
                get {
                    return this.color;
                }
                set {
                    this.color = value;

                    this.pwm.SetDutyCycle(this.redChannel, value.R / 255.0);
                    this.pwm.SetDutyCycle(this.greenChannel, value.G / 255.0);
                    this.pwm.SetDutyCycle(this.blueChannel, value.B / 255.0);
                }
            }

            internal RgbLed(PCA9685 pwm, int redChannel, int greenChannel, int blueChannel) {
                this.color = Color.Black;
                this.pwm = pwm;
                this.redChannel = redChannel;
                this.greenChannel = greenChannel;
                this.blueChannel = blueChannel;
            }

            /// <summary>
            /// Turns the LED off.
            /// </summary>
            public void TurnOff() {
                this.pwm.SetDutyCycle(this.redChannel, 0.0);
                this.pwm.SetDutyCycle(this.greenChannel, 0.0);
                this.pwm.SetDutyCycle(this.blueChannel, 0.0);
            }
        }

        /// <summary>
        /// Represents an onboard servo.
        /// </summary>
        public class Servo {
            private PCA9685 pwm;
            private int channel;
            private double position;
            private double minAngle;
            private double maxAngle;
            private double scale;
            private double offset;
            private bool limitsSet;

            /// <summary>
            /// The current position of the servo between the minimumAngle and maximumAngle passed to SetLimits.
            /// </summary>
            public double Position {
                get {
                    return this.position;
                }
                set {
                    if (!this.limitsSet) throw new InvalidOperationException($"You must call {nameof(this.SetLimits)} first.");
                    if (value < this.minAngle || value > this.maxAngle) throw new ArgumentOutOfRangeException(nameof(value));

                    this.position = value;

                    this.pwm.SetChannel(this.channel, 0x0000, (ushort)(this.scale * value + this.offset));
                }
            }

            internal Servo(PCA9685 pwm, int channel) {
                this.pwm = pwm;
                this.channel = channel;
                this.position = 0.0;
                this.limitsSet = false;
            }

            /// <summary>
            /// Sets the limits of the servo.
            /// </summary>
            /// <param name="minimumPulseWidth">The minimum pulse width in milliseconds.</param>
            /// <param name="maximumPulseWidth">The maximum pulse width in milliseconds.</param>
            /// <param name="minimumAngle">The minimum angle of input passed to Position.</param>
            /// <param name="maximumAngle">The maximum angle of input passed to Position.</param>
            public void SetLimits(int minimumPulseWidth, int maximumPulseWidth, double minimumAngle, double maximumAngle) {
                if (minimumPulseWidth < 0) throw new ArgumentOutOfRangeException(nameof(minimumPulseWidth));
                if (maximumPulseWidth < 0) throw new ArgumentOutOfRangeException(nameof(maximumPulseWidth));
                if (minimumAngle < 0) throw new ArgumentOutOfRangeException(nameof(minimumAngle));
                if (maximumAngle < 0) throw new ArgumentOutOfRangeException(nameof(maximumAngle));
                if (minimumPulseWidth >= maximumPulseWidth) throw new ArgumentException(nameof(minimumPulseWidth));
                if (minimumAngle >= maximumAngle) throw new ArgumentException(nameof(minimumAngle));

                if (this.pwm.Frequency != 50)
                    this.pwm.Frequency = 50;

                this.minAngle = minimumAngle;
                this.maxAngle = maximumAngle;

                var period = 1000000.0 / this.pwm.Frequency;

                minimumPulseWidth = (int)(minimumPulseWidth / period * 4096.0);
                maximumPulseWidth = (int)(maximumPulseWidth / period * 4096.0);

                this.scale = ((maximumPulseWidth - minimumPulseWidth) / (maximumAngle - minimumAngle));
                this.offset = minimumPulseWidth;

                this.limitsSet = true;
            }
        }

        /// <summary>
        /// Represents an onboard motor.
        /// </summary>
        public class Motor : IDisposable {
            private double speed;
            private bool disposed;
            private PCA9685 pwm;
            private GpioPin direction1;
            private GpioPin direction2;
            private int pwmChannel;

            /// <summary>
            /// The speed of the motor. The sign controls the direction while the magnitude controls the speed (0 is off, 1 is full speed).
            /// </summary>
            public double Speed {
                get {
                    return this.speed;
                }
                set {
                    this.pwm.SetDutyCycle(this.pwmChannel, 0);

                    this.direction1.Write(speed > 0 ? GpioPinValue.High : GpioPinValue.Low);
                    this.direction2.Write(speed > 0 ? GpioPinValue.Low : GpioPinValue.High);

                    this.pwm.SetDutyCycle(this.pwmChannel, Math.Abs(value));

                    this.speed = value;
                }
            }

            /// <summary>
            /// Disposes of the object releasing control the pins.
            /// </summary>
            public void Dispose() => this.Dispose(true);

            internal Motor(PCA9685 pwm, int pwmChannel, int direction1Pin, int direction2Pin) {
                var gpioController = GpioController.GetDefault();

                this.speed = 0.0;
                this.pwm = pwm;
                this.disposed = false;

                this.direction1 = gpioController.OpenPin(direction1Pin);
                this.direction2 = gpioController.OpenPin(direction2Pin);
                this.pwmChannel = pwmChannel;

                this.direction1.SetDriveMode(GpioPinDriveMode.Output);
                this.direction2.SetDriveMode(GpioPinDriveMode.Output);
            }

            /// <summary>
            /// Stops the motor.
            /// </summary>
            public void Stop() {
                this.pwm.SetDutyCycle(this.pwmChannel, 0.0);
            }

            /// <summary>
            /// Disposes of the object releasing control the pins.
            /// </summary>
            /// <param name="disposing">Whether or not this method is called from Dispose().</param>
            protected virtual void Dispose(bool disposing) {
                if (!this.disposed) {
                    if (disposing) {
                        this.direction1.Dispose();
                        this.direction2.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }
    }
}