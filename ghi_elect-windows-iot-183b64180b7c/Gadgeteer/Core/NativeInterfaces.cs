using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using WD = Windows.Devices;

namespace GHIElectronics.UAP.Gadgeteer.NativeInterfaces {
    public class DigitalIO : SocketInterfaces.DigitalIO {
        private WD.Gpio.GpioPin pin;

        public static Task<WD.Gpio.GpioPin> CreateInterfaceAsync(int pinNumber) {
            return Task.FromResult(WD.Gpio.GpioController.GetDefault().OpenPin(pinNumber));
        }

        public DigitalIO(WD.Gpio.GpioPin pin) {
            this.pin = pin;
        }

        private void OnInterrupt(WD.Gpio.GpioPin sender, WD.Gpio.GpioPinValueChangedEventArgs e) => this.OnValueChanged(e.Edge == WD.Gpio.GpioPinEdge.RisingEdge);

        protected override void EnableInterrupt() => this.pin.ValueChanged += this.OnInterrupt;
        protected override void DisableInterrupt() => this.pin.ValueChanged -= this.OnInterrupt;

        protected override bool ReadInternal() => this.pin.Read() == WD.Gpio.GpioPinValue.High;
        protected override void WriteInternal(bool value) => this.pin.Write(value ? WD.Gpio.GpioPinValue.High : WD.Gpio.GpioPinValue.Low);

        public override WD.Gpio.GpioPinDriveMode DriveMode {
            get {
                return this.pin.GetDriveMode();
            }
            set {
                this.pin.SetDriveMode(value);
            }
        }
    }

    public class AnalogIO : SocketInterfaces.AnalogIO {
        public override double MaxVoltage => 3.3;

        protected override double ReadInternal() {
            throw new NotSupportedException();
        }

        protected override void WriteInternal(double voltage) {
            throw new NotSupportedException();
        }

        public override WD.Gpio.GpioPinDriveMode DriveMode {
            get {
                return WD.Gpio.GpioPinDriveMode.Input;
            }
            set {
                throw new NotSupportedException();
            }
        }
    }

    public class PwmOutput : SocketInterfaces.PwmOutput {
        protected override void SetEnabled(bool state) {
            throw new NotSupportedException();
        }

        protected override void SetValues(double frequency, double dutyCycle) {
            throw new NotSupportedException();
        }
    }

    public class I2cDevice : SocketInterfaces.I2cDevice {
        private WD.I2c.I2cDevice device;

        public static async Task<WD.I2c.I2cDevice> CreateInterfaceAsync(string deviceId, WD.I2c.I2cConnectionSettings connectionSettings) {
            var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.I2c.I2cDevice.GetDeviceSelector(deviceId));

            return await WD.I2c.I2cDevice.FromIdAsync(infos[0].Id, connectionSettings);
        }

        public I2cDevice(WD.I2c.I2cDevice device) {
            this.device = device;
        }

        public override void Write(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.device.Write(buffer);
        }

        public override void Read(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.device.Read(buffer);
        }

        public override void WriteThenRead(byte[] writeBuffer, byte[] readBuffer) {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            this.device.WriteRead(writeBuffer, readBuffer);
        }
    }

    public class SpiDevice : SocketInterfaces.SpiDevice {
        private WD.Spi.SpiDevice device;

        public static async Task<WD.Spi.SpiDevice> CreateInterfaceAsync(string deviceId, WD.Spi.SpiConnectionSettings connectionSettings) {
            var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.Spi.SpiDevice.GetDeviceSelector(deviceId));

            return await WD.Spi.SpiDevice.FromIdAsync(infos[0].Id, connectionSettings);
        }

        public SpiDevice(WD.Spi.SpiDevice device) {
            this.device = device;
        }

        public override void Write(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.device.Write(buffer);
        }

        public override void Read(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.device.Read(buffer);
        }

        public override void WriteAndRead(byte[] writeBuffer, byte[] readBuffer) {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            this.device.TransferFullDuplex(writeBuffer, readBuffer);
        }

        public override void WriteThenRead(byte[] writeBuffer, byte[] readBuffer) {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            this.device.TransferSequential(writeBuffer, readBuffer);
        }
    }

    public class SerialDevice : SocketInterfaces.SerialDevice {
        private WD.SerialCommunication.SerialDevice device;
        private DataWriter writer;
        private DataReader reader;

        public override uint BaudRate { get { return this.device.BaudRate; } set { this.device.BaudRate = value; } }
        public override ushort DataBits { get { return this.device.DataBits; } set { this.device.DataBits = value; } }
        public override WD.SerialCommunication.SerialHandshake Handshake { get { return this.device.Handshake; } set { this.device.Handshake = value; } }
        public override WD.SerialCommunication.SerialParity Parity { get { return this.device.Parity; } set { this.device.Parity = value; } }
        public override WD.SerialCommunication.SerialStopBitCount StopBits { get { return this.device.StopBits; } set { this.device.StopBits = value; } }

        public static async Task<WD.SerialCommunication.SerialDevice> CreateInterfaceAsync(string deviceId) {
            var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.SerialCommunication.SerialDevice.GetDeviceSelector(deviceId));

            return await WD.SerialCommunication.SerialDevice.FromIdAsync(infos[0].Id);
        }

        public SerialDevice(WD.SerialCommunication.SerialDevice device) {
            this.device = device;
            this.writer = new DataWriter(this.device.OutputStream);
            this.reader = new DataReader(this.device.InputStream);
        }

        public override void Write(byte[] buffer) {
            this.writer.WriteBytes(buffer);
        }

        public override void Read(byte[] buffer) {
            this.reader.ReadBytes(buffer);
        }
    }
}