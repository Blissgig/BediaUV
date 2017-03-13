using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace BediaUV
{
    public sealed partial class BediaClock : UserControl
    {
        private DateTime DisplayedTime = new DateTime(DateTime.Now.Ticks);
        private bool DimAtNight = false; //Used for late night dimming
        private BediaTransitions bediaTransitions = new BediaTransitions();
        
        public BediaClock()
        {
            this.InitializeComponent();
        }

        public BediaClock(SolidColorBrush FontColor, byte FontSize)
        {
            try
            {
                InitializeComponent();

                this.Hour.Foreground = FontColor;
                this.Colon.Foreground = FontColor;
                this.MinutesTen.Foreground = FontColor;
                this.Minutes.Foreground = FontColor;
                this.AMPM.Foreground = FontColor;

                this.Hour.FontSize = FontSize;
                this.Colon.FontSize = FontSize;
                this.MinutesTen.FontSize = FontSize;
                this.Minutes.FontSize = FontSize;
                this.AMPM.FontSize = FontSize;
                

                //Set back 13 hours to insure that the first iteration changes the Hour, Minutes and Seconds
                DisplayedTime = DisplayedTime.Subtract(new TimeSpan(13, 11, 31));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ClockStart(bool Dim = false)
        {
            try
            {
                this.Colon.Text = ":";
                this.DimAtNight = Dim;

                Storyboard sbClock = new Storyboard();

                // - Colon blinking on and off
                DoubleAnimation animColon = new DoubleAnimation();
                animColon.From = 0.0;
                animColon.To = 1.0;
                animColon.Duration = new Duration(new TimeSpan(0, 0, 2));
                animColon.RepeatBehavior = RepeatBehavior.Forever;
                animColon.AutoReverse = true;

                Storyboard.SetTarget(animColon, this.Colon);
                Storyboard.SetTargetProperty(animColon, "Opacity");
                sbClock.Children.Add(animColon);

                sbClock.Begin();

                ClockSet();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ClockSet()
        {
            try
            {
                DateTime CurrentTime = new DateTime(DateTime.Now.Ticks);


                if (this.DimAtNight == true)
                {
                    if (CurrentTime.Hour > 20 || CurrentTime.Hour < 7)
                    {
                        if (this.Opacity > 0.5)
                        {
                            this.Opacity = 0.5;
                        }
                    }
                    else
                    {
                        if (this.Opacity < 1.0)
                        {
                            this.Opacity = 1.0;
                        }
                    }
                }

                // - HOUR -
                if (CurrentTime.Hour != DisplayedTime.Hour)
                {
                    bediaTransitions.TextChange(this.Hour, CurrentTime.ToString(" h"));
                }

                // - MINUTES TEN -
                if (CurrentTime.ToString("mm").Substring(0, 1) != DisplayedTime.ToString("mm").Substring(0, 1))
                {
                    bediaTransitions.TextChange(this.MinutesTen, CurrentTime.ToString("mm").Substring(0, 1));
                }

                // - MINUTE WILL ALWAYS CHANGE
                bediaTransitions.TextChange(this.Minutes, CurrentTime.ToString("mm").Substring(1, 1));

                //- AM/PM -
                if (CurrentTime.ToString("tt") != DisplayedTime.ToString("tt") || this.AMPM.Text.Trim().Length == 0)
                {
                    bediaTransitions.TextChange(this.AMPM, CurrentTime.ToString("tt"));
                }

                DisplayedTime = CurrentTime;

                ClockPause();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void ClockPause()
        {
            try
            {
                byte bSeconds = Convert.ToByte(60 - DateTime.Now.Second);
                
                await Task.Delay(TimeSpan.FromSeconds(bSeconds));

                ClockSet();
            }
            catch { }
        }
    }
}
