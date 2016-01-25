using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GT = GHIElectronics.UAP.Gadgeteer;
using GTM = GHIElectronics.UAP.Gadgeteer.Modules;

namespace GHIElectronics.UAP.Examples.FEZCream {
	public sealed partial class MainPage : Page {
		private GTM.FEZCream mainboard;
		private GTM.LEDStrip ledStrip;
		private GTM.Button button;
		private DispatcherTimer timer;

		public MainPage() {
			this.InitializeComponent();

			this.Setup();
		}

		private async void Setup() {
			this.mainboard = await GT.Module.CreateAsync<GTM.FEZCream>();
			this.ledStrip = await GT.Module.CreateAsync<GTM.LEDStrip>(this.mainboard.GetProvidedSocket(4));
			this.button = await GT.Module.CreateAsync<GTM.Button>(this.mainboard.GetProvidedSocket(3));

			this.ProgramStarted();
		}

		private void ProgramStarted() {
			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(100);
			this.timer.Tick += this.OnTick;
			this.timer.Start();
		}

		private void OnTick(object sender, object e) {
			var pressed = this.button.IsPressed();

			this.ledStrip.SetAll(pressed);

			this.LedsTextBox.Text = pressed ? "On" : "Off";
		}
	}
}