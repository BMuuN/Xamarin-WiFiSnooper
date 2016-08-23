using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WiFiSnooper.Adapters
{
    using WiFiSnooper.Models;

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
            View view = convertView // re-use an existing view, if one is available
                        ?? this.context.LayoutInflater.Inflate(Resource.Layout.NetworkListItem, parent, false); // otherwise create a new one

            // set view properties to reflect data for the given row
            view.FindViewById<TextView>(Resource.Id.NetworkSSID).Text = items[position].SSID;
            view.FindViewById<TextView>(Resource.Id.NetworkEncryption).Text = items[position].Capabilities;

            ImageView networkStrengthImage = view.FindViewById<ImageView>(Resource.Id.NetworkStrength);
            networkStrengthImage.SetImageResource(Resource.Mipmap.ic_signal_wifi_off_black_36dp);

            // return the view, populated with data, for display
            return view;
        }
    }
}