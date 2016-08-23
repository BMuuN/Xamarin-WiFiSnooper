using System;

namespace WiFiSnooper.Models
{

    public class WiFiNetwork
    {
        public string BSSID { get; set; }

        public string SSID { get; set; }

        public string Capabilities { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int Frequency { get; set; }
        
        public NetworkAddress Address { get; set; }

        public NetworkLocation Location { get; set; }
    }
}