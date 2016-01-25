using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class CurrentACS712 : Module {
		public override string Name => "CurrentACS712";
		public override string Manufacturer => "GHI Electronics, LLC";

		public uint SampleCount { get; set; } = 10;

		private AnalogIO input;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Five);
		}

		public double ReadACCurrent() {
			var sum = 0.0;

			for (var i = 0; i < this.SampleCount; i++)
				sum += Math.Abs(this.input.ReadVoltage() - 2.083);

			sum /= this.SampleCount;
			sum /= 0.116;

			return sum;
		}

		public double ReadDCCurrent() {
			var sum = 0.0;

			for (var i = 0; i < this.SampleCount; i++) {
				sum += this.input.ReadVoltage();

				Task.Delay(2).Wait();
			}

			sum /= this.SampleCount;

			sum -= 2.083;
			sum /= 0.116;

			return sum;
		}
	}
}