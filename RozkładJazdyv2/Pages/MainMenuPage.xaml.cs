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
            RegisterButtonHooks();
        }

        private void RegisterButtonHooks()
            => ButtonListGridView.SelectionChanged += ButtonClicked;

        private void ButtonClicked(object sender, SelectionChangedEventArgs e)
        {
            var clickedButton = ((GridView)sender).SelectedItem as MainMenuButton;
            if (clickedButton == null)
                return;
            switch(clickedButton.Type)
            {
                case MainMenuButton.ButtonType.Lines:
                    MainFrameHelper.GetMainFrame().Navigate(typeof(Pages.Lines.LinesViewPage));
                    break;
                case MainMenuButton.ButtonType.Stops:
                    break;
                case MainMenuButton.ButtonType.Favourites:
                    break;
                case MainMenuButton.ButtonType.Communicates:
                    break;
                default:
                    break;
            }
        }

        private void AddButtonsToPage()
        {
            ButtonHelper.CreateButtonList(ButtonListGridView);
            var backgroundColor = new Color() { R = 121, G = 124, B = 129, A = 255 }; //"gray"
            ButtonHelper.AddButton("Zobacz listę linii", "Linie", "\xE806", backgroundColor, MainMenuButton.ButtonType.Lines);
            ButtonHelper.AddButton("Zobacz listę przystanków", "Przystanki", "\xE174", backgroundColor, MainMenuButton.ButtonType.Stops);
            ButtonHelper.AddButton("Zobacz ulubione", "Ulubione", "\xE082", backgroundColor, MainMenuButton.ButtonType.Favourites);
            ButtonHelper.AddButton("Zobacz komunikaty", "Komunikaty", "\xEC15", backgroundColor, MainMenuButton.ButtonType.Communicates);
        }

        private void ButtonListGridViewContentChanged(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var gridOfButton = ((Grid)args.ItemContainer.ContentTemplateRoot);
            MainMenuButton buttonClass = ((MainMenuButton)args.Item);
            gridOfButton.Background = buttonClass.BackgroundColor;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => ResetButtonSelected();

        private void ResetButtonSelected()
            => ButtonListGridView.SelectedIndex = -1;
    }
}
