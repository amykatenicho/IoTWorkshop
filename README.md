Windows 10 IoT Core Hands-on Lab
========================================
ConnectTheDots will help you get tiny devices connected to Microsoft Azure, and to implement great IoT solutions taking advantage of Microsoft Azure advanced analytic services such as Azure Stream Analytics and Azure Machine Learning.

> This lab is stand-alone, but is used at Microsoft to accompany a presentation about Azure, Windows 10 IoT Core, and our IoT services. If you wish to follow this on your own, you are encouraged to do so. If not, consider attending a Microsoft-led IoT lab in your area.



In this lab you will use a [Raspberry Pi 2](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) device with [Windows 10 Iot Core](http://ms-iot.github.io/content/en-US/Downloads.htm) and a [FEZ HAT](https://www.ghielectronics.com/catalog/product/500) sensor hat. Using a Windows 10 Universal Application, the sensors get the raw data and format it into a JSON string. That string is then shuttled off to the [Azure IoT Hub](https://azure.microsoft.com/en-us/services/iot-hub/), where it gathers the data and is then displayed in a chart using Power BI.
The JSON string is sent to the IoT Hub in one of two ways: packaged into an AMQP message or in a REST packet. If you use AMQP, which is the recommended approach, you will need to have the AMQP port open on your firewall/router.

> **Note:** Although AMQP is the recommended approach, at the time this lab was written that protocol was not supported by the [Azure IoT Core SDK](https://github.com/Azure/azure-iot-sdks) for UWP (Universal Windows Platform) applications. However, it is expected to be implemented in a short time, since it's currently under development.  


This lab includes the following tasks:

1. [Setup](#Task1)
	1. [Software](#Task11)
	2. [Devices](#Task12)
	3. [Azure Account](#Task13)
	4. [Device Registration](#Task14)
2. [Creating a Universal App](#Task2)
	1. [Read FEZ HAT sensors](#Task21)
	2. [Send telemetry data to Azure IoT Hub](#Task22)
3. [Consuming the IoT Hub data](#Task3)
	1. [Using Power BI](#Task32)
	2. [Consuming the IoT Hub data from a Website](#Task33)
4. [Sending commands to your devices](#Task4)
5. [Summary](#Summary)

<a name="Task1" />
## Setup
The following sections are intended to setup your environment to be able to create and run your solutions with Windows 10 IoT Core.

<a name="Task11" />
### Setting up your Software
To setup your Windows 10 IoT Core development PC, you first need to install the following:

- Windows 10 (build 10240) or better

- Visual Studio 2015 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient.

	> **NOTE:** If you choose to install a different edition of VS 2015, make sure to do a **Custom** install and select the checkbox **Universal Windows App Development Tools** -> **Tools and Windows SDK**.

	You can validate your Visual Studio installation by selecting _Help > About Microsoft Visual Studio_. The required version of **Visual Studio** is 14.0.23107.0 D14Rel. The required version of **Visual Studio Tools for Universal Windows Apps** is 14.0.23121.00 D14OOB.

- Windows IoT Core Project Templates. You can download them from [here](https://visualstudiogallery.msdn.microsoft.com/55b357e1-a533-43ad-82a5-a88ac4b01dec). Alternatively, the templates can be found by searching for Windows IoT Core Project Templates in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/) or directly from Visual Studio in the Extension and Updates dialog (Tools > Extensions and Updates > Online).

- Make sure you’ve **enabled developer mode** in Windows 10 by following [these instructions](https://msdn.microsoft.com/library/windows/apps/xaml/dn706236.aspx).

- To register your devices in the Azure IoT Hub Service and to monitor the communication between them you need to install the [Azure Device Explorer](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md).

<a name="Task12" />
### Setting up your Devices

For this project, you will need the following:

- [Raspberry Pi 2 Model B](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) with power supply
- [GHI FEZ HAT](https://www.ghielectronics.com/catalog/product/500)
- Your PC running Windows 10, RTM build or later
- An Ethernet port on the PC, or an auto-crossover USB->Ethernet adapter like the [Belkin F4U047](http://www.amazon.com/Belkin-USB-Ethernet-Adapter-F4U047bt/dp/B00E9655LU/ref=sr_1_2).
- Standard Ethernet cable
- A good 16GB or 32GB Class 10 SD card. We recommend Samsung or Sandisk. Other cards will usually work, but tend to die more quickly.
- A WiFi dongle from the [list of devices that are currently supported by Windows 10 IoT Core on the Raspberry Pi 2](http://ms-iot.github.io/content/en-US/win10/SupportedInterfaces.htm#WiFi-Dongles)

To setup your devices perform the following steps:

1. Plug the **GHI FEZ HAT** into the **Raspberry Pi 2**.

	![fezhat-connected-to-raspberri-pi-2](Images/fezhat-connected-to-raspberri-pi-2.png?raw=true)

	_The FEZ hat connected to the Raspberry Pi 2 device_

2. Get a Windows 10 IoT Core SD Card or download the **Windows 10 IoT Core** image as per the instructions on <http://ms-iot.github.io/content/en-US/win10/RPI.htm>, be sure to follow the steps to mount the image, and run the installer on your development PC. If you already have the OS image on a card, you still need to follow this step to get the IoT Core Watcher and Visual Studio templates on to your PC.

3. Once you have the image on the card, insert the micro SD card in the Raspberry Pi device.

4. Connect the Raspberry Pi to a power supply, optionally a keyboard, mouse and monitor, and use the Ethernet cable to connect your device and your development PC. You can do it by plugging in one end of the spare Ethernet cable to the extra Ethernet port on your PC, and the other end of the cable to the Ethernet port on your IoT Core device. (Do this using an on-board port or an auto-crossover USB->Ethernet interface.)

	![windows-10-iot-core-fez-hat-hardware-setup](Images/windows-10-iot-core-fez-hat-hardware-setup.png)

5. Wait for the OS to boot.

6. Run the **Windows 10 IoT Core Watcher** utility (installed in step 2) in your development PC and copy your Raspberry Pi IP address by right-clicking on the detected device and selecting **Copy IP Address**.

	- Click the windows "**Start**" button
	- Type "**WindowsIoTCoreWatcher**" to pull it up in the search results
	- You may want to right click on the program name and select "**Pin to Start**" to pin it to your start screen for easy access
	- Press **Enter** to run it

	![windows-iot-core-watcher](Images/windows-iot-core-watcher.png?raw=true)

7. Launch an administrator PowerShell console on your local PC. The easiest way to do this is to type _powershell_ in the **Search the web and Windows** textbox near the Windows Start Menu. Windows will find **PowerShell** on your machine. Right-click the **Windows PowerShell** entry and select **Run as administrator**. The PS console will show.

	![Running Powershell as Administrator](Images/running-powershell-as-administrator.png?raw=true)

8. You may need to start the **WinRM** service on your desktop to enable remote connections. From the PS console type the following command:

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

#### Renaming your Device and Checking the Date and Time

1. To change the _computer name_, use the **setcomputername** utility. In PowerShell, type the following command.

	`setcomputername <new-name>`

1. The date and time on the Pi must be correct for the security tokens used to publish to Azure later in the lab to be valid.  To check the current time zone setting on the Pi, type:

	`tzutil /g`

1. If the time zone reported is not correct, you can find a list of valid time zones using (you may need to increase the buffer size on your powershell window):

	`tzutil /l`

1. To set the time zone, locate the id of the time zone you want from the step above, then use:

	`tzutil /s "Your TimeZone Name"

	For example, for "Pacific Standard Time"

	`tzutil /s "Pacific Standard Time"

1. To check the date on the Raspberry Pi, type

	`Get-Date`

1. If the date or time is incorrect, use the `Set-Date` utility

	`Set-Date "mm/dd/yy hh:mm:ss AM/PM"`

	For Example, if it was 12:15 pm on January 3rd, 2016:

	`Set-Date "01/03/16 12:15 PM"`

2. **Reboot the device for the change to take effect**. You can use the shutdown command as follows:

	`shutdown /r /t 0`

3. Finally, connect to the Raspberry Pi to the same network as your Windows 10 development PC.

You can also rename the device and set the time zone by using the web server, but certain functions, like actually changing the date and time, are currently available only through PowerShell. Now that you understand how to connect through PowerShell, we'll use the web server to set up WiFi.

#### Using WiFi on your Device

1. To configure your device, run the **Windows 10 IoT Core Watcher** utility in your development PC and open the [web-based management](http://ms-iot.github.io/content/en-US/win10/tools/DevicePortal.htm) application by right-clicking the detected device and selecting **Web Browser Here**.  To launch the "Windows 10 IoT Core Watcher" utility:

	![windows-iot-core-watcher-open-browser](Images/windows-iot-core-watcher-open-browser.png?raw=true)

2. To use WiFi, you will need to provide Windows 10 IoT core with the WiFi network credentials.
	1. Enter **Administrator** for the username, and supply your password (_**p@ssw0rd**_ by default).
	2. Click **Networking** in the left-hand pane.
	3. Under **Available networks**, select network you would like to connect to and supply the connection credentials. Click **Connect** to initiate the connection.  Make sure the "**Create profile (auto re-connect)**" check box is **checked** so that the WiFi network will reconnect automatically if the Raspberry Pi reboots. 

	![networking-wifi-adapters](Images/networking-wifi-adapters.png?raw=true)


	> **Note:** You can find more info in [Using WiFi on your Windows 10 IoT Core device](https://ms-iot.github.io/content/en-US/win10/SetupWiFi.htm).

<a name="Task13" />
### Setting up your Azure Account
You will need a Microsoft Azure subscription ([free trial subscription] (http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)

#### Creating an IoT Hub

1. Enter the Azure portal, by browsing to http://portal.azure.com
2. Create a new IoT Hub. To do this, click **New** in the jumpbar, then click **Internet of Things**, then click **Azure IoT Hub**.

	![Creating a new IoT Hub](Images/creating-a-new-iot-hub.png?raw=true "Createing a new IoT Hub")

	_Creating a new IoT Hub_

3. Configure the **IoT hub** with the desired information:
 - Enter a **Name** for the hub e.g. _iot-sample_,
 - Select a **Pricing and scale tier** (_F1 Free_ tier is enough),
 - Create a new resource group, or select and existing one. For more information, see [Using resource groups to manage your Azure resources](https://azure.microsoft.com/en-us/documentation/articles/resource-group-portal/).
 - Select the **Region** such as _East US_ where the service will be located.

	![new iot hub settings](Images/new-iot-hub-settings.png?raw=true "New IoT Hub settings")

	_New IoT Hub Settings_

4. It can take a few minutes for the IoT hub to be created. Once it is ready, open the blade of the new IoT hub, take note of the URI and select the key icon at the top to access to the shared access policy settings:

	![IoT hub shared access policies](Images/iot-hub-shared-access-policies.png?raw=true)

5. Select the Shared access policy called **iothubowner**, and take note of the **Primary key** and **connection string** in the right blade.  You should copy these into a text file for future use. 

	![Get IoT Hub owner connection string](Images/get-iot-hub-owner-connection-string.png?raw=true)

#### Creating a Stream Analitycs Job

To create a Stream Analytics Job, perform the following steps.

1. Currently the new Azure Portal doesn't support all of the features of Stream Analytics required for this lab.  For that reason, the Stream Analaytics configuration needs to be done in the **classic portal**.   To open the classic portal, in your browser navigate to https://manage.windowsazure.com and login in with your Azure Subscription's credentials. 

1. In the Azure Management Portal, click **NEW**, then click **DATA SERVICES**, then **STREAM ANALYTICS**, and finally **QUICK CREATE**.

2. Enter the **Job Name**, select a **Region**, such as _East US_ (if it is an option, create the Stream Analytics Job in the same region as your IoT Hub) and the enter a **NEW STORAGE ACCOUNT NAME** if this is the first storage account in the region used for Stream Analitycs, if not you have to select the one already used for that region.

3. Click **CREATE A STREAM ANALITYCS JOB**.

	![Creating a Stream Analytics Job](Images/createStreamAnalytics.png?raw=true)

	_Creating a Stream Analytics Job_

4. After the _Stream Analytics_ job is created, in the left pane click **STORAGE**, then click the account you used in the previous step, and click **MANAGE ACCESS KEYS**. As with the IoT Hub details, copy the **STORAGE ACCOUNT NAME** and the **PRIMARY ACCESS KEY** into a text file as you will use those values later.

	![manage access keys](Images/manage-access-keys.png?raw=true)

	_Managing Access Keys_

<a name="Task14" />
### Registering your device
You must register your device in order to be able to send and receive information from the Azure IoT Hub. This is done by registering a [Device Identity](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-devguide/#device-identity-registry) in the IoT Hub.

1. Open the Device Explorer app (C:\Program Files (x86)\Microsoft\DeviceExplorer\DeviceExplorer.exe) and fill the **IoT Hub Connection String** field with the connection string of the IoT Hub you created in previous steps and click on **Update**.

	![Configure Device Explorer](Images/configure-device-explorer.png?raw=true)

2. Go to the **Management** tab and click on the **Create** button. The Create Device popup will be displayed. Fill the **Device ID** field with a new Id for your device (_myFirstDevice_ for example) and click on Create:

	![Creating a Device Identity](Images/creating-a-device-identity.png?raw=true)

3. Once the device identity is created, it will be displayed in the grid. Right click on the identity you just created, select **Copy connection string for selected device** and take note of the value copied to your clipboard, since it will be required to connect your device with the IoT Hub.

	![Copying Device connection information](Images/copying-device-connection-information.png?raw=true)

	> **Note:** The device identities registration can be automated using the Azure IoT Hubs SDK. An example of how to do that can be found [here](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/#create-a-device-identity).


<a name="Task2" />
## Creating a Universal App
Now that the device is configured, you will see how to create an application to read the value of the FEZ HAT sensors, and then send those values to an Azure IoT Hub.

<a name="Task21" />
### Read FEZ HAT sensors
In order to get the information out of the hat sensors, you will take advantage of the [Developers' Guide](https://www.ghielectronics.com/docs/329/fez-hat-developers-guide "GHI Electronics FEZ HAT Developer's Guide") that [GHI Electronics](https://www.ghielectronics.com/ "GHI Electronics")  published.

1. Download the [zipped repository](https://bitbucket.org/ghi_elect/windows-iot/get/183b64180b7c.zip "Download FEZ HAT Developers' Guide repository")

1. Unblock the .zip file before extracting it. Unblocking the .zip file will keep Visual Studio from prompting you about "Trustworthy Sources". To Unblock the .zip file:

	- Right click on the downloaded .zip file
	- Select "**Properties**" from the popup menu. 
	- In the "**Properties**" window, on the "**General**" tab, turn on the checkbox labeled "**Unblock**"
	- Click "**OK**"

1. Extract the files in your file system and locate the _GHIElectronics.UAP.sln_ solution file (You must have **Visual Studio** installed in order to open the solution).

2. After opening the solution you will see several projects. The _Developers's Guide_ comes with examples of many of the shields provided by the company. Right-click the one named _GHIElectronics.UAP.Examples.FEZHAT_, and select **Set as Startup Project**.

	![Set FEZ HAT examples project as default](Images/set-fez-hat-examples-project-as-default.png?raw=true)

	_Setting the FEZ hat example as the default project_

1. Ensure that the target platform for the project is set to "**ARM**":

	![arm-target-platform](Images/arm-target-platform.png?raw=true)

1. Build the solution to restore the NuGet packages, and make sure it builds:

	![ghifezhat-build-solution](Images/ghifezhat-build-solution.png?raw=true)

	![ghifezhat-build-succeeded](Images/ghifezhat-build-succeeded.png?raw=true)


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

5. If a remote machine has not been selected before, the **Select Remote Connection** screen will be displayed:

	![Remote Connection](Images/remote-connection.png?raw=true)

	_Setting up the Remote Connection_

6. If the device is not auto-detected, the Raspberry Pi IP or name can be entered in the **Address** field. Otherwise, click the desired device. Change the **Authentication Mode** to **Universal (Unencrypted Protocol)**:

	![Set Authentication mode to Universal](Images/set-authentication-mode-to-universal.png?raw=true)

	_Setting the Authentication Mode_

7. If you want to change the registered remote device later it can be done in the project **Properties** page. Right-click the project name (_GHIElectronics.UAP.Examples.FEZHAT_) and select **Properties**. In the project Properties' page, select the **Debug** tab and enter the new device name or IP in the **Remote Machine** field.

	![Change Remote connection](Images/change-remote-connection.png?raw=true)

	_Changing the Remote Connection Settings_

	> **Note:** Clicking the **Find** button will display the **Remote Connection** screen.

1. If you don't have a screen connected to the _Raspberry_, you can add the following code to the **OnTick** method in order to show the value of the sensors in the Visual Studio **Output Console**.  (Insert the code after reading the sensors).

	````C#
	// Add diagnostics information
	System.Diagnostics.Debug.WriteLine("Light: {0}, Temp: {1}, Accel: {2}, Button18: {3}, Button22: {4}",
	    this.LightTextBox.Text,
	    this.TempTextBox.Text,
	    this.AccelTextBox.Text,
	    this.Button18TextBox.Text,
	    this.Button22TextBox.Text);
	````


1. Click the debug button to start the deployment to the Raspberry Pi.  The first deployment will take some time as the remote debug tools, frameworks, and your code all need to be deployed.  This could take up to a couple of minutes to completely deploy.  You can monitor the status in the Visual Studio "**Output**" window.

	![debug-ghifezhat](Images/debug-ghifezhat.png?raw=true)

8. If the program is successfully deployed to the device, the current value of the different sensors will be displayed on the screen. The shield leds will also be turned on and off alternately. In addition, if you added the Debug.Writeline code above to the OnTick method, the "**Output**" window will display sensor data:

	![ghifezhat-debug-output](Images/ghifezhat-debug-output.png?raw=true)

<a name="Task22" />
### Send telemetry data to the Azure IoT Hub

Now that you know how to read the FEZ HAT sensors data, you will send that information to an Azure IoT Hub. To do that, you will use an existing project located in the **Code\Begin** folder. This project is based on the [ConnectTheDots Raspberry Pi with Windows 10 IoT sample project](https://github.com/Azure/connectthedots/tree/master/Devices/DirectlyConnectedDevices/WindowsIoTCorePi2) but using the [Azure IoT SDK](https://github.com/Azure/azure-iot-sdks) to connect with Azure, instead of using an Event Hub.

1. Open the solution located in the **Code\Begin** folder.

2. Before running the application, you must set the **Device connection** information. Go to the _MainPage_ method of the _MainPage.xaml.cs_ file and replace **IOT_CONNECTION_STRING** with your device connection string, obtained in previous steps using the Device Explorer app:

	````C#
	ctdHelper = new ConnectTheDotsHelper(iotDeviceConnectionString: "IOT_CONNECTION_STRING",
	    organization: "YOUR_ORGANIZATION_OR_SELF",
	    location: "YOUR_LOCATION",
	    sensorList: sensors);
	````

	![Copying Device connection information](Images/copying-device-connection-information.png?raw=true)

	> **Note:** An **Organization** and **Location** may also be provided. Those values will be part of the telemetry message payload, and could be used to get a better classification of the data received.

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

		Which will make the Timer ticks twice a second.

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


5. Before adding real sensor information you can run this code to see how the device connects to your **Azure IoT Hub** and sends information. Run the application.

	![Debug Console output](Images/debug-console-output.png?raw=true)

	_Debugging in the Output Window_

6. After the app has been successfully deployed, it can start sending messages to the IoT Hub. If the device is connected to a display and a mouse, click the **Click Me** button several times.

	The information being sent can be monitored using the Device Explorer application. Run the application and go to the **Data** tab and select the name of the device you want to monitor (_myFirstDevice_ in your case), then click on **Monitor**

	![Monitoring messages sent](Images/monitoring-messages-sent.png?raw=true)

	> **Note:** If the Device Explorer hub connection is not configured yet, you can follow the instructions explained in the [Registering your device](#Task14) section

7. If you followed the instructions for devices without a screen attached, you will need to delete the timer created in that flow before you continue. A new timer will be created in the next steps replacing the previous one. Remove the **Timer_Tick** method you created before and delete the following lines from the **MainPage** constructor

	````C#
	var timer = new DispatcherTimer();
	timer.Interval = TimeSpan.FromMilliseconds(500);
	timer.Tick += Timer_Tick;
	timer.Start();
	````

8. Now that the device is connected to the Azure IoT Hub, add some real sensor information. First, you need to add a reference the FEZ HAT drivers. To do so, instead of manually adding the projects included in the GHI Developer's Guide, you will install the [NuGet package](https://www.nuget.org/packages/GHIElectronics.UWP.Shields.FEZHAT/ "FEZ HAT NuGet Package") that they provide. To do this, open the **Package Manager Console** (Tools/NuGet Package Manager/Package Manager Console) and execute the following command:

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
 - **hat**: Of the type **Shields.FEZHAT**, will contain the hat driver object that you will use to communicate with the FEZ hat through the Raspberry Pi hardware.
  - **telemetryTimer**: of the type **DispatchTimer**, that will be used to poll the hat sensors at regular basis. For every _tick_ of the timer the value of the sensors will be get and sent to Azure.

	````C#
	FEZHAT hat;
	DispatcherTimer telemetryTimer;
	````

11. You will add the following method to initialize the objects used to handle the communication with the hat. The **TelemetryTimer_Tick** method will be defined next, and will be executed every 500 ms according to the value hardcoded in the **Interval** property.

	````C#
	private async Task SetupHatAsync()
	{
	    this.hat = await FEZHAT.CreateAsync();

	    this.telemetryTimer = new DispatcherTimer();

	    this.telemetryTimer.Interval = TimeSpan.FromMilliseconds(500);
	    this.telemetryTimer.Tick += this.TelemetryTimer_Tick;

	    this.telemetryTimer.Start();
	}
	````

12. The following method will be executed every time the timer ticks, and will poll the value of the hat's temperature sensor, send it to the Azure IoT Hub and show the value obtained.

	````C#
	private void TelemetryTimer_Tick(object sender, object e)
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

13. Before running the application you need to add the call to the **SetupHatAsync** method. Replace the **Page_Loaded** method with this block of code:

	````C#
	private async void Page_Loaded(object sender, RoutedEventArgs e)
	{
		// Initialize FEZ HAT shield
		await SetupHatAsync();
	}
	````

	> Note that the **async** modifier was added to the event handler to properly handle the asynchronous call to the FEZ HAT initialization method.

14. Now you are ready to run the application. Connect the Raspberry Pi with the FEZ HAT and run the application. After the app is deployed you will start to see in the output console (also in the screen) the values polled from the sensor. The information sent to Azure is also shown in the console.

	![Console output](Images/console-output.png?raw=true)

	_Output Window_

	You can also check that the messages were successfully received by monitoring them using the Device Explorer

	![Telemetry messages received](Images/telemetry-messages-received.png?raw=true)

15. Now you will add the information from another sensor. To incorporate the data from the Light sensor you will need to add a new _ConnectTheDots_ sensor:
	<!-- mark:3 -->
	````C#
	// Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
	List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
		new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Light", "L"),
		new ConnectTheDotsSensor("d93ffbab-7dff-440d-a9f0-5aa091630201", "Temperature", "C"),
	};
	````

16. Next, add the following code to the **TelemetryTimer_Tick** method to poll the data from the temperature sensor and send it to Azure.

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
		x:Class="WindowsIoTCorePi2FezHat.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:WindowsIoTCorePi2FezHat"
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

18. Lastly, you will update the **TelemetryTimer_Tick** method to adapt it to the new UI structure. Replace the method with the following code:

	````C#
	private void TelemetryTimer_Tick(object sender, object e)
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
## Consuming the IoT Hub data
You have seen how to use the Device Explorer to peek the data being sent to the Azure IoT Hub. However, the Azure IoT suite offers many different ways to generate meaningful information from the data gathered by the devices. In the following sections you will explore two of them: You will see how the Azure Services Bus Messaging system can be used in a Website (part of the ConnectTheDots project), and how to use Azure Stream Analytics in combination with Microsoft Power BI to consume the data and to generate meaningful reports.

<a name="Task32" />
### Using Power BI

One of the most interesting ways to use the information received from the connected device/s is to get near real-time analysis using the **Microsoft Power BI** tool. In this section you will see how to configure this tool to get an online dashboard showing summarized information about the different sensors.

<a name="Task321" />
#### Setting up a Power BI account
If you don't have a Power BI account already, you will need to create one (a free account is enough to complete this lab). If you already have an account set you can skip this step.

1. Go to the [Power BI website](https://powerbi.microsoft.com/) and follow the sign-up process.

	> **Note:** At the moment this lab was written, only users with corporate email accounts are allowed to sign up. Free consumer email accounts (like Outlook, Hotmail, Gmail, Yahoo, etc.) can't be used.

2. You will be asked to enter your email address. Then a confirmation email will be sent. After following the confirmation link, a form to enter your personal information will be displayed. Complete the form and click Start.

	The preparation of your account will take several minutes, and when it's ready you will see a screen similar to the following:

	![Power BI Welcome screen](Images/power-bi-welcome-screen.png?raw=true)

	_Power BI welcome screen_

Now that your account is set, you are ready to set up the data source that will feed the Power BI dashboard.

<a name="Task3220" />
##### Create a Service Bus Consumer Group
In order to allow several consumer applications to read data from the IoT Hub independently at their own pace a Consumer Group must be configured for each one. If all of the consumer applications (the Device Explorer, Stream Analytics / Power BI, the Web site you will configure in the next section) read the data from the default consumer group, one application will block the others.

To create a new Consumer Group for the IoT Hub that will be used by the Stream Analytics job you are about to configure, follow these steps:

- Open the Azure Portal (https://portal.azure.com/), and select the IoT Hub you created.
- From the settings blade, click on **Messaging**
- At the bottom of the Messaging blade, type the name of the new Consumer Group "PowerBI"
- From the top menu, click on the Save icon

![Create Consumer Group](Images/create-consumer-group.png?raw=true)

<a name="Task322" />
#### Setting the data source
In order to feed the Power BI reports with the information gathered by the hats and to get that information in near real-time, **Power BI** supports **Azure Stream Analytics** outputs as data source. The following section will show how to configure the Stream Analytics job created in the Setup section to take the input from the IoT Hub and push that summarized information to Power BI.

<a name="Task3221" />
##### Stream Analytics Input Setup
Before the information can be delivered to **Power BI**, it must be processed by a **Stream Analytics Job**. To do so, an input for that job must be provided. As the Raspberry devices are sending information to an IoT Hub, it will be set as the input for the job.

1. Go to the classic [Azure management portal](Stream Analytics Input ) (https://manage.windowsazure.com) and select the **Stream Analytics** service. There you will find the Stream Analytics job created during the _Azure services setup_. Click on the job to enter the Stream Analytics configuration screen.

	![Stream Analytics configuration](Images/stream-analytics-configuration.png?raw=true)

	_Stream Analytics Configuration_

2. As you can see, the Start button is disabled since the job is not configured yet. To set the job input click on the **INPUTS** tab and then in the **Add an input** button.

3. In the **Add an input to your job** popup, select the **Data Stream** option and click **Next**. In the following step, select the option **IoT Hub** and click **Next**. Lastly, in the **IoT Hub Settings** screen, provide the following information:

	- **Input Alias:** _TelemetryHub_
	- **Subscription:** Use IoT Hub from Current Subscription (you can use an Event Hub from another subscription too by selecting the other option)
	- **Choose an IoT Hub:** _iot-sample_ (or the name used during the IoT Hub creation)
	- **IoT Hub Shared Access Policy Name:** _iothubowner_
	- **IoT Hub Consumer Group:** _powerbi_

	![Stream Analytics Input configuration](Images/stream-analytics-input-configuration.png?raw=true)

	_Stream Analytics Input Configuration_

4. Click **Next**, and then **Complete** (leave the Serialization settings as they are).

<a name="Task3222" />
##### Stream Analytics Output Setup
The output of the Stream Analytics job will be Power BI.

1. To set up the output, go to the Stream Analytics Job's **OUTPUTS** tab, and click the **ADD AN OUTPUT** link.

2. In the **Add an output to your job** popup, select the **POWER BI** option and the click the **Next button**.

3. In the following screen you will setup the credentials of your Power BI account in order to allow the job to connect and send data to it. Click the **Authorize Now** link.

	![Stream Analytics Output configuration](Images/steam-analytics-output-configuration.png?raw=true)

	_Stream Analytics Output Configuration_

	You will be redirected to the Microsoft login page.

4. Enter your Power BI account email and password and click **Continue**. If the authorization is successful, you will be redirected back to the **Microsoft Power BI Settings** screen.

5. In this screen you will enter the following information:

	- **Output Alias**: _PowerBI_
	- **Dataset Name**: _Raspberry_
	- **Table Name**: _Telemetry_
	- **Group Name**: _My Workspace_

	![Power BI Settings](Images/power-bi-settings.png?raw=true)

	_Power BI Settings_

6. Click the checkmark button to create the output.

<a name="Task3223" />
##### Stream Analytics Query configuration
Now that the job's inputs and outputs are already configured, the Stream Analytics Job needs to know how to transform the input data into the output data source. To do so, you will create a new Query.

1. Go to the Stream Analytics Job **QUERY** tab and replace the query with the following statement:

	````SQL
	SELECT
		iothub.iothub.connectiondeviceid displayname,
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
		iothub.iothub.connectiondeviceid, location, guid, measurename, unitofmeasure,
		TumblingWindow(Second, 10)
	````

	The query takes the data from the input (using the alias defined when the input was created **TelemetryHUB**) and inserts into the output (**PowerBI**, the alias of the output) after grouping it using 10 seconds chunks.

2. Click on the **SAVE** button and **YES** in the confirmation dialog.

<a name="Task3234" />
##### Starting the Stream Analytics Job
Now that the job is configured, the **START** button is enabled. Click the button to start the job and then select the **JOB START TIME** option in the **START OUTPUT** popup. After clicking **OK** the job will be started.

Once the job starts it creates the Power BI datasource associated with the given subscription.

<a name="Task324" />
#### Setting up the Power BI dashboard
1. Now that the datasource is created, go back to your Power BI session, and go to **My Workspace** by clicking the **Power BI** link.

	After some minutes of the job running you will see that the dataset that you configured as an output for the Job, is now displayed in the Power BI workspace Datasets section.

	![Power BI new datasource](Images/power-bi-new-datasource.png?raw=true)

	_Power BI: New Datasource_

	> **Note:** The Power BI dataset will only be created if the job is running and if it is receiving data from the IoT Hub input, so check that the Universal App is running and sending data to Azure to ensure that the dataset be created. To check if the Stream Analytics job is receiving and processing data you can check the Azure management Stream Analytics monitor.

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
### Consuming the IoT Hub data from a Website

1. Before starting you need to create Consumer Groups to avoid colliding with the Stream Analytics job, in the same way you did in the [Using Power BI section](Task3220). The website needs two different consumer groups:
  - _website_
  - _local_ (used when debugging)
  
	![Adding website consumer groups](Images/adding-website-consumer-groups.png?raw=true)
  
2. Also take note of the **Event Hub-compatible name** and **Event Hub-compatible endpoint** values in the _Messaging_ blade

3. Browse to the **Assets/WebSite** folder and open the Web Site project (_ConnectTheDotsWebSite.sln_) in Visual Studio.

4. Edit the _Web.config_ file and add the corresponding values for the following keys:
	- **Microsoft.ServiceBus.EventHubDevices**: Event hub-compatible name.
	- **Microsoft.ServiceBus.ConnectionStringDevices**: Event hub-compatible connection string which is composed by the **Event hub-compatible endpoint** and the **_iothubowner_ Shared access policy Primary Key**.
	![IoT Hub shared access policy primary key](Images/iot-hub-shared-access-policy-primary-key.png?raw=true)
	
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


6. After you deployed the site, it is required that you enable **Web sockets**. To do this, perform the following steps:
	1. Browse to https://portal.azure.com and select your _Azure Web App.
	2. Click **All settings**.
	3. Click **Applicattion settings**
	4. Then set **Web sockets** to **On** and click **Save**.

		![Enabling Web Sockets](Images/enabling-web-sockets.png?raw=true)

		_Enabling Web Sockets in your website_

7. Browse to your recently deployed Web Application. You will see something like in the following screenshot. There will be 2 real-time graphics representing the values read from the temperature and light sensors. Take into account that the Universal app must be running and sending information to the IoT Hub in order to see the graphics.

	![Web Site Consuming the IoT Hub Data](Images/web-site-consuming-the-event-hub-data.png?raw=true)

	_Web Site Consuming the IoT Hub data_

	> **Note:** At the bottom of the page you should see “**Connected**.”. If you see “**ERROR undefined**” you likely didn’t enable **WebSockets** for the Azure Web Site.

<a name="Task4">
## Sending commands to your devices
Azure IoT Hub is a service that enables reliable and secure bi-directional communications between millions of IoT devices and an application back end. In this section you will see how to send cloud-to-device messages to your device to command it to change the color of one of the FEZ HAT leds, using the Device Explorer app as the back end.

1. Open the Universal app you created before and add the following method to the **ConnectTheDotsHelper.cs** file:

	````C#
	public async Task<string> ReceiveMessage()
	{
		if (this.HubConnectionInitialized)
		{
			try
			{
				var receivedMessage = await this.deviceClient.ReceiveAsync();

				if (receivedMessage != null)
				{
					var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
					this.deviceClient.CompleteAsync(receivedMessage);
					return messageData;
				}
				else
				{
					return string.Empty;
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("Exception when receiving message:" + e.Message);
				return string.Empty;
			}
		}
		else
		{
			return string.Empty;
		}
	}
	````

	The _ReceiveAsync_ method returns the received message at the time that it is received by the device. The call to _CompleteAsync()_ notifies IoT Hub that the message has been successfully processed and that it can be safely removed from the device queue. If something happened that prevented the device app from completing the processing of the message, IoT Hub will deliver it again.
	
2. Now you will add the logic to process the messages received. Open the **MainPage.xaml.cs** file and add a new timer to the _MainPage_ class:

	````C#
	DispatcherTimer commandsTimer;
	````

3. Add the following method, which will be on charge of processing the commands:

	````C#
	private async void CommandsTimer_Tick(object sender, object e)
	{
		string message = await ctdHelper.ReceiveMessage();

		if (message != string.Empty)
		{
			System.Diagnostics.Debug.WriteLine("Command Received: {0}", message);
			switch (message.ToUpperInvariant())
			{
				case "RED":
					hat.D2.Color = new FEZHAT.Color(255, 0, 0);
					break;
				case "GREEN":
					hat.D2.Color = new FEZHAT.Color(0, 255, 0);
					break;
				case "BLUE":
					hat.D2.Color = new FEZHAT.Color(0, 0, 255);
					break;
				case "OFF":
					hat.D2.TurnOff();
					break;
				default:
					System.Diagnostics.Debug.WriteLine("Unrecognized command: {0}", message);
					break;
			}
		}
	}
	````

	It reads the message received, and according to the text of the command, it set the value of the _hat.D2.Color_ attribute to change the color of the FEZ HAT's LED D2. When the "OFF" command is received the _TurnOff()_ method is called, which turns the LED off.
	
4. Lastly, add the following piece of code to the _SetupHatAsync_ method in order to initialize the timer used to poll for messages. 

	````C#
	this.commandsTimer = new DispatcherTimer();
	this.commandsTimer.Interval = TimeSpan.FromSeconds(60);
	this.commandsTimer.Tick += this.CommandsTimer_Tick;
	this.commandsTimer.Start();
	````

	> **Note:** The recommended interval for HTTP/1 message polling is 25 minutes. For debugging and demostration purposes a 1 minute polling interval is fine (you can use an even smaller interval for testing), but bear it in mind for production development. Check this [article](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-devguide/) for guidance. When AMQP becomes available for the IoT Hub SDK using UWP apps a different approach can be taken for message processing, since AMQP supports server push when receiving cloud-to-device messages, and it enables immediate pushes of messages from IoT Hub to the device. The following [article](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-c2d/) explains how to handle cloud-to-device messages using AMQP.
	
5. Deploy the app to the device and open the Device Explorer app.

6. Once it's loaded (and configured to point to your IoT hub), go to the **Messages To Device** tab, check the **Monitor Feedback Endpoint** option and write your command in the **Message** field. Click on **Send**

	![Sending cloud-to-device message](Images/sending-cloud-to-device-message.png?raw=true)

7. After a few seconds the message will be processed by the device and the LED will turn on in the color you selected. The feedback will also be reflected in the Device Explorer screen after a few seconds.

	![cloud-to-device message received](Images/cloud-to-device-message-received.png?raw=true)


<a name="Summary" />
## Summary
In this lab, you have learned how to create a Universal app that reads from the sensors of a FEZ hat connected to a Raspberry Pi 2 running Windows 10 IoT Core, and upload those readings to an Azure IoT Hub. You also learned how to read and consume the information in the IoT Hub using Power BI to get near real-time analysis of the information gathered from the FEZ hat sensors and to create simple reports and how to consume it using a website. You also saw how to use the IoT Hubs Cloud-To-Device messages feature to send simple commands to your devices.
