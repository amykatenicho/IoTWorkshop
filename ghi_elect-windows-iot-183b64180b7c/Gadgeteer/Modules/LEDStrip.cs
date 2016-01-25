using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class LEDStrip : Module {
		public override string Name => "LED Strip";
		public override string Manufacturer => "GHI Electronics, LLC";

		private DigitalIO[] outputPins;

		protected async override Task Initialize(ISocket parentSocket) {
			this.outputPins = new DigitalIO[7];

			for (var i = 0; i < 7; i++)
				this.outputPins[i] = await parentSocket.CreateDigitalIOAsync((SocketPinNumber)(i + 3), false);
		}

		public void TurnAllOn() {
			foreach (var p in this.outputPins)
				p.SetHigh();
		}

		public void TurnAllOff() {
			foreach (var p in this.outputPins)
				p.SetLow();
		}

		public void TurnOn(int led) {
            if (led < 0 || led > 7) throw new ArgumentOutOfRangeException(nameof(led));

            this.outputPins[led].SetHigh();
		}

		public void TurnOff(int led) {
            if (led < 0 || led > 7) throw new ArgumentOutOfRangeException(nameof(led));

			this.outputPins[led].SetLow();
		}

        public void Set(int led, bool state) {
            if (led < 0 || led > 7) throw new ArgumentOutOfRangeException(nameof(led));

            this.outputPins[led].Write(state);
        }

		public void SetAll(bool state) {
			foreach (var p in this.outputPins)
				p.Write(state);
		}
	}
}