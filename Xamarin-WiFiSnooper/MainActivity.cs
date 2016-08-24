using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using Android.Locations;
using Android.Util;
using Android.Support.V7.App;

using WiFiSnooper.Adapters;
using WiFiSnooper.Models;

namespace WiFiSnooper
{
    using Android.Net;
    using Android.Views;

    using WiFiSnooper.ViewModels;

    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        private int count = 1;
        private Context context = null;
        private static MainActivity thisActivity;

        // WiFi variables
        private static WifiManager wifi;
        private static List<WiFiNetwork> wiFiNetworks;
        private static NetworkLocation wiFiLocation;
        private static NetworkAddress wiFiAddress;

        private ListView listView;

        // location variables
        private static readonly string Tag = "X:" + typeof(MainActivity).Name;
        private LocationManager locationManager;
        private string locationProvider;
        private Location currentLocation;

        // you need a view model to bind to
        private MainActivityPageViewModel ViewModel { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            // setup your ViewModel
            ViewModel = new MainActivityPageViewModel
            {
                IsBusy = true
            };

            thisActivity = this;
            this.context = ApplicationContext;
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource, and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += delegate { button.Text = $"{count++} clicks!"; };
            
            listView = FindViewById<ListView>(Resource.Id.NetworkListView);
            listView.FastScrollEnabled = true;
            listView.ItemClick += NetworkListView_ItemClick;

            // Init location manager
            InitializeLocationManager();

            // Get list of WiFi networks
            GetWifiNetworks();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.locationManager.RequestLocationUpdates(this.locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.locationManager.RemoveUpdates(this);
        }

        #region WiFi

        private void GetWifiNetworks()
        {
            wiFiNetworks = new List<WiFiNetwork>();

            // Get a handle to the Wifi
            wifi = (WifiManager)context.GetSystemService(Context.WifiService);

            // check wifi is not disabled
            if (wifi.WifiState != WifiState.Enabled)
            {
                Android.Widget.Toast.MakeText(this, "WiFi disabled, enabling wifi network...", Android.Widget.ToastLength.Short).Show();
                wifi.SetWifiEnabled(true);
                Task.Delay(3000);
            }

            // Start a scan and register the Broadcast receiver to get the list of Wifi Networks
            var wifiReceiver = new WifiReceiver();
            var wifiSignal = new WiFiSignalChanged();
            context.RegisterReceiver(wifiReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction)); // list of networks changed
            //context.RegisterReceiver(wifiSignal, new IntentFilter(WifiManager.RssiChangedAction)); // signal strength changed
            wifi.StartScan();
        }

        /// <summary>
        /// Amend signal strength
        /// </summary>
        private class WiFiSignalChanged : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                // if wifi has been disabled then don't retrieve the results
                WifiManager wifi = (WifiManager)context.GetSystemService(Context.WifiService);
                if (!wifi.IsWifiEnabled) return;

                // get the network for which the signal has changed
                var scanWiFiNetworks = wifi.ScanResults;
                var network = scanWiFiNetworks.Single(n => n.Bssid.ToLower() == wifi.ConnectionInfo.BSSID.ToLower());
                if (network == null) return;

                var wifiNetwork = wiFiNetworks.Single(n => n.BSSID.ToLower() == network.Bssid.ToLower());

                // calculate the signal
                int level = WifiManager.CalculateSignalLevel(wifi.ConnectionInfo.Rssi, network.Level);
                int difference = level * 100 / network.Level;
                int signalStrength = 0;

                if (difference >= 100) wifiNetwork.SignalStrength = 4;
                else if (difference >= 75) wifiNetwork.SignalStrength = 3;
                else if (difference >= 50) wifiNetwork.SignalStrength = 2;
                else if (difference >= 25) wifiNetwork.SignalStrength = 1;

