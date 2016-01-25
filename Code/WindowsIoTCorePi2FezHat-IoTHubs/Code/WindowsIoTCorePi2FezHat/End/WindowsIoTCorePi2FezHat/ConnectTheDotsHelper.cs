using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using Microsoft.Azure.Devices.Client;

namespace WindowsIoTCorePi2FezHat
{
    public class ConnectTheDotsHelper
    {
        // App Settings variables
        public AppSettings localSettings = new AppSettings();

        public List<ConnectTheDotsSensor> sensors;

        // Http connection string, SAS tokem and client
        DeviceClient deviceClient;
        bool HubConnectionInitialized = false;

        public ConnectTheDotsHelper(string iotDeviceConnectionString = "",
            string organization = "",
            string location = "",
            List<ConnectTheDotsSensor> sensorList = null)
        {
            localSettings.IoTDeviceConnectionString = iotDeviceConnectionString;
            localSettings.Organization = organization;
            localSettings.Location = location;

            sensors = sensorList;

            SaveSettings();
        }

        /// <summary>
        /// Validate the settings 
        /// </summary>
        /// <returns></returns>
        bool ValidateSettings()
        {
            if ((localSettings.IoTDeviceConnectionString == "") ||
                (localSettings.Organization == "") ||
                (localSettings.Location == ""))
            {
                this.localSettings.SettingsSet = false;
                return false;
            }

            this.localSettings.SettingsSet = true;
            return true;

        }

        /// <summary>
        /// Apply new settings to sensors collection
        /// </summary>
        public bool SaveSettings()
        {
            if (ValidateSettings())
            {
                ApplySettingsToSensors();
                this.InitHubConnection();
                return true;
            } else {
                return false;
            }
        }


        /// <summary>
        ///  Apply settings to sensors collection
        /// </summary>
        public void ApplySettingsToSensors()
        {
            foreach (ConnectTheDotsSensor sensor in sensors)
            {
                sensor.location = this.localSettings.Location;
                sensor.organization = this.localSettings.Organization;
            }
        }

        private void SendAllSensorData()
        {
            foreach (ConnectTheDotsSensor sensor in sensors)
            {
                sensor.timecreated = DateTime.UtcNow.ToString("o");
                sendMessage(sensor.ToJson());
            }
        }

        public void SendSensorData(ConnectTheDotsSensor sensor)
        {
            sensor.timecreated = DateTime.UtcNow.ToString("o");
            sendMessage(sensor.ToJson());
        }

        /// <summary>
        /// Send message to an IoT Hub using IoT Hub SDK
        /// </summary>
        /// <param name="message"></param>
        public async void sendMessage(string message)
        {
            if (this.HubConnectionInitialized)
            {
                try
                {
                    var content = new Message(Encoding.UTF8.GetBytes(message));
                    await deviceClient.SendEventAsync(content);

                    Debug.WriteLine("Message Sent: {0}", message, null);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception when sending message:" + e.Message);
                }
            }
        }

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

        /// <summary>
        /// Initialize Hub connection
        /// </summary>
        public bool InitHubConnection()
        {
            try
            {
                this.deviceClient = DeviceClient.CreateFromConnectionString(localSettings.IoTDeviceConnectionString);
                this.HubConnectionInitialized = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
