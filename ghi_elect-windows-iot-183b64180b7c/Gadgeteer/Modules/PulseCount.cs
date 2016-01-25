using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class PulseCount : RotaryH1 {
		public override string Name => "PulseCount";

		public async Task<DigitalIO> CreateInput() {
			return await this.socket.CreateDigitalIOAsync(SocketPinNumber.Three);
		}
	}
}