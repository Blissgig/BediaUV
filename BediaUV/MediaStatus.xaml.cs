using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;


namespace BediaUV
{
    public sealed partial class MediaStatus : UserControl
    {
        public MediaStatus()
        {
            this.InitializeComponent();
        }

        public MediaStatus(
            SolidColorBrush BackgroundColor, 
            SolidColorBrush ForegroundColor,
            byte MenuSize)
        {
            InitializeComponent();
            
            SetColors(BackgroundColor, ForegroundColor);

            this.Col0.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col1.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col2.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col3.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col4.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col5.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col6.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col7.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col8.Width = new Windows.UI.Xaml.GridLength(MenuSize);
            this.Col9.Width = new Windows.UI.Xaml.GridLength(MenuSize);
        }

        public void SetColors(SolidColorBrush BackgroundColor, SolidColorBrush ForegroundColor)
        {
            try
            {
                //Application Volume
                this.Status0.Stroke = BackgroundColor;
                this.Status1.Stroke = BackgroundColor;
                this.Status2.Stroke = BackgroundColor;
                this.Status3.Stroke = BackgroundColor;
                this.Status4.Stroke = BackgroundColor;
                this.Status5.Stroke = BackgroundColor;
                this.Status6.Stroke = BackgroundColor;
                this.Status7.Stroke = BackgroundColor;
                this.Status8.Stroke = BackgroundColor;
                this.Status9.Stroke = BackgroundColor;

                this.Status0Outline.Stroke = ForegroundColor;
                this.Status1Outline.Stroke = ForegroundColor;
                this.Status2Outline.Stroke = ForegroundColor;
                this.Status3Outline.Stroke = ForegroundColor;
                this.Status4Outline.Stroke = ForegroundColor;
                this.Status5Outline.Stroke = ForegroundColor;
                this.Status6Outline.Stroke = ForegroundColor;
                this.Status7Outline.Stroke = ForegroundColor;
                this.Status8Outline.Stroke = ForegroundColor;
                this.Status9Outline.Stroke = ForegroundColor;

                //Windows Volume
                this.WindowsVolume0.Stroke = ForegroundColor;
                this.WindowsVolume1.Stroke = ForegroundColor;
                this.WindowsVolume2.Stroke = ForegroundColor;
                this.WindowsVolume3.Stroke = ForegroundColor;
                this.WindowsVolume4.Stroke = ForegroundColor;
                this.WindowsVolume5.Stroke = ForegroundColor;
                this.WindowsVolume6.Stroke = ForegroundColor;
                this.WindowsVolume7.Stroke = ForegroundColor;
                this.WindowsVolume8.Stroke = ForegroundColor;
                this.WindowsVolume9.Stroke = ForegroundColor;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetStatus(byte Percent)
        {
            try
            {
                byte bCount = 2;
                byte bPercent = Percent;
                Ellipse StatusBubble = null;
                BediaTransitions bediaTransitions = new BediaTransitions();
                BediaStoryboardOptions storyboardOptions = new BediaStoryboardOptions();


                storyboardOptions.Milliseconds = 400;

                if (Percent != 100 && Percent != 5)
                { 
                    SetWindowsVolume(0); //To insure that the Windows Volume is hidden
                }

                if (Percent < 11)
                {
                    bCount = 1;
                }
                else if (Percent == 100)
                {
                    bCount = 11;
                    bPercent = 10;
                }
                else
                {
                    bCount = Convert.ToByte(Percent.ToString().Substring(0, 1));
                    bPercent = Convert.ToByte(Percent - (bCount * 10));
                    bCount += 1;
                }
                

                //Hide non-used bubbles
                for (byte b = bCount; b < 10; b++)
                {
                    StatusBubble = (Ellipse)this.FindName("Status" + b.ToString() + "Outline");
                    storyboardOptions.uiElement = StatusBubble;
                    storyboardOptions.To = 0.25;
                    bediaTransitions.UIOpacity(storyboardOptions);

                    StatusBubble = (Ellipse)this.FindName("Status" + b.ToString());
                    storyboardOptions.uiElement = StatusBubble;
                    storyboardOptions.To = 0.0;
                    bediaTransitions.UIOpacity(storyboardOptions);
                }


                //Show bubbles that have some value
                for (byte b = 0; b < bCount; b++)
                {
                    StatusBubble = (Ellipse)this.FindName("Status" + b.ToString() + "Outline");
                    //StatusBubble.Opacity = 1.0;
                    storyboardOptions.uiElement = StatusBubble;
                    storyboardOptions.To = 1.0;
                    bediaTransitions.UIOpacity(storyboardOptions);

                    StatusBubble = (Ellipse)this.FindName("Status" + b.ToString());
                    //StatusBubble.Opacity = 1.0;
                    storyboardOptions.uiElement = StatusBubble;
                    storyboardOptions.To = 1.0;
                    bediaTransitions.UIOpacity(storyboardOptions);

                    if (b == (bCount - 1))
                    {
                        StatusBubble.StrokeThickness = (StatusBubble.ActualWidth / 2) * (bPercent * 0.1);
                    }
                    else
                    {
                        StatusBubble.StrokeThickness = (StatusBubble.ActualWidth / 2);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetWindowsVolume(byte Percent)
        {
            try
            {
                BediaTransitions bediaTransitions = new BediaTransitions();
                BediaStoryboardOptions storyboardOptions = new BediaStoryboardOptions();
                Ellipse WindowsVolumeBubble = null;
                byte bCount = 0;
                byte bPercent = Percent;


                storyboardOptions.Milliseconds = 400;
                storyboardOptions.To = 0.0;

                if (Percent < 11)
                {
                    bCount = 0; 
                }
                else if (Percent == 100)
                {
                    bCount = 11;
                    bPercent = 10;
                }
                else
                {
                    bCount = Convert.ToByte(Percent.ToString().Substring(0, 1));
                    bPercent = Convert.ToByte(Percent - (bCount * 10));
                    bCount += 1;
                }


                //Hide non-used bubbles
                for (byte b = bCount; b < 10; b++)
                {
                    WindowsVolumeBubble = (Ellipse)this.FindName("WindowsVolume" + b.ToString());
                    storyboardOptions.uiElement = WindowsVolumeBubble;
                    bediaTransitions.UIOpacity(storyboardOptions);
                }

                if (Percent <= 0) { return; }

                storyboardOptions.To = 1.0;

                //Show bubbles that have some value
                for (byte b = 0; b < bCount; b++)
                {
                    WindowsVolumeBubble = (Ellipse)this.FindName("WindowsVolume" + b.ToString());
                    storyboardOptions.uiElement = WindowsVolumeBubble;
                    bediaTransitions.UIOpacity(storyboardOptions);

                    if (b == (bCount - 1))
                    {
                        WindowsVolumeBubble.StrokeThickness = (WindowsVolumeBubble.ActualWidth / 2) * (bPercent * 0.1);
                    }
                    else
                    {
                        WindowsVolumeBubble.StrokeThickness = (WindowsVolumeBubble.ActualWidth / 2);
                    }
                }


                bediaTransitions = null;
                storyboardOptions = null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
