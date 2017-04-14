using RozkładJazdyv2.Model;
using RozkładJazdyv2.Model.MainMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        private string _APP_VERSION { get { return Model.Application.Version.VERSION; } }
        private string _TimetableLastUpdate { get { return new FileInfo(SQLServices.SQLFilePath).CreationTime.ToUniversalTime().ToString(); } }

        private List<Grid> _ListOfGrid = new List<Grid>();

        private readonly double _SizeMultiplier = 3.75; //if greather then buttons will be smaller
        private readonly double _MaxSizeOfButton = 215;

        public MainMenu()
        {
            this.InitializeComponent();
            this.SetIsBackFromPageAllowed(false);
            
            AddButtonsToPage();
            RegisterButtonHooks();
    
            this.Loaded += (s, e) =>
            {
                ButtonListGridView.InvalidateMeasure(); //update layout
                ButtonListGridView.UpdateLayout();
                ButtonListGridView.InvalidateArrange();

                ChangeSizeOfButtons(this.ActualHeight, this.ActualWidth);
            };

            this.SizeChanged += MainMenu_SizeChanged;
        }

        private void ChangeSizeOfButtons(double height, double width)
        {
            double size = ((width / _SizeMultiplier) + (height / _SizeMultiplier)) / 2;
            double multiplier = 1;

            foreach (Grid buttonGrid in _ListOfGrid)
            {
                multiplier = size / buttonGrid.Width;
                if (buttonGrid.Width * multiplier > _MaxSizeOfButton)
                    return;

                buttonGrid.Width = buttonGrid.Height *= multiplier;

                foreach (TextBlock textBlock in buttonGrid.Children)
                    textBlock.FontSize *= multiplier;
            }
        }

        private async void ButtonClicked(object sender, SelectionChangedEventArgs e)
        {
            var clickedButton = ButtonListGridView.SelectedItem as MainMenuButton;
            if (clickedButton == null)
                return;

            ResetButtonSelected();
            await Task.Delay(250);

            Frame frame = MainFrameHelper.GetMainFrame();
            switch (clickedButton.Type)
            {
                case MainMenuButton.ButtonType.Lines:
                    frame.Navigate(typeof(Pages.Lines.LinesListPage));
                    break;

                case MainMenuButton.ButtonType.Stops:
                    frame.Navigate(typeof(Pages.BusStops.BusStopsListPage));
                    break;

                case MainMenuButton.ButtonType.Favourites:
                    frame.Navigate(typeof(Pages.Favourites.FavouritePage));
                    break;

                case MainMenuButton.ButtonType.Communicates:
                    frame.Navigate(typeof(Pages.RSS.CommunicatesPage));
                    break;

                default:
                    break;
            }
        }

        private void AddButtonsToPage()
        {
            ButtonHelper.CreateButtonList(ButtonListGridView);
            Color backgroundColor = new Color() { R = 121, G = 124, B = 129, A = 255 }; //"gray"

            ButtonHelper.AddButton("Zobacz listę linii", "Linie", "\xE806", backgroundColor, MainMenuButton.ButtonType.Lines);
            ButtonHelper.AddButton("Zobacz listę przystanków", "Przystanki", "\xE174", backgroundColor, MainMenuButton.ButtonType.Stops);
            ButtonHelper.AddButton("Zobacz ulubione", "Ulubione", "\xE082", backgroundColor, MainMenuButton.ButtonType.Favourites);
            ButtonHelper.AddButton("Zobacz komunikaty", "Komunikaty", "\xEC15", backgroundColor, MainMenuButton.ButtonType.Communicates);
        }

        private void ButtonListGridViewContentChanged(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            Grid gridOfButton = ((Grid)args.ItemContainer.ContentTemplateRoot);
            bool isGridInList = _ListOfGrid.FirstOrDefault(p => p == gridOfButton) != null;

            if (!isGridInList)
                _ListOfGrid.Add(gridOfButton);

            MainMenuButton buttonClass = ((MainMenuButton)args.Item);
            gridOfButton.Background = buttonClass.BackgroundColor;
        }

        private void RegisterButtonHooks()
            => ButtonListGridView.SelectionChanged += ButtonClicked;

        private void MainMenu_SizeChanged(object sender, SizeChangedEventArgs e)
            => ChangeSizeOfButtons(e.NewSize.Height, e.NewSize.Width);

        protected override void OnNavigatedTo(NavigationEventArgs e)
            => ResetButtonSelected();

        private void ResetButtonSelected()
            => ButtonListGridView.SelectedIndex = -1;

        private void RefreshTimetableButton_Click(object sender, RoutedEventArgs e)
            => Model.MainFrameHelper.GetMainFrame().Navigate(typeof(MainPage), true);
    }
}
