using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace BediaUV
{
    public sealed partial class BediaMenuUI : UserControl
    {
        public BediaItem Bediaitem;

        public Int16 Top { get; set; }
        
        public BediaMenuUI()
        {
            this.InitializeComponent();
        }

        public BediaMenuUI(
            BediaIcon bediaIcon, 
            BediaItem bediaitem,
            string Title,
            byte FontSize,
            byte MenuSize)
        {
            InitializeComponent();
            
            try
            {
                this.colIcon.Width = new GridLength(MenuSize);
                this.rowTitlebar.Height = new GridLength(MenuSize);

                this.Icon.Source = bediaIcon.Icon.Source;
                this.Title.Text = Title;
                this.IconBackground.Width = bediaIcon.Size;
                this.IconBackground.Height = bediaIcon.Size;
                this.IconBackground.Fill = bediaIcon.Fill; 
                this.IconBackground.Stroke = bediaIcon.Stroke;
                this.IconBackground.StrokeThickness = bediaIcon.StrokeThickness;
                this.Title.FontSize = FontSize;

                Bediaitem = bediaitem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetIcon(BediaIcon bediaIcon, bool Hide = false, bool NowPlaying = false)
        {
            try
            {
                if (this.StatusIcon.Opacity == 0.0)
                {
                    //Icon is already hidden, just show it
                    ShowIcon(bediaIcon, Hide, NowPlaying);
                }
                else
                {
                    Int16 iMilliseconds = 288;
                    Storyboard sbFadeOut = new Storyboard();
                    DoubleAnimation aniFadeOutIcon = new DoubleAnimation();
                    DoubleAnimation aniFadeOutBackground = new DoubleAnimation();


                    aniFadeOutIcon.Duration = new Duration(TimeSpan.FromMilliseconds(iMilliseconds));
                    aniFadeOutIcon.From = this.StatusIcon.Opacity;
                    aniFadeOutIcon.To = 0.0;
                    Storyboard.SetTarget(aniFadeOutIcon, this.StatusIcon);
                    Storyboard.SetTargetProperty(aniFadeOutIcon, "(UIElement.Opacity)");
                    sbFadeOut.Children.Add(aniFadeOutIcon);

                    aniFadeOutBackground.Duration = new Duration(TimeSpan.FromMilliseconds(iMilliseconds));
                    aniFadeOutBackground.From = this.StatusIconBackground.Opacity;
                    aniFadeOutBackground.To = 0.0;
                    Storyboard.SetTarget(aniFadeOutBackground, this.StatusIconBackground);
                    Storyboard.SetTargetProperty(aniFadeOutBackground, "(UIElement.Opacity)");
                    sbFadeOut.Children.Add(aniFadeOutBackground);

                    sbFadeOut.Completed += (s, e) =>
                    {
                        ShowIcon(bediaIcon, Hide, NowPlaying);
                    };
                    sbFadeOut.Begin();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ShowIcon(BediaIcon bediaIcon, bool Hide, bool NowPlaying)
        {
            try
            {
                if (Hide == false)
                {
                    //Set status icon and it's background object
                    Int16 iMilliseconds = 288;
                    this.StatusIcon.Source = bediaIcon.Icon.Source;   
                    this.StatusIconBackground.Fill = bediaIcon.Fill;
                    this.StatusIconBackground.Height = bediaIcon.Size;
                    this.StatusIconBackground.Width = bediaIcon.Size;
                    this.StatusIconBackground.Stroke =  bediaIcon.Stroke;
                    this.StatusIconBackground.StrokeThickness = bediaIcon.StrokeThickness;
                    
                    
                    Storyboard sbFadeIn = new Storyboard();
                    DoubleAnimation aniFadeInIcon = new DoubleAnimation();
                    DoubleAnimation aniFadeInBackground = new DoubleAnimation();
                    
                    aniFadeInIcon.Duration = new Duration(TimeSpan.FromMilliseconds(iMilliseconds));
                    aniFadeInIcon.From = 0.0;
                    aniFadeInIcon.To = 1.0;
                    Storyboard.SetTarget(aniFadeInIcon, this.StatusIcon);
                    Storyboard.SetTargetProperty(aniFadeInIcon, "(UIElement.Opacity)");
                    sbFadeIn.Children.Add(aniFadeInIcon);

                    aniFadeInBackground.Duration = new Duration(TimeSpan.FromMilliseconds(iMilliseconds));
                    aniFadeInBackground.From = 0.0;
                    aniFadeInBackground.To = 1.0;
                    Storyboard.SetTarget(aniFadeInBackground, this.StatusIconBackground);
                    Storyboard.SetTargetProperty(aniFadeInBackground, "(UIElement.Opacity)");
                    sbFadeIn.Children.Add(aniFadeInBackground);
                    sbFadeIn.Begin();
                }

                if (NowPlaying == true)
                {
                    Storyboard sbNowPlaying = new Storyboard();
                    ExponentialEase ease = new ExponentialEase();
                    ColorAnimation aniNowPlaying = new ColorAnimation();

                    ease.EasingMode = EasingMode.EaseInOut;

                    aniNowPlaying.Duration = new Duration(new TimeSpan(0, 0, 1));
                    aniNowPlaying.EasingFunction = ease;

                    aniNowPlaying.From = ((Windows.UI.Xaml.Media.SolidColorBrush)(this.StatusIconBackground.Fill)).Color; 
                    aniNowPlaying.To = ((Windows.UI.Xaml.Media.SolidColorBrush)(this.StatusIconBackground.Stroke)).Color; 
                    aniNowPlaying.AutoReverse = true;
                    aniNowPlaying.RepeatBehavior = RepeatBehavior.Forever;

                    Storyboard.SetTarget(aniNowPlaying, this.StatusIconBackground);
                    Storyboard.SetTargetProperty(aniNowPlaying, "(Ellipse.Fill).(SolidColorBrush.Color)");
                    sbNowPlaying.Children.Add(aniNowPlaying);
                    sbNowPlaying.Begin();
                 }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
