WindowsIoTCorePi2FezHat Solution
================================
This solution shows how to read sensor data from a Rapsberry Pi 2 device running in Windows 10 IoT Core. It also shows how this sensor data is uploaded to Azure IoT Hub.

To run the solution, perform the following steps.

1.  Open the **MainPage.xaml.cs** file, and replace the placeholder values with the values corresponding to your Azure Subscription.
2. In the Visual Studio Toolbar, expand the **Solution Platforms** combobox and select **ARM** as shown in the following picture.

	![Selecting CPU](Images/selecting-cpu.png?raw=true)

3. Expand the **Debug** combobox and select **Remote**. The **Remote Connections** windows may open. If the device is Auto Detected, just select it, and then click **Select**; otherwise, use the **Manual Configuration**.

	![Setting Up the Remote Connection](Images/setting-up-the-remote-connection.png?raw=true)

4. Press **F5** to start debugging the solution.