using GHIElectronics.UAP.Drivers;
using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;
using System;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using System.Collections.Generic;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
    public class FEZCream : Module {
        private ADS7830 analog;
        private PCA9685 pwm;
        private PCA9535[] gpios;
        private Dictionary<int, Dictionary<SocketPinNumber, Tuple<int, int>>> gpioMap;
        private Dictionary<int, Dictionary<SocketPinNumber, int>> analogMap;
        private Dictionary<int, Dictionary<SocketPinNumber, int>> pwmMap;
        private Dictionary<int, Tuple<int, int>> analogSharedMap;
        private Dictionary<int, Tuple<int, int, int, int>> pwmSharedMap;

        public override string Name => "FEZ Cream";
        public override string Manufacturer => "GHI Electronics, LLC";
        public override int RequiredSockets => 0;

        protected async override Task Initialize() {
            this.gpioMap = FEZCream.CreateGpioMap();
            this.analogMap = FEZCream.CreateAnalogMap();
            this.pwmMap = FEZCream.CreatePwmMap();
            this.analogSharedMap = FEZCream.CreateAnalogSharedMap();
            this.pwmSharedMap = FEZCream.CreatePwmSharedMap();

            this.analog = new ADS7830(await NativeInterfaces.I2cDevice.CreateInterfaceAsync("I2C1", new I2cConnectionSettings(ADS7830.GetAddress(false, false))));
            this.pwm = new PCA9685(await NativeInterfaces.I2cDevice.CreateInterfaceAsync("I2C1", new I2cConnectionSettings(PCA9685.GetAddress(true, true, true, true, true, true))));

            this.gpios = new PCA9535[2];
            this.gpios[0] = new PCA9535(await NativeInterfaces.I2cDevice.CreateInterfaceAsync("I2C1", new I2cConnectionSettings(PCA9535.GetAddress(true, true, false))), await NativeInterfaces.DigitalIO.CreateInterfaceAsync(22));
            this.gpios[1] = new PCA9535(await NativeInterfaces.I2cDevice.CreateInterfaceAsync("I2C1", new I2cConnectionSettings(PCA9535.GetAddress(true, false, true))), await NativeInterfaces.DigitalIO.CreateInterfaceAsync(26));

            for (var i = 2; i <= 7; i++) {
                this.gpios[0].SetDriveMode(i, GpioPinDriveMode.Output);
                this.gpios[0].Write(i, true);
            }

            Socket socket;

            socket = this.CreateSocket(1);
            socket.AddSupportedTypes(SocketType.I);
            socket.SetNativePin(SocketPinNumber.Three, 18);
            socket.NativeI2cDeviceId = "I2C1";
            socket.DigitalIOCreator = this.DigitalIOCreator;

            socket = this.CreateSocket(2);
            socket.AddSupportedTypes(SocketType.U);
            socket.NativeSerialDeviceId = "COM1";
            socket.DigitalIOCreator = this.DigitalIOCreator;

            socket = this.CreateSocket(3);
            socket.AddSupportedTypes(SocketType.S, SocketType.X);
            socket.SetNativePin(SocketPinNumber.Three, 24);
            socket.SetNativePin(SocketPinNumber.Four, 25);
            socket.SetNativePin(SocketPinNumber.Five, 13);
            socket.NativeSpiDeviceId = "SPI0";
            socket.NativeSpiChipSelectPin = 0;

            socket = this.CreateSocket(4);
            socket.AddSupportedTypes(SocketType.Y);
            socket.SetNativePin(SocketPinNumber.Three, 6);
            socket.DigitalIOCreator = this.DigitalIOCreator;

            socket = this.CreateSocket(5);
            socket.AddSupportedTypes(SocketType.A);
            socket.SetNativePin(SocketPinNumber.Three, 12);
            socket.DigitalIOCreator = this.DigitalIOCreator;
            socket.AnalogIOCreator = this.AnalogIOCreator;

            socket = this.CreateSocket(6);
            socket.AddSupportedTypes(SocketType.A);
            socket.SetNativePin(SocketPinNumber.Three, 16);
            socket.DigitalIOCreator = this.DigitalIOCreator;
            socket.AnalogIOCreator = this.AnalogIOCreator;

            socket = this.CreateSocket(7);
            socket.AddSupportedTypes(SocketType.P, SocketType.Y);
            socket.SetNativePin(SocketPinNumber.Three, 5);
            socket.DigitalIOCreator = this.DigitalIOCreator;
            socket.PwmOutputCreator = this.PwmOutputCreator;

            socket = this.CreateSocket(8);
            socket.AddSupportedTypes(SocketType.P, SocketType.Y);
            socket.SetNativePin(SocketPinNumber.Three, 27);
            socket.DigitalIOCreator = this.DigitalIOCreator;
            socket.PwmOutputCreator = this.PwmOutputCreator;

            socket = this.CreateSocket(9);
            socket.AddSupportedTypes(SocketType.I);
            socket.SetNativePin(SocketPinNumber.Three, 23);
            socket.NativeI2cDeviceId = "I2C1";
            socket.DigitalIOCreator = this.DigitalIOCreator;
        }

        public override void SetDebugLed(bool state) {
            if (state) {
                this.pwm.TurnOn(15);
            }
            else {
                this.pwm.TurnOff(15);
            }
        }

        private Task<DigitalIO> DigitalIOCreator(Socket socket, SocketPinNumber pinNumber) {
            if (!this.gpioMap.ContainsKey(socket.Number)) throw new UnsupportedPinModeException();
            if (!this.gpioMap[socket.Number].ContainsKey(pinNumber)) throw new UnsupportedPinModeException();

            var pin = this.gpioMap[socket.Number][pinNumber];

            if (this.pwmMap.ContainsKey(socket.Number) && this.pwmMap[socket.Number].ContainsKey(pinNumber)) {
                var channel = this.pwmMap[socket.Number][pinNumber];
                var shared = this.pwmSharedMap[channel];

                this.gpios[shared.Item3 - 1].SetDriveMode(shared.Item4, GpioPinDriveMode.Output);
                this.gpios[shared.Item3 - 1].Write(shared.Item4, true);
            }

            return Task.FromResult<DigitalIO>(new IndirectedDigitalIO(pin.Item2, this.gpios[pin.Item1 - 1]));
        }

        private async Task<AnalogIO> AnalogIOCreator(Socket socket, SocketPinNumber pinNumber) {
            if (!this.analogMap.ContainsKey(socket.Number)) throw new UnsupportedPinModeException();
            if (!this.analogMap[socket.Number].ContainsKey(pinNumber)) throw new UnsupportedPinModeException();

            var channel = this.analogMap[socket.Number][pinNumber];

            if (this.analogSharedMap.ContainsKey(channel)) {
                var shared = this.analogSharedMap[channel];

                if (shared.Item1 == 0) {
                    using (var gpio = await NativeInterfaces.DigitalIO.CreateInterfaceAsync(shared.Item2)) {
                        gpio.SetDriveMode(GpioPinDriveMode.Input);
                    }
                }
                else {
                    this.gpios[shared.Item1 - 1].SetDriveMode(shared.Item2, GpioPinDriveMode.Input);
                }
            }

            return new IndirectedAnalogIO(channel, this.analog);
        }

        private Task<PwmOutput> PwmOutputCreator(Socket socket, SocketPinNumber pinNumber) {
            if (!this.pwmMap.ContainsKey(socket.Number)) throw new UnsupportedPinModeException();
            if (!this.pwmMap[socket.Number].ContainsKey(pinNumber)) throw new UnsupportedPinModeException();

            var channel = this.pwmMap[socket.Number][pinNumber];
            var shared = this.pwmSharedMap[channel];

            this.gpios[shared.Item1 - 1].SetDriveMode(shared.Item2, GpioPinDriveMode.Input);

            this.gpios[shared.Item3 - 1].SetDriveMode(shared.Item4, GpioPinDriveMode.Output);
            this.gpios[shared.Item3 - 1].Write(shared.Item4, false);

            return Task.FromResult<PwmOutput>(new IndirectedPwmOutput(channel, this.pwm));
        }

        private static Dictionary<int, Dictionary<SocketPinNumber, Tuple<int, int>>> CreateGpioMap() {
            var s1 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s1.Add(SocketPinNumber.Six, Tuple.Create(2, 7));

            var s2 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s2.Add(SocketPinNumber.Three, Tuple.Create(2, 5));
            s2.Add(SocketPinNumber.Six, Tuple.Create(2, 6));

            var s4 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s4.Add(SocketPinNumber.Four, Tuple.Create(2, 0));
            s4.Add(SocketPinNumber.Five, Tuple.Create(2, 17));
            s4.Add(SocketPinNumber.Six, Tuple.Create(2, 1));
            s4.Add(SocketPinNumber.Seven, Tuple.Create(2, 4));
            s4.Add(SocketPinNumber.Eight, Tuple.Create(2, 2));
            s4.Add(SocketPinNumber.Nine, Tuple.Create(2, 3));

            var s5 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s5.Add(SocketPinNumber.Four, Tuple.Create(2, 14));
            s5.Add(SocketPinNumber.Six, Tuple.Create(2, 16));

            var s6 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s6.Add(SocketPinNumber.Four, Tuple.Create(2, 13));
            s6.Add(SocketPinNumber.Six, Tuple.Create(2, 15));

            var s7 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s7.Add(SocketPinNumber.Four, Tuple.Create(2, 11));
            s7.Add(SocketPinNumber.Five, Tuple.Create(2, 10));
            s7.Add(SocketPinNumber.Six, Tuple.Create(2, 12));
            s7.Add(SocketPinNumber.Seven, Tuple.Create(1, 12));
            s7.Add(SocketPinNumber.Eight, Tuple.Create(1, 11));
            s7.Add(SocketPinNumber.Nine, Tuple.Create(1, 10));

            var s8 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s8.Add(SocketPinNumber.Four, Tuple.Create(1, 0));
            s8.Add(SocketPinNumber.Five, Tuple.Create(1, 16));
            s8.Add(SocketPinNumber.Six, Tuple.Create(1, 1));
            s8.Add(SocketPinNumber.Seven, Tuple.Create(1, 15));
            s8.Add(SocketPinNumber.Eight, Tuple.Create(1, 14));
            s8.Add(SocketPinNumber.Nine, Tuple.Create(1, 13));

            var s9 = new Dictionary<SocketPinNumber, Tuple<int, int>>();
            s9.Add(SocketPinNumber.Six, Tuple.Create(1, 17));

            return new Dictionary<int, Dictionary<SocketPinNumber, Tuple<int, int>>>() { { 1, s1 }, { 2, s2 }, { 4, s4 }, { 5, s5 }, { 6, s6 }, { 7, s7 }, { 8, s8 }, { 9, s9 } };
        }

        private static Dictionary<int, Dictionary<SocketPinNumber, int>> CreateAnalogMap() {
            var s5 = new Dictionary<SocketPinNumber, int>();
            s5.Add(SocketPinNumber.Three, 4);
            s5.Add(SocketPinNumber.Four, 5);
            s5.Add(SocketPinNumber.Five, 3);

            var s6 = new Dictionary<SocketPinNumber, int>();
            s6.Add(SocketPinNumber.Three, 2);
            s6.Add(SocketPinNumber.Four, 0);
            s6.Add(SocketPinNumber.Five, 1);

            return new Dictionary<int, Dictionary<SocketPinNumber, int>>() { { 5, s5 }, { 6, s6 } };
        }

        private static Dictionary<int, Dictionary<SocketPinNumber, int>> CreatePwmMap() {
            var s7 = new Dictionary<SocketPinNumber, int>();
            s7.Add(SocketPinNumber.Seven, 2);
            s7.Add(SocketPinNumber.Eight, 1);
            s7.Add(SocketPinNumber.Nine, 0);

            var s8 = new Dictionary<SocketPinNumber, int>();
            s8.Add(SocketPinNumber.Seven, 3);
            s8.Add(SocketPinNumber.Eight, 4);
            s8.Add(SocketPinNumber.Nine, 5);

            return new Dictionary<int, Dictionary<SocketPinNumber, int>>() { { 7, s7 }, { 8, s8 } };
        }

        private static Dictionary<int, Tuple<int, int>> CreateAnalogSharedMap() {
            return new Dictionary<int, Tuple<int, int>>() {
                { 4, Tuple.Create(0, 12) },
                { 5, Tuple.Create(2, 14) },
                { 2, Tuple.Create(0, 16) },
                { 0, Tuple.Create(2, 13) },
            };
        }

        private static Dictionary<int, Tuple<int, int, int, int>> CreatePwmSharedMap() {
            return new Dictionary<int, Tuple<int, int, int, int>>() {
                { 2, Tuple.Create(1, 12, 1, 5) },
                { 1, Tuple.Create(1, 11, 1, 6) },
                { 0, Tuple.Create(1, 10, 1, 7) },
                { 3, Tuple.Create(1, 15, 1, 2) },
                { 4, Tuple.Create(1, 14, 1, 3) },
                { 5, Tuple.Create(1, 13, 1, 4) },
            };
        }

        private class IndirectedDigitalIO : DigitalIO {
            private int pin;
            private PCA9535 gpio;
            private GpioPinDriveMode driveMode;

            public IndirectedDigitalIO(int channel, PCA9535 pca) {
                this.pin = channel;
                this.gpio = pca;
                this.driveMode = GpioPinDriveMode.Input;
            }

            private void OnPinChanged(PCA9535 sender, PCA9535.PinChangedEventArgs e) {
                if (e.Pin == this.pin)
                    this.OnValueChanged(e.NewState);
            }

            protected override void EnableInterrupt() {
                this.gpio.PinChanged += this.OnPinChanged;
            }

            protected override void DisableInterrupt() {
                this.gpio.PinChanged -= this.OnPinChanged;
            }

            protected override bool ReadInternal() {
                if (this.DriveMode != GpioPinDriveMode.Input)
                    this.DriveMode = GpioPinDriveMode.Input;

                return this.gpio.Read(pin);
            }

            protected override void WriteInternal(bool value) {
                if (this.DriveMode != GpioPinDriveMode.Output)
                    this.DriveMode = GpioPinDriveMode.Output;

                this.gpio.Write(pin, value);
            }

            public override GpioPinDriveMode DriveMode {
                get {
                    return this.driveMode;
                }
                set {
                    this.gpio.SetDriveMode(this.pin, value);

                    this.driveMode = value;
                }
            }
        }

        private class IndirectedAnalogIO : AnalogIO {
            private int channel;
            private ADS7830 analog;

            public override double MaxVoltage => 3.3;

            public IndirectedAnalogIO(int channel, ADS7830 analog) {
                this.channel = channel;
                this.analog = analog;
            }

            protected override double ReadInternal() {
                return this.analog.Read(this.channel) * this.MaxVoltage;
            }

            protected override void WriteInternal(double voltage) {
                throw new NotSupportedException();
            }

            public override GpioPinDriveMode DriveMode {
                get {
                    return GpioPinDriveMode.Input;
                }
                set {
                    if (value != GpioPinDriveMode.Input)
                        throw new NotSupportedException();
                }
            }
        }

        private class IndirectedPwmOutput : PwmOutput {
            private int channel;
            private PCA9685 pwm;

            public IndirectedPwmOutput(int channel, PCA9685 pwm) {
                this.channel = channel;
                this.pwm = pwm;
            }

            protected override void SetEnabled(bool state) {
                if (state) {
                    this.SetValues(this.Frequency, this.DutyCycle);
                } else {
                    this.pwm.TurnOff(this.channel);
                }
            }

            protected override void SetValues(double frequency, double dutyCycle) {
                this.pwm.Frequency = (int)frequency;

                if (dutyCycle != 1.0) {
                    this.pwm.SetDutyCycle(this.channel, dutyCycle);
                }
                else {
                    this.pwm.TurnOn(this.channel);
                }
            }
        }
    }
}