                //tv.setText(tv.getText() + "\nDifference :" + difference + " signal state:" + signalStrength);
            }
        }

        /// <summary>
        /// List avialble WiFi networks
        /// </summary>
        private class WifiReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                // if wifi has been disabled then don't retrieve the results
                WifiManager wifi = (WifiManager)context.GetSystemService(Context.WifiService);
                if (!wifi.IsWifiEnabled) return;

                var scanWiFiNetworks = wifi.ScanResults;
                foreach (ScanResult wifinetwork in scanWiFiNetworks)
                {
                    var network = new WiFiNetwork()
                    {
                        BSSID = wifinetwork.Bssid,
                        SSID = wifinetwork.Ssid,
                        Frequency = wifinetwork.Frequency,
                        Capabilities = wifinetwork.Capabilities,
                        //TimeStamp = DateTimeOffset.FromFileTime(wifinetwork.Timestamp)
                        TimeStamp = DateTimeOffset.Now,
                        Location =  wiFiLocation,
                        Address = wiFiAddress
                    };
                    
                    // calculate the signal
                    int level = WifiManager.CalculateSignalLevel(wifi.ConnectionInfo.Rssi, wifinetwork.Level);
                    int difference = level * 100 / wifinetwork.Level;
                    int signalStrength = 0;

                    if (difference >= 100) network.SignalStrength = 4;
                    else if (difference >= 75) network.SignalStrength = 3;
                    else if (difference >= 50) network.SignalStrength = 2;
                    else if (difference >= 25) network.SignalStrength = 1;
                    
                    if (!wiFiNetworks.Exists(n => n.SSID == network.SSID && n.BSSID == network.BSSID))
                    {
                        wiFiNetworks.Add(network);
                    }
                }

                thisActivity.UpdateList();
            }
        }

        public void UpdateList()
        {
            var listAdapter = new WiFiListAdapter(thisActivity, wiFiNetworks);
            listView.Adapter = listAdapter;
            //listView.ItemClick += NetworkListView_ItemClick;
            ViewModel.IsBusy = false; // hide the spinner notification
        }

        protected void NetworkListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var network = wiFiNetworks[e.Position];
            //Android.Widget.Toast.MakeText(this, network.SSID, Android.Widget.ToastLength.Short).Show();

            if (network.Location == null)
            {
                Android.Widget.Toast.MakeText(this, "Location data not available for  this network.", Android.Widget.ToastLength.Short).Show();
                return;
            }
            
            var geoUri = Android.Net.Uri.Parse($"geo:{network.Location.Latitude},{network.Location.Longitude}");
            var mapIntent = new Intent(Intent.ActionView, geoUri);
            StartActivity(mapIntent);
        }

        #endregion

        #region Location

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        private async void InitializeLocationManager()
        {
            this.locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };

            IList<string> acceptableLocationProviders = this.locationManager.GetProviders(criteriaForLocationService, true);
            this.locationProvider = acceptableLocationProviders.Any() ? acceptableLocationProviders.First() : string.Empty;
            Log.Debug(Tag, "Location Provider: " + this.locationProvider + ".");

            // get last known location
            this.currentLocation = locationManager.GetLastKnownLocation(this.locationProvider);

            if (currentLocation == null)
            {
                Android.Widget.Toast.MakeText(this, "Unable to determine location", Android.Widget.ToastLength.Short).Show();
                return;
            }

            var objNetworkLocation = new NetworkLocation()
                                         {
                                             Provider = this.currentLocation.Provider,
                                             Latitude = this.currentLocation.Latitude,
                                             Longitude = this.currentLocation.Longitude,
                                             Altitude = this.currentLocation.HasAltitude ? this.currentLocation.Altitude : 0,
                                             Accuracy = this.currentLocation.HasAccuracy ? this.currentLocation.Accuracy : 0,
                                             Speed = this.currentLocation.HasSpeed ? this.currentLocation.Speed : 0,
                                             Bearing = this.currentLocation.HasBearing ? this.currentLocation.Bearing : 0,
                                             Time = this.currentLocation.Time
                                         };
            wiFiLocation = objNetworkLocation;

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
        }

        public async void OnLocationChanged(Location location)
        {
            this.currentLocation = location;
            if (this.currentLocation != null)
            {
                //_locationText.Text = string.Format("{0:f6},{1:f6}", currentLocation.Latitude, currentLocation.Longitude);
                Address address = await ReverseGeocodeCurrentLocation();
                DisplayAddress(address);
            }
        }

        private async Task<Address> ReverseGeocodeCurrentLocation()
        {
            try
            {
                Geocoder geoCoder = new Geocoder(this);
                IList<Address> addressList = await geoCoder.GetFromLocationAsync(this.currentLocation.Latitude, this.currentLocation.Longitude, 10);
                Address address = addressList.FirstOrDefault();
                return address;
            }
            catch (Exception)
            {
                Android.Widget.Toast.MakeText(this, "Unable to retrieve address from server", Android.Widget.ToastLength.Short).Show();
            }

            return null;
        }

        private void DisplayAddress(Address address)
        {
            if (address != null)
            {
                System.Text.StringBuilder deviceAddress = new System.Text.StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                }

                var objAddress = new NetworkAddress(deviceAddress.ToString());
                wiFiAddress = objAddress;

                // Remove the last comma from the end of the address.
                //_addressText.Text = deviceAddress.ToString();
                Log.Debug(Tag, "Address: " + objAddress.ToString() + ".");
            }
            else
            {
                //_addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }

        #endregion
    }
}

