using GHIElectronics.UAP.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer.Modules {
	public class MotorDriverL298 : Module {
		private PwmOutput[] pwms;
		private DigitalIO[] directions;

		public override string Name => "MotorDriverL298";
		public override string Manufacturer => "GHI Electronics, LLC";

		public int Frequency { get; set; } = 25000;

		protected async override Task Initialize(ISocket parentSocket) {
			this.pwms = new PwmOutput[] {
				await parentSocket.CreatePwmOutputAsync(SocketPinNumber.Eight),
				await parentSocket.CreatePwmOutputAsync(SocketPinNumber.Seven)
			};

			this.directions = new DigitalIO[] {
				await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Six, false),
				await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Nine, false)
			};

			this.StopAll();
		}

		public enum Motor {
			Motor1 = 0,
			Motor2 = 1,
		}

		public void StopAll() {
			this.SetSpeed(Motor.Motor1, 0);
			this.SetSpeed(Motor.Motor2, 0);
		}

		public void SetSpeed(Motor motor, double speed) {
			if (speed > 1 || speed < -1) new ArgumentOutOfRangeException(nameof(speed), "Must be between -1 and 1.");
			if (motor != Motor.Motor1 && motor != Motor.Motor2) throw new ArgumentException(nameof(motor), "You must specify a valid motor.");

			if (speed == 1.0)
				speed = 0.99;

			if (speed == -1.0)
				speed = -0.99;

			this.directions[(int)motor].Write(speed < 0);
			this.pwms[(int)motor].Set(this.Frequency, speed < 0 ? 1 + speed : speed);
		}
	}
}