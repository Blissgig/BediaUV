using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace BediaUV
{
    public sealed partial class MainPage : Page
    {
        BediaCore bediaCore;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bediaCore = new BediaCore(this);
            
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                bediaCore.MenuBack();
                a.Handled = true;
            };

            Window.Current.CoreWindow.KeyDown += (s, ka) =>
            {
                bediaCore.KeyDown(ka);
            };
        }
        
        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            bediaCore.PointerPressed(e);
        }

        private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            bediaCore.PointerReleased(e);
        }

        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            bediaCore.PointerWheelChanged(e);
        }

        private void BediaMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            bediaCore.MediaEnded();
        }

        private void BediaMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            bediaCore.MediaEnded();
        }

        private void BediaMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            bediaCore.MediaOpened = true;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (bediaCore != null)
                {
                    bediaCore.PageSizeChanged(e);
                }
            }
            catch { }
        }

        private void BediaMap_MapRightTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapRightTappedEventArgs args)
        {
            bediaCore.MenuBack();
        }
    }
}
