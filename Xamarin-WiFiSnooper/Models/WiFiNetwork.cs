using System;

namespace WiFiSnooper.Models
{
    public class WiFiNetwork
    {
        private string _capabilities;

        public string BSSID { get; set; }

        public string SSID { get; set; }

        public string Capabilities
        {
            get
            {
                return _capabilities;
            }
            set
            {
                _capabilities = value;
                HasEncryption = (value.ToUpper().Contains("WEP") || value.ToUpper().Contains("WPA"));
            }
        }

        public DateTimeOffset TimeStamp { get; set; }

        public int Frequency { get; set; }

        public int SignalStrength { get; set; }

        public bool HasEncryption { get; private set; }

        public NetworkAddress Address { get; set; }

        public NetworkLocation Location { get; set; }
    }
}