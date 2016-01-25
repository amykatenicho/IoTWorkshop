using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GIS = GHIElectronics.UAP.Shields;

namespace GHIElectronics.UAP.Examples.FEZUtility {
	public sealed partial class MainPage : Page {
		private GIS.FEZUtility utility;
		private DispatcherTimer timer;
		private bool next;

		public MainPage() {
			this.InitializeComponent();

			this.Setup();
		}

		private async void Setup() {
			this.utility = await GIS.FEZUtility.CreateAsync();
			this.utility.SetDigitalDriveMode(GIS.FEZUtility.DigitalPin.V00, GpioPinDriveMode.Output);

			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(100);
			this.timer.Tick += this.OnTick;
			this.timer.Start();
		}

		private void OnTick(object sender, object e) {
			this.LedsTextBox.Text = this.next.ToString();
			this.AnalogTextBox.Text = this.utility.ReadAnalog(GIS.FEZUtility.AnalogPin.A0).ToString("N2");

			this.utility.WriteDigital(GIS.FEZUtility.DigitalPin.V00, this.next);
			this.utility.SetPwmDutyCycle(GIS.FEZUtility.PwmPin.P0, this.next ? 1.0 : 0.0);
			this.utility.SetLedState(GIS.FEZUtility.Led.Led1, this.next);

			this.next = !this.next;
		}
	}
}