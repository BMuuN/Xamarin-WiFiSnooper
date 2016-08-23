namespace WiFiSnooper.Models
{
    public class NetworkLocation
    {
        private string provider;

        public string Provider
        {
            get
            {
                return provider;
            }
            set
            {
                provider = value.ToUpper();
            }
        }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public float Accuracy { get; set; }

        public float Speed { get; set; }

        public float Bearing { get; set; }

        public long Time { get; set; }
    }
}