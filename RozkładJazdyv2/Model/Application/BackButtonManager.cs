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
            var currentPageType = mainAppFrame.CurrentSourcePageType;
            if (currentPageType == typeof(Pages.Lines.LinesViewPage))
                mainAppFrame.GoBack();
        }
    }
}
