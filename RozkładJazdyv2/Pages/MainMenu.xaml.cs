using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.MainMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace RozkładJazdyv2.Pages
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainMenu : Page
    {
        private string APP_VERSION { get { return Model.Application.Version.VERSION; } }
        private string TimetableLastUpdate { get { return new FileInfo(SQLServices.SQLFilePath).CreationTime.ToUniversalTime().ToString(); } }
        public MainMenu()
        {
            this.InitializeComponent();
            AddButtonsToPage();
        }

        private void AddButtonsToPage()
        {
            ButtonHelper.CreateButtonList(ButtonListGridView);
            var backgroundColor = new Color() { R = 121, G = 124, B = 129, A = 255 }; //"gray"
            ButtonHelper.AddButton("Zobacz listę linii", "Linie", "\xE806", backgroundColor);
            ButtonHelper.AddButton("Zobacz listę przystanków", "Przystanki", "\xE174", backgroundColor);
            ButtonHelper.AddButton("Zobacz ulubione", "Ulubione", "\xE082", backgroundColor);
            ButtonHelper.AddButton("Zobacz komunikaty", "Komunikaty", "\xEC15", backgroundColor);
        }

        private void ButtonListGridViewContentChanged(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var gridOfButton = ((Grid)args.ItemContainer.ContentTemplateRoot);
            MainMenuButton buttonClass = ((MainMenuButton)args.Item);
            gridOfButton.Background = buttonClass.BackgroundColor;
        }
    }
}
