using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using WiFiSnooper.Models;

namespace WiFiSnooper.Adapters
{
    public class WiFiListAdapter : BaseAdapter<WiFiNetwork>
    {
        List<WiFiNetwork> items;
        Activity context;

        public WiFiListAdapter(Activity context, List<WiFiNetwork> items) : base() {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override WiFiNetwork this[int position] => this.items[position];

        public override int Count => this.items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            WiFiNetwork network = items[position];
            View view = convertView // re-use an existing view, if one is available
                        ?? this.context.LayoutInflater.Inflate(Resource.Layout.NetworkListItem, parent, false); // otherwise create a new one

            // set view properties to reflect data for the given row
            view.FindViewById<TextView>(Resource.Id.NetworkSSID).Text = network.SSID;
            view.FindViewById<TextView>(Resource.Id.NetworkEncryption).Text = network.Capabilities;

            ImageView networkStrengthImage = view.FindViewById<ImageView>(Resource.Id.NetworkStrength);

            int imageId =0;
            switch (network.SignalStrength)
            {
                case 0:
                    imageId = Resource.Mipmap.ic_signal_wifi_0_bar_black_36dp;
                    break;

                case 1:
                    imageId = network.HasEncryption
                                  ? Resource.Mipmap.ic_signal_wifi_1_bar_lock_black_36dp
                                  : Resource.Mipmap.ic_signal_wifi_1_bar_black_36dp;
                    break;

                case 2:
                    imageId = network.HasEncryption
                                  ? Resource.Mipmap.ic_signal_wifi_2_bar_lock_black_36dp
                                  : Resource.Mipmap.ic_signal_wifi_2_bar_black_36dp;
                    break;

                case 3:
                    imageId = network.HasEncryption
                                  ? Resource.Mipmap.ic_signal_wifi_3_bar_lock_black_36dp
                                  : Resource.Mipmap.ic_signal_wifi_3_bar_black_36dp;
                    break;

                case 4:
                    imageId = network.HasEncryption
                                  ? Resource.Mipmap.ic_signal_wifi_4_bar_lock_black_36dp
                                  : Resource.Mipmap.ic_signal_wifi_4_bar_black_36dp;
                    break;

                default:
                    imageId = Resource.Mipmap.ic_signal_wifi_off_black_36dp;
                    break;
            }

            networkStrengthImage.SetImageResource(imageId);

            // return the view, populated with data, for display
            return view;
        }
    }
}