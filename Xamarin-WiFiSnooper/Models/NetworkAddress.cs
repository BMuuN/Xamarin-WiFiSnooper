namespace WiFiSnooper.Models
{
    public class NetworkAddress
    {
        public NetworkAddress(string address)
        {
            this.Street = address.Split('\n')[0];
            this.Town = address.Split('\n')[1];
            this.PostCode = address.Split('\n')[2];
        }

        public string Street { get; set; }

        public string Town { get; set; }

        public string PostCode { get; set; }

        public override string ToString()
        {
            return $"{this.Street}, {this.Town}, {this.PostCode.ToUpper()}";
        }
    }
}