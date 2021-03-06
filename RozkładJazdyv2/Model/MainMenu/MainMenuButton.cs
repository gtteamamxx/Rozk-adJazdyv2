﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace RozkładJazdyv2.Model.MainMenu
{
    public class MainMenuButton
    {
        public enum ButtonType
        {
            Lines = 0,
            Stops,
            Favourites,
            Communicates
        }

        public string Description { get; set; }
        public string Header { get; set; }
        public string Logo { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public ButtonType Type { get; set; }
    }
}
