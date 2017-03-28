using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace RozkładJazdyv2.Model
{
    public class MainFrameHelper
    {
        public static Frame _Frame;

        private MainFrameHelper() { }

        public static void SetMainFrame(Frame frame)
        {
            if (_Frame == null)
                _Frame = frame;
        }

        public static Frame GetMainFrame()
            => _Frame;
    }
}
