using RozkładJazdyv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RozkładJazdyv2.Pages.Lines
{
    public sealed partial class LineUserControl : UserControl
    {
        public delegate void LineFavouriteChanged(Line line);
        public static event LineFavouriteChanged OnLineFavouriteChanged;
        public LineUserControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (this.DataContext != null)
                    SetStyle(this.DataContext as Line);
            };
        }

        private void SetStyle(Line line)
        {
            FavouriteTextBlock.Text = line.FavouriteText;
            EditedNameTextBlock.Text = line.EditedName;
        }

        private void LineGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
            => ShowAttachedFlyout(sender);

        private void LineGrid_Holding(object sender, HoldingRoutedEventArgs e)
            => ShowAttachedFlyout(sender);

        private void ShowAttachedFlyout(object sender)
        {
            Line line = (sender as Grid).DataContext as Line;
            LineFlyoutItem.Text = line.IsFavourite ? "Usuń z ulubionych" : "Dodaj do ulubionych";
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void LineFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Line line = (sender as MenuFlyoutItem).DataContext as Line;
            line.IsFavourite = !line.IsFavourite;
            OnLineFavouriteChanged?.Invoke(line);
        }
    }
}
