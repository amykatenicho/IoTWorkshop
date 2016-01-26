using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsIoTCorePi2FezHat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int counter = 0; // dummy temp counter value;

        ConnectTheDotsHelper ctdHelper;

        //DECLARE VARIABLES HERE

        /// <summary>
        /// Main page constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
            List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
                new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Temperature", "C"),
            };

            ctdHelper = new ConnectTheDotsHelper(iotDeviceConnectionString: "IOT_CONNECTION_STRING",
                organization: "YOUR_ORGANIZATION_OR_SELF",
                location: "YOUR_LOCATION",
                sensorList: sensors);

            //Button_Click(null, null);
            //var timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += Timer_Tick;
            //timer.Start();
        }

        /*private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
            sensor.value = counter++;
            ctdHelper.SendSensorData(sensor);
        }*/

        //ADD TIMER_TICK METHOD HERE


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // ADD CALL TO SETUP HAT ASYNC

        }

        //ENTER SETUP HAT ASYNC METHOD HERE


        //ENTER TELEMENTRYTIMER_TICK METHOD HERE


        //ENTER COMMANDSTIMER_TICK METHOD HERE
    }
}
