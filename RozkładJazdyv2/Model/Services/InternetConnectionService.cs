using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace RozkładJazdyv2.Model
{
    public class InternetConnectionService
    {
        private InternetConnectionService() { }

        public static bool IsInternetConnectionAvailable()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            return (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }
    }
}
