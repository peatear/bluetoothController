using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using bluetoothController.Resources;
using Windows.Networking.Sockets;
using Windows.Networking.Proximity;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace bluetoothController
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor

        private StreamSocket s = null;
        DataWriter output;
        String outString = "";
        bool connected = false;
        btConManager controllerManager;

        public MainPage()
        {
            InitializeComponent();
            //controllerManager = new btConManager();
            //controllerManager.Initialize();
            init(SetupBluetoothLink());

        }
        private async void init(Task<bool> setupOK)
        {
            if (!await setupOK)
                return;
        }
        private async Task<bool> SetupBluetoothLink()
        {
            // Tell PeerFinder that we're a pair to anyone that has been paried with us over BT
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            // Find all peers
            var devices = await PeerFinder.FindAllPeersAsync();

            // If there are no peers, then complain
            if (devices.Count == 0)
            {
                MessageBox.Show("No bluetooth devices are paired, please pair your FoneAstra");

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the FoneAstra
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("Windows Phone"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                MessageBox.Show("No paired FoneAstra was found, please pair your FoneAstra");
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();
            await s.ConnectAsync(peerInfo.HostName, "1");
            connected = true;
            return true;
        }
        
        private async void writeData(String data)
        {
            output.WriteString(data);
            await output.StoreAsync();
        }


        private async void throttle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                throttleText.Text = "Throttle: "+e.NewValue.ToString("0.0");
            });
            
              //  sendThrottle(e.NewValue);
            //if (controllerManager != null)
                //await controllerManager.SendCommandDouble(e.NewValue);
            //if (connected)
                //writeData("hello");
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            //init(SetupBluetoothLink());
            //connected = true;
            //AppToApp();
            if (connected)
                writeData("hello");
        }

        private async void AppToApp()
        {
            PeerFinder.Start();
            connectButton.Content = "Connecting...";
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();

            if (pairedDevices.Count == 0)
            {
                MessageBox.Show("No paired devices were found.");
            }
            else
            {
                foreach (var pairedDevice in pairedDevices)
                {
                    if (pairedDevice.DisplayName == connectName.Text)
                    {
                        
                        controllerManager.Connect(pairedDevice.HostName);
                        //PeerInformation[] peers = pairedDevices.ToArray();
                        //PeerInformation peerInfo = pairedDevices.FirstOrDefault(c => c.DisplayName.Contains(WindowsPhoneName.Text));

                        //await s.ConnectAsync(peerInfo.HostName, "1");
                        connectButton.Content = "Connected";
                        connectName.IsReadOnly = true;
                        connectButton.IsEnabled = false;

                        //input = new DataReader(s.InputStream);
                        //connected = true;
                        continue;
                    }
                }
            }
        }


    }
}