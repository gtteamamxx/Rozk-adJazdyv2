using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RozkładJazdyv2.Model.MainMenu
{
    public class ButtonHelper
    {
        public static ObservableCollection<MainMenuButton> Buttons { get; private set; }

        private ButtonHelper() { }

        public static void CreateButtonList(GridView gridView)
        {
            if (Buttons == null)
            {
                Buttons = new ObservableCollection<MainMenuButton>();
                gridView.ItemsSource = Buttons;
            }
        }

        public static void AddButton(string description, string header, string logo, Color backgroundColor, MainMenuButton.ButtonType type)
            => Buttons.Add(new MainMenuButton()
            {
                Description = description,
                Header = header,
                Logo = logo,
                BackgroundColor = new SolidColorBrush(backgroundColor),
                Type = type
            });
    }
}
