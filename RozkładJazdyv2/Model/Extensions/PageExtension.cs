using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace RozkładJazdyv2.Model
{
    public static class PageExtension
    {
        private static Dictionary<Page, bool> _IsBackFromPageAllowedDict;

        internal static bool GetIsBackFromPageAllowed(this Page page)
        {
            try
            {
                return _IsBackFromPageAllowedDict.First(p => p.Key == page).Value;
            }
            catch
            {
                return false;
            }
        }

        public static void SetIsBackFromPageAllowed(this Page page, bool value)
        {
            if (_IsBackFromPageAllowedDict == null)
                _IsBackFromPageAllowedDict = new Dictionary<Page, bool>();

            _IsBackFromPageAllowedDict.Add(page, value);
        }
    }
}
