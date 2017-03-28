using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace RozkładJazdyv2.Model.Application
{
    public class BackButtonManager
    {
        private BackButtonManager() { }

        public static void BackButtonPressed(object sender, BackRequestedEventArgs e)
        {
            Frame mainAppFrame = MainFrameHelper.GetMainFrame();

            if (IsGoBackFromPageAllowed(mainAppFrame))
            {
                mainAppFrame.GoBack();
                e.Handled = true;
                return;
            }

            e.Handled = false;
            //App.Current.Exit(); 
        }

        private static bool IsGoBackFromPageAllowed(Frame mainAppFrame)
        {
            var currentPageType = mainAppFrame.CurrentSourcePageType;
            bool isBackAllowed = ((Page)mainAppFrame.Content).GetIsBackFromPageAllowed();

            if (currentPageType == typeof(Pages.Lines.LineBusStopPage) && isBackAllowed)
                RemoveLineBusStopPageStackFromFrame();

            return isBackAllowed;
        }

        private static void RemoveLineBusStopPageStackFromFrame()
        {
            Frame mainAppFrame = MainFrameHelper.GetMainFrame();

            mainAppFrame.BackStack.ToList().ForEach(p =>
            {
                if (p.SourcePageType == typeof(Pages.Lines.LineBusStopPage))
                    mainAppFrame.BackStack.Remove(p);
            });
        }
    }
}
