using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;

namespace GHIElectronics.UAP.Gadgeteer.SocketInterfaces {
    public delegate Task<DigitalIO> DigitalIOCreator(Socket socket, SocketPinNumber pinNumber);
    public delegate Task<AnalogIO> AnalogIOCreator(Socket socket, SocketPinNumber pinNumber);
    public delegate Task<PwmOutput> PwmOutputCreator(Socket socket, SocketPinNumber pinNumber);
    public delegate Task<I2cDevice> I2cDeviceCreator(Socket socket);
    public delegate Task<SpiDevice> SpiDeviceCreator(Socket socket);
    public delegate Task<SerialDevice> SerialDeviceCreator(Socket socket);

    public class DigitalIOValueChangedEventArgs : EventArgs {
        public DateTime When { get; set; }
        public bool Value { get; set; }
    }

    public abstract class DigitalIO {
        private TypedEventHandler<DigitalIO, DigitalIOValueChangedEventArgs> valueChanged;

        protected abstract bool ReadInternal();
        protected abstract void WriteInternal(bool value);
        protected abstract void EnableInterrupt();
        protected abstract void DisableInterrupt();

        public abstract GpioPinDriveMode DriveMode { get; set; }
        public GpioPinEdge InterruptType { get; set; }

        public event TypedEventHandler<DigitalIO, DigitalIOValueChangedEventArgs> ValueChanged {
            add {
                if (this.valueChanged == null)
                    this.EnableInterrupt();

                this.valueChanged += value;
            }
            remove {
                this.valueChanged -= value;

                if (this.valueChanged == null)
                    this.DisableInterrupt();
            }
        }

        public bool Read() {
            this.DriveMode = GpioPinDriveMode.Input;

            return this.ReadInternal();
        }

        public void Write(bool value) {
            this.DriveMode = GpioPinDriveMode.Output;

            this.WriteInternal(value);
        }

        public bool Value {
            get {
                return this.ReadInternal();
            }
            set {
                this.WriteInternal(value);
            }
        }

        public void SetHigh() {
            this.WriteInternal(true);
        }

        public void SetLow() {
            this.WriteInternal(false);
        }

        public bool IsHigh() {
            return this.ReadInternal();
        }

        public bool IsLow() {
            return !this.ReadInternal();
        }

        protected void OnValueChanged(bool e) {
            if ((e && (this.InterruptType & GpioPinEdge.RisingEdge) != 0) || (!e && (this.InterruptType & GpioPinEdge.FallingEdge) != 0))
                this.valueChanged?.Invoke(this, new DigitalIOValueChangedEventArgs() { When = DateTime.UtcNow, Value = e });
        }
    }

    public abstract class AnalogIO {
        protected abstract double ReadInternal();
        protected abstract void WriteInternal(double voltage);

        public abstract double MaxVoltage { get; }
        public abstract GpioPinDriveMode DriveMode { get; set; }

        public double ReadVoltage() {
            this.DriveMode = GpioPinDriveMode.Input;

            return this.ReadInternal();
        }

        public void WriteVoltage(double value) {
            this.DriveMode = GpioPinDriveMode.Output;

            this.WriteInternal(value);
        }

        public double ReadProportion() {
            return this.ReadInternal() / this.MaxVoltage;
        }

        public void WriteProportion(double value) {
            this.WriteInternal(value / this.MaxVoltage);
        }

        public double Voltage {
            get {
                return this.ReadInternal();
            }
            set {
                this.WriteInternal(value);
            }
        }

        public double Proportion {
            get {
                return this.ReadProportion();
            }
            set {
                this.WriteProportion(value);
            }
        }
    }

    public abstract class PwmOutput {
        private bool enabled;
        private double frequency;
        private double dutyCycle;

        protected abstract void SetEnabled(bool state);
        protected abstract void SetValues(double frequency, double dutyCycle);

        public void Set(double frequency, double dutyCycle) {
            if (this.Enabled)
                this.SetValues(frequency, dutyCycle);

            this.frequency = frequency;
            this.dutyCycle = dutyCycle;
        }

        public bool Enabled {
            get {
                return this.enabled;
            }
            set {
                this.enabled = value;

                this.SetEnabled(value);
            }
        }

        public double Frequency {
            get {
                return this.frequency;
            }
            set {
                this.Set(value, this.dutyCycle);
            }
        }

        public double DutyCycle {
            get {
                return this.dutyCycle;
            }
            set {
                this.Set(this.frequency, value);
            }
        }
    }

    public abstract class I2cDevice {
        private byte[] write1;
        private byte[] write2;
        private byte[] read1;

        public abstract void Write(byte[] buffer);
        public abstract void Read(byte[] buffer);
        public abstract void WriteThenRead(byte[] writeBuffer, byte[] readBuffer);

        protected I2cDevice() {
            this.write1 = new byte[1];
            this.write2 = new byte[2];
            this.read1 = new byte[1];
        }

        public void WriteRegister(byte register, byte value) {
            this.write2[0] = register;
            this.write2[1] = value;

            this.Write(this.write2);
        }

        public void WriteRegisters(byte register, byte[] values) {
            if (values == null) throw new ArgumentNullException(nameof(values));

            var buffer = new byte[values.Length + 1];

            buffer[0] = register;

            Array.Copy(values, 0, buffer, 1, values.Length);

            this.Write(buffer);
        }

        public byte ReadRegister(byte register) {
            this.write1[0] = register;

            this.WriteThenRead(this.write1, this.read1);

            return read1[0];
        }

        public void ReadRegisters(byte register, byte[] values) {
            if (values == null) throw new ArgumentNullException(nameof(values));

            this.write1[0] = register;

            this.WriteThenRead(this.write1, values);
        }

        public byte[] ReadRegisters(byte register, int count) {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} must be positive.");

            var result = new byte[count];

            this.write1[0] = register;

            this.WriteThenRead(this.write1, result);

            return result;
        }
    }

    public abstract class SpiDevice {
        public abstract void WriteAndRead(byte[] writeBuffer, byte[] readBuffer);
        public abstract void WriteThenRead(byte[] writeBuffer, byte[] readBuffer);
        public abstract void Write(byte[] buffer);
        public abstract void Read(byte[] buffer);

        protected SpiDevice() {

        }
    }

    public abstract class SerialDevice {
        public abstract uint BaudRate { get; set; }
        public abstract ushort DataBits { get; set; }
        public abstract SerialHandshake Handshake { get; set; }
        public abstract SerialParity Parity { get; set; }
        public abstract SerialStopBitCount StopBits { get; set; }

        public abstract void Write(byte[] buffer);
        public abstract void Read(byte[] buffer);

        protected SerialDevice() {

        }

        public byte ReadByte() {
            var buffer = new byte[1];

            this.Read(buffer);

            return buffer[0];
        }

        public void WriteByte(byte data) {
            this.Write(new byte[] { data });
        }
    }
}