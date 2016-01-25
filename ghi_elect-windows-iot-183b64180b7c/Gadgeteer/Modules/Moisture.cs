using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class Moisture : Module {
		public override string Name => "Moisture";
		public override string Manufacturer => "GHI Electronics, LLC";

		private AnalogIO input;
		private DigitalIO enable;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
			this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Six, true);
		}

		public double GetReading() {
			return this.input.ReadProportion() / 1.6;
		}
	}
}