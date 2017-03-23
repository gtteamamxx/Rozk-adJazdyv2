using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace RozkładJazdyv2.Model.Application
{
    public class BackButtonManager
    {
        private BackButtonManager() { }

        public static void BackButtonPressed(object sender, BackRequestedEventArgs e)
        {
            Frame mainAppFrame = MainFrameHelper.GetMainFrame();
            Type currentPageType = mainAppFrame.CurrentSourcePageType;
            bool goBack = IsGoBackFromPageAllowed(currentPageType);

            if (goBack)
            {
                mainAppFrame.GoBack();
                e.Handled = true;
                return;
            }

            App.Current.Exit();
        }

        private static bool IsGoBackFromPageAllowed(Type currentPageType)
        {
            if (currentPageType == typeof(Pages.Lines.LinesViewPage))
                return true;
            if (currentPageType == typeof(Pages.Lines.LinePage))
                return true;

            return false;
        }
    }
}
