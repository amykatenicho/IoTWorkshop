using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class Button : Module {
        public override string Name => "Button";
		public override string Manufacturer => "GHI Electronics, LLC";

		private DigitalIO inputPin;
		private DigitalIO outputPin;

        public event TypedEventHandler<Button, object> Pressed;
        public event TypedEventHandler<Button, object> Released;

        protected async override Task Initialize(ISocket parentSocket) {
			this.outputPin = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, false);
			this.inputPin = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three, GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge);

            this.inputPin.ValueChanged += (s, e) => {
                if (e.Value) {
                    this.Released?.Invoke(this, null);
                }
                else {
                    this.Pressed?.Invoke(this, null);
                }
            };
		}

		public bool IsPressed() {
			return !this.inputPin.Read();
		}

        public void TurnOnLed() {
            this.outputPin.SetHigh();
        }

        public void TurnOffLed() {
            this.outputPin.SetLow();
        }

        public void SetLed(bool state) {
			this.outputPin.Write(state);
		}
	}
}