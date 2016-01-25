Windows 10 IoT Core Hands-on Lab
========================================
ConnectTheDots will help you get tiny devices connected to Microsoft Azure, and to implement great IoT solutions taking advantage of Microsoft Azure advanced analytic services such as Azure Stream Analytics and Azure Machine Learning.

> This lab is stand-alone, but is used at Microsoft to accompany a presentation about Azure, Windows 10 IoT Core, and our IoT services. If you wish to follow this on your own, you are encouraged to do so. If not, consider attending a Microsoft-led IoT lab in your area.



In this lab you will use a [Raspberry Pi 2](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) device with [Windows 10 Iot Core](http://ms-iot.github.io/content/en-US/Downloads.htm) and a [FEZ HAT](https://www.ghielectronics.com/catalog/product/500) sensor hat. Using a Windows 10 Universal Application, the sensors get the raw data and format it into a JSON string. That string is then shuttled off to the Azure Event Hub, where it gathers the data and is then displayed in a chart using Power BI.
The JSON string is sent to the Event Hub in one of two ways: packaged into an AMQP message or in a REST packet. If you use AMQP, which is the recommended approach, you will need to have the AMQP port open on your firewall/router.
You will also create a simple console application that reads messages live from the Event Hub.

This lab includes the following tasks:

1. [Setup](#Task1)
	1. [Software](#Task11)
	2. [Devices](#Task12)
	3. [Azure Account](#Task13)
2. [Creating a Universal App](#Task2)
	1. [Read FEZ HAT sensors](#Task21)
	2. [Send telemetry data to Azure Event Hub](#Task22)
3. [Consuming the Event Hub data](#Task3)
	1. [Creating a console application to read the Azure Event Hub](#Task31)
	2. [Using Power BI](#Task32)
	3. [Consuming the Event Hub data from a Website](#Task33)
5. [Summary](#Summary)

<a name="Task1" />
##Setup##
The following sections are intended to setup your environment to be able to create and run your solutions with Windows 10 IoT Core. An email was sent prior to Data Culture Technical Day with the instructions below.

<a name="Task11" />
###Setting up your Software###
To setup your Windows 10 IoT Core development PC, you first need to install the following:

- Windows 10 (build 10240) or better

- Visual Studio 2015 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient.

	> **NOTE:** If you choose to install a different edition of VS 2015, make sure to do a **Custom** install and select the checkbox **Universal Windows App Development Tools** -> **Tools and Windows SDK**.

	You can validate your Visual Studio installation by selecting _Help > About Microsoft Visual Studio_. The required version of **Visual Studio** is 14.0.23107.0 D14Rel. The required version of **Visual Studio Tools for Universal Windows Apps** is 14.0.23121.00 D14OOB.

- Windows IoT Core Project Templates. You can download them from [here](https://visualstudiogallery.msdn.microsoft.com/06507e74-41cf-47b2-b7fe-8a2624202d36). Alternatively, the templates can be found by searching for Windows IoT Core Project Templates in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/) or directly from Visual Studio in the Extension and Updates dialog (Tools > Extensions and Updates > Online). 

- Make sure you’ve **enabled developer mode** in Windows 10 by following [these instructions](https://msdn.microsoft.com/library/windows/apps/xaml/dn706236.aspx).

<a name="Task12" />
###Setting up your Devices###

For this project, you will need the following:

- [Raspberry Pi 2 Model B](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) with power supply
- [GHI FEZ HAT](https://www.ghielectronics.com/catalog/product/500)
- Your PC running Windows 10, RTM build or later
- An Ethernet port on the PC, or an auto-crossover USB->Ethernet adapter like the [Belkin F4U047](http://www.amazon.com/Belkin-USB-Ethernet-Adapter-F4U047bt/dp/B00E9655LU/ref=sr_1_2).
- Standard Ethernet cable
- A good 16GB or 32GB Class 10 SD card. We recommend Samsung or Sandisk. Other cards will usually work, but tend to die more quickly.

To setup your devices perform the following steps:

1. Plug the **GHI FEZ HAT** into the **Raspberry Pi 2**. 

	![fezhat-connected-to-raspberri-pi-2](Images/fezhat-connected-to-raspberri-pi-2.png?raw=true)
	
	_The FEZ hat connected to the Raspberry Pi 2 device_

2. Get a Windows 10 IoT Core SD Card or download the **Windows 10 IoT Core** image as per the instructions on <https://ms-iot.github.io/content/en-US/win10/SetupRPI.htm>, be sure to follow the steps to mount the image, and run the installer on your development PC. If you already have the OS image on a card, you still need to follow this step to get the IoT Core Watcher and Visual Studio templates on to your PC.

3. Once you have the image on the card, insert the micro SD card in the Raspberry Pi device. (SD card will already be imaged in the Data Culture Labs)

5. Connect the Raspberry Pi to a power supply [optionally a keyboard, mouse and monitor] and use the Ethernet cable to connect your device and your development PC. You can do it by plugging in one end of the spare Ethernet cable to the extra Ethernet port on your PC, and the other end of the cable to the Ethernet port on your IoT Core device. (Do this using an on-board port or an auto-crossover USB->Ethernet interface.)

6. Wait for the OS to boot.

7. Run the **Windows 10 IoT Core Watcher** utility (installed in step 2) in your development PC and copy your Raspberry Pi IP address by right-clicking on the detected device and selecting **Copy IP Address**. (Download tool from here: http://go.microsoft.com/fwlink/?LinkId=619755)

	![windows-iot-core-watcher](Images/windows-iot-core-watcher.png?raw=true)

8. Launch an administrator PowerShell console on your local PC. The easiest way to do this is to type _powershell_ in the **Search the web and Windows** textbox near the Windows Start Menu. Windows will find **PowerShell** on your machine. Right-click the **Windows PowerShell** entry and select **Run as administrator**. The PS console will show.
 
	![Running Powershell as Administrator](Images/running-powershell-as-administrator.png?raw=true)

9. You may need to start the **WinRM** service on your desktop to enable remote connections. From the PS console type the following command:
 
	`net start WinRM`

9. From the PS console, type the following command, substituting '<_IP Address_>' with the IP value copied in prev:

	`Set-Item WSMan:\localhost\Client\TrustedHosts -Value <machine-name or IP Address>`

10.  Type **Y** and press **Enter** to confirm the change.

11. Now you can start a session with you Windows IoT Core device. From you administrator PS console, type:

	`Enter-PSSession -ComputerName <IP Address> -Credential localhost\Administrator`

12. In the credential dialog enter the following default password: `p@ssw0rd`.

	> **Note:** The connection process is not immediate and can take up to 30 seconds.

	If you successfully connected to the device, you should see the IP address of your device before the prompt.
	
	![Connected to the Raspberry using PS](Images/connected-to-the-raspberry-using-ps.png?raw=true)

####Renaming your Device ####

1. To change the _computer name_, use the **setcomputername** utility. In PowerShell, type the following command.

	`setcomputername <new-name>`
	
2. Reboot the device for the change to take effect. You can use the shutdown command as follows:

	`shutdown /r /t 0`

3. Finally, connect to the Raspberry Pi to the same network as your Windows 10 development PC.

You can also rename the device by using the web server, but certain functions are available only through PowerShell. Now that you understand how to connect through PowerShell, we'll use the web server to set up WiFi.

####Using WiFi on your Device ####
We will not provide compatible Wi-Fi dongles in the Data Culture Technical IoT Track labs, however feel free to refer back to these resources when out of the lab enviroment

1. To configure your device, run the **Windows 10 IoT Core Watcher** utility in your development PC and open the [web-based management](https://ms-iot.github.io/content/en-US/win10/tools/Webb.htm) application by right-clicking the detected device and selecting **Web Browser Here**.

	![windows-iot-core-watcher-open-browser](Images/windows-iot-core-watcher-open-browser.png?raw=true)
	
2. To use WiFi, you will need to provide Windows 10 IoT core with the WiFi network credentials.
	1. Enter **Administrator** for the username, and supply your password (_p@ssw0rd_ by default).
	2. Click **Networking** in the left-hand pane.
	3. Under **Available networks**, select network you would like to connect to and supply the connection credentials. Click **Connect** to initiate the connection.

	![networking-wifi-adapters](Images/networking-wifi-adapters.png?raw=true)
	
	
	> **Note:** You can find more info in [Using WiFi on your Windows 10 IoT Core device](https://ms-iot.github.io/content/en-US/win10/SetupWiFi.htm).
	
<a name="Task13" />
###Setting up your Azure Account###
You will need a Microsoft Azure subscription ([free trial subscription] (http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)

####Creating an Event Hub and a Shared Access Policy####

1. Enter the Azure portal, by browsing to http://manage.windowsazure.com
2. Create a new Event Hub. To do this, click **NEW**, then click **APP SERVICES**, then click **SERVICE BUS**, then **EVENT HUB**, and finally click **CUSTOM CREATE**.

	![creating a new app service](Images/creating a new app service.png?raw=true)
	_Creating a new Event Hub_

3. In the **Add a new Event Hub** screen, enter an **EVENT HUB NAME** e.g. _Windows10IoT_, select a **REGION** such as _West US_, and enter a **NAMESPACE NAME**.

	![creating event hub](Images/creating event hub.png?raw=true)
	
	_New Event Hub Settings_

	> **Note:** If you already have a service bus namespace on your suscription, you can select that one in this step.
	
4. Copy into Notepad the **EventHub Name** and **Event Hub Namespace Name** as yout will use these values later. Click the next arrow to continue.
5. In the **Configure Event Hub** screen, type _4_ in the **Partition Count** field; and in the **Message Retention** textbox, type _1_. Click the checkmark to continue.

	![configuring event hub](Images/configuring event hub.png?raw=true)
	
	_Configuring Event Hub_

6. Now, you will create a **Shared Access Policy**. To do this, after the service is activated, click on the newly created namespace. Then click the **Event Hubs** tab, and click the recently created service bus.
7. Click the **Configure** tab. In the **shared access policies** section, enter a **name** for the new policy, such as _manage_, in the **permissions** column, select **manage**.

	![creating a SAS](Images/creating a SAS.png?raw=true)

	_Creating a Shared Access Policy_
	
####Creating a Stream Analitycs Job####

To create a Stream Analytics Job, perform the following steps.

1. In the Azure Management Portal, click **NEW**, then click **DATA SERVICES**, then **STREAM ANALYTICS**, and finally **QUICK CREATE**.
2. Enter the **Job Name**, select a **Region**, such as _Central US_; and the enter a **NEW STORAGE ACCOUNT NAME** if this is the first storage account in the region used for Stream Analitycs; if not you have to select the one already used for that matter.
3. Click **CREATE A STREAM ANALITYCS JOB**.

	![Image 17](Images/createStreamAnalytics.png?raw=true)
	
	_Creating a Stream Analytics Job_

	
4. After the _Stream Analytics_ is created, in the left pane click **STORAGE**, then click the account you used in the previous step, and click **MANAGE ACCESS KEYS**. Write down the **STORAGE ACCOUNT NAME** and the **PRIMARY ACCESS KEY** as you will use those value later.
	
	![manage access keys](Images/manage-access-keys.png?raw=true)

	_Managing Access Keys_

<a name="Task2" />
##Creating a Universal App##
Now that the device is configured, you will see how to create an application to read the value of the FEZ HAT sensors, and then send those values to an Azure Event Hub.

<a name="Task21" />
###Read FEZ HAT sensors###
In order to get the information out of the hat sensors, you will take advantage of the [Developers' Guide](https://www.ghielectronics.com/docs/329/fez-hat-developers-guide "GHI Electronics FEZ HAT Developer's Guide") that [GHI Electronics](https://www.ghielectronics.com/ "GHI Electronics")  published.

1. Download the [zipped repository](https://bitbucket.org/ghi_elect/windows-iot/get/183b64180b7c.zip "Download FEZ HAT Developers' Guide repository"), extract the files in your file system or navigate to GHI folder within this GitHub repo. Then locate the _GHIElectronics.UAP.sln_ solution file (You must have **Visual Studio** installed in order to open the solution).

2. After opening the solution you will see several projects. The _Developers's Guide_ comes with examples of many of the shields provided by the company. Right-click the one named _GHIElectronics.UAP.Examples.FEZHAT_, and select **Set as Startup Project**.

	![Set FEZ HAT examples project as default](Images/set-fez-hat-examples-project-as-default.png?raw=true)
	
	_Setting the FEZ hat example as the default project_

	> **Note:** Now you will inspect the sample code to see how it works. Bear in mind that this example is intended to show all the available features of the shield, while in this lab you will use just a couple of them (temperature and light sensors).

3. Open the _MainPage.xaml.cs_ file and locate the **Setup** method.

	````C#
	private async void Setup()
	{
		this.hat = await GIS.FEZHAT.CreateAsync();

		this.hat.S1.SetLimits(500, 2400, 0, 180);
		this.hat.S2.SetLimits(500, 2400, 0, 180);

		this.timer = new DispatcherTimer();
		this.timer.Interval = TimeSpan.FromMilliseconds(100);
		this.timer.Tick += this.OnTick;
		this.timer.Start();
	}
	````

	In the first line, the program creates an instance of the FEZ HAT driver and stores it in a local variable. The driver is used for interacting with the shield. Then, after setting the limits for the servos (not used in this lab), a new **DispatchTimer** is created. A timer is often used in projects of this kind to poll the state of the sensors and perform operations. In this case the **OnTick** method is called every 100 miliseconds. You can see this method below.

	````C#
	private void OnTick(object sender, object e)
	{
		double x, y, z;

		this.hat.GetAcceleration(out x, out y, out z);

		this.LightTextBox.Text = this.hat.GetLightLevel().ToString("P2", CultureInfo.InvariantCulture);
		this.TempTextBox.Text = this.hat.GetTemperature().ToString("N2", CultureInfo.InvariantCulture);
		this.AccelTextBox.Text = $"({x:N2}, {y:N2}, {z:N2})";
		this.Button18TextBox.Text = this.hat.IsDIO18Pressed().ToString();
		this.Button22TextBox.Text = this.hat.IsDIO22Pressed().ToString();
		this.AnalogTextBox.Text = this.hat.ReadAnalog(GIS.FEZHAT.AnalogPin.Ain1).ToString("N2", CultureInfo.InvariantCulture);

		...
	}
	````
	This sample shows how to use the FEZ HAT driver to get data from the sensors. The data is then shown in the UI. (To see how the UI is designed check the _MainPage.xaml_ file)

4. To deploy the application to the Raspberry Pi, the device has to be on the same network as the development computer. To run the program, select **Remote device** in the _Debug Target_ dropdown list:

	![Deploy to Remote machine](Images/deploy-to-remote-machine.png?raw=true)
	
	_Deploying the application to a Remote Machine_

5. If the remote machine has not been selected before, the **Select Remote Connection** screen is displayed:

	![Remote Connection](Images/remote-connection.png?raw=true)
	
	_Setting up the Remote Connection_

6. If the device is not auto-detected, the Raspberry IP or name can be entered in the **Address** field. Otherwise, click the desired device. Change the **Authentication Mode** to **None**:

	![Set Authentication mode to None](Images/set-authentication-mode-to-none.png?raw=true)
	
	_Setting the Authentication Mode_

7. If you want to change the registered remote device it can be done in the project **Properties** page. Right-click the project name (_GHIElectronics.UAP.Examples.FEZHAT_) and select **Properties**. In the project Properties' page, select the **Debug** tab and enter the new device name or IP in the **Remote Machine** field.

	![Change Remote connection](Images/change-remote-connection.png?raw=true)
	
	_Changing the Remote Connection Settings_

	> **Note:** Clicking the **Find** button will display the **Remote Connection** screen.

8. If the program is successfully deployed to the device, the current value of the different sensors will be displayed on the screen. The shield leds will also be turned on and off alternately. If you don't have a screen connected to the _Raspberry_, you can add the following code to the **OnTick** method in order to show the value of the sensors in the Visual Studio **Output Console**.  (Insert the code after reading the sensors).

	````C#
	// Add diagnostics information
	System.Diagnostics.Debug.WriteLine("Light: {0}, Temp: {1}, Accel: {2}, Button18: {3}, Button22: {4}",
	    this.LightTextBox.Text,
	    this.TempTextBox.Text,
	    this.AccelTextBox.Text,
	    this.Button18TextBox.Text,
	    this.Button22TextBox.Text);
	````

<a name="Task22" />
###Send telemetry data to Azure Event Hub###

Now that you know how to read the FEZ HAT sensors, you will send that information to an Azure Event Hub. To do that, you will use an existing [ConnectTheDots](https://github.com/Azure/connectthedots "Connect the Dots website") example which shows how to connect a Windows 10 IoT core device to Azure and send sensor information.

1. Clone this repository and extract the files to a folder in your file system. Open the solution located in the **DataCultureIoT\Code\WindowsIoTCorePi2FezHat\Begin** folder.


2. Before running the application, you must set the **Azure Event Hub** connection information. In order to allow the application to send data to the **Event Hub**, the following information must be provided:

	- **Name**: Is the name given to the Event Hub. In the **Azure Management Portal** go to the **Service Bus** service and select the **Namespace** under which the Event Hub was created. Then click the **Event Hubs** tab and you will see the name of the Event Hub.
	
		![Get Event Hub name](Images/get-event-hub-name.png?raw=true)
	
	- **Service Bus Namespace**: Is the name of the Service Bus namespace at which your Event Hub belongs.
	- **Shared Access Policy**: Is the policy created to grant access to the Event Hub. It can be obtained from the Event Hub **Configuration** tab of the **Azure Management Portal**. From the previous step, click **Event Hub** and then click the **Configuration** tab. 
	
		![Get Event Hub Shared Access Policy](Images/get-event-hub-shared-access-policy.png?raw=true)
		
	The following information is required:
	 - _Shared Access Key Name_: The Policy Name
	 - _Shared Access Key_: The Policy Primary Key

	To set the Event Hub connection information go to the _MainPage_ method of the _MainPage.xaml.cs_ file and replace the following lines with the information gathered in the previous step:

	````C#
	ctdHelper = new ConnectTheDotsHelper(serviceBusNamespace: "windows10iot-ns",
		eventHubName: "windows10iot",
		keyName: "manage",
		key: "YOUR_KEY",
	````

	> **Note:** Write down the **Shared Access Key Name** and **Shared Access Key** values, as you will use them again later.
	
3. 	Before the app can be deployed you need to change the solution target platform, since the Raspberry Pi is based on the ARM architecture. To do that select **ARM** in the **Solution Platform** dropdown:

	![Set Solution Platform](Images/set-solution-platform.png?raw=true)
	
	_Setting the Solution Platform_

	As you can see in the sample code, the app uses a button in the UI to _simulate_ a real sensor. Every time the user presses the button the value of the sensor is incremented and that info is then sent to Azure.

	````C#
	private void Button_Click(object sender, RoutedEventArgs e)
	{
	    ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
	    sensor.value = counter++;
	    ctdHelper.SendSensorData(sensor);
	}
	````

4. For those devices lacking a monitor or display, the button will be replaced by a **Timer** so the same function can be performed without needing to click any button. If this is your case, perform the following steps.

	1. Replace the **Button_Click** method for this one:
	
		````C#
		private void Timer_Tick(object sender, object e)
		{
		    ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
		    sensor.value = counter++;
		    ctdHelper.SendSensorData(sensor);
		}
		````

	2. Then, replace the call to the **Button_Click** method in the **MainPage** method for the following piece of code:
	
		````C#
		//Button_Click(null, null);
		var timer = new DispatcherTimer();
		timer.Interval = TimeSpan.FromMilliseconds(500);
		timer.Tick += Timer_Tick;
		timer.Start();
		````

		Which will make the Timer tick twice in a second.
		
	3. Lastly, remove the button from the UI:

		<!-- mark:4 -->
		````XAML
		<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
			<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
				<TextBox x:Name="HelloMessage" Text="ConnectTheDots on IoT" Margin="10" IsReadOnly="True"/>
				<!--<Button x:Name="ClickMe" Content="Click Me!"  Margin="10" HorizontalAlignment="Center" Click="Button_Click"/>-->
			</StackPanel>
		</Grid>
		````


5. Before adding real sensor information you can run this code to see how the device connects to your **Azure Event Hub** and sends information. Run the application.

	![Debug Console output](Images/debug-console-output.png?raw=true)
	
	_Debugging in the Output Window_

6. After the app has been successfully deployed, it can start sending messages to the Event Hub. If the device is connected to a display and a mouse, click the **Click Me** button several times. 

	Although the information sent cannot be seen in the Azure Management Portal (you will see how to read that information in the following sections), you could check the Event Hub dashboard monitor to see how many messages has been processed.

	![Event Hub Dashboard](Images/event-hub-dashboard.png?raw=true)
	
	_Event Hub dashboard_

	> **Note:** The information in the Azure Portal Dashboard is not refreshed instantly and could take a couple of minutes.

7. If you followed the flow for devices without a screen attached, you will need to delete the timer created in that flow before continuing, because another timer will be created in the following steps so it will no longer needed. Remove the **Timer_Tick** method you created before and delete the following lines from the **MainPage** constructor

	````C#
	var timer = new DispatcherTimer();
	timer.Interval = TimeSpan.FromMilliseconds(500);
	timer.Tick += Timer_Tick;
	timer.Start();
	````

8. Now that the device is connected to the Azure Event Hub, add some real sensor information. First, you need to add a reference the FEZ HAT drivers. To do so, instead of manually adding the projects included in the GHI Developer's Guide, you will install the [NuGet package](https://www.nuget.org/packages/GHIElectronics.UWP.Shields.FEZHAT/ "FEZ HAT NuGet Package") that they provide. To do this, open the **Package Manager Console** (Tools/NuGet Package Manager/Package Manager Console) and execute the following command:

	````PowerShell
	PM> Install-Package GHIElectronics.UWP.Shields.FEZHAT
	````

	![Intalling GHI Electronics NuGet package](Images/intalling-ghi-electronics-nuget-package.png?raw=true)
	
	_Installing the FEZ hat Nuget package_

9. Add a reference to the FEZ HAT library namespace in the _MainPage.xaml.cs_ file:

	````C#
	using GHIElectronics.UWP.Shields;
	````

10. Declare the variables that will hold the reference to the following objects:
 - **hat**: Of the type **Shields.FEZHAT**, will contain the hat driver object that you will use to communicate with the FEZ hat through the Raspberry.
  - **timer**: of the type **DispatchTimer**, that will be used to poll the hat sensors at regular basis. For every _tick_ of the timer the value of the sensors will be get and sent to Azure.
  
	````C#
	FEZHAT hat;
	DispatcherTimer timer;
	````

11. You will add the following method to initialize the objects used to handle the communication with the hat. The **Timer_Tick** method will be defined next, and will be executed every 500 ms according to the value hardcoded in the **Interval** property.

	````C#
	private async void SetupHat()
	{
	    this.hat = await FEZHAT.CreateAsync();

	    this.timer = new DispatcherTimer();
	    
	    this.timer.Interval = TimeSpan.FromMilliseconds(500);
	    this.timer.Tick += this.Timer_Tick;

	    this.timer.Start();
	}
	````

12. The following method will be executed every time the timer ticks, and will poll the value of the hat's temperature sensor, send it to the Azure Event Hub and show the value obtained.

	````C#
	private void Timer_Tick(object sender, object e)
	{
	    // Temperature Sensor
	    var tSensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
	    tSensor.value = this.hat.GetTemperature();
	    this.ctdHelper.SendSensorData(tSensor);

	    this.HelloMessage.Text = "Temperature: " + tSensor.value.ToString("N2");

	    System.Diagnostics.Debug.WriteLine("Temperature: {0} °C", tSensor.value);
	}
	````

	The first statement gets the _ConnectTheDots_ sensor from the sensor collection already in place in the project (the temperature sensor was already included in the sample solution). Next, the temperature is polled out from the hat's temperature sensor using the driver object you initialized in the previous step. Then, the value obtained is sent to Azure using the _ConnectTheDots_'s helper object **ctdHelper** which is included in the sample solution.
	
	The last two lines are used to show the current value of the temperature sensor to the screen and the debug console respectively.
	
	> **Note:** To show the value of the sensor in the screen the app is using the Welcome message textbox. In the following steps the UI will be improved.

13. Before running the application add the call to the **SetupHat** method in the **MainPage** constructor:

	<!-- mark:8 -->
	````C#
	public MainPage()
	{
	    this.InitializeComponent();

	    ...
	    
	    // Initialize FEZ HAT shield
	    SetupHat();
	    
	}
	````

	
14. Now you are ready to run the application. Connect the Raspberry Pi with the FEZ HAT and run the application. After the app is deployed you will start to see in the output console (also in the screen) the value polled from the sensor. The information sent to Azure is also shown in the console.

	![Console output](Images/console-output.png?raw=true)
	
	_Output Window_

	You can also check that the messages were successfully received by going to the Azure Management console's Event Hub Dashboard
	
	![Event Hub Dashboard](Images/event-hub-dashboard.png?raw=true)
	
	_Event Hub Dashboard_
	
15. Now you will add the information from another sensor. To incorporate the data from the Light sensor you will need to add a new _ConnectTheDots_ sensor:
	<!-- mark:3 -->
	````C#
	// Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
	List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
		new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Light", "L"),
		new ConnectTheDotsSensor("d93ffbab-7dff-440d-a9f0-5aa091630201", "Temperature", "C"),
	};
	````

	You can also change the unit of measure used by the Temperature sensor from Farenheit (F) to Celsius (C) since the FEZ HAT measueres temperature in Celsius.
	
16. Next, add the following code to the **Timer_Tick** method to poll the data from the temperature sensor and send it to Azure.
	
	````C#
	// Light Sensor
	ConnectTheDotsSensor lSensor = ctdHelper.sensors.Find(item => item.measurename == "Light");
	lSensor.value = this.hat.GetLightLevel();

	this.ctdHelper.SendSensorData(lSensor);
	
	this.HelloMessage.Text = String.Format("Temperature: {0} °C, Light {1}", tSensor.value.ToString("N2"), lSensor.value.ToString("P2", System.Globalization.CultureInfo.InvariantCulture));

	System.Diagnostics.Debug.WriteLine("Temperature: {0} °C, Light {1}", tSensor.value.ToString("N2"), lSensor.value.ToString("P2", System.Globalization.CultureInfo.InvariantCulture));
	````

	> **Note:** If you want to poll the Light sensor data at a different rate, you will need to create another _DispatchTimer_ and set it to different interval.

	After running the app you will see the following output in the debug console. In this case two messages are sent to Azure in every timer tick:

	![Debug console after adding Light Sensor](Images/debug-console-after-adding-light-sensor.png?raw=true)
	
	_Output Window after Adding the Light Sensor_

17. (Optional) If you have a screen connected to the Raspberry Pi device, you can improve the existing UI to show the sensor information in the screen. To do that, replace the content of the **MainPage.xaml** file code for this one:

	````XAML
	<Page
		x:Class="ConnectTheDotsIoT.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:ConnectTheDotsIoT"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d">
		<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
			<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal">
				<StackPanel Width="100">
					<TextBlock Text="Light: " FontSize="26.667" Margin="10" />
					<TextBlock Text="Temp: " FontSize="26.667" Margin="10" />
				</StackPanel>
				<StackPanel>
					<StackPanel Margin="10">
					    <TextBlock Name="LightTextBox" FontSize="26.667" />
					    <ProgressBar Name="LightProgress" Value="0" Minimum="0" Maximum="1" Width="150"></ProgressBar>
					</StackPanel>
					<TextBlock Name="TempTextBox" FontSize="26.667" Margin="10" />
				</StackPanel>
			</StackPanel>
		</Grid>
	</Page>
	````

	The preceding view code incorporates a new textbox for every sensor and also a progress bar to graphically show the value of the Light sensor.
	
18. Lastly, you will update the **Timer_Tick** method to adapt it to the new UI structure. Replace the method with the following code:

	````C#
	private void Timer_Tick(object sender, object e)
	{
	    // Light Sensor
	    ConnectTheDotsSensor lSensor = ctdHelper.sensors.Find(item => item.measurename == "Light");
	    lSensor.value = this.hat.GetLightLevel();
	    
	    this.ctdHelper.SendSensorData(lSensor);
	    this.LightTextBox.Text = lSensor.value.ToString("P2", System.Globalization.CultureInfo.InvariantCulture);
	    this.LightProgress.Value = lSensor.value;

	    // Temperature Sensor
	    var tSensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
	    tSensor.value = this.hat.GetTemperature();
	    this.ctdHelper.SendSensorData(tSensor);

	    this.TempTextBox.Text = tSensor.value.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

	    System.Diagnostics.Debug.WriteLine("Temperature: {0} °C, Light {1}", tSensor.value.ToString("N2", System.Globalization.CultureInfo.InvariantCulture), lSensor.value.ToString("P2", System.Globalization.CultureInfo.InvariantCulture));
	}
	````

	![App UI Screenshot](Images/app-ui-screenshot.png?raw=true)
	
	_Universal app UI_
	
<a name="Task3" />
##Consuming the Event Hub data##
In the following sections you will see different ways of consuming the sensor data that is being uploaded to the Azure Event Hub. You will use a simple Console App, a Website, and Power BI to consume this data and to generate meaningful information.

<a name="Task31" />
###Creating a Console Application to read the Azure Event Hub###
In the previous task you created an Universal App that read data from the sensors and sent it to an Azure Event Hub. Now, you will create a simple console application that will read the information that is in the Event Hub.

> **Note:** This is an optional section. You can find the completed version of the solution created here in the _Code\EventHubReader_ folder. To make it work you will need to replace the placeholders in the **Program.cs** file with the corresponding values.

1. In Visual Studio, create a new solution by clicking **File** -> **New Project**.
2. In the Installed Templates pane, select **Visual C#**, and then choose **Console Application**. Enter a **Name** for the solution, select a **Location**, and click **OK**.

	![Creating a Console Application](Images/creating-a-console-application.png?raw=true)
	
	_Creating a new Console Application_

3. Right-click the project name, and click **Manage Nuget Packages**. Search for the **Microsoft.Azure.ServiceBus.EventProcessorHost** nuget package.

4. Select the nuget package and click **Install**. You will be prompted to accept the license agreement, click **I Accept** to do so. Wait until the nuget installation finishes.

	![Installing the EventProcessorHost Nuget Package](Images/installing-the-eventprocessorhost-nuget-packa.png?raw=true)
	
	_Installing the EventProcessorHost Nuget Package_

5. Add a new _EventProcessor_ class. To do this, right-click the project name, point to **Add**, and click **Class**. Enter _EventProcessor_ as the class **Name**, and click **Add**.

6. At the top of the class, add a the following using statements.

	````C#
	using System.Diagnostics;
	using Microsoft.ServiceBus.Messaging;
	using Newtonsoft.Json;
	````
7. Change the signature of the _EventProcessor_ class to be public and to implement the **IEventProcessor** interface. This is shown in the following code.

	````C#
	public class EventProcessor : IEventProcessor
	````

8.  Point to the **IEventProcessor** interface that should be underlined, click on the down arrow near the light bulb, and click **Implement interface**.

	![Implementing the IEventProcessor Interface](Images/implementing-the-ieventprocessor-interface.png?raw=true)
	
	_Implementing the IEventProcessor Interface_

9. Add the following variable definition inside the class body.

	````C#
	private Stopwatch stopWatch;
	````

	
10. Locate the **OpenAsync** method, and add the following code inside the method body.

	````C#
	public Task OpenAsync(PartitionContext context)
	{
	    Console.WriteLine("EventProcessor started");
	    this.stopWatch = new Stopwatch();
	    this.stopWatch.Start();
	    return Task.FromResult<object>(null);
	}
	````

	This logic will be run at the start of the EventProcessor and will instantiate a Stopwatch that will be used for saving 
	
11. Go to the **CloseAsync** method, and add the **async** keyword at the start of the method.

12. Add the following code inside this method to close the connection of the Event Processor.

	<!-- mark:3-7 -->
	````C#
	public async Task CloseAsync(PartitionContext context, CloseReason reason)
	{
	    Console.WriteLine("Shutting Down Event Processor");
	    if (reason == CloseReason.Shutdown)
	    {
	        await context.CheckpointAsync();
	    }
	}
	````
	
	This code will be run when the Event Processor is shut down, and will save checkpoint information, to resume from this point in a next session.

13.  Go to the **ProcessEventsAsync** method, and add the **async** keyword at the start of the method.

14. Add the following code inside this method to get the events from the event hub and write the information in the console.

	````C#
	public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
	{
		foreach (EventData eventData in messages)
		{
			dynamic eventBody = JsonConvert.DeserializeObject(Encoding.Default.GetString(eventData.GetBytes()));
			Console.WriteLine(
			    "Part.ID: {0}, Part.Key: {1}, Guid: {2}, Location: {3}, Measure Name: {4}, Unit of Measure: {5}, Value: {6}",
			    context.Lease.PartitionId, 
			    eventData.PartitionKey, 
			    eventBody.guid, 
			    eventBody.location, 
			    eventBody.measurename, 
			    eventBody.unitofmeasure, 
			    eventBody.value);
		}

		if (this.stopWatch.Elapsed > TimeSpan.FromMinutes(5))
		{
			await context.CheckpointAsync();
			this.stopWatch.Restart();
		}
	}
	````

	Messages are read by this method, deserialized, and sent to the console window. Additionally, there is logic that will set a checkpoint to resume processing from that point in case the worker stops. The checkpoint information will be saved in Azure Storage.
	
15. Open the _Program.cs_ file.
16. At the top of the file, add the following using statements.

	````C#
	using Microsoft.ServiceBus.Messaging;
	````
	
18. Inside the **Main** method, add the following code.
	<!-- mark:3-19 -->
	````C#
	public static void Main(string[] args)
	{
		string eventHubConnectionString = "Endpoint=sb://[EventHubNamespaceName].servicebus.windows.net/;SharedAccessKeyName=[SASKeyName];SharedAccessKey=[SASKey]";
		string eventHubName = "[EventHubName]";
		string storageAccountName = "[StorageAccountName]";
		string storageAccountKey = "[StorageAccountKey]";
		string storageConnectionString = string.Format(
			"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
				storageAccountName, 
				storageAccountKey);

		string eventProcessorHostName = Guid.NewGuid().ToString();
		EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
		Console.WriteLine("Registering the EventProcessor");
		eventProcessorHost.RegisterEventProcessorAsync<EventProcessor>().Wait();

		Console.WriteLine("Listening... Press enter to stop.");
		Console.ReadLine();
		eventProcessorHost.UnregisterEventProcessorAsync().Wait();
	}
	````

	The preceding code sets the configuration values required to connect to the event hub and the storage account, register the **EventProcessor** created previously, and will start listening for the messages in the event hub.
	
19. Replace all the placeholders in the previous code, values inside the square brackets **[]**, with their corresponding values that you took note in the set up section. The placeholders are:
	- [EventHubNamespaceName]
	- [SASKeyName]
	- [SASKey]
	- [EventHubName]
	- [StorageAccountName]
	- [StorageAccountKey]
	
20. Press **F5** to run the solution.

	![Reading the Event Hub](Images/reading-the-event-hub.png?raw=true)
	
	_The console app reading the event hub_
	
	You will see in the console app, all the values that are read from the event hub. These values are parsed by the **EventProcessor** and will show the values read from the sensors of the _Raspeberri Pi2_, light and temperature.
	
<a name="Task32" />
###Using Power BI###

Another (and more interesting) way to use the information received from the connected device/s is to get near real-time analysis using the **Microsoft Power BI** tool. In this section you will see how to configure this tool to get an online dashoard showing summarized information about the different sensors.

<a name="Task321" />
#### Setting up a Power BI account ####
If you don't have a Power BI account already, you will need to create one (a free account is enough to complete this lab). If you already have an account set you can skip this step.

1. Go to the [Power BI website](https://powerbi.microsoft.com/) and follow the sign-up process.

	> **Note:** At the moment this lab was written, only users with corporate email accounts are allowed to sign up. Free consumer email accounts (like Outlook, Hotmail, Gmail, Yahoo, etc.) can't be used.

2. You will be asked to enter your email address. Then a confirmation email will be sent. After following the confirmation link, a form to enter your personal information will be displayed. Complete the form and click Start.

	The preparation of your account will take several minutes, and when it's ready you will see an screen similar to the following:

	![Power BI Welcome screen](Images/power-bi-welcome-screen.png?raw=true)
	
	_Power BI welcome screen_

Now that your account is set, you are ready to set up the data source that will feed the Power BI dashboard.

<a name="Task3220" />
##### Create a Service Bus Consumer Group #####
In order to allow several consumer applications to read data from the Event Hub independently at their own pace a Consumer Group must be configured for each one. If all of the consumer applications (the Console application, Stream Analytics / Power BI, the Web site you will configure in the next section) read the data from the default consumer group, one application will block the others.

To create a new Consumer Group for the Event Hub that will be used by the Stream Analytics job you are about to configure, follow these steps:

- Open the Azure Management Portal, and select Service Bus
- Select the Namespace you used for your solution
- From the top menu, select Event Hubs
- From the left menu, select the Event Hub
- From the top menu, select Consumer Groups
- Select "+" Create at the bottom to create a new Consumer Group
- Give it the name "CG4PBI" and click OK

![Create Consumer Group](Images/create-consumer-group.png?raw=true)

<a name="Task322" />
#### Setting the data source ####
In order to feed the Power BI reports with the information gathered by the hats and to get that information in near real-time, **Power BI** supports **Azure Stream Analytics** outputs as data source. The following section will show how to configure the Stream Analytics job created in the Setup section to take the input from the Event Hub and push that summarized information to Power BI.

<a name="Task3221" />
##### Stream Analytics Input Setup #####
Before the information can be delivered to **Power BI**, it must be processed by a **Stream Analytics Job**. To do so, an input for that job must be provided. As the Raspberry devices are sending information to an Event Hub, it will be set as the input for the job.

1. Go to the Azure management portal and select the **Stream Analytics** service. There you will find the Stream Analytics job created during the _Azure services setup_. Click on the job to enter the Stream Analytics configuration screen.

	![Stream Analytics configuration](Images/stream-analytics-configuration.png?raw=true)
	
	_Stream Analytics Configuration_

2. As you can see, the Start button is disabled since the job is not configured yet. To set the job input click on the **INPUTS** tab and then in the **Add an input** button.

3. In the **Add an input to your job** popup, select the **Data Stream** option and click **Next**. In the following step, select the option **Event Hub** and click **Next**. Lastly, in the **Event Hub Settings** screen, provide the following information:

	- **Input Alias:** _TelemetryHub_
	- **Subscription:** Use Event Hub from Current Subscription (you can use an Event Hub from another subscription too by selecting the other option)
	- **Choose a Namespace:** _Windows10IoT-ns_ (or the namespace name selected during the Event Hub creation)
	- **Choose an Event Hub:** _Windows10IoT_ (or the name used during the Event Hub creation)
	- **Event Hub Policy Name:** _RootManageSharedAccessKey_
	- **Choose a Consumer Group:** _cg4pbi_

	![Stream Analytics Input configuration](Images/stream-analytics-input-configuration.png?raw=true)
	
	_Stream Analytics Input Configuration_

4. Click **Next**, and then **Complete** (leave the Serialization settings as they are).

<a name="Task3222" />
##### Stream Analytics Output Setup #####
The output of the Stream Analytics job will be Power BI.

1. To set up the output, go to the Stream Analytics Job's **OUTPUTS** tab, and click the **ADD AN INPUT** link.

2. In the **Add an output to your job** popup, select the **POWER BI** option and the click the **Next button**.

3. In the following screen you will setup the credentials of your Power BI account in order to allow the job to connect and send data to it. Click the **Authorize Now** link.

	![Stream Analytics Output configuration](Images/steam-analytics-output-configuration.png?raw=true)
	
	_Stream Analytics Output Configuration_

	You will be redirected to the Microsoft login page.

4. Enter your Power BI account email and click **Continue**, then select your account type (Work, School account, or Microsoft account) and then enter your password. If the authorization is successful, you will be redirected back to the **Microsoft Power BI Settings** screen.

5. In this screen you will enter the following information:

	- **Output Alias**: _PowerBI_
	- **Dataset Name**: _Raspberry_
	- **Table Name**: _Telemetry_
	- **Group Name**: _My Workspace_

	![Power BI Settings](Images/power-bi-settings.png?raw=true)
	
	_Power BI Settings_

6. Click the checkmark button to create the output.

<a name="Task3223" />
##### Stream Analytics Query configuration #####
Now that the job's inputs and outputs are already configured, the Stream Analytics Job needs to know how to transform the input data into the output data source. To do so, you will create a new Query.

1. Go to the Stream Analytics Job **QUERY** tab and replace the query with the following statement:

	````SQL
	SELECT
	    displayname,
	    location,
	    guid,
	    measurename,
	    unitofmeasure,
	    Max(timecreated) timecreated,
	    Avg(value) AvgValue
	INTO
	    [PowerBI]
	FROM
	    [TelemetryHUB] TIMESTAMP by timecreated
	GROUP BY
	    displayname, location, guid, measurename, unitofmeasure,
	    TumblingWindow(Second, 10)
	````

	The query takes the data from the input (using the alias defined when the input was created **TelemetryHUB**) and inserts into the output (**PowerBI**, the alias of the output) after grouping it using 10 seconds chunks.

2. Click on the **SAVE** button and **YES** in the confirmation dialog.

<a name="Task3234" />
##### Starting the Stream Analytics Job #####
Now that the job is configured, the **START** button is enabled. Click the button to start the job and then select the **JOB START TIME** option in the **START OUTPUT** popup. After clicking **OK** the job will be started.

Once the job starts it creates the Power BI datasource associated with the given subscription.

<a name="Task324" />
#### Setting up the Power BI dashboard ####
1. Now that the datasource is created, go back to your Power BI session, and go to **My Workspace** by clicking the **Power BI** link.

	After some minutes of the job running you will see that the dataset that you configured as an output for the Job, is now displayed in the Power BI workspace Datasets section.

	![Power BI new datasource](Images/power-bi-new-datasource.png?raw=true)

	_Power BI: New Datasource_
	
	> **Note:** The Power BI dataset will only be created if the job is running and if it is receiving data from the Event Hub input, so check that the Universal App is running and sending data to Azure to ensure that the dataset be created. To check if the Stream Analytics job is receiving and processing data you can check the Azure management Stream Analytics monitor.

2. Once the datasource becomes available you can start creating reports. To create a new Report click on the **Raspberry** datasource:

	![Power BI Report Designer](Images/power-bi-report-designer.png?raw=true)
	
	_Power BI: Report Designer_

	The Report designer will be opened showing the list of fields available for the selected datasource and the different visualizations supported by the tool.

3. To create the _Average Light by time_ report, select the following fields:

	- avgvalue
	- timecreated

	As you can see the **avgvalue** field is automatically set to the **Value** field and the **timecreated** is inserted as an axis. Now change the chart type to a **Line Chart**:

	![Select Line Chart](Images/select-line-chart.png?raw=true)
	
	_Selecting the Line Chart_

4. Then you will set a filter to show only the Light sensor data. To do so drag the **measurename** field to the **Filters** section and then select the **Light** value:

	![Select Report Filter](Images/select-report-filter.png?raw=true)
	![Select Light sensor values](Images/select-light-sensor-values.png?raw=true)
	
	_Selecting the Report Filters_

5. Now the report is almost ready. Click the **SAVE** button and set _Light by Time_ as the name for the report.

	![Light by Time Report](Images/light-by-time-report.png?raw=true)
	
	_Light by Time Report_

6. Now you will create a new Dashboard, and pin this report to it. Click the plus sign (+) next to the **Dashboards** section to create a new dashboard. Set _Raspberry Telemetry_ as the **Title** and press Enter. Now, go back to your report and click the pin icon to add the report to the recently created dashboard.

	![Pin a Report to the Dashboard](Images/pin-a-report-to-the-dashboard.png?raw=true)
	
	_Pinning a Report to the Dashboard_

1. To create a second chart with the information of the average Temperature follow these steps:
	1. Click on the **Raspberry** datasource to create a new report.
	2. Select the **avgvalue** field
	3. Drag the **measurename** field to the filters section and select **Temperature**
	4. Now change the visualization to a **gauge** chart:
	
		![Change Visualization to Gauge](Images/change-visualization-to-gauge.png?raw=true "Gauge visualization")
		
		_Gauge visualization_
	
	5. Change the **Value** from **Sum** to **Average**
	
		![Change Value to Average](Images/change-value-to-average.png?raw=true)
		
		_Change Value to Average_
		
		Now the Report is ready:
		
		![Gauge Report](Images/gauge-report.png?raw=true)
		
		_Gauge Report_
	
	6. Save and then Pin it to the Dashboard.

7. Following the same directions, create a _Temperature_ report and add it to the dashboard.
8. Lastly, edit the reports name in the dashboard by clicking the pencil icon next to each report.

	![Edit Report Title](Images/edit-report-title.png?raw=true)
	
	_Editing the Report Title_

	After renaming both reports you will get a dashboard similar to the one in the following screenshot, which will be automatically refreshed as new data arrives.

	![Final Power BI Dashboard](Images/final-power-bi-dashboard.png?raw=true)
	
	_Final Power BI Dashboard_

<a name="Task33" />
###Consuming the Event Hub data from a Website###
	
1. Locate the WebSite folder in this GitHub repo and download.
2. Find the **Assets** folder and copy the _Web.config_ file to inside the **ConnectTheDotsWebSite** folder of the Website.

	![Copying the web config to the website solution](Images/copying-the-web-config-to-the-website-solutio.png?raw=true)

	_Copying the **Web.config** to the WebSite solution_

3. Open the Web Site project (_ConnectTheDotsWebSite.sln_) in Visual Studio.
4. Edit the _Web.config_ file and add the corresponding values for the following keys:
	- **Microsoft.ServiceBus.EventHubDevices**: Name of the event hub.
	- **Microsoft.ServiceBus.ConnectionString**: Namespace endpoint connection string.
	- **Microsoft.ServiceBus.ConnectionStringDevices**: Event hub endpoint connection string.
	- **Microsoft.Storage.ConnectionString**: Storage account endpoint, in this case use the **storage account name** and **storage account primary key** to complete the endpoint.
5. Deploy the website to an Azure Web Site. To do this, perform the following steps.
	1. In Visual Studio, right-click on the project name and select **Publish**.
	2. Select **Microsoft Azure Web Apps**.
	
		![Selecting Publish Target](Images/selecting-publish-target.png?raw=true)
		
		_Selecting Publish target_
		
	3. Click **New** and use the following configuration.
	
		- **Web App name**: Pick something unique.
		- **App Service plan**: Select an App Service plan in the same region used for _Stream Analytics_ or create a new one using that region.
		- **Region**: Pick same region as you used for _Stream Analytics_.
		- **Database server**: No database.
		
	4. Click **Create**. After some time the website will be created in Azure.
	
		![Creating a New Web App](Images/creating-a-new-web-app.png?raw=true)
		
		_Creating a new Web App on Microsoft Azure_
		
	3. Click **Publish**.
	
		> **Note:** You might need to install **WebDeploy** extension if you are having an error stating that the Web deployment task failed. You can find WebDeploy [here](http://www.iis.net/downloads/microsoft/web-deploy).

	
6. After you deployed the site, it is required that you enable **Websockets**. To do this, perform the following steps:
	1. Browse to https://manage.windowsazure.com and select your _Azure Web Site_.
	2. Click the **Configure** tab.
	3. Then set **WebSockets** to **On** and click **Save**.
	
		![Enabling Web Sockets](Images/enabling-web-sockets.png?raw=true)
	
		_Enabling Web Sockets in your website_

7. Browse to your recently deployed Web Application. You will see something like in the following screenshot. There will be 2 real-time graphics representing the values read from the temperature and light sensors. Take into account that the Universal app must be running and sending information to the Event Hub in order to see the graphics.

	![Web Site Consuming the Event Hub Data](Images/web-site-consuming-the-event-hub-data.png?raw=true)
	
	_Web Site Consuming the Event Hub data_
	
	> **Note:** At the bottom of the page you should see “**Connected**.”. If you see “**ERROR undefined**” you likely didn’t enable **WebSockets** for the Azure Web Site.

	
<a name="Summary" />
##Summary##
In this lab, you have learned how to create a Universal app that reads from the sensors of a FEZ hat connected to a Raspberry Pi 2 running Windows 10 IoT Core, and upload those readings to an Azure Event Hub. You also learned how to read and consume the information in the Event Hub using Power BI to get near real-time analysis of the information gathered from the FEZ hat sensors and to create simple reports and how to consume it using a website.
