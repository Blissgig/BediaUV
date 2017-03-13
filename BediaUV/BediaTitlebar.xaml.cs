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


namespace BediaUV
{
    public sealed partial class BediaTitlebar : UserControl
    {
      
        public BediaTitlebar()
        {
            this.InitializeComponent();
        }

        public BediaTitlebar(
            SolidColorBrush BackgroundColor, 
            SolidColorBrush ForegroundColor,
            byte FontSize)
        {
            try
            {
                InitializeComponent();

                SetColors(BackgroundColor, ForegroundColor);

                this.TitlebarTitle.FontSize = FontSize;
                this.TitlebarCountX.FontSize = FontSize;
                this.TitlebarCountOf.FontSize = FontSize;
                this.TitlebarCountY.FontSize = FontSize;

                this.BediaProgress.Height = FontSize;
                this.BediaProgress.Width = FontSize;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetColors(SolidColorBrush BackgroundColor, SolidColorBrush ForegroundColor)
        {
            try
            {
                this.TitlebarGrid.Background = BackgroundColor;

                this.TitlebarTitle.Foreground = ForegroundColor;
                this.TitlebarCountX.Foreground = ForegroundColor;
                this.TitlebarCountOf.Foreground = ForegroundColor;
                this.TitlebarCountY.Foreground = ForegroundColor;

                this.PageUpBackground.Fill = BackgroundColor;
                this.PageUpBackground.Stroke = ForegroundColor;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetOpacity(double Opacity)
        {
            try
            {
                this.TitlebarGrid.Opacity = Opacity;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
