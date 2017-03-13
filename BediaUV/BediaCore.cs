using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.Web.Syndication;
using System.Threading;
using Windows.Storage.Streams;
using Windows.Foundation;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.Devices.Geolocation;
using Microsoft.OneDrive.Sdk;
using Windows.UI;
using Windows.Storage.Search;
//using Windows.Devices.Geolocation; //TODO: if it changes from the current location method

namespace BediaUV
{
    class ScaleConverter : IValueConverter
    {
        //For menu font size
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resolutionScale = (int)DisplayInformation.GetForCurrentView().ResolutionScale / 100.0;
            var baseValue = int.Parse(parameter as string);
            var scaledValue = baseValue * resolutionScale;
            if (targetType == typeof(GridLength))
                return new GridLength(scaledValue);
            return scaledValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IPInfo
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string loc { get; set; }
        public string org { get; set; }
        public string postal { get; set; }
    }

    public class ISSLocation
    {
        //http://open-notify.org/Open-Notify-API/ISS-Location-Now/

        public string message { get; set; }

        public string timestamp { get; set; }

        public ISSLocationLatLong iss_position { get; set; }
    }

    public class ISSLocationLatLong
    {
        public double latitude { get; set; }

        public double longitude { get; set; }
    }

    public class ISSAddress
    {
        public string geoplugin_place { get; set; }

        public string geoplugin_countryCode { get; set; }

        public string geoplugin_region { get; set; }

        public string geoplugin_regionAbbreviated { get; set; }

        public string geoplugin_latitude { get; set; }

        public string geoplugin_longitude { get; set; }

        public string geoplugin_distanceMiles { get; set; }

        public string geoplugin_distanceKilometers { get; set; }
    }

    public class BediaWeather
    {
        public string Day { get; set; }
        public string Conditions { get; set; }
        public string TempMin { get; set; }
        public string TempMax { get; set; }
    }

    public class BediaBubble
    {
        public string Name = "";
        public Int16 SizeStart = 0;
        public Int16 SizeEnd = 0;
        public Int16 SizeTime = 500; //Millisecond
        public Int16 TopStart = 0;
        public Int16 TopEnd = 0;
        public Int16 LeftStart = 0;
        public Int16 LeftEnd = 0;
        public Int16 PositionTime = 500;
        public double OpacityStart = 1.0;
        public double OpacityEnd = 1.0;
        public Int16 OpacityTime = 500;
        public SolidColorBrush BubbleColorStart = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 49, 123, 193)); //Default color
        public SolidColorBrush BubbleColorEnd = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 49, 123, 193)); //Default color
        public byte StrokeStart = 2;
        public byte StrokeEnd = 2;
        public Int16 StrokeTime = 500;
        public SolidColorBrush StrokeColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)); //White
        public Ellipse Bubble = null;
        public BediaBubble AfterEffects = null;
        public bool Reverse = false;
        public bool Remove = true;
    }
    
    public class BediaIO
    {
        public async void FileDelete(StorageFile MediaFile)
        {
            try
            {
                await MediaFile.DeleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async void FolderDelete(string path)
        {
            try
            {
                StorageFolder sf = await StorageFolder.GetFolderFromPathAsync(path);
                await sf.DeleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async void FolderCreate(StorageFolder Folder, string FolderName)
        {
            try
            {
                await Folder.CreateFolderAsync(FolderName, CreationCollisionOption.FailIfExists);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> FolderExists(string path)
        {
            bool bReturn = false;

            try
            {
                StorageFolder sf = await StorageFolder.GetFolderFromPathAsync(path);

                if (sf != null)
                {
                    bReturn = true;
                }

            }
            catch { }

            return bReturn;
        }

        public string FolderFilePath(StorageFolder Folder, string FileName)
        {
            string sPath = Folder.Path;

            try
            {
                if (FileName.IndexOf("\\") < 0)
                {
                    if (Folder.Path.IndexOf("\\") != Folder.Path.Length)
                    {
                        FileName = "\\" + FileName;
                    }
                }

                sPath += FileName;
            }
            catch (Exception)
            {
                throw;
            }

            return sPath;
        }

        public async Task<bool> FileExists(StorageFolder Folder, string FileName)
        {
            bool bReturn = false;

            try
            {
				try
				{
					if (await Folder.TryGetItemAsync(FileName) != null)
					{
						bReturn = true;
					}
				}
				catch { }
             
                
                if (bReturn == false)
                {
                    if (FileName.IndexOf("\\") < 0)
                    {
                        if (Folder.Path.IndexOf("\\") != Folder.Path.Length)
                        {
                            FileName = "\\" + FileName;
                        }
                    }

                    StorageFile sfMedia = await StorageFile.GetFileFromPathAsync(FolderFilePath(Folder, FileName));

                    bReturn = true;

                    sfMedia = null;
                }
            }
            catch  { }
            
            return bReturn;
        }
    }

    public class BediaStoryboardOptions
    {
        public BediaStoryboardOptions() { }
		private Int16 miMilliseconds = 888;

        public BediaStoryboardOptions(UIElement uiElement, double To)
        {
            this.uiElement = uiElement;
            this.To = To;
        }

        public BediaStoryboardOptions(UIElement uiElement, Int16 Milliseconds, double To)
        {
            this.uiElement = uiElement;
            this.Milliseconds = Milliseconds;
            this.To = To;
        }

        public Int16 Milliseconds
        {
            get { return miMilliseconds; }

            set { miMilliseconds = value; }
        }

        public Double To { get; set; }

        public UIElement uiElement { get; set; }
        
        public bool AutoReverse { get; set; }
    }

    public class BediaTransitions
    {
        public BediaTransitions()
        { }

        public void TextChange(TextBlock UITextBlock, string Text)
        {
            try
            {
                Storyboard sbFadeOut = new Storyboard();
                DoubleAnimation animFadeOut = new DoubleAnimation();

                animFadeOut.Duration = new Duration();
                animFadeOut.From = 1.0;
                animFadeOut.To = 0.0;
                animFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                Storyboard.SetTarget(animFadeOut, UITextBlock);
                Storyboard.SetTargetProperty(animFadeOut, "Opacity");

                sbFadeOut.Children.Add(animFadeOut);

                sbFadeOut.Completed +=
                    (sndr, evtArgs) =>
                    {
                        UITextBlock.Text = Text;
                        Storyboard sbFadeIn = new Storyboard();
                        DoubleAnimation animFadeIn = new DoubleAnimation();

                        animFadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                        animFadeIn.From = 0.0;
                        animFadeIn.To = 1.0;

                        Storyboard.SetTarget(animFadeIn, UITextBlock);
                        Storyboard.SetTargetProperty(animFadeIn, "Opacity");

                        sbFadeIn.Children.Add(animFadeIn);
                        sbFadeIn.Begin();
                    };

                sbFadeOut.Begin();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ImageChange(Windows.UI.Xaml.Controls.Image UIImage, Windows.UI.Xaml.Controls.Image Icon)
        {
            try
            {
                Storyboard sbFadeOut = new Storyboard();
                DoubleAnimation animFadeOut = new DoubleAnimation();

                animFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                animFadeOut.From = UIImage.Opacity; //From whatever value it is currently set
                animFadeOut.To = 0.0;

                Storyboard.SetTarget(animFadeOut, UIImage);
                Storyboard.SetTargetProperty(animFadeOut, "(UIElement.Opacity)");

                sbFadeOut.Children.Add(animFadeOut);

                sbFadeOut.Completed +=
                    (sndr, evtArgs) =>
                    {
                        UIImage.Source = Icon.Source;
                        Storyboard sbFadeIn = new Storyboard();
                        DoubleAnimation animFadeIn = new DoubleAnimation();

                        animFadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        animFadeIn.From = 0.0;
                        animFadeIn.To = 1.0;

                        Storyboard.SetTarget(animFadeIn, UIImage);
                        Storyboard.SetTargetProperty(animFadeIn, "(UIElement.Opacity)");

                        sbFadeIn.Children.Add(animFadeIn);
                        sbFadeIn.Begin();
                    };

                sbFadeOut.Begin();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UIOpacity(BediaStoryboardOptions storyboardOptions)
        {
            try
            {
                if (storyboardOptions.uiElement.Opacity != storyboardOptions.To)
                {
                    Storyboard sbUI = new Storyboard();
                    DoubleAnimation aniUI = new DoubleAnimation();

                    aniUI.Duration = new Duration(TimeSpan.FromMilliseconds(storyboardOptions.Milliseconds));
                    aniUI.From = storyboardOptions.uiElement.Opacity;
                    aniUI.To = storyboardOptions.To;
                    //aniUI.AutoReverse = storyboardOptions.AutoReverse;
                    
                    Storyboard.SetTarget(aniUI, storyboardOptions.uiElement);
                    Storyboard.SetTargetProperty(aniUI, "(UIElement.Opacity)");
                    sbUI.Children.Add(aniUI);
                    sbUI.Begin();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class PastItem
    {
        public string Path = "";
        public string Hash = "";
        public double Seconds = 0;
        public string FileDate = "";
        public ulong Size = 0;
        public DateTime PlayedDate = DateTime.Now;
        public bool PlayState = false;
        
        public string Name
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
        }
    }

    public class BediaIcon
    {
        public double Size { get; set; }
        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public Windows.UI.Xaml.Controls.Image Icon { get; set; }
    }

    public class NavigationItem
    {
        public NavigationType Left = NavigationType.Menu;
        public NavigationType Right = NavigationType.PlayPause;

        public NavigationItem() { }

        public NavigationItem(NavigationType left, NavigationType right)
        {
            Left = left;
            Right = right;
        }

        public enum NavigationType
        {
            Menu,
            PlayPause,
            Stop,
            VolumeUp,
            VolumeDown,
            JumpForwardSeconds30,
            JumpBackSeconds30,
            JumpForwardMinute1,
            JumpBackMinute1,
            JumpForwardMinute5,
            JumpBackMinute5,
            JumpForwardMinute10,
            JumpBackMinute10,
            Previous,
            Next
        }

        public string Title(NavigationType NavType)
        {
            string sTitle = "Play";

            switch (NavType)
            {
                case NavigationType.JumpBackMinute1:
                    sTitle = "Back 1 Minute";
                    break;

                case NavigationType.JumpBackMinute10:
                    sTitle = "Back 10 Minutes";
                    break;

                case NavigationType.JumpBackMinute5:
                    sTitle = "Back 5 Minutes";
                    break;

                case NavigationType.JumpBackSeconds30:
                    sTitle = "Back 30 Seconds";
                    break;

                case NavigationType.JumpForwardMinute1:
                    sTitle = "Forward 1 Minute";
                    break;

                case NavigationType.JumpForwardMinute10:
                    sTitle = "Forward 10 Minutes";
                    break;

                case NavigationType.JumpForwardMinute5:
                    sTitle = "Back 5 Minutes";
                    break;

                case NavigationType.JumpForwardSeconds30:
                    sTitle = "Forward 30 Seconds";
                    break;

                case NavigationType.PlayPause:
                    sTitle = "Play";
                    break;

                case NavigationType.Stop:
                    sTitle = "Stop";
                    break;

                case NavigationType.VolumeDown:
                    sTitle = "Quieter";
                    break;

                case NavigationType.VolumeUp:
                    sTitle = "Louder";
                    break;
            }

            return sTitle;
        }


        public Windows.UI.Xaml.Controls.Image TitleImage(NavigationType NavType)
        {
            string sIcon = "NavPlay";
            Windows.UI.Xaml.Controls.Image imgReturn = new Windows.UI.Xaml.Controls.Image();

            switch (NavType)
            {
                case NavigationType.JumpBackMinute1:
                    sIcon = "Back 1 Minute";
                    break;

                case NavigationType.JumpBackMinute10:
                    sIcon = "Back 10 Minutes";
                    break;

                case NavigationType.JumpBackMinute5:
                    sIcon = "Back 5 Minutes";
                    break;

                case NavigationType.JumpBackSeconds30:
                    sIcon = "Back 30 Seconds";
                    break;

                case NavigationType.JumpForwardMinute1:
                    sIcon = "Forward 1 Minute";
                    break;

                case NavigationType.JumpForwardMinute10:
                    sIcon = "Forward 10 Minutes";
                    break;

                case NavigationType.JumpForwardMinute5:
                    sIcon = "Back 5 Minutes";
                    break;

                case NavigationType.JumpForwardSeconds30:
                    sIcon = "Forward 30 Seconds";
                    break;

                case NavigationType.PlayPause:
                    sIcon = "Play";
                    break;

                case NavigationType.Stop:
                    sIcon = "Stop";
                    break;

                case NavigationType.VolumeDown:
                    sIcon = "Quieter";
                    break;

                case NavigationType.VolumeUp:
                    sIcon = "Louder";
                    break;
            }

            imgReturn.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + sIcon + ".png", UriKind.RelativeOrAbsolute));

            return imgReturn;
        }
    }

    public class BediaItem
    {
        #region Members
        public StorageFile BediaFile;
        public StorageFolder BediaFolder;
        public PastItem HistoryInfo = null;
        public MenuTypes MenuType = MenuTypes.Info;
        public bool Selected = false;
        private string msSortBy = "";
        private string msTitle = "";
        private string msValue = "";
        private Int16 miValue = 0;
        private object moValue = null;
        #endregion

        public enum MenuTypes
        {
            About,
            AddRemoveHomeFolders,
            AddRemoveHomeFolder,
            AddRemoveHomeFolderSelect,
            AddRemoveFolders,
            Audio,
            EmptyQueue,
            CloseMenu,
            ExitApp,
            ExitAppYes,
            ExitAppNo,
            Folder,
            Home,
            Info,
            IPTVHome,
            IPTVPlaylist,
            IPTVVideo,
            Library,
            Libraries,
            PastAlphabetical,
            PastHome,
            PastItem,
            PastList,
            PastFolder,
            PausedAlphabetical,
            PausedFolder,
            PausedHome,
            Page,
            Play,
            Playlist,
            PlaylistItem,
            PlaylistCurrent,
            Playlists,
            PlaylistOnHomePage,
            Podcast,
            Podcasts,
            PodcastItem,
            SelectAll,
            SelectFolder,
            Services,
            SettingsHome,
            SettingsAlbumFolderArt,
            SettingsBubbles,
            SettingsHistoryClear,
            SettingsHistoryClearOK,
            SettingsHistoryClearCancel,
            SettingsHistorySave,
            SettingsHistorySync,
            SettingsIPTVFolder,
            SettingsLateNight,
            SettingsLibrary,
            SettingsMediaDisplayInfo,
            SettingsMenuFont,
            SettingsMenuFonts,
            SettingsMenuColors,
            SettingsMenuColor,
            SettingsMenuCustomColorSelect,
            SettingsMenuCustomColorSelected,
            SettingsMenuCustomColor,
            SettingsMenuOptionItem,
            SettingsMenuSizeTile,
            SettingsMenuSpeed,
            SettingsMenuSpeeds,
            SettingsMonitorFolders,
            SettingsMultipleMonitors,
            SettingsMultipleMonitorsItem,
            SettingsNavigationIcons,
            SettingsNavigationSet,
            SettingsNavigationSets,
            SettingsOptionalMenus,
            SettingsOptionalMenu,
            SettingsPartyBubbles,
            SettingsPodcast,
            SettingsPodcasts,
            SettingsScreenSaverComments,
            SettingsScreenSaverClock,
            SettingsSpeechRecognition,
            SettingsStartWithWindows,
            SettingsStretchVideo,
            SettingsTitlebarClock,
            SettingsTransitMenus,
            SettingsTransitMenu,
            SettingsVolumePercentage,
            SettingsAdvanced,
            SettingsVisualizations,
            SettingsVisualizationItem,
            SettingsVisualizationsMultipleMonitors,
            SettingsVisualizationsMultipleMonitor,
            SettingsWindowsVolume,
            SpaceHome,
            SpaceItem,
            SpaceISSLocationHome,
            SpaceISSLocationItem,
            StretchVideo,
            TheatreTimesHome,
            TheatreTimesMovie,
            TheatreTimesTheatre,
            TheatreTimesItem,
            TransitHome,
            TransitItem,
            Video,
            WeatherHome,
            WeatherItem,
            YouTubeHome,
            YouTubeFolder,
            YouTubePlaylist,
            YouTubeVideo
        }

        public BediaItem()
        {
            //Default, with no presets
        }

        public BediaItem(string Title, MenuTypes MenuType)
        {
            this.Title = Title;
            this.MenuType = MenuType;
        }

        public BediaItem(string Title, StorageFile BediaFile, MenuTypes MenuType)
        {
            this.Title = Title;
            this.BediaFile = BediaFile;
            this.MenuType = MenuType;
        }

        #region Properties
        public string Title
        {
            get
            {
                if (msTitle.Length == 0)
                {
                    if (BediaFile == null)
                    {
                        return "";
                    }
                    else
                    {
                        return BediaFile.Name;
                    }
                }
                else
                {
                    return msTitle;
                }
            }

            set
            {
                msTitle = value;
                SortedBy = value;
            }
        }

        public string SortedBy
        {
            get { return msSortBy; }

            set
            {
                if (value.IndexOf("The ") == 0)
                {
                    msSortBy = value.Substring(4); // + ", The";
                }
                else if (value.IndexOf("A ") == 0)
                {
                    msSortBy = value.Substring(2); // + ", A";
                }
                else
                {
                    msSortBy = value;
                }
            }
        }

        public string Value
        {
            get { return msValue; }

            set { msValue = value; }
        }

        public Int16 ValueInt
        {
            get { return miValue; }

            set { miValue = value; }
        }

        public object ValueObject
        {
            get { return moValue; }

            set { moValue = value; }
        }
        #endregion
    }

    public static class GradientStopCollectionExtensions
    {
        //https://stackoverflow.com/questions/9650049/get-color-in-specific-location-on-gradient
        public static Color GetRelativeColor(this GradientStopCollection gsc, double offset)
        {
            GradientStop before = gsc.Where(w => w.Offset == gsc.Min(m => m.Offset)).First();
            GradientStop after = gsc.Where(w => w.Offset == gsc.Max(m => m.Offset)).First();

            foreach (var gs in gsc)
            {
                if (gs.Offset < offset && gs.Offset > before.Offset)
                {
                    before = gs;
                }
                if (gs.Offset > offset && gs.Offset < after.Offset)
                {
                    after = gs;
                }
            }

            var color = new Color();

            color.A = (byte)((offset - before.Offset) * (after.Color.A - before.Color.A) / (after.Offset - before.Offset) + before.Color.A);
            color.R = (byte)((offset - before.Offset) * (after.Color.R - before.Color.R) / (after.Offset - before.Offset) + before.Color.R);
            color.G = (byte)((offset - before.Offset) * (after.Color.G - before.Color.G) / (after.Offset - before.Offset) + before.Color.G);
            color.B = (byte)((offset - before.Offset) * (after.Color.B - before.Color.B) / (after.Offset - before.Offset) + before.Color.B);

            return color;
        }
    }

    class BediaCore
    {
        #region Private Members
        private MainPage mMainPage;
        private CompositeTransform TitlebarTransform = new CompositeTransform();
        private IPInfo bediaLocation = null;
        private BediaClock TitlebarClock;
        private BediaTitlebar bediaTitlebar;
        private SolidColorBrush BediaBaseColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 102, 204)); 
        private SolidColorBrush BediaHighlightColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
        private PointerPoint TouchDown = null;
        private string msAppName = "Bedia UV";
        private string msIPTVPath = "IPTV";
        private Random BediaRandom = new Random();
        private Storyboard BubbleStoryboard = new Storyboard();
        private Storyboard TitlebarStoryboard = new Storyboard();
        private string msHistoryFile = "Bedia History.txt";
        private Point CurrentPoint;
        private GradientStopCollection BediaGradientStopCollection = new GradientStopCollection();

        //Menu Settings
        private bool mbProcessCompleted = false;
        private bool mbMenuLoadCompleted = true;
        private Int16 miMenuCurrentPosition = 0;
        private List<BediaItem> BediaMenuItems = new List<BediaItem>();
        private List<BediaItem> MenuBreadrumbs = new List<BediaItem>();
        private List<string> ScreenSaverComments = new List<string>();
        private bool FullPageScroll = false;
        private BediaItem BookmarkItem;
        private byte mbMenusCount = 0;

        //Settings / Options
        private byte mbMenuSize = 90;
        private Int16 miAnimationSpeed = 400; //Milliseconds: Quick:250, Med:400 (default value), Long:800
        private byte mbScreenSaverMinutes = 8;
        private bool mbStretchVideo = false;

        //-- PLAYBACK / MEDIA --
        private MediaStatus mediastatus;
        private BediaNavigation medianavigation;
        private List<BediaItem> BediaPlaylist = new List<BediaItem>();
        private List<PastItem> PastListHistory = new List<PastItem>();
        private BediaItem HackBediaItem = null;  //This is a hack, used for a few times when need to store a value
        private Int16 miMediaCurrentPosition = 0;
        private bool IsPlaying = false;
        private bool IsPaused = false;
        private bool IsMenuDisplayed = true;
        public double mdVolume = 0.35;
        List<NavigationItem> NavigationItems = new List<NavigationItem>();
        private Int16 miNavigationCurrentPosition = 0;
        private DateTime NavigationDisplayDateTime = DateTime.Now;
        private byte NavigationDisplaySeconds = 60;
        private List<PastItem> NewMediaItems = new List<PastItem>();
        private dynamic MovieList;
        private DateTime MovieListDate = Convert.ToDateTime("09/08/2007"); //Our wedding date (awwww)
        private string msMediaDetails = ""; //Used for removing previous MediaDetails textblock
        private string msMediaDetailsShadow = "";
        private string msMediaTypesAudio = "*.mp3,*.wma,*.wav,*.m4a,*.flac,";
        private string msMediaTypesVideo = "*.avi,*.asf,*.mp4,*.m4v,*.mpg,*.mpeg,*.mpeg2,*.mpeg4,*.wmv,*.3gp,*.mov,*.mts,*.divx,*.mkv,";
        private string msMediaTypesPlaylist = "*.m3u,";

        //Various timers
        Timer NavigationTimer = null;
        Timer StatusTimer = null;
        Timer ScreenSaverTimer = null;
        Timer ScreenSaverCommentsTimer = null;
        Timer ISSMapTimer = null;
        #endregion

        #region Public Properties

        public string HomePageTitle { get; set; }

        public byte MenuSize
        {
            get { return mbMenuSize; }

            set
            {
                if (value < 20)
                {
                    value = 100; //Just in case.
                }
                mbMenuSize = value;
            }
        }

        public byte MenuFontSize
        {
            get { return Convert.ToByte(this.MenuSize - 10); }
        }

        public bool MediaOpened { get; set; }

        public Int16 AnimationSpeed
        {
            get { return miAnimationSpeed; }

            set
            {
                if (value < 100)
                {
                    miAnimationSpeed = 400;  //Default
                }
                else if (value > 5000)
                {
                    miAnimationSpeed = 400;  //Default
                }
                else
                {
                    miAnimationSpeed = value;
                }
            }
        }

        public double Volume
        {
            get { return mdVolume; }

            set
            {
                mdVolume = value;

                if (mdVolume > 1.0)
                {
                    mdVolume = 1.0;
                }
                else if (mdVolume < 0.01)
                {
                    mdVolume = 0.0;
                }

                SettingSave("Volume", mdVolume);
            }
        }
        
        public bool StretchVideo
        {
            get { return mbStretchVideo; }

            set
            {
                mbStretchVideo = value;

                if (mbStretchVideo == true)
                {
                    mMainPage.BediaMedia.Stretch = Stretch.Fill;
                }
                else
                {
                    mMainPage.BediaMedia.Stretch = Stretch.Uniform; // Stretch.None;
                }
            }
        }

        public bool Bubbles { get; set; }
        
        public bool BubblesParty { get; set; }

        public bool SavePast { get; set; }

        public bool SaveOneDrive { get; set; }

        #endregion

        public BediaCore(MainPage mainPage)
        {
            try
            {
                LocationGet();  //Started asap so that the data is available for Weather, Transit, etc.

                this.mMainPage = mainPage;
                this.mMainPage.ScreenSaver.Opacity = 1.0;

                Window.Current.CoreWindow.PointerCursor = null; //Hide mouse cursor
                

                ////Hide Titlebar.  Buttons still show
                ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                formattableTitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                
                SettingsDefaults();

                SettingsGet();  //Make sure this is handled early because other items may rely on these values
                
                mMainPage.rowTitlebar.Height = new GridLength(this.MenuSize);
                

                bediaTitlebar = new BediaTitlebar(BediaBaseColor, BediaHighlightColor, MenuFontSize);

                bediaTitlebar.Opacity = 1.0;
                bediaTitlebar.Height = this.MenuSize;
                bediaTitlebar.rowTitlebar.Height = new GridLength(this.MenuSize);
                bediaTitlebar.colIcon.Width = new GridLength(this.MenuSize);
                mMainPage.BediaBase.Children.Add(bediaTitlebar);
                Grid.SetRow(bediaTitlebar, 0);
                Grid.SetColumn(bediaTitlebar, 0);
                
                mediastatus = new MediaStatus(BediaBaseColor, BediaHighlightColor, this.MenuSize);
                mediastatus.Opacity = 0.0;
                mediastatus.Height = this.MenuSize;
                mMainPage.BediaBase.Children.Add(mediastatus);
                Grid.SetRow(mediastatus, 0);
                Grid.SetColumn(mediastatus, 0);

                medianavigation = new BediaNavigation();
                medianavigation.Opacity = 0.0;
                medianavigation.Height = this.MenuSize;
                medianavigation.colCenter.Width = new GridLength(this.MenuSize * 2); //to insure that the Naviagtion and Status Bubbles line up
                mMainPage.BediaBase.Children.Add(medianavigation);
                Grid.SetRow(medianavigation, 0);
                Grid.SetColumn(medianavigation, 0);

                SetTitlebarClock();

                ScreenSaverStop();

                mMainPage.BediaMap.MapServiceToken = "vVYRMXuDD2kbbQAVljUo~2GD-btHW5Zw7HjyNPC7gXg~AugCyzNohOEcUk8aDBi3l9O8OV7afhUTvB3OZD_ltcg68vOPUtoQ3Sqq2i5onbHZ";
                mMainPage.BediaMap.ZoomLevel = 6;
                mMainPage.BediaMap.Style = Windows.UI.Xaml.Controls.Maps.MapStyle.AerialWithRoads;  // .Terrain; // .AerialWithRoads; <-nice! // .Aerial3D; // .Aerial;
                mMainPage.BediaMap.LandmarksVisible = true;
                mMainPage.BediaMap.Width = mMainPage.ActualWidth;
                mMainPage.BediaMap.Height = (mMainPage.ActualHeight - this.MenuSize);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private string AppName()
        {
            string sAppName = "Bedia UV"; //Default, just in case

            try
            {
                sAppName = Package.Current.DisplayName;
            }
            catch (Exception)
            { }

            return sAppName;
        }

        private void ExitApp()
        {
            try
            {
                PastSave();

                Application.Current.Exit();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private string DaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        private BediaIcon IconByMenuType(BediaItem.MenuTypes MenuType)
        {
            BediaIcon biReturn = new BediaIcon();
            
            biReturn.Icon = IconByName("Info");    //Default, just in case
            biReturn.Fill = this.BediaBaseColor;
            biReturn.Stroke = this.BediaHighlightColor;
            biReturn.StrokeThickness = 4;
            biReturn.Size = (this.MenuSize - (biReturn.StrokeThickness * 2)); //Always set size after stroke thickness

            if (this.IsMobile == true)
            {
                biReturn.StrokeThickness = 2;
            }

            try
            {
                string sIcon = "Info";

                switch (MenuType)
                {
                    case BediaItem.MenuTypes.Home:
                        sIcon = "Home";
                        biReturn.StrokeThickness = 0;
                        biReturn.Size = 0;
                            break;

                    case BediaItem.MenuTypes.ExitAppNo:
                        sIcon = "HomeSmall";
                        break;

                    case BediaItem.MenuTypes.Folder:
                    case BediaItem.MenuTypes.AddRemoveFolders:
                    case BediaItem.MenuTypes.AddRemoveHomeFolder:
                    case BediaItem.MenuTypes.AddRemoveHomeFolders:
                    case BediaItem.MenuTypes.AddRemoveHomeFolderSelect:
                    case BediaItem.MenuTypes.SettingsMonitorFolders:
                    case BediaItem.MenuTypes.SettingsLibrary:
                    case BediaItem.MenuTypes.YouTubeFolder:
                    case BediaItem.MenuTypes.Library:
                    case BediaItem.MenuTypes.Libraries:
                        biReturn.StrokeThickness = 0;
                        sIcon = "Folder";
                        break;

                    case BediaItem.MenuTypes.SettingsPodcasts:
                    case BediaItem.MenuTypes.Services:
                        sIcon = "Services";
                        break;

                    case BediaItem.MenuTypes.SettingsAlbumFolderArt:
                    case BediaItem.MenuTypes.SettingsHome:
                    case BediaItem.MenuTypes.SettingsBubbles:
                    case BediaItem.MenuTypes.SettingsAdvanced:
                    case BediaItem.MenuTypes.SettingsLateNight:
                    case BediaItem.MenuTypes.SettingsMultipleMonitors:
                    case BediaItem.MenuTypes.SettingsMultipleMonitorsItem:
                    case BediaItem.MenuTypes.SettingsPartyBubbles:
                    case BediaItem.MenuTypes.SettingsScreenSaverComments:
                    case BediaItem.MenuTypes.SettingsStartWithWindows:
                    case BediaItem.MenuTypes.StretchVideo:
                    case BediaItem.MenuTypes.SettingsVisualizations:
                    case BediaItem.MenuTypes.SettingsVisualizationItem:
                    case BediaItem.MenuTypes.SettingsVisualizationsMultipleMonitors:
                    case BediaItem.MenuTypes.SettingsVisualizationsMultipleMonitor:
                    case BediaItem.MenuTypes.SettingsNavigationIcons:
                    case BediaItem.MenuTypes.SettingsWindowsVolume:
                        biReturn.Size = (this.MenuSize / 1.23); //1.25
                        biReturn.StrokeThickness = 5;
                        sIcon = "Settings";
                        break;

                    case BediaItem.MenuTypes.TheatreTimesHome:
                    case BediaItem.MenuTypes.TheatreTimesMovie:
                    case BediaItem.MenuTypes.TheatreTimesItem:
                    case BediaItem.MenuTypes.TheatreTimesTheatre:
                        sIcon = "MovieTimes";
                        break;

                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.SettingsMediaDisplayInfo:
                        biReturn.StrokeThickness = 0;
                        sIcon = "Audio";
                        break;
                        
                    case BediaItem.MenuTypes.SpaceItem:
                    case BediaItem.MenuTypes.Video:
                    case BediaItem.MenuTypes.IPTVVideo:
                    case BediaItem.MenuTypes.YouTubeVideo:
                        biReturn.Size = (this.MenuSize / 1.6);
                        biReturn.StrokeThickness = 0;
                        sIcon = "Video";
                        break;

                    case BediaItem.MenuTypes.EmptyQueue:
                    case BediaItem.MenuTypes.SettingsHistoryClear:
                        sIcon = "Recycle";
                        break;

                    case BediaItem.MenuTypes.CloseMenu:
                    case BediaItem.MenuTypes.SettingsHistoryClearCancel:
                        sIcon = "Close";
                        break;

                    case BediaItem.MenuTypes.ExitApp:
                        sIcon = "Power";
                        break;

                    case BediaItem.MenuTypes.About:
                    case BediaItem.MenuTypes.Info:
                        sIcon = "Info";
                        break;

                    case BediaItem.MenuTypes.SettingsNavigationSet:
                    case BediaItem.MenuTypes.SettingsNavigationSets:
                        sIcon = "Navigator";
                        break;

                    case BediaItem.MenuTypes.Play:
                        sIcon = "Play";
                        break;

                    case BediaItem.MenuTypes.IPTVHome:
                    case BediaItem.MenuTypes.SettingsIPTVFolder:
                        sIcon = "IPTV";
                        break;

                    case BediaItem.MenuTypes.Playlist:
                    case BediaItem.MenuTypes.PlaylistCurrent:
                    case BediaItem.MenuTypes.IPTVPlaylist:
                    case BediaItem.MenuTypes.YouTubePlaylist:
                        sIcon = "Playlist";
                        break;

                    case BediaItem.MenuTypes.Podcast:
                    case BediaItem.MenuTypes.Podcasts:
                    case BediaItem.MenuTypes.PodcastItem:
                    case BediaItem.MenuTypes.SettingsPodcast:
                        sIcon = "Podcast";
                        break;

                    case BediaItem.MenuTypes.SpaceHome:
                        sIcon = "Space";
                        break;

                    case BediaItem.MenuTypes.SpaceISSLocationHome:
                    case BediaItem.MenuTypes.SpaceISSLocationItem:
                        sIcon = "ISS";
                        break;

                    case BediaItem.MenuTypes.SettingsScreenSaverClock:
                    case BediaItem.MenuTypes.SettingsTitlebarClock:
                        sIcon = "Clock";
                        break;

                    case BediaItem.MenuTypes.SelectAll:
                        sIcon = "Selected";
                        break;

                    case BediaItem.MenuTypes.SettingsMenuColor:
                    case BediaItem.MenuTypes.SettingsMenuColors:
                    case BediaItem.MenuTypes.SettingsMenuFont:
                    case BediaItem.MenuTypes.SettingsMenuFonts:
                    case BediaItem.MenuTypes.SettingsMenuSizeTile:
                    case BediaItem.MenuTypes.SettingsOptionalMenu:
                    case BediaItem.MenuTypes.SettingsOptionalMenus:
                    case BediaItem.MenuTypes.SettingsMenuSpeed:
                    case BediaItem.MenuTypes.SettingsMenuSpeeds:
                    case BediaItem.MenuTypes.SettingsMenuOptionItem:
                        sIcon = "Menu";
                        break;

                    case BediaItem.MenuTypes.TransitHome:
                    case BediaItem.MenuTypes.TransitItem:
                    case BediaItem.MenuTypes.SettingsTransitMenus:
                    case BediaItem.MenuTypes.SettingsTransitMenu:
                        sIcon = "Transit";
                        break;

                    case BediaItem.MenuTypes.SettingsHistoryClearOK:
                    case BediaItem.MenuTypes.ExitAppYes:
                        sIcon = "Question";
                        break;

                    case BediaItem.MenuTypes.SettingsSpeechRecognition:
                        sIcon = "Speech";
                        break;

                    case BediaItem.MenuTypes.WeatherHome:
                    case BediaItem.MenuTypes.WeatherItem:
                        sIcon = "Weather";
                        break;

                    case BediaItem.MenuTypes.PastHome:
                    case BediaItem.MenuTypes.PastFolder:
                    case BediaItem.MenuTypes.PastAlphabetical:
                    case BediaItem.MenuTypes.SettingsHistorySave:
                    case BediaItem.MenuTypes.SettingsHistorySync:
                    case BediaItem.MenuTypes.PastList:
                        sIcon = "History";
                        break;

                    case BediaItem.MenuTypes.PausedAlphabetical:
                    case BediaItem.MenuTypes.PausedHome:
                    case BediaItem.MenuTypes.PausedFolder:
                        sIcon = "Pause";
                        break;

                    case BediaItem.MenuTypes.YouTubeHome:
                        sIcon = "YouTube";
                        break;
                }

                biReturn.Icon = IconByName(sIcon);
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return biReturn;
        }
        
        private BediaIcon IconByNameSmall(string Name)
        {
            //Selected, Now Playing, Now Paused, Played (History), Paused (History)
            BediaIcon biReturn = new BediaIcon();
            biReturn.Icon = IconByName(Name);    
            biReturn.Fill = this.BediaBaseColor;
            biReturn.Stroke = this.BediaHighlightColor;
            biReturn.StrokeThickness = 2;
            biReturn.Size = (this.MenuSize / 2.2); //Always set size after stroke thickness
            
            return biReturn;
        }

        private Windows.UI.Xaml.Controls.Image IconByName(string Name)
        {
            Windows.UI.Xaml.Controls.Image imgReturn = new Windows.UI.Xaml.Controls.Image();

            imgReturn.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + Name + ".png"));
            
            return imgReturn;
        }

        private bool IsURL(string value)
        {
            bool bReturn = false;

            try
            {
                if (value.ToLower().Substring(0, 4) == "http")
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private bool Compare(BediaItem One, BediaItem Two)
        {
            bool bReturn = false;

            try
            {
                if (One.MenuType == Two.MenuType)
                {
                    if (One.Title == Two.Title)
                    {
                        //If both are not null, then check if they are
                        if ((One.BediaFile == null && Two.BediaFile == null) || (One.BediaFile.Path == Two.BediaFile.Path))
                        {
                            if (One.BediaFolder.Path == Two.BediaFolder.Path)
                            {
                                if (One.HistoryInfo == Two.HistoryInfo)
                                {
                                    if (One.Value == Two.Value)
                                    {
                                        if (One.ValueInt == One.ValueInt)
                                        {
                                            if (One.ValueObject == One.ValueObject)
                                            {
                                                bReturn = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private bool IsMediaPlaying(StorageFile File)
        {
            bool bReturn = false;

            try
            {
                if ((this.IsPlaying || this.IsPaused) && BediaPlaylist.Count > 0)
                {
                    BediaItem biCurrent = BediaPlaylist[miMediaCurrentPosition];

                    if (biCurrent.BediaFile != null)
                    {
                        if (biCurrent.BediaFile.Path == File.Path)
                        {
                            bReturn = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        public bool IsMobile
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
            }
        }

        private bool IsAudio(BediaItem bediaItem)
        {
            bool bReturn = false;

            try
            {
                switch (bediaItem.MenuType)
                {
                    case BediaItem.MenuTypes.Audio:
                        bReturn = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private async Task<bool> IsFolderValid(StorageFolder Folder)
        {
            //Check if there are any folders or valid files in this folder, if not then don't show it
            //This is one way to help avoid the "Nothing Found Here" situation, 
            //after all why show a folder if there are no folders or files in that file.
            bool bReturn = false;
            bool bFolders = false;
            IReadOnlyList<StorageFolder> Folders = await Folder.GetFoldersAsync();

            if (Folders.Count > 0)
            {
                bFolders = true;
            }

            //No need to check for files, if there are folders
            if (bReturn == false)
            {
                IReadOnlyList<StorageFile> Files = await Folder.GetFilesAsync();

                foreach (StorageFile file in Files)
                {
                    if (msMediaTypesAudio.IndexOf(file.FileType.ToLower()) > -1)
                    {
                        bReturn = true;
                    }

                    if (msMediaTypesVideo.IndexOf(file.FileType.ToLower()) > -1)
                    {
                        bReturn = true;
                    }

                    if (msMediaTypesPlaylist.IndexOf(file.FileType.ToLower()) > -1)
                    {
                        bReturn = true;
                    }

                    if (bReturn == true)
                    {
                        break;
                    }
                }

                Files = null;
            }

            //This recursion, on a very deep folder took < 500 milliseconds
            //It recurses all subfolders to see if there is an acceptable file somewhere within this folder.
            if (bReturn == false && bFolders == true)
            {
                foreach (StorageFolder SubFolder in Folders)
                {
                    if (await IsFolderValid(SubFolder) == true)
                    {
                        bReturn = true;
                        break;
                    }
                }
            }

            Folders = null;
            
            return bReturn;
        }

        private bool LateNight()
        {
            bool bReturn = false;

            try
            {
                if (SettingGetBool("LateNight") == true && (DateTime.Now.Hour > 20 || DateTime.Now.Hour < 7))
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }
        
        private void SetNavigationSet(string NavItems)
        {
            try
            {
                NavigationItem NavItem = new NavigationItem();

                NavigationItems.Clear();

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.Previous,
                    NavigationItem.NavigationType.Next);
                NavigationItems.Add(NavItem);

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.JumpBackMinute5,
                    NavigationItem.NavigationType.JumpForwardMinute10);
                NavigationItems.Add(NavItem);

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.JumpBackMinute1,
                    NavigationItem.NavigationType.JumpForwardMinute5);
                NavigationItems.Add(NavItem);

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.JumpBackSeconds30,
                    NavigationItem.NavigationType.JumpForwardMinute1);
                NavigationItems.Add(NavItem);

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.VolumeDown,
                    NavigationItem.NavigationType.VolumeUp);
                NavigationItems.Add(NavItem);

                NavItem = new NavigationItem(
                    NavigationItem.NavigationType.Menu,
                    NavigationItem.NavigationType.PlayPause);
                NavigationItems.Add(NavItem);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public async Task<bool> NewMediaItemsGet()
        {
            bool bReturn = false;

            try
            {
                NewMediaItems.Clear();

                string FileName = "New Media.txt";
                BediaIO bediaIO = new BediaIO();
                StorageFolder appFolder = ApplicationData.Current.LocalFolder;
                string sLine;
                string[] Values;

                if (await bediaIO.FileExists(appFolder, FileName) == true)
                {
                    StorageFile NewMediaFile = await appFolder.GetFileAsync(FileName);
                    var buffer = await FileIO.ReadBufferAsync(NewMediaFile);

                    using (var dataReader = DataReader.FromBuffer(buffer))
                    {
                        sLine = dataReader.ReadString(buffer.Length);
                        Values = sLine.Split('|');

                        PastItem hi = new PastItem();
                        hi.Path = Values[0];
                        hi.PlayedDate = Convert.ToDateTime(Values[1]);

                        NewMediaItems.Add(hi);

                        bReturn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private async void NewMediaItemClear()
        {
            try
            {
                bool bFound = false;
                string sMediaPath = "";
                BediaItem bi = BediaPlaylist[miMediaCurrentPosition];
                BediaIO bediaIO = new BediaIO();


                switch (bi.MenuType)
                {
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                        bFound = true;
                        sMediaPath = bi.BediaFile.Path;
                        break;
                }

                if (bFound == false) { return; }
                bFound = false; //Reset


                foreach (PastItem hi in NewMediaItems)
                {
                    if (hi.Path.ToLower() == sMediaPath.ToLower())
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == true)
                {
                    string sNewMediaPath = "New Media.txt";
                    StorageFolder BediaFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                    if (await bediaIO.FileExists(BediaFolder, sNewMediaPath) == true)
                    {
                        StorageFile NewMedia = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(sNewMediaPath);
                        bediaIO.FileDelete(NewMedia);
                    }


                    StorageFile NewMediaFile = await BediaFolder.CreateFileAsync(sNewMediaPath, CreationCollisionOption.ReplaceExisting);

                    foreach (PastItem pi in NewMediaItems)
                    {
                        if (pi.Path.ToLower() != sMediaPath.ToLower())
                        {
                            await FileIO.AppendTextAsync(NewMediaFile, pi.Path + "|" + pi.PlayedDate.ToString() + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void LocationGet()
        {
            try
            {
                //https://msdn.microsoft.com/en-us/library/windows/apps/mt219698.aspx?f=255&MSPPError=-2147217396

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://checkip.dyndns.org");
                request.BeginGetResponse(new AsyncCallback(LocationReturn), request);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void LocationReturn(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                StreamReader stream = new StreamReader(response.GetResponseStream());

                string sIPAddress = stream.ReadToEnd();


                sIPAddress = sIPAddress.
                    Replace("<html><head><title>Current IP Check</title></head><body>Current IP Address: ", string.Empty).
                    Replace("</body></html>", string.Empty);

                HttpClient client = new HttpClient();
                HttpResponseMessage responseMsg = await client.GetAsync("http://ipinfo.io/" + sIPAddress); 
                responseMsg.EnsureSuccessStatusCode();
                string sBody = await responseMsg.Content.ReadAsStringAsync();
                
                bediaLocation = JsonConvert.DeserializeObject<IPInfo>(sBody);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private void BediaColorsSetup()
        {
            try
            {
                ProgressRing(true, "");

                MenusClear();
                BediaMenuItems.Clear();
                miMenuCurrentPosition = 0;

                Double dOffset = 0.0;
                LinearGradientBrush colorGradient = new LinearGradientBrush();
                colorGradient.StartPoint = new Point(0, 0);
                colorGradient.EndPoint = new Point(1, 0);


                List<Color> bediaColors = new List<Color>();
                bediaColors.Add(Colors.DarkBlue);
                bediaColors.Add(Colors.Indigo);
                bediaColors.Add(Colors.DarkViolet);
                bediaColors.Add(Colors.DarkRed);
                bediaColors.Add(Colors.DarkGreen);
                bediaColors.Add(Colors.DarkGray);
                bediaColors.Add(Colors.Black);

                
                BediaItem biMenuItem = new BediaItem("Select a Color", BediaItem.MenuTypes.SettingsMenuCustomColorSelected);
                BediaMenuItems.Add(biMenuItem);
                
                for(byte bColor = 0; bColor <bediaColors.Count; bColor++)
                {
                    Color gradientColor = (Color)bediaColors[bColor];

                    GradientStop gradientStop = new GradientStop();
                    GradientStop gradientStopUI = new GradientStop();
                    

                    gradientStop.Color = gradientColor;
                    gradientStop.Offset = dOffset;

                    gradientStopUI.Color = gradientColor;
                    gradientStopUI.Offset = dOffset;


                    BediaGradientStopCollection.Add(gradientStop);
                    colorGradient.GradientStops.Add(gradientStopUI);

                    dOffset = ((double)(bColor + 1) / (double)(bediaColors.Count - 1));
                }


                mMainPage.BediaColors.CanvasColors.Background = colorGradient;
                mMainPage.BediaColors.Width = mMainPage.ActualWidth;
                mMainPage.BediaColors.Height = (mMainPage.ActualHeight - this.MenuSize);


                BediaTransitions bediaTransitions = new BediaTransitions();
                bediaTransitions.UIOpacity(new BediaStoryboardOptions(mMainPage.BediaColors, 1.0));
                bediaTransitions.TextChange(bediaTitlebar.TitlebarTitle, "Custom Menu Color");
                bediaTransitions = null;

                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);

                bediaColors.Clear();

                ProgressRing(false, "Menu");
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void BediaColorSelected()
        {
            try
            {
                byte bSize = this.MenuSize;
                double dPosition = ((CurrentPoint.X - (bSize / 2)) / mMainPage.ActualWidth);
                Color selectedColor = BediaGradientStopCollection.GetRelativeColor(dPosition);  //https://stackoverflow.com/questions/9650049/get-color-in-specific-location-on-gradient

                Ellipse SelectedSpot = new Ellipse();
                SelectedSpot.Stroke = this.BediaHighlightColor;
                SelectedSpot.StrokeThickness = 4;
                SelectedSpot.Fill = new SolidColorBrush(selectedColor); 
                SelectedSpot.Height = bSize;
                SelectedSpot.Width = bSize;

                mMainPage.BediaColors.CanvasColors.Children.Clear();
                mMainPage.BediaColors.CanvasColors.Children.Add(SelectedSpot);
                
                Canvas.SetLeft(SelectedSpot, (CurrentPoint.X - (bSize / 2)));
                Canvas.SetTop(SelectedSpot, CurrentPoint.Y - this.MenuSize - (bSize / 2));

                
                SettingSave("CustomMenuColor", new SolidColorBrush(selectedColor));
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ISSMapSetup()
        {
            try
            {
                ProgressRing(true, "");

                MenusClear();

                BediaMenuItems.Clear();

                miMenuCurrentPosition = 0;
                

                BediaTransitions bediaTrans = new BediaTransitions();
                bediaTrans.TextChange(bediaTitlebar.TitlebarTitle, "ISS Location");
                bediaTrans.UIOpacity(new BediaStoryboardOptions(mMainPage.BediaMap, 1.0));
                bediaTrans = null;

                if (bediaLocation != null)
                {
                    string[] sLocation = bediaLocation.loc.ToString().Split(',');

                    DependencyObject userMarker = UserMarker();
                    BasicGeoposition userGeoposition = new BasicGeoposition();
                    userGeoposition.Latitude = Convert.ToDouble(sLocation[0]);
                    userGeoposition.Longitude = Convert.ToDouble(sLocation[1]);
                    Geopoint userGeopoint = new Geopoint(userGeoposition);

                    mMainPage.BediaMap.Children.Add(userMarker);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(userMarker, userGeopoint);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(userMarker, new Point(0.5, 0.5));
                }
                
                ProgressRing(false, "ISS");
                ISSLocationGet(new object());
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private void ISSLocationGet(object state)
        {
            try
            {
                mbProcessCompleted = false;

                mbMenuLoadCompleted = false;
                
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://api.open-notify.org/iss-now.json");
                webRequest.BeginGetResponse(new AsyncCallback(ISSLocationReturn), webRequest);
                webRequest = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void ISSLocationReturn(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                StreamReader stream = new StreamReader(response.GetResponseStream());

                string sISSLocation = stream.ReadToEnd();
                ISSLocation oISSLocation = JsonConvert.DeserializeObject<ISSLocation>(sISSLocation);

                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DependencyObject issMarker = ISSMarker();
                    BasicGeoposition issGeoposition = new BasicGeoposition();
                    issGeoposition.Latitude = oISSLocation.iss_position.latitude;
                    issGeoposition.Longitude = oISSLocation.iss_position.longitude;
                    Geopoint issGeopoint = new Geopoint(issGeoposition);

                    mMainPage.BediaMap.Center = issGeopoint;

                    if (mMainPage.BediaMap.Children.Count > 1)
                    {
                        mMainPage.BediaMap.Children.RemoveAt(1);
                    }
                    
                    mMainPage.BediaMap.Children.Add(issMarker);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(issMarker, issGeopoint);
                    Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(issMarker, new Point(0.5, 0.5));
                });

                mbProcessCompleted = true;

                mbMenuLoadCompleted = true;
                
                ISSLocationReset();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ISSLocationReset()
        {
            try
            {
                if (ISSMapTimer != null)
                {
                    ISSMapTimer.Dispose();
                    ISSMapTimer = null;
                }

                ISSMapTimer = new Timer(ISSLocationGet, null, Convert.ToInt32(TimeSpan.FromSeconds(8).TotalMilliseconds), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ISSLocationStop()
        {
            try
            {
                if (ISSMapTimer != null)
                {
                    ISSMapTimer.Dispose();
                    ISSMapTimer = null;
                }
                
                BediaTransitions bediaTransitions = new BediaTransitions();
                bediaTransitions.UIOpacity(new BediaStoryboardOptions(mMainPage.BediaMap, 0.0));
                bediaTransitions = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private UIElement ISSMarker()
        {
            Canvas issCanvas = new Canvas();
            Ellipse outer = new Ellipse() { Width = this.MenuSize, Height = this.MenuSize };
            outer.Fill = this.BediaBaseColor;
            outer.Stroke = this.BediaHighlightColor;
            outer.StrokeThickness = 4;

            Windows.UI.Xaml.Controls.Image issImage = new Windows.UI.Xaml.Controls.Image();
            issImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/ISS.png", UriKind.RelativeOrAbsolute));
            issImage.Width = this.MenuSize;
            issImage.Height = this.MenuSize;

            issCanvas.Children.Add(outer);
            issCanvas.Children.Add(issImage);

            
            return issCanvas;
        }

        private UIElement UserMarker()
        {
            Canvas userCanvas = new Canvas();
            Ellipse outer = new Ellipse() { Width = (this.MenuSize / 2), Height = (this.MenuSize / 2)};
            outer.Fill = this.BediaBaseColor;
            outer.Stroke = this.BediaHighlightColor;
            outer.StrokeThickness = 2;

            Windows.UI.Xaml.Controls.Image issImage = new Windows.UI.Xaml.Controls.Image();
            issImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/User.png", UriKind.RelativeOrAbsolute));
            issImage.Width = (this.MenuSize / 2);
            issImage.Height = (this.MenuSize / 2);

            userCanvas.Children.Add(outer);
            userCanvas.Children.Add(issImage);


            return userCanvas;
        }

        private Windows.UI.Color ColorFromString(string Values)
        {
            Windows.UI.Color ColorReturn = Windows.UI.Color.FromArgb(255, 0, 102, 204);

            try
            {
                string[] sColors = Values.Split(',');

                ColorReturn.A = Convert.ToByte(sColors[0]);
                ColorReturn.R = Convert.ToByte(sColors[1]);
                ColorReturn.G = Convert.ToByte(sColors[2]);
                ColorReturn.B = Convert.ToByte(sColors[3]);
            }
            catch { }

            return ColorReturn;

        }

        private async void SettingsGet()
        {
            try
            {
                BediaIO bediaIO = new BediaIO();
                string sValue = "";
                bool bFound = false;

     
                sValue = SettingGet("BediaBaseColor");
                if (sValue.Trim().Length > 0)
                {
                    try
                    {
                        //Just in case the sValue is not a color
                        this.BediaBaseColor = new SolidColorBrush(ColorFromString(sValue));
                    }
                    catch { }
                }

                this.HomePageTitle = SettingGet("HomePageTitle"); 
                this.Bubbles = SettingGetBool("Bubbles");
                this.BubblesParty = SettingGetBool("BubblesParty");
                this.SavePast = SettingGetBool("PastSave");
                this.StretchVideo = SettingGetBool("StretchVideo");
                this.Volume = SettingGetDouble("Volume");
                this.AnimationSpeed = Convert.ToInt16(SettingGetInt("MenuSpeed"));
                this.MenuSize = Convert.ToByte(SettingGetInt("MenuSize"));

                if (IsMobile == true)
                {
                    this.MenuSize = Convert.ToByte(this.MenuSize / 2);
                }

                msAppName = AppName();  //This is because of the need for this value in some functions that are threaded

                msHistoryFile = msAppName + " History.txt";

                bFound = await bediaIO.FolderExists(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);

                if (bFound == false)
                {
                    bediaIO.FolderCreate(ApplicationData.Current.LocalFolder, msIPTVPath);
                }

                SetNavigationSet("");

                PastListRefresh();

                ScreenSaverMessagesLoad();

                bediaIO = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private string SettingGet(string setting)
        {
            string sReturn = "";

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                sReturn = localSettings.Values[setting].ToString();
            }
            catch { }

            return sReturn;
        }

        private SolidColorBrush SettingGetBrush(string setting)
        {
            SolidColorBrush brhReturn = new SolidColorBrush(Colors.White);

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                string sColor = localSettings.Values[setting].ToString();

                brhReturn = new SolidColorBrush(ColorFromString(sColor));
            }
            catch { }

            return brhReturn;
        }

        private void SettingsDefaults()
        {
            try
            {
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AlbumFolderArt"))
                {
                    ApplicationData.Current.LocalSettings.Values["AlbumFolderArt"] = false;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("Bubbles"))
                {
                    ApplicationData.Current.LocalSettings.Values["Bubbles"] = true;
                }
                
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("BubblesParty"))
                {
                    ApplicationData.Current.LocalSettings.Values["BubblesParty"] = false;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("PastSave"))
                {
                    ApplicationData.Current.LocalSettings.Values["PastSave"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("NavigationIcons"))
                {
                    ApplicationData.Current.LocalSettings.Values["NavigationIcons"] = true;
                }
                
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("HomePageTitle"))
                {
                    ApplicationData.Current.LocalSettings.Values["HomePageTitle"] = "Home";
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("HomeMenus"))
                {
                    ApplicationData.Current.LocalSettings.Values["HomeMenus"] = "";
                }
                
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("HomePageLibraries"))
                {
                    ApplicationData.Current.LocalSettings.Values["HomePageLibraries"] = "MusicLibrary,VideoLibrary,";
                }
                
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("Startup"))
                {
                    ApplicationData.Current.LocalSettings.Values["Startup"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("StretchVideo"))
                {
                    ApplicationData.Current.LocalSettings.Values["StretchVideo"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("Volume"))
                {
                    ApplicationData.Current.LocalSettings.Values["Volume"] = 0.5;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("AnimationSpeed"))
                {
                    ApplicationData.Current.LocalSettings.Values["AnimationSpeed"] = 250;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("MenuSize"))
                {
                    ApplicationData.Current.LocalSettings.Values["MenuSize"] = 90;
                }
                
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("MultiMonitor"))
                {
                    ApplicationData.Current.LocalSettings.Values["MultiMonitor"] = "";
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("ScreenSaverClock"))
                {
                    ApplicationData.Current.LocalSettings.Values["ScreenSaverClock"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("ScreenSaverComments"))
                {
                    ApplicationData.Current.LocalSettings.Values["ScreenSaverComments"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("TitlebarClock"))
                {
                    ApplicationData.Current.LocalSettings.Values["TitlebarClock"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("WindowsVolume"))
                {
                    ApplicationData.Current.LocalSettings.Values["WindowsVolume"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("MediaDisplayInfo"))
                {
                    ApplicationData.Current.LocalSettings.Values["MediaDisplayInfo"] = true;
                }

                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("CustomMenuColor"))
                {
                    SettingSave("CustomMenuColor", BediaBaseColor);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private bool SettingGetBool(string setting)
        {
            bool bReturn = false;

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                bReturn = Convert.ToBoolean(localSettings.Values[setting]);
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private double SettingGetDouble(string setting)
        {
            double dReturn = 0.0;

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                dReturn = Convert.ToDouble(localSettings.Values[setting]);
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return dReturn;
        }

        private int SettingGetInt(string setting)
        {
            int iReturn = 0;

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                iReturn = Convert.ToInt32(localSettings.Values[setting]);
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return iReturn;
        }

        private void SettingSave(string setting, string value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                localSettings.Values[setting] = value;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SettingSave(string setting, SolidColorBrush value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                localSettings.Values[setting] =
                    value.Color.A.ToString() + "," +
                    value.Color.R.ToString() + "," +
                    value.Color.G.ToString() + ", " +
                    value.Color.B.ToString();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SettingSave(string setting, bool value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                localSettings.Values[setting] = value;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SettingSave(string setting, int value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                localSettings.Values[setting] = value;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SettingSave(string setting, double value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                localSettings.Values[setting] = value;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        public void SetCustomColor(PointerRoutedEventArgs e)
        {
            try
            {
                Ellipse SelectedSpot = new Ellipse();


                SelectedSpot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); 
                SelectedSpot.Stroke = BediaHighlightColor; 
                SelectedSpot.StrokeThickness = 4;
                SelectedSpot.Width = 40;
                SelectedSpot.Height = 40;

                mMainPage.BediaColors.CanvasColors.Children.Clear();
                mMainPage.BediaColors.CanvasColors.Children.Add(SelectedSpot);

                Canvas.SetLeft(SelectedSpot, e.GetCurrentPoint(mMainPage.BediaColors).Position.X - (SelectedSpot.Width / 2));
                Canvas.SetTop(SelectedSpot, e.GetCurrentPoint(mMainPage.BediaColors).Position.Y - (SelectedSpot.Height / 2));

            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SetBackground(Grid grid)
        {
            try
            {
                //Paint Background.  There may be a need to setup this differently for
                //different platform Phone, tablet, tv.  Maybe different for different sizes
                bool bLateNight = LateNight();

                LinearGradientBrush HorizontalGradient = new LinearGradientBrush();
                HorizontalGradient.StartPoint = new Windows.Foundation.Point(0, 0.5);
                HorizontalGradient.EndPoint = new Windows.Foundation.Point(1, 0.88);

                GradientStop GradientBlack = new GradientStop();
                GradientBlack.Color = Windows.UI.Color.FromArgb(255, 0, 0, 0);

                if (bLateNight == false)
                {
                    GradientStop GradientPrimary = new GradientStop();
                    GradientPrimary.Color = Windows.UI.Color.FromArgb(
                        BediaBaseColor.Color.A,
                        BediaBaseColor.Color.R,
                        BediaBaseColor.Color.G,
                        BediaBaseColor.Color.B);
                    GradientPrimary.Offset = 0.98;
                    HorizontalGradient.GradientStops.Add(GradientPrimary);

                    GradientBlack.Offset = 0.35;

                    bediaTitlebar.SetOpacity(1.0);
                }
                else
                {
                    GradientBlack.Offset = 1.0;

                    bediaTitlebar.SetOpacity(0.6);
                }

                HorizontalGradient.GradientStops.Add(GradientBlack);

                grid.Background = HorizontalGradient;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void BubblesLoad()
        {
            try
            {
                if (this.Bubbles == false) { return; }

                if (this.IsMobile == true) { return; } 

                double dBubbleOpacity;
                double dSize;
                Int16 iSeconds;
                Int16 iTotalDistance;
                Int64 iFirstTime;
                double dStart;
                double dEnd;
                double dTop;
                SolidColorBrush StrokeBrush;
                SolidColorBrush BubbleBase = BediaBaseColor;
                byte bBubbles = Convert.ToByte(BediaRandom.Next(88, 116));
                bool bLateNight = LateNight();


                for (byte b = 0; b < bBubbles; b++)
                {
                    dBubbleOpacity = BediaRandom.NextDouble();
                    StrokeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(Convert.ToByte(BediaRandom.Next(50, 255)), BediaHighlightColor.Color.R, BediaHighlightColor.Color.G, BediaHighlightColor.Color.B));
                    dSize = Convert.ToDouble(BediaRandom.Next(48, 188));
                    iSeconds = Convert.ToInt16(BediaRandom.Next(28, 58));
                    dStart = Convert.ToDouble(BediaRandom.Next(Convert.ToInt16(mMainPage.ActualWidth / 4), Convert.ToInt16((mMainPage.ActualWidth + dSize))));
                    dEnd = ((mMainPage.ActualWidth / 5) - dStart) + BediaRandom.Next(-88, 88);
                    dTop = Convert.ToInt16(BediaRandom.Next((this.MenuSize - 4), Convert.ToInt16(mMainPage.ActualHeight - (dSize + 100))));
                    iTotalDistance = Convert.ToInt16((dStart + dEnd) + (mMainPage.ActualWidth - dStart));

                    //TIME
                    iFirstTime = Convert.ToInt64(((dStart + dEnd) / iTotalDistance) * (iSeconds * 1000));

                    if (this.BubblesParty == true)
                    {
                        iSeconds = Convert.ToInt16(BediaRandom.Next(18, 38));
                        iFirstTime = Convert.ToInt64(((dStart + dEnd) / iTotalDistance) * (iSeconds * 1000));
                        BubbleBase = new SolidColorBrush(Windows.UI.Color.FromArgb(255,
                            Convert.ToByte(BediaRandom.Next(0, 255)),
                            Convert.ToByte(BediaRandom.Next(0, 255)),
                            Convert.ToByte(BediaRandom.Next(0, 255))));
                    }


                    // -- BUBBLE --
                    Ellipse Bubble = new Ellipse();
                    Bubble.Name = "Bubble" + Guid.NewGuid().ToString().Replace("-", "");
                    Bubble.Width = dSize;
                    Bubble.Height = dSize;
                    Bubble.StrokeThickness = 5;
                    Bubble.Opacity = dBubbleOpacity;
                    
                    if (bLateNight == false)
                    {
                        Bubble.Fill = BubbleBase;
                    }
                    else
                    {
                        Bubble.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); //Alpha 0 to make transparent
                        if (this.BubblesParty == true)
                        {
                            StrokeBrush = BubbleBase;
                        }
                        else
                        {
                            StrokeBrush = BediaBaseColor;
                        }
                    }
                    Bubble.Stroke = StrokeBrush;


                    mMainPage.BediaBubbles.Children.Add(Bubble);
                    Canvas.SetTop(Bubble, dTop);
                    Canvas.SetLeft(Bubble, dStart);


                    //-------------------------------------
                    // -- MOVE --
                    CompositeTransform compTransform = new CompositeTransform();
                    Bubble.RenderTransform = compTransform;
                    

                    DoubleAnimationUsingKeyFrames MoveAnimation = new DoubleAnimationUsingKeyFrames();
                    MoveAnimation.Duration = TimeSpan.FromSeconds(iSeconds);
                    MoveAnimation.RepeatBehavior = RepeatBehavior.Forever;

                    LinearDoubleKeyFrame ld1 = new LinearDoubleKeyFrame();
                    ld1.Value = dEnd;
                    ld1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(iFirstTime));
                    MoveAnimation.KeyFrames.Add(ld1);


                    LinearDoubleKeyFrame ld2 = new LinearDoubleKeyFrame();
                    ld2.Value = (mMainPage.ActualWidth - dStart);
                    ld2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(iFirstTime + 10)); 
                    MoveAnimation.KeyFrames.Add(ld2);

                    LinearDoubleKeyFrame ld3 = new LinearDoubleKeyFrame();
                    ld3.Value = 0;
                    ld3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(iSeconds));
                    MoveAnimation.KeyFrames.Add(ld3);

                    Storyboard.SetTarget(MoveAnimation, Bubble);
                    Storyboard.SetTargetProperty(MoveAnimation, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");  
                    BubbleStoryboard.Children.Add(MoveAnimation);


                    //-------------------------------------
                    // - OPACITY --
                    DoubleAnimationUsingKeyFrames OpacityAnimation = new DoubleAnimationUsingKeyFrames();
                    OpacityAnimation.Duration = TimeSpan.FromSeconds(iSeconds);
                    OpacityAnimation.RepeatBehavior = RepeatBehavior.Forever;

                    LinearDoubleKeyFrame lDKF1 = new LinearDoubleKeyFrame();
                    lDKF1.Value = 0.0;
                    lDKF1.KeyTime = TimeSpan.FromMilliseconds(iFirstTime);
                    OpacityAnimation.KeyFrames.Add(lDKF1);

                    LinearDoubleKeyFrame lDKF2 = new LinearDoubleKeyFrame();
                    lDKF2.Value = dBubbleOpacity;
                    lDKF2.KeyTime = TimeSpan.FromSeconds(iSeconds);
                    OpacityAnimation.KeyFrames.Add(lDKF2);

                    Storyboard.SetTarget(OpacityAnimation, Bubble);
                    Storyboard.SetTargetProperty(OpacityAnimation, "(UIElement.Opacity)");
                    BubbleStoryboard.Children.Add(OpacityAnimation);
                }
                
                BubbleStoryboard.Begin(); 
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void BubblesStop()
        {
            try
            {
                BubbleStoryboard.Stop();
                BubbleStoryboard.Children.Clear();
                mMainPage.BediaBubbles.Children.Clear();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void ProgressRing(bool Active, string TitlebarIcon)
        {
            try
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Storyboard sbIconFade = new Storyboard();
                    DoubleAnimation aniIconFade = new DoubleAnimation();
                    DoubleAnimation aniBackFade = new DoubleAnimation();
                    double From = 1.0;
                    double To = 0.0;

                    if (Active == false)
                    {
                        From = 0.0;
                        To = 1.0;

                        BediaItem.MenuTypes bmt = BediaItem.MenuTypes.Home; //In case of Home page, or other odd situation

                        if (MenuBreadrumbs.Count > 0)
                        {
                            bmt = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)].MenuType;
                        }

                        BediaIcon bi = IconByMenuType(bmt);

                        bediaTitlebar.TitlebarIcon.Source = bi.Icon.Source;
                        bediaTitlebar.TitlebarIconBackground.Fill = bi.Fill;
                        bediaTitlebar.TitlebarIconBackground.Stroke = bi.Stroke;
                        bediaTitlebar.TitlebarIconBackground.StrokeThickness = bi.StrokeThickness;
                        bediaTitlebar.TitlebarIconBackground.Width = bi.Size;
                        bediaTitlebar.TitlebarIconBackground.Height = bi.Size;

                        bi = null;
                    }


                    aniBackFade.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    aniBackFade.From = From;
                    aniBackFade.To = To;
                    Storyboard.SetTarget(aniBackFade, bediaTitlebar.TitlebarIconBackground);
                    Storyboard.SetTargetProperty(aniBackFade, "(UIElement.Opacity)");
                    sbIconFade.Children.Add(aniBackFade);

                    aniIconFade.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    aniIconFade.From = From;
                    aniIconFade.To = To;
                    Storyboard.SetTarget(aniIconFade, bediaTitlebar.TitlebarIcon);
                    Storyboard.SetTargetProperty(aniIconFade, "(UIElement.Opacity)");
                    sbIconFade.Children.Add(aniIconFade);
                    sbIconFade.Begin();

                    bediaTitlebar.BediaProgress.IsActive = Active;
                });
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void logValue(string value)
        {
            try
            {
                //string sPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + msAppName + "\\Exceptions.txt";

                //using (StreamWriter sw = File.AppendText(sPath))
                //{
                //    sw.WriteLine("------------------");
                //    sw.WriteLine(DateTime.Now.ToString());
                //    sw.WriteLine("Value: " + value);
                //    sw.WriteLine("------------------");
                //}
                System.Diagnostics.Debug.WriteLine("Output: " + value);
            }
            catch { }
        }

        public async void logException(Exception ex)
        {
            try
            {
                BediaIO bediaIO = new BediaIO();
                string sFileName = "Exceptions.txt";
                StringBuilder sbException = new StringBuilder();
                StorageFolder BediaFolder = ApplicationData.Current.LocalFolder;
                bool bFound = await bediaIO.FileExists(BediaFolder, sFileName);
                StorageFile ExceptionFile = null;
                StackTrace st = new StackTrace(ex, true);
                var StackFrames = st.GetFrames();


                if (bFound  == false)
                {
                    ExceptionFile = await BediaFolder.CreateFileAsync(sFileName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    ExceptionFile  = await BediaFolder.GetFileAsync(sFileName);
                }

                sbException.Append("----------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                
                foreach (StackFrame stackFrame in StackFrames)
                {
                    var method = stackFrame.GetMethod();

                    //Line zero reported for .NET internals.  Nothing I can do about these
                    if (stackFrame.GetFileLineNumber() > 0)
                    {
                        sbException.Append(DateTime.Now.ToString() + Environment.NewLine);
                        sbException.Append("Line: " + stackFrame.GetFileLineNumber().ToString() + Environment.NewLine);
                        sbException.Append("Method: " + method.Name + Environment.NewLine);
                        sbException.Append("Exception: " + ex.ToString() + Environment.NewLine);
                        sbException.Append("Stack Trace: " + ex.StackTrace.ToString() + Environment.NewLine);
                        sbException.Append("************************************************************************" + Environment.NewLine);
                    }

                }

                //The "-----" is unnecessary if the stack's line number is zero
                if (sbException.Length > 1)
                {
                    await FileIO.AppendTextAsync(ExceptionFile, sbException.ToString() + Environment.NewLine);
                }


                //Cleanup
                ExceptionFile = null;
                BediaFolder = null;
                bediaIO = null;
                sbException = null;
            }
            catch { }
        }

        private async void MenuProcessing(string LoadType, string Title, string Icon)
        {
            try
            {
                bool bMenus = false;
                BediaTransitions bediaTrans = new BediaTransitions();

                BediaMenuItems.Clear();

                mbProcessCompleted = false;

                mbMenuLoadCompleted = false;

                this.IsMenuDisplayed = true;

                miMenuCurrentPosition = 0;
                
                OneMomentPlease();

                MenusClear();

                bMenus = await MenusCreate(LoadType);

                mbProcessCompleted = true;

                bMenus = await MenuLoad();

                bediaTrans.TextChange(bediaTitlebar.TitlebarTitle, Title);
                
                ProgressRing(false, Icon);

                mbMenuLoadCompleted = true;

                bediaTrans = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<bool> MenuCreate(StorageFolder Folder, StorageFile file)
        {
            try
            {
                bool bFound = false;
                BediaItem biItem = new BediaItem();
                
                if (msMediaTypesAudio.IndexOf(file.FileType.ToLower()) > -1)
                {
                    bFound = true;
                    biItem.MenuType = BediaItem.MenuTypes.Audio;
                }

                if (bFound == false)
                {
                    if (msMediaTypesVideo.IndexOf(file.FileType.ToLower()) > -1)
                    {
                        bFound = true;
                        biItem.MenuType = BediaItem.MenuTypes.Video;
                    }
                }
                
                if (bFound == false)
                {
                    if (msMediaTypesPlaylist.IndexOf(file.FileType.ToLower()) > -1)
                    {
                        bFound = true;
                        biItem.MenuType = BediaItem.MenuTypes.Playlist;
                    }
                }
                

                if (bFound == true)
                {
                    biItem.BediaFile = file;
                    biItem.BediaFolder = Folder;
                    biItem.Title = file.DisplayName; 
                    biItem.Selected = MenuSelected(file);
                    BediaMenuItems.Add(biItem);
                }

                biItem = null;

                await Task.Delay(TimeSpan.FromMilliseconds(1)); //to insure that the function is awaited
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private async Task<bool> MenusCreate(string LoadType)
        {
            bool bReturn = false;

            try
            {
                BediaItem biMenuItem;
                BediaItem CurrentMenu;
                BediaIO bediaIO = new BediaIO();
                List<BediaItem> TempMenuItems = new List<BediaItem>();
                bool bFound = false;
                string sTemp = "";

                switch (LoadType)
                {
                    case "LoadHome":
                        #region Home
                        string sHomeMenus = SettingGet("HomeMenus");
                        string sMenuOptions = SettingGet("HomePageMenuOptions");
                        string sHomePageLibraries = SettingGet("HomePageLibraries");

                        MenuBreadrumbs.Clear();

                        if (sHomeMenus.Trim().Length == 0 && sHomePageLibraries.Trim().Length == 0)
                        {
                            //No menus set, give the "Add Menu" option
                            biMenuItem = new BediaItem("Change Home Folders", BediaItem.MenuTypes.AddRemoveHomeFolders);
                            BediaMenuItems.Add(biMenuItem);
                        }
                        else
                        {
                            string[] HomeFolders = sHomeMenus.Split(',');
                            
                            foreach (string Token in HomeFolders)
                            {
                                if (Token.Length > 0)
                                {
                                    try
                                    {
                                        //If an external drive is disconnected an error is thrown, don't care, move to the next token
                                        StorageFolder Homefolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
                                        bFound = await bediaIO.FolderExists(Homefolder.Path);

                                        if (bFound == true)
                                        {
                                            biMenuItem = new BediaItem(Homefolder.DisplayName, BediaItem.MenuTypes.Folder);
                                            biMenuItem.BediaFolder = Homefolder;
                                            biMenuItem.Selected = false;
                                            biMenuItem.Value = Token;
                                            BediaMenuItems.Add(biMenuItem);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            
                            //Sort by the title. 
                            BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();

                            if (sHomePageLibraries.Contains("MusicLibrary"))
                            {
                                biMenuItem = new BediaItem("Music", BediaItem.MenuTypes.Library);
                                biMenuItem.Value = "Music";
                                BediaMenuItems.Add(biMenuItem);
                            }

                            if (sHomePageLibraries.Contains("VideoLibrary"))
                            {
                                biMenuItem = new BediaItem("Videos", BediaItem.MenuTypes.Library);
                                biMenuItem.Value = "Videos";
                                BediaMenuItems.Add(biMenuItem);
                            }
                        }
                        
                        //Show only if option and/or if there are any History Playlist files
                        if (PastListHistory.Count > 0 && this.SavePast == true)
                        {
                            biMenuItem = new BediaItem("Past", BediaItem.MenuTypes.PastHome);
                            BediaMenuItems.Add(biMenuItem);
                        }
                       
                        if (PausedItemsAvailable() == true)
                        {
                            biMenuItem = new BediaItem("Paused", BediaItem.MenuTypes.PausedHome);
                            BediaMenuItems.Add(biMenuItem);
                        }
            
                        if (sMenuOptions.IndexOf("Podcasts,") > -1)
                        {
                            biMenuItem = new BediaItem("Podcasts", BediaItem.MenuTypes.Podcasts);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        //if (BediaPlaylist.Count > 0 && sMenuOptions.IndexOf("Queue,") > -1)
                        if (BediaPlaylist.Count > 0)
                        {
                            biMenuItem = new BediaItem("Queue", BediaItem.MenuTypes.PlaylistCurrent);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        biMenuItem = new BediaItem("Services", BediaItem.MenuTypes.Services);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Settings", BediaItem.MenuTypes.SettingsHome);
                        BediaMenuItems.Add(biMenuItem);

                        if (sMenuOptions.IndexOf("Space,") > -1)
                        {
                            biMenuItem = new BediaItem("Space", BediaItem.MenuTypes.SpaceHome);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        if (sMenuOptions.IndexOf("Theater,") > -1)
                        {
                            biMenuItem = new BediaItem("Theater", BediaItem.MenuTypes.TheatreTimesHome);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        if (sMenuOptions.IndexOf("Transit,") > -1)
                        {
                            biMenuItem = new BediaItem("Transit", BediaItem.MenuTypes.TransitHome);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        if (sMenuOptions.IndexOf("Weather,") > -1)
                        {
                            biMenuItem = new BediaItem("Weather", BediaItem.MenuTypes.WeatherHome);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        biMenuItem = new BediaItem("Exit", BediaItem.MenuTypes.ExitApp);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "Folder":
                        #region Folder
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        List<BediaItem> BediaFolders = new List<BediaItem>();
                        var FoldersFiles = await CurrentMenu.BediaFolder.GetItemsAsync();

                        foreach (IStorageItem FolderFile in FoldersFiles)
                        {
                            if (FolderFile.IsOfType(StorageItemTypes.Folder))
                            {
                                biMenuItem = new BediaItem(FolderFile.Name, BediaItem.MenuTypes.Folder);
                                biMenuItem.BediaFolder = (StorageFolder)FolderFile;
                                BediaFolders.Add(biMenuItem);
                            }
                            else
                            {
                                bFound = await MenuCreate(CurrentMenu.BediaFolder, (StorageFile)FolderFile);
                            }
                        }

                        //- SORT -
                        BediaFolders = BediaFolders.OrderBy(x => x.SortedBy).ToList();
                        BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();

                        if (BediaMenuItems.Count > 1)
                        {
                            //"Select All" is only displayed if there are files in this folder.  
                            //The recursion "select all' now is no longer the pattern.  
                            //Select all will only select current folder.
                            biMenuItem = new BediaItem("Select All", BediaItem.MenuTypes.SelectAll);
                            TempMenuItems.Add(biMenuItem);
                        }

						// ADD LISTS TOGETHER (obviously)
                        TempMenuItems.AddRange(BediaFolders);
                        TempMenuItems.AddRange(BediaMenuItems);
                        BediaMenuItems = TempMenuItems;
                        TempMenuItems = null;
                        BediaFolders = null;
                        break;
                    #endregion

                    case "LoadPlaylist":
                        #region Load Playlist
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        bFound = await PlaylistLoad(CurrentMenu);
                        break;
                    #endregion

                    case "LoadLibrary":
                        #region Library
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        StorageLibrary MediaLibrary = null;
                        switch (CurrentMenu.Value)
                        {
                            case "Music":
                                MediaLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
                                break;

                            case "Videos":
                                MediaLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);
                                break;
                        }
                        IObservableVector<Windows.Storage.StorageFolder> MediaFolders = MediaLibrary.Folders;

                        //Folders
                        foreach (var MediaFolder in MediaFolders)
                        {
                            if (await IsFolderValid(MediaFolder) == true)
                            {
                                IReadOnlyList<StorageFolder> MusicSubFolders = await MediaFolder.GetFoldersAsync();
                                        
                                foreach(var SubFolder in MusicSubFolders)
                                {
                                    if (await IsFolderValid(SubFolder) == true)
                                    {
                                        biMenuItem = new BediaItem(SubFolder.Name, BediaItem.MenuTypes.Folder);
                                        biMenuItem.BediaFolder = SubFolder;
                                        BediaMenuItems.Add(biMenuItem);
                                    }
                                }

                            }
                        }

                        //Files
                        foreach (var MediaFolder in MediaFolders)
                        {
                            if (await IsFolderValid(MediaFolder) == true)
                            {
                                IReadOnlyList<StorageFile> MusicFiles = await MediaFolder.GetFilesAsync();

                                foreach (var File in MusicFiles)
                                {
                                    bFound = await MenuCreate(MediaFolder, File);
                                }
                            }
                        }
                        break;
                    #endregion

                    case "Queue":
                        #region Queue
                        foreach(BediaItem bi in BediaPlaylist)
                        {
                            BediaMenuItems.Add(bi);
                        }
                        break;
                    #endregion

                    case "HistoryItem":
                        #region History Item
                        if (HackBediaItem.HistoryInfo.Seconds > 0)
                        {
                            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(HackBediaItem.HistoryInfo.Seconds));
                            string sTime = "";

                            if (ts.Hours > 0)
                            {
                                sTime = ts.Hours.ToString() + " hour";

                                if (ts.Hours > 1)
                                {
                                    sTime += "s"; //Just hate it when software is lazy and shows values; "Minute(s)"  PULLEZE!
                                }
                            }
                            if (sTime.Length > 0) { sTime += " "; }

                            if (ts.Minutes > 0)
                            {
                                sTime += ts.Minutes.ToString() + " minute";

                                if (ts.Minutes > 1)
                                {
                                    sTime += "s";
                                }
                            }
                            if (sTime.Length > 0) { sTime += " "; }

                            //Only need to show seconds if there are no hours.  
                            if (ts.Hours == 0 && ts.Seconds > 0)
                            {
                                sTime += ts.Seconds.ToString() + " second";

                                if (ts.Seconds > 1)
                                {
                                    sTime += "s";
                                }
                            }

                            biMenuItem = new BediaItem("Resume at " + sTime, BediaItem.MenuTypes.PastItem);
                            biMenuItem.Value = HackBediaItem.HistoryInfo.Seconds.ToString();
                            BediaMenuItems.Add(biMenuItem);

                            biMenuItem = new BediaItem("Start from the beginning", BediaItem.MenuTypes.PastItem);
                            biMenuItem.Value = "0";
                            BediaMenuItems.Add(biMenuItem);
                        }


                        biMenuItem = new BediaItem("Last played on " + HackBediaItem.HistoryInfo.PlayedDate.ToString("MM/dd/yyyy hh:mm tt"), BediaItem.MenuTypes.PastItem);
                        biMenuItem.Value = "0";
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "Past":
                        #region Past
                        DateTime dt = new DateTime(0);
                        List<PastItem> HistoryDates = PastListHistory.OrderByDescending(hl => hl.PlayedDate).ToList();

                        if (HistoryDates.Count > 0)
                        {
                            biMenuItem = new BediaItem("Alphabetical", BediaItem.MenuTypes.PastAlphabetical);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        foreach (PastItem hi in HistoryDates)
                        {
                            if (hi.PlayedDate.ToString("MM/yyyy") != dt.ToString("MM/yyyy"))
                            {
                                biMenuItem = new BediaItem(hi.PlayedDate.ToString("MMMM yyyy"), BediaItem.MenuTypes.PastFolder);
                                biMenuItem.Value = hi.PlayedDate.ToString("MM/dd/yyyy");
                                BediaMenuItems.Add(biMenuItem);

                                dt = hi.PlayedDate;
                            }
                        }

                        biMenuItem = new BediaItem("Clear Past", BediaItem.MenuTypes.SettingsHistoryClear);
                        BediaMenuItems.Add(biMenuItem);

                        HistoryDates = null;
                        break;
                    #endregion

                    case "PastAlphabetical":
                        #region Past Alphabetical
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        List<PastItem> PastAlpha = PastListHistory.OrderBy(hl => hl.Name).ToList();
                        foreach (PastItem hi in PastAlpha)
                        {
                            if (IsURL(hi.Path) == false)
                            {
                                bFound = await MenuCreate(CurrentMenu.BediaFolder, await StorageFile.GetFileFromPathAsync(hi.Path));
                            }
                        }
                        break;
                    #endregion

                    case "PastFolder":
                        #region Past Folder
                        DateTime dtDateRangeFrom = Convert.ToDateTime(HackBediaItem.Value);
                        dtDateRangeFrom = dtDateRangeFrom.AddDays((-dtDateRangeFrom.Day + 1));//Get the first day of the month
                        DateTime dtDateRangeTo = dtDateRangeFrom.AddMonths(1);
                        List<PastItem> HistoryFolder = PastListHistory.Where(i => i.PlayedDate > dtDateRangeFrom && i.PlayedDate <= dtDateRangeTo).ToList();
                        HistoryFolder = HistoryFolder.OrderByDescending(x => x.PlayedDate).ToList(); //Need to be sorted to insure that duplicate dates are in order
                        DateTime dt2 = new DateTime(0);

                        foreach (PastItem hi in HistoryFolder)
                        {
                            if (hi.PlayedDate.ToString("dd/MM/yyyy") != dt2.ToString("dd/MM/yyyy"))
                            {
                                //Converting "dd" to Int to remove the preceeding "0" is because just "d" returns the full date, even though it says it shouldn't
                                biMenuItem = new BediaItem(
                                    hi.PlayedDate.ToString("dddd") +
                                    " the " +
                                    Convert.ToInt16(hi.PlayedDate.ToString("dd")) +
                                    DaySuffix(Convert.ToInt16(hi.PlayedDate.ToString("dd"))),
                                    BediaItem.MenuTypes.PastList);
                                biMenuItem.Value = hi.PlayedDate.ToString("yyyy.MM.dd");
                                BediaMenuItems.Add(biMenuItem);

                                dt2 = hi.PlayedDate;
                            }
                        }
                        BediaMenuItems = BediaMenuItems.OrderByDescending(x => x.Value).ToList();

                        break;
                    #endregion

                    case "HistoryList":
                        #region "History List"
                        StorageFile filHistory;
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        DateTime dtPast = Convert.ToDateTime(HackBediaItem.Value);
                        List<PastItem> PastItems = PastListHistory.Where(i => i.PlayedDate.ToString("MMddyyyy") == dtPast.ToString("MMddyyyy")).ToList();

                        foreach (PastItem hi in PastItems)
                        {
                            filHistory = await StorageFile.GetFileFromPathAsync(hi.Path);
                            bFound = await MenuCreate(CurrentMenu.BediaFolder, filHistory);
                            filHistory = null;
                        }
                        break;
                    #endregion

                    case "PausedHome":
                        #region Pauses Home
                        DateTime dtPausedDate = new DateTime(0);
                        List<PastItem> PausedDates = PastListHistory.Where(e => e.Seconds > 0).OrderByDescending(hl => hl.PlayedDate).ToList();

                        if (PausedDates.Count > 0)
                        {
                            biMenuItem = new BediaItem("Alphabetical", BediaItem.MenuTypes.PausedAlphabetical);
                            BediaMenuItems.Add(biMenuItem);
                        }

                        foreach (PastItem hi in PausedDates)
                        {
                            if (hi.PlayedDate.ToString("MM/yyyy") != dtPausedDate.ToString("MM/yyyy"))
                            {
                                biMenuItem = new BediaItem(hi.PlayedDate.ToString("MMMM yyyy"), BediaItem.MenuTypes.PausedFolder);
                                biMenuItem.Value = hi.PlayedDate.ToString("MM/dd/yyyy");
                                BediaMenuItems.Add(biMenuItem);

                                dtPausedDate = hi.PlayedDate;
                            }
                        }
                        break;
                    #endregion

                    case "PausedAlphabetical":
                        #region Paused Alphabetical
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        List<PastItem> PausedAlpha = PastListHistory.Where(e => e.Seconds > 0).OrderBy(hl => hl.Name).ToList();

                        foreach (PastItem hi in PausedAlpha)
                        {
                            if (IsURL(hi.Path) == false)
                            {
                                bFound = await MenuCreate(CurrentMenu.BediaFolder, await StorageFile.GetFileFromPathAsync(hi.Path));
                            }
                        }
                        break;
                    #endregion

                    case "PausedFolder":
                        #region Pause Folder
                        StorageFile filPaused;
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        DateTime dtPausedDateRangeFrom = Convert.ToDateTime(HackBediaItem.Value);
                        dtPausedDateRangeFrom = dtPausedDateRangeFrom.AddDays((-dtPausedDateRangeFrom.Day + 1));//Get the first day of the month
                        DateTime dtPausedDateRangeTo = dtPausedDateRangeFrom.AddMonths(1);
                        List<PastItem> PausedMonthFolder = PastListHistory.Where(
                           i => i.PlayedDate > dtPausedDateRangeFrom &&
                           i.PlayedDate <= dtPausedDateRangeTo &&
                           i.Seconds > 0).ToList();

                        foreach (PastItem hi in PausedMonthFolder)
                        {
                            filPaused = await StorageFile.GetFileFromPathAsync(hi.Path);
                            bFound = await MenuCreate(CurrentMenu.BediaFolder, filPaused);
                            filHistory = null;
                        }

                        break;
                    #endregion

                    case "Services":
                        #region Services
                        bFound = await bediaIO.FolderExists(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);
                        if (bFound == true)
                        {
                            StorageFolder IPTVFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);
                            IReadOnlyList<StorageFile> IPTVFiles = await IPTVFolder.GetFilesAsync();
                            if (IPTVFiles.Count > 0)
                            {
                                biMenuItem = new BediaItem("IPTV", BediaItem.MenuTypes.IPTVHome);
                                BediaMenuItems.Add(biMenuItem);
                            }
                            IPTVFolder = null;
                            IPTVFiles = null;
                        }

                        biMenuItem = new BediaItem("Podcasts", BediaItem.MenuTypes.Podcasts);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Space", BediaItem.MenuTypes.SpaceHome);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Theater", BediaItem.MenuTypes.TheatreTimesHome);
                        BediaMenuItems.Add(biMenuItem);

                        if (bediaLocation != null)
                        {
                            if (bediaLocation.region.ToLower() == "new york")
                            {
                                biMenuItem = new BediaItem("Transit", BediaItem.MenuTypes.TransitHome);
                                BediaMenuItems.Add(biMenuItem);
                            }

                            if (bediaLocation.postal != null)
                            {
                                biMenuItem = new BediaItem("Weather", BediaItem.MenuTypes.WeatherHome);
                                BediaMenuItems.Add(biMenuItem);
                            }
                        }

                        break;
                    #endregion

                    case "LoadSettings":
                        #region Settings
                        biMenuItem = new BediaItem("Change Home Folders", BediaItem.MenuTypes.AddRemoveHomeFolders);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Bubbles", BediaItem.MenuTypes.SettingsBubbles);
                        biMenuItem.Selected = this.Bubbles;
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Colored Bubbles", BediaItem.MenuTypes.SettingsPartyBubbles);
                        biMenuItem.Selected = this.BubblesParty;
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Display Audio Info", BediaItem.MenuTypes.SettingsMediaDisplayInfo);
                        biMenuItem.Selected = SettingGetBool("MediaDisplayInfo");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("IPTV Folder", BediaItem.MenuTypes.SettingsIPTVFolder);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Late Night", BediaItem.MenuTypes.SettingsLateNight);
                        biMenuItem.Selected = SettingGetBool("LateNight");
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Menu Colors", BediaItem.MenuTypes.SettingsMenuColors);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Menu Options", BediaItem.MenuTypes.SettingsOptionalMenus);
                        BediaMenuItems.Add(biMenuItem);
                       
                        biMenuItem = new BediaItem("Past Clear", BediaItem.MenuTypes.SettingsHistoryClear);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Past Save", BediaItem.MenuTypes.SettingsHistorySave);
                        biMenuItem.Selected = this.SavePast;
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Screen Saver Clock", BediaItem.MenuTypes.SettingsScreenSaverClock);
                        biMenuItem.Selected = SettingGetBool("ScreenSaverClock");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Screen Saver Comments", BediaItem.MenuTypes.SettingsScreenSaverComments);
                        biMenuItem.Selected = SettingGetBool("ScreenSaverComments");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Titlebar Clock", BediaItem.MenuTypes.SettingsTitlebarClock);
                        biMenuItem.Selected = SettingGetBool("TitlebarClock");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Video Stretched", BediaItem.MenuTypes.StretchVideo);
                        biMenuItem.Selected = mbStretchVideo;
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Windows Volume", BediaItem.MenuTypes.SettingsWindowsVolume);
                        biMenuItem.Selected = SettingGetBool("WindowsVolume");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("About " + msAppName, BediaItem.MenuTypes.About);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion


                    //---------------------------
                    //  SERVICES
                    //---------------------------
                    case "IPTV":
                        #region IPTV
                        bFound = await bediaIO.FolderExists(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);
                        if (bFound == true)
                        {
                            StorageFolder IPTVFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);
                            IReadOnlyList<StorageFile> IPTVFiles = await IPTVFolder.GetFilesAsync();

                            if (IPTVFiles.Count > 1)
                            {
                                foreach (StorageFile IPTVFile in IPTVFiles)
                                {
                                    BediaItem biIPTVPlaylist = new BediaItem(System.IO.Path.GetFileNameWithoutExtension(IPTVFile.Path), BediaItem.MenuTypes.IPTVPlaylist);
                                    biIPTVPlaylist.BediaFile = IPTVFile;
                                    BediaMenuItems.Add(biIPTVPlaylist);
                                }
                            }
                            else if (IPTVFiles.Count == 1)
                            {
                                bFound = await PlaylistLoad(IPTVFiles[0]);
                            }

                            if (BediaMenuItems.Count > 0)
                            {
                                BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();
                            }
                        }
                        break;
                    #endregion

                    case "IPTVPlaylist":
                        #region IPTV Playlist
                        if (BediaMenuItems.Count > 0)
                        {
                            BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();
                        }
                        break;
                    #endregion

                    case "LoadPodcasts":
                        #region Load Podcasts
                        //biMenuItem = new BediaItem("NAME_HERE", BediaItem.MenuTypes.Podcast);
                        //biMenuItem.Value = "URL_HERE";
                        //BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("BBC Global News", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://downloads.bbc.co.uk/podcasts/worldservice/globalnews/rss.xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("The Daily Show", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://thedailyshow.comedycentral.libsynpro.com/rss";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("The Dumbasses Guide To Knowledge", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.feedburner.com/dumbassguide?format=xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Filmcast", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.feedburner.com/filmcast";
                        BediaMenuItems.Add(biMenuItem);
                                                
                        biMenuItem = new BediaItem("Great American Broadcast Network", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://alexbennett.com/podcast/gap.xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("The Joe Rogan Experience", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://joeroganexp.joerogan.libsynpro.com/rss";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem(".NET Rocks", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master&tags=WPF%2cJavascript%2cXAML%2cWinRT%2cKinect%2cSpace%2cNon-SQL+Databases%2cSecurity%2cTablet%2cHTML+5%2cData%2cSource+Control%2cTesting%2cASP.NET+MVC%2cCSS%2cVB.NET%2cUser+Experience%2cWindows+8%2cCloud%2cASP.NET%2cC%23%2cFramework%2cArchitecture%2cVisual+Studio";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("New Yorker Radio Hour", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.wnyc.org/newyorkerradiohour";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Radiolab", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.wnyc.org/radiolab";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Real Time with Bill Maher", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.hbo.com/podcasts/billmaher/podcast.xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Ricky Gervais", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://podcast.rickygervais.com/podcast.xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Savage Lovecast", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.thestranger.com/stranger/savage";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Science Friday", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.sciencefriday.com/audio/scifriaudio.xml";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Sex Out Loud with Tristan Taormino", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.voiceamerica.com/rss/show/2096";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("The Skeptics' Guide to the Universe", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.theskepticsguide.org/feed";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Star Talk Radio", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feeds.soundcloud.com/users/soundcloud:users:38128127/sounds.rss";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Stuff You Missed In History Class", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://www.howstuffworks.com/podcasts/stuff-you-missed-in-history-class.rss";
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("This American Life", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://feed.thisamericanlife.org/talpodcast";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("WTF with Marc Maron", BediaItem.MenuTypes.Podcast);
                        biMenuItem.Value = "http://wtfpod.libsyn.com/rss";
                        BediaMenuItems.Add(biMenuItem);

                        break;
                    #endregion

                    case "LoadPodcast":
                        #region Load Podcast
                        CurrentMenu = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        SyndicationClient SyndClient = new SyndicationClient();
                        SyndicationFeed feed = await SyndClient.RetrieveFeedAsync(new Uri(CurrentMenu.Value));


                        foreach (SyndicationItem item in feed.Items)
                        {
                            bFound = false;

                            foreach (SyndicationLink link in item.Links)
                            {
                                 bFound = true;
                                 sTemp = link.Uri.ToString();
                                
                                if (bFound == true) { break; }
                            }

                            if (bFound == true)
                            {
                                biMenuItem = new BediaItem(item.Title.Text, BediaItem.MenuTypes.PodcastItem);
                                biMenuItem.Value = sTemp;
                                BediaMenuItems.Add(biMenuItem);
                            }
                        }
                        SyndClient = null;
                        feed = null;
                        CurrentMenu = null;
                        break;
                    #endregion

                    case "SpaceHome":
                        #region SpaceHome
                        ISSLocationStop();

                        biMenuItem = new BediaItem("ISS Location", BediaItem.MenuTypes.SpaceISSLocationHome);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("ISS View #1", BediaItem.MenuTypes.SpaceItem);
                        biMenuItem.Value = "http://iphone-streaming.ustream.tv/uhls/17074538/streams/live/iphone/playlist.m3u8";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("ISS View #2", BediaItem.MenuTypes.SpaceItem);
                        biMenuItem.Value = "http://iphone-streaming.ustream.tv/uhls/9408562/streams/live/iphone/playlist.m3u8";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Kennedy Space Center Audio", BediaItem.MenuTypes.SpaceItem);
                        biMenuItem.Value = "http://audio2.broadcastify.com/ksc.mp3";
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("NASA", BediaItem.MenuTypes.SpaceItem);
                        biMenuItem.Value = "http://nasatv-lh.akamaihd.net/i/NASA_101@319270/index_1000_av-p.m3u8?sd=10&rebase=on";
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("UrtheCast", BediaItem.MenuTypes.SpaceItem);
                        biMenuItem.Value = "https://d2ai41bknpka2u.cloudfront.net/live/iss.stream_source/playlist.m3u8";
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion
                        
                    case "MoviesDetais":
                        #region Theatre / Movie Times
                        //http://developer.tmsapi.com/docs/read/data_v1_1/movies/Movie_showtimes
                        string sKey = "MY KEY REMOVED, GET YOUR OWN KEY"; //Gracenote APIs
                        string sDate = DateTime.Now.ToString("yyyy-MM-dd");
                        string sURL = "http://data.tmsapi.com/v1.1/movies/showings?startDate=" + sDate + "&numDays=2&zip=" + bediaLocation.postal + "&radius=10&api_key=" + sKey;
                        //http://data.tmsapi.com/v1.1/movies/showings?startDate=2015-06-04&numDays=2&zip=10035&radius=10&api_key=fh4eurew6tga4wavtasbsx7v


                        if (MovieListDate.ToString("MM/DD/YYYY") != DateTime.Now.ToString("MM/DD/YYYY") || MovieList == null)
                        {
                            //There is a very low limit on the number of hits per day this app can make without paying Gracenote a fee.
                            //This will allow the list to only regenerate after a day.
                            HttpClient MoviesClient = new HttpClient();
                            HttpResponseMessage MoviesResponse = await MoviesClient.GetAsync(sURL);
                            MoviesResponse.EnsureSuccessStatusCode();
                            string sMovieList = await MoviesResponse.Content.ReadAsStringAsync();
                            MovieList = JsonConvert.DeserializeObject<object>(sMovieList);
                        }

                        foreach (var Movie in MovieList)
                        {
                            if (Movie != null)
                            {
                                sTemp = Movie.title;
                                biMenuItem = new BediaItem(sTemp, BediaItem.MenuTypes.TheatreTimesMovie);
                                biMenuItem.Value = Movie.tmsId;
                                BediaMenuItems.Add(biMenuItem);
                            }
                        }
                        BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();
                        break;
                    #endregion

                    case "TheatresDetails":
                        #region Theatre Details
                        string sTheatre = "";

                        foreach (var Movie in MovieList)
                        {
                            if (Movie != null && Movie.tmsId == HackBediaItem.Value)
                            {
                                sTemp = Movie.shortDescription;
                                biMenuItem = new BediaItem(sTemp, BediaItem.MenuTypes.TheatreTimesItem);
                                BediaMenuItems.Add(biMenuItem);

                                //Some movies do not have ratings, or cast or...
                                if (Movie.ratings != null)
                                {
                                    foreach (var Rating in Movie.ratings)
                                    {
                                        sTemp = Rating.code;
                                        biMenuItem = new BediaItem("Rated " + sTemp, BediaItem.MenuTypes.TheatreTimesItem);
                                        BediaMenuItems.Add(biMenuItem);
                                    }
                                }

                                if (Movie.topCast != null)
                                {
                                    foreach (var CastMember in Movie.topCast)
                                    {
                                        sTemp = CastMember;
                                        biMenuItem = new BediaItem("Staring " + sTemp, BediaItem.MenuTypes.TheatreTimesItem);
                                        BediaMenuItems.Add(biMenuItem);
                                    }
                                }

                                if (Movie.showtimes != null)
                                {
                                    foreach (var ShowTime in Movie.showtimes)
                                    {
                                        //No need to show the theatre name multiple times
                                        if (sTheatre != Convert.ToString(ShowTime.theatre.name))
                                        {
                                            sTemp = ShowTime.theatre.name;
                                            biMenuItem = new BediaItem(sTemp, BediaItem.MenuTypes.TheatreTimesTheatre);
                                            TempMenuItems.Add(biMenuItem);
                                            sTheatre = ShowTime.theatre.name;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        TempMenuItems = TempMenuItems.OrderBy(x => x.SortedBy).ToList();
                        BediaMenuItems.AddRange(TempMenuItems);
                        break;
                    #endregion

                    case "TheatreDateTimes":
                        #region Theatre Date Times
                        string sTheatreName = "";

                        foreach (var Movie in MovieList)
                        {
                            if (Movie.title == HackBediaItem.Value)
                            {
                                if (Movie.showtimes != null)
                                {
                                    foreach (var ShowTime in Movie.showtimes)
                                    {
                                        sTheatreName = ShowTime.theatre.name;

                                        if (HackBediaItem.Title.ToLower() == sTheatreName.ToLower())
                                        {
                                            sTemp = ShowTime.dateTime;
                                            sTemp = sTemp.Replace('T', ' ');
                                            DateTime MovieTime = Convert.ToDateTime(sTemp);
                                            biMenuItem = new BediaItem(MovieTime.ToString("MM/DD/YYYY") + " " + MovieTime.ToString("MM/DD/YYYY"), BediaItem.MenuTypes.TheatreTimesItem);
                                            BediaMenuItems.Add(biMenuItem);
                                        }
                                    }
                                }
                            }
                        }
                        BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();
                        break;
                    #endregion

                    case "LoadTransit":
                        #region Transit
                        HttpClient client = new HttpClient();
                        Stream stream = await client.GetStreamAsync("http://mta.info/status/serviceStatus.txt");
                        XDocument xTransit = XDocument.Load(stream);

                        foreach(XElement BaseElements in xTransit.Elements())
                        {
                            foreach(XElement Child in BaseElements.Elements())
                            {
                                switch (Child.Name.ToString().ToLower())
                                {
                                    case "subway":
                                    case "bus":
                                    case "bt":
                                    case "lirr":
                                    case "metronorth":
                                        bFound = await TransitLines(Child, Child.Name.ToString());
                                        break;
                                }
                            }
                        }
                        xTransit = null;
                        client.Dispose();
                        client = null;
                        stream.Dispose();
                        stream = null;
                        break;
                    #endregion

                    case "LoadWeather":
                        #region Weather
                        ////http://forecast.weather.gov/MapClick.php?lat=40.78158&lon=-73.96648&FcstType=dwml
                        List<BediaWeather> Weathers = new List<BediaWeather>();
                        List<string> DaysOfWeek = new List<string>();
                        List<string> MinTemp = new List<string>();
                        List<string> MaxTemp = new List<string>();
                        List<string> WeatherConditions = new List<string>();
                        byte bWeatherSummary = 0;
                        string[] sLocation = bediaLocation.loc.ToString().Split(',');
                        string sWeatherURL = "http://forecast.weather.gov/MapClick.php?lat=" + sLocation[0] + "&lon=" + sLocation[1] + "&FcstType=dwml";  //"http://forecast.weather.gov/MapClick.php?lat=40.78158&lon=-73.96648&FcstType=dwml";
                        HttpClient WeatherClient = new HttpClient();
                        WeatherClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Stackoverflow/1.0");
                        string sXML = await WeatherClient.GetStringAsync(sWeatherURL);
                        System.Xml.XmlDocument xmlWeather = new System.Xml.XmlDocument();
                        xmlWeather.LoadXml(sXML);
                        System.Xml.XmlNodeList CurrentTemp = xmlWeather.GetElementsByTagName("dwml");


                        //Gather the weather data
                        foreach (XmlNode MasterNodes in xmlWeather.ChildNodes)
                        {
                            foreach(XmlNode BaseNodes in MasterNodes.ChildNodes)
                            {
                                if (BaseNodes.Name.ToLower() == "data")
                                {
                                    foreach(XmlNode childNode in BaseNodes.ChildNodes)
                                    {
                                        switch (childNode.Name.ToLower())
                                        {
                                            case "location":
                                                biMenuItem = new BediaItem("Conditions for " + childNode.ChildNodes.Item(1).InnerText, BediaItem.MenuTypes.WeatherItem);
                                                BediaMenuItems.Add(biMenuItem);
                                                break;

                                            case "time-layout":
                                                #region WeatherTimes
                                                foreach (XmlNode DOW in childNode.ChildNodes)
                                                {
                                                    if (bFound == false)  //To save a little time
                                                    { 
                                                        if (DOW.Name == "layout-key" && DOW.InnerText == "k-p24h-n7-1")
                                                        {
                                                            //There are multiple sections named "time-layout" so, that's why there is this
                                                            bFound = true;
                                                        }
                                                    }

                                                    if (DOW.Name == "start-valid-time" && bFound == true)
                                                    {
                                                        DaysOfWeek.Add(DOW.Attributes["period-name"].Value);
                                                    }
                                                }
                                                bFound = false;
                                                #endregion
                                                break;

                                            case "parameters":
                                                #region WeatherParameters
                                                foreach (XmlNode TempNodes in childNode.ChildNodes)
                                                {
                                                     switch(TempNodes.Name.ToLower())
                                                    {
                                                        case "temperature":
                                                            switch (TempNodes.Attributes["type"].Value)
                                                            {
                                                                case "minimum":
                                                                    //
                                                                    foreach (XmlNode minimumTemp in TempNodes.ChildNodes)
                                                                    {
                                                                        if (minimumTemp.Name.ToLower() == "value")
                                                                        {
                                                                            MinTemp.Add(minimumTemp.InnerText);
                                                                        }
                                                                    }
                                                                    break;

                                                                case "maximum":
                                                                    foreach (XmlNode maximumTemp in TempNodes.ChildNodes)
                                                                    {
                                                                        if (maximumTemp.Name.ToLower() == "value")
                                                                        {
                                                                            MaxTemp.Add(maximumTemp.InnerText);
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                            break;

                                                        case "weather":
                                                            foreach (XmlNode weatherCondition in TempNodes.ChildNodes)
                                                            {
                                                                if (weatherCondition.Name.ToLower() == "weather-conditions")
                                                                {
                                                                    WeatherConditions.Add(weatherCondition.Attributes["weather-summary"].Value);
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                #endregion
                                                break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        //Create the weather menus
                        for(byte bWeather=0; bWeather< 6; bWeather++)
                        {
                            sTemp = ""; //Reset

                            if (bWeather <= MinTemp.Count())
                            {
                                sTemp += MinTemp[bWeather] + "f";
                            }

                            if (bWeather <= MaxTemp.Count())
                            {
                                sTemp += " - " + MaxTemp[bWeather] + "f";
                            }

                            biMenuItem = new BediaItem(DaysOfWeek[bWeather] + " " + sTemp  + " " + WeatherConditions[bWeatherSummary], BediaItem.MenuTypes.WeatherItem);
                            BediaMenuItems.Add(biMenuItem);

                            bWeatherSummary += 2;
                        }

                        //Cleanup
                        Weathers.Clear();
                        DaysOfWeek.Clear();
                        MinTemp.Clear();
                        MaxTemp.Clear();
                        WeatherConditions.Clear();
                        WeatherClient.Dispose();
                        WeatherClient = null;
                        xmlWeather = null;
                        break;
                        #endregion

                   
                    //---------------------------
                    //  SETTINGS
                    //---------------------------
                    case "LoadAddRemoveHomeFolders":
                        #region AddRemove Folders
                        biMenuItem = new BediaItem("Add Home Folder", BediaItem.MenuTypes.AddRemoveHomeFolder);
                        BediaMenuItems.Add(biMenuItem);
                        
                        string sHomeMenuTokens = SettingGet("HomeMenus");

                        if (sHomeMenuTokens.Trim().Length > 0)
                        {
                            string[] HomeMenuTokens = sHomeMenuTokens.Split(',');
                            
                            foreach (string Token in HomeMenuTokens)
                            {
                                if (Token.Trim().Length > 0)
                                {
                                    try
                                    {
                                        //If an external drive is not available the following line will fail.  Don't care, move on to the next token
                                        StorageFolder Homefolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
                                        bFound = await bediaIO.FolderExists(Homefolder.Path); //Just in case it is no longer available
                                        if (bFound == true)
                                        {
                                            BediaItem biHomeFolders = new BediaItem(Homefolder.DisplayName, BediaItem.MenuTypes.AddRemoveHomeFolderSelect);
                                            biHomeFolders.BediaFolder = Homefolder;
                                            biHomeFolders.Selected = true;
                                            biHomeFolders.Value = Token;
                                            BediaMenuItems.Add(biHomeFolders);
                                        }
                                    }
                                    catch { }
                                }
                            }

                            BediaMenuItems = BediaMenuItems.OrderBy(x => x.SortedBy).ToList();
                        }


                        string sLibraries = SettingGet("HomePageLibraries");

                        biMenuItem = new BediaItem("Music Library", BediaItem.MenuTypes.SettingsLibrary);
                        biMenuItem.Value = "MusicLibrary";
                        if (sLibraries.Contains("MusicLibrary") == true)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Video Library", BediaItem.MenuTypes.SettingsLibrary);
                        biMenuItem.Value = "VideoLibrary";
                        if (sLibraries.Contains("VideoLibrary") == true)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "AudioVisualizations":
                        #region Audio Visualizations
                        string sAudioVisualizations = SettingGet("AudioVisualizations");

                        biMenuItem = new BediaItem("Bloom", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "Bloom";
                        if (sAudioVisualizations.IndexOf("Bloom") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Fireflies", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "Fireflies";
                        if (sAudioVisualizations.IndexOf("Fireflies") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Iris", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "Iris";
                        if (sAudioVisualizations.IndexOf("Iris") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Lilypads", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "Lilypads";
                        if (sAudioVisualizations.IndexOf("Lilypads") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Raindrops", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "Raindrops";
                        if (sAudioVisualizations.IndexOf("Raindrops") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Snap Dragon", BediaItem.MenuTypes.SettingsVisualizationItem);
                        biMenuItem.Value = "SnapDragon";
                        if (sAudioVisualizations.IndexOf("SnapDragon") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "HomePageOptions":
                        #region Home Page Menu Options
                        sTemp = SettingGet("HomePageMenuOptions");

                        biMenuItem = new BediaItem("Past", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Past";
                        if (sTemp.IndexOf("Past") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Paused", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Paused";
                        if (sTemp.IndexOf("Paused") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("IPTV", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "IPTV";
                        if (sTemp.IndexOf("IPTV") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Podcasts", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Podcasts";
                        if (sTemp.IndexOf("Podcasts") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Space", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Space";
                        if (sTemp.IndexOf("Space") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Theatre", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Theatre";
                        if (sTemp.IndexOf("Theatre") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Transit", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Transit";
                        if (sTemp.IndexOf("Transit") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Weather", BediaItem.MenuTypes.SettingsMenuOptionItem);
                        biMenuItem.Value = "Weather";
                        if (sTemp.IndexOf("Weather") > -1)
                        {
                            biMenuItem.Selected = true;
                        }
                        BediaMenuItems.Add(biMenuItem);

                        break;
                    #endregion

                    case "MenuColors":
                        #region Menu Colors
                        //These three lines are for a Just In Case the user has selected the Custom Menu Select option, and now has selected; Back
                        BediaTransitions bediaTransitions = new BediaTransitions();
                        bediaTransitions.UIOpacity(new BediaStoryboardOptions(mMainPage.BediaColors, 0.0));
                        bediaTransitions = null;

                        biMenuItem = new BediaItem("Bedia Blue 2.0", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 49, 123, 193));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Bedia Blue 3.0", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 102, 204));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Dark Blue", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 102));
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Deep Pink", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 20, 147));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Red", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Bedroom Red", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 0, 0));
                        BediaMenuItems.Add(biMenuItem);
                                                
                        biMenuItem = new BediaItem("Purple", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 0, 128));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Purple-ish", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 102, 255));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Dark Purple", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 75, 0, 130));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Living Room Green", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 117, 132, 102));
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Dark Green", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 100, 0));
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Gray", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128));
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Black", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                        BediaMenuItems.Add(biMenuItem);
                        
                        biMenuItem = new BediaItem("Custom Color", BediaItem.MenuTypes.SettingsMenuColor);
                        biMenuItem.ValueObject = SettingGetBrush("CustomMenuColor");
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Select Custom Color", BediaItem.MenuTypes.SettingsMenuCustomColorSelect);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion
                        
                    case "ClearHistory":
                        #region Clear Past
                        biMenuItem = new BediaItem("Clear Past", BediaItem.MenuTypes.SettingsHistoryClearOK);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Cancel", BediaItem.MenuTypes.SettingsHistoryClearCancel);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "LoadAbout":
                        #region About
                        Package BediaPackage = Package.Current;
                        
                        biMenuItem = new BediaItem(msAppName + " " + BediaPackage.Id.Version.Major + "." + BediaPackage.Id.Version.Minor + "." + BediaPackage.Id.Version.Build + " © 2010 - " + DateTime.Now.Year.ToString() + " James Rose", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("This is freeware.  Details at Blissgig.com", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Icons from IconFinder.com", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Theatre / Movie info provided by Gracenote.com", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Transit info provided by MTA.info", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("View from space provided by Urthecast.com", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Weather provided by Weather.gov", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                    #endregion

                    case "ExitApp":
                        #region Exit App
                        biMenuItem = new BediaItem("Do you wish to exit " + msAppName + "?", BediaItem.MenuTypes.ExitAppYes);
                        BediaMenuItems.Add(biMenuItem);

                        biMenuItem = new BediaItem("Return to Home Page", BediaItem.MenuTypes.ExitAppNo);
                        BediaMenuItems.Add(biMenuItem);
                        break;
                        #endregion
                }

                //Cleanup
                bediaIO = null;
                biMenuItem = null;
                CurrentMenu = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }
        
        private async Task<bool> TransitLines(XElement TransitLine, string Title)
        {
            try
            {
                string sLine = "";
                string sStatus = "";

                BediaItem biMenuItem = new BediaItem(Title, BediaItem.MenuTypes.TransitItem);
                BediaMenuItems.Add(biMenuItem);

                foreach (XElement Subway in TransitLine.Elements())
                {
                    sLine = "";
                    sStatus = ""; //To insure reset
                    foreach (XElement Line in Subway.Elements())
                    {
                        if (Line.Name.ToString().ToLower() == "name")
                        {
                            sLine = Line.Value;
                        }
                        else if (Line.Name.ToString().ToLower() == "status")
                        {
                            sStatus = Line.Value;
                        }
                    }

                    biMenuItem = new BediaItem(sLine + ": " + sStatus, BediaItem.MenuTypes.TransitItem);
                    BediaMenuItems.Add(biMenuItem);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1));

            return true;
        }

        private void MenuSelect(BediaItem.MenuTypes MenuType, bool DeselectOthers = true)
        {
            try
            {
                foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                {
                    if (BediaMenu.Bediaitem.MenuType == MenuType)
                    {
                        BediaMenu.SetIcon(IconByNameSmall("Selected"), !BediaMenu.Bediaitem.Selected);
                    }
                    else
                    {
                        if (DeselectOthers == true)
                        {
                            BediaMenu.SetIcon(IconByNameSmall("Selected"), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MenuSelect(string value, bool DeselectOthers = true)
        {
            try
            {
                bool bEscape = false;

                foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                {
                    if (BediaMenu.Bediaitem.Value == value)
                    {
                        if (DeselectOthers == false)
                        {
                            bEscape = true;
                        }
                    }
                    else
                    {
                        if (DeselectOthers == true)
                        {
                            BediaMenu.Bediaitem.Selected = false;
                        }
                    }
                    BediaMenu.SetIcon(IconByNameSmall("Selected"), !BediaMenu.Bediaitem.Selected);

                    if (bEscape == true) { break; }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SetTitlebarClock()
        {
            try
            {
                if (this.IsMobile == true) { return; } //Mobile already has a clock

                bool bFound = SettingGetBool("TitlebarClock");

                if (bFound == true)
                {
                    TitlebarClock = new BediaClock(BediaHighlightColor, this.MenuFontSize);
                    TitlebarClock.Height = this.MenuSize;
                    TitlebarClock.ClockStart(); 

                    bediaTitlebar.TitlebarGrid.Children.Add(TitlebarClock);
                    Grid.SetColumn(TitlebarClock, 5);
                }
                else
                {
                    bediaTitlebar.TitlebarGrid.Children.Remove(TitlebarClock);
                    TitlebarClock = null;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaMenu(bool Show = true)
        {
            try
            {
                double From = 0.0;
                double To = 0.68;

                this.IsMenuDisplayed = Show;

                if (Show == false)
                {
                    From = mMainPage.MediaOverlay.Opacity;
                    To = 0.0;
                    TitlebarHide("");
                    ScreenSaverCancel();
                    MenusClear(); 
                }
                else
                {
                    TitlebarHide("title");
                    if (MenuBreadrumbs.Count > 0)
                    {
                        BediaItem bediaitem = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                        MenuBreadrumbs.RemoveAt(MenuBreadrumbs.Count - 1); //It's about to be added again in the MenuSelect function.
                        MenuSelected(bediaitem);
                    }
                    else
                    {
                        MenuProcessing("LoadHome", this.HomePageTitle, "Home");
                    }
                }

                Storyboard sbFade = new Storyboard();
                DoubleAnimation animFade = new DoubleAnimation();

                animFade.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                animFade.From = From;
                animFade.To = To;

                Storyboard.SetTarget(animFade, mMainPage.MediaOverlay);
                Storyboard.SetTargetProperty(animFade, "(UIElement.Opacity)");
                sbFade.Children.Add(animFade);

                sbFade.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void MediaEnded()
        {
            try
            {
                this.IsPlaying = false;
                this.IsPaused = false;
                MediaNext();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaJump(TimeSpan Jump)
        {
            try
            {
                Duration MediaLength;
                Duration CurrentPosition;
                byte Percentage;


                if (Jump.TotalSeconds != 0)
                {
                    mMainPage.BediaMedia.Position += Jump;
                }

                MediaLength = mMainPage.BediaMedia.NaturalDuration;
                CurrentPosition = mMainPage.BediaMedia.Position;
                Percentage = Convert.ToByte(Math.Round((mMainPage.BediaMedia.Position.TotalSeconds / MediaLength.TimeSpan.TotalSeconds) * 100));


                //Show the current position in the MediaStatus bubbles
                if (medianavigation.Opacity > 0.0)
                {
                    Storyboard sbMediaNavigation = new Storyboard();
                    DoubleAnimation animNavigationBar = new DoubleAnimation();
                    animNavigationBar.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    animNavigationBar.From = medianavigation.Opacity;
                    animNavigationBar.To = 0.0;
                    Storyboard.SetTarget(animNavigationBar, medianavigation);
                    Storyboard.SetTargetProperty(animNavigationBar, "(UIElement.Opacity)");
                    sbMediaNavigation.Children.Add(animNavigationBar);
                    sbMediaNavigation.Begin();
                }

                MediaStatusBubbles(Percentage);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaStatusBubbles(byte Percent)
        {
            try
            {
                if (mediastatus.Opacity < 1.0)
                {
                    Storyboard sbMediaStatus = new Storyboard();
                    DoubleAnimation animMediaStatus = new DoubleAnimation();
                    animMediaStatus.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed * 2));
                    animMediaStatus.From = mediastatus.Opacity;
                    animMediaStatus.To = 1.0;
                    Storyboard.SetTarget(animMediaStatus, mediastatus);
                    Storyboard.SetTargetProperty(animMediaStatus, "(UIElement.Opacity)");
                    sbMediaStatus.Children.Add(animMediaStatus);
                    sbMediaStatus.Begin();
                }

                //- Animate the status bubbles
                mediastatus.SetStatus(Percent);

                if (StatusTimer != null)
                {
                    StatusTimer.Dispose();
                    StatusTimer = null;
                }

                StatusTimer = new Timer(StatusHide, null, Convert.ToInt32(TimeSpan.FromSeconds(8).TotalMilliseconds), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void NavigationDisplay(Int16 Change)
        {
            try
            {
                //Only change the value if the amount of time is less than the default number of seconds (NavigationDisplaySeconds)
                DateTime dtNow = DateTime.Now;
                BediaTransitions bediaTransitions = new BediaTransitions();


                if (dtNow.Subtract(NavigationDisplayDateTime).TotalSeconds < NavigationDisplaySeconds)
                {
                    miNavigationCurrentPosition += Change;

                    if (miNavigationCurrentPosition >= NavigationItems.Count)
                    {
                        miNavigationCurrentPosition = 0;
                    }

                    if (miNavigationCurrentPosition < 0)
                    {
                        miNavigationCurrentPosition = Convert.ToByte(NavigationItems.Count - 1);
                    }
                }
                NavigationDisplayDateTime = DateTime.Now; //Reset


                NavigationItem CurrentNavitaion = NavigationItems[miNavigationCurrentPosition];
                
                if (medianavigation.Opacity < 1.0)
                {
                    //Only fade the change if the control is already displayed, in this case just set it.
                    BediaIcon bi = NavigationIcon(CurrentNavitaion.Left);

                    medianavigation.IconBackgroundLeft.Fill = bi.Fill;
                    medianavigation.IconBackgroundLeft.Height = bi.Size;
                    medianavigation.IconBackgroundLeft.Width = bi.Size;
                    medianavigation.IconBackgroundLeft.Stroke = bi.Stroke;
                    medianavigation.IconBackgroundLeft.StrokeThickness = bi.StrokeThickness;

                    medianavigation.IconBackgroundRight.Fill = bi.Fill;
                    medianavigation.IconBackgroundRight.Height = bi.Size;
                    medianavigation.IconBackgroundRight.Width = bi.Size;
                    medianavigation.IconBackgroundRight.Stroke = bi.Stroke;
                    medianavigation.IconBackgroundRight.StrokeThickness = bi.StrokeThickness;

                    medianavigation.NavLeftImage.Source = bi.Icon.Source;
                    medianavigation.NavRightImage.Source = NavigationIcon(CurrentNavitaion.Right).Icon.Source;
                    
                    Storyboard sbNavigation = new Storyboard();

                    if (mediastatus.Opacity > 0.0)
                    {
                        DoubleAnimation animStatus = new DoubleAnimation();
                        animStatus.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed / 2));
                        animStatus.From = mediastatus.Opacity;
                        animStatus.To = 0.0;
                        Storyboard.SetTarget(animStatus, mediastatus);
                        Storyboard.SetTargetProperty(animStatus, "(UIElement.Opacity)");
                        sbNavigation.Children.Add(animStatus);
                    }

                    DoubleAnimation animFadeIn = new DoubleAnimation();
                    animFadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    animFadeIn.From = medianavigation.Opacity;
                    animFadeIn.To = 1.0;
                    Storyboard.SetTarget(animFadeIn, medianavigation);
                    Storyboard.SetTargetProperty(animFadeIn, "(UIElement.Opacity)");
                    sbNavigation.Children.Add(animFadeIn);
                    sbNavigation.Begin();
                }
                else
                {
                    ImageChange(medianavigation.NavLeftImage, NavigationIcon(CurrentNavitaion.Left).Icon);
                    ImageChange(medianavigation.NavRightImage, NavigationIcon(CurrentNavitaion.Right).Icon);
                }


                if (NavigationTimer != null)
                {
                    NavigationTimer.Dispose();
                    NavigationTimer = null;
                }

                NavigationTimer = new Timer(NavigationHide, null, Convert.ToInt32(TimeSpan.FromSeconds(8).TotalMilliseconds), Timeout.Infinite);
                CurrentNavitaion = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void NavigationHide(object state)
        {
            try
            {
                await mMainPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => {
                        Storyboard sbFadeOut = new Storyboard();
                        DoubleAnimation anFadeOut = new DoubleAnimation();
                        anFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                        anFadeOut.From = medianavigation.Opacity;
                        anFadeOut.To = 0.0;
                        Storyboard.SetTarget(anFadeOut, medianavigation);
                        Storyboard.SetTargetProperty(anFadeOut, "(UIElement.Opacity)");
                        sbFadeOut.Children.Add(anFadeOut);
                        sbFadeOut.Begin();
                    });
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void NavigationSelect(PointerRoutedEventArgs e)
        {
            try
            {
                NavigationItem CurrentNavitaion = NavigationItems[miNavigationCurrentPosition];
                NavigationItem.NavigationType Nav = CurrentNavitaion.Left;
                

                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    var properties = e.GetCurrentPoint(mMainPage).Properties;
                    if (properties.IsLeftButtonPressed)
                    {
                        Nav = CurrentNavitaion.Left;
                    }
                    else if (properties.IsRightButtonPressed)
                    {
                        Nav = CurrentNavitaion.Right;
                    }
                    else
                    {
                        return; //Just in case any other button is pressed
                    }
                }

                NavigationSelected(Nav); 
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void NavigationSelected(NavigationItem.NavigationType Nav)
        {
            try
            {
                TimeSpan Jump;

                switch (Nav)
                {
                    case NavigationItem.NavigationType.JumpBackMinute1:
                        Jump = new TimeSpan(0, -1, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpBackMinute10:
                        Jump = new TimeSpan(0, -10, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpBackMinute5:
                        Jump = new TimeSpan(0, -5, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpBackSeconds30:
                        Jump = new TimeSpan(0, 0, -30);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpForwardMinute1:
                        Jump = new TimeSpan(0, 1, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpForwardMinute10:
                        Jump = new TimeSpan(0, 10, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpForwardMinute5:
                        Jump = new TimeSpan(0, 5, 0);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.JumpForwardSeconds30:
                        Jump = new TimeSpan(0, 0, 30);
                        MediaJump(Jump);
                        break;

                    case NavigationItem.NavigationType.Menu:
                        MediaMenu();
                        break;

                    case NavigationItem.NavigationType.Next:
                        MediaNext();
                        break;

                    case NavigationItem.NavigationType.PlayPause:
                        MediaPlayPause();
                        break;

                    case NavigationItem.NavigationType.Previous:
                        MediaPrior();
                        break;

                    case NavigationItem.NavigationType.Stop:
                        MediaStop();
                        break;

                    case NavigationItem.NavigationType.VolumeDown:
                        MediaVolume(-0.05);
                        break;

                    case NavigationItem.NavigationType.VolumeUp:
                        MediaVolume(0.05);
                        break;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
            finally
            {
                NavigationDisplayDateTime = DateTime.Now; //Reset
            }
        }

        private void MediaVolume(double Change)
        {
            try
            {
                this.Volume += Change;

                ////Windows Volume if at 100% or 5%
                if (SettingGetBool("WindowsVolume") == true)
                {
                    if (this.Volume >= 1.0)
                    {
                        //Raise Windows volume
                        WindowsVolume(0.05);
                    }
                    else if (this.Volume < 0.05)
                    {
                        //Lower Windows volume
                        WindowsVolume(-0.05);
                        this.Volume = 0.05;
                    }
                }


                byte bVolume = Convert.ToByte(this.Volume * 100);

                mMainPage.BediaMedia.Volume = this.Volume;

                MediaStatusBubbles(bVolume);
                TitlebarHide("volume");
                SettingSave("Volume", mdVolume);


                if (StatusTimer != null)
                {
                    StatusTimer.Dispose();
                    StatusTimer = null;
                }

                StatusTimer = new Timer(StatusHide, null, Convert.ToInt32(TimeSpan.FromSeconds(8).TotalMilliseconds), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void WindowsVolume(double volumeChange)
        {
            try
            {
                float fValue = VolumeControl.ChangeVolumeToLevel(volumeChange);

                byte bVolume = Convert.ToByte(fValue * 100);

                mediastatus.SetWindowsVolume(bVolume);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void StatusHide(object state)
        {
            try
            {
                await mMainPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => {
                        Storyboard sbFadeOut = new Storyboard();
                        DoubleAnimation anFadeOut = new DoubleAnimation();
                        anFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                        anFadeOut.From = mediastatus.Opacity;
                        anFadeOut.To = 0.0;
                        Storyboard.SetTarget(anFadeOut, mediastatus);
                        Storyboard.SetTargetProperty(anFadeOut, "(UIElement.Opacity)");
                        sbFadeOut.Children.Add(anFadeOut);
                        sbFadeOut.Begin();
                    });
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void XButton1()
        {
            try
            {
                if (this.IsPaused == true)
                {
                    MediaMenu(false);
                    mMainPage.BediaMedia.Play();
                    this.IsPaused = false;
                    this.IsPlaying = true;
                    return;
                }

                if (this.IsPlaying == true)
                {
                    if (BediaPlaylist.Count > 0)
                    {
                        //Play whatever is in the list
                        MediaMenu(false);
                    }
                    else
                    {
                        MediaPlay();
                    }
                }
                else
                {
                    //If there are any items in the list, just play
                    if (BediaPlaylist.Count > 0)
                    {
                        MediaPlay(new PastItem()); //add blank History info so to avoid history activating. 
                    }
                    else
                    {
                        BediaItem bediaitem = BediaMenuItems[miMenuCurrentPosition];
                        switch (bediaitem.MenuType)
                        {
                            case BediaItem.MenuTypes.Audio:
                            case BediaItem.MenuTypes.Video:
                            case BediaItem.MenuTypes.PodcastItem:
                            case BediaItem.MenuTypes.SpaceItem:
                            case BediaItem.MenuTypes.YouTubeVideo:
                            case BediaItem.MenuTypes.IPTVVideo:
                                BediaPlaylist.Add(bediaitem);
                                MediaPlay(new PastItem()); //add blank History info so to avoid history activating. 
                                break;

                            case BediaItem.MenuTypes.Library:
                            case BediaItem.MenuTypes.Folder:
                                MenuSelected(bediaitem);
                                break;

                            case BediaItem.MenuTypes.Playlist:
                                bool b = await PlaylistLoad(bediaitem);

                                foreach (BediaItem bi in BediaMenuItems)
                                {
                                    switch (bi.MenuType)
                                    {
                                        case BediaItem.MenuTypes.Audio:
                                        case BediaItem.MenuTypes.Video:
                                        case BediaItem.MenuTypes.Podcast:
                                            BediaPlaylist.Add(bi);
                                            break;
                                    }
                                }
                                MediaPlay(new PastItem());
                                break;
                        }
                        bediaitem = null;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void XButton2()
        {
            PageUpDown();
        }
        
        public void PointerReleased(PointerRoutedEventArgs e)
        {
            try
            {
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    PointerPoint ptrTouchUp = e.GetCurrentPoint(mMainPage);

                    Int16 iDiff = Convert.ToInt16(ptrTouchUp.Position.Y - TouchDown.Position.Y);


                    if (this.IsMenuDisplayed == true)
                    {
                        ScreenSaverReset();

                        if (iDiff > this.MenuSize || iDiff < (-this.MenuSize))
                        {
                            bool bFullPageScroll = FullPageScroll; //Keep what the current state is, reset it below
                            byte MenuCount = 3;

                            //- SCROLL -
                            if (iDiff > (this.MenuSize * MenuCount) || iDiff < (-(this.MenuSize * MenuCount)))
                            {
                                //User moves thier finger over a large area, so Full Page scroll
                                FullPageScroll = true;

                                if (iDiff < 0)
                                {
                                    MenuScrollUpPage();
                                }
                                else
                                {
                                    MenuScrollDownPage();
                                }
                            }
                            else
                            {
                                //Setting it to negative even if it is on via the mouse, so the user can make small moves even if it is set to Full Page
                                FullPageScroll = false;
                            }

                            FullPageScroll = bFullPageScroll; //Reset it back to what it was
                            return;
                        }


                        if (ptrTouchUp.Position.X > (mMainPage.ActualWidth / 2))
                        {
                            MenuBack();
                        }
                        else
                        {
                            Int16 iPosition = Convert.ToInt16(ptrTouchUp.Position.Y);
                      
                            foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                            {
                                var ttv = BediaMenu.TransformToVisual(mMainPage);
                                Point MenuCoordinates = ttv.TransformPoint(new Point(0, 0));

                                if ((iPosition >= MenuCoordinates.Y) && (iPosition <= (MenuCoordinates.Y + this.MenuSize)))
                                {
                                    MenuSelected(BediaMenu.Bediaitem);
                                    return;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (iDiff < this.MenuSize)
                        {
                            //MEDIA PLAYING
                            NavigationItem CurrentNavitaion = NavigationItems[miNavigationCurrentPosition];
                            NavigationItem.NavigationType Nav = CurrentNavitaion.Left;

                            if (ptrTouchUp.Position.X > (mMainPage.ActualWidth / 2))
                            {
                                Nav = CurrentNavitaion.Right;
                            }
                            NavigationSelected(Nav);
                        }
                        else
                        {
                            if (iDiff < 0)
                            {
                                NavigationDisplay(-1);
                            }
                            else
                            {
                                NavigationDisplay(1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void PointerPressed(PointerRoutedEventArgs e)
        {
            try
            {
                if (mbMenuLoadCompleted == false) { return; } //TODO: this seems to not have the desired effect
                
                if (mMainPage.ScreenSaver.Opacity == 1.0)
                {
                    ScreenSaverStop();
                    return;
                }
                else if (mMainPage.ScreenSaver.Opacity > 0.5)//Changed from 0.0 to give the user a chance to select something as the screen saver closes
                {
                    return; //Only show items if the screen saver is completely closed
                }

                
                CurrentPoint = e.GetCurrentPoint(mMainPage).Position; // Used for selecting Bedia Custom Menu Color (and maybe something else... someday)


                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    //See PointerReleased() for what happens when the user presses the app with their finger
                    TouchDown = e.GetCurrentPoint(mMainPage);
                    return;
                }

                //Check if media is playing, if so AND menu is not displayed then activate Media Menu Select
                if ((this.IsPlaying == true || this.IsPaused == true) && this.IsMenuDisplayed == false)
                {
                    NavigationSelect(e);
                }
                else
                {
                    if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                    {
                        var properties = e.GetCurrentPoint(mMainPage).Properties;
                        if (properties.IsLeftButtonPressed)
                        {
                            //LEFT
                            BediaItem bediaitem = BediaMenuItems[miMenuCurrentPosition];
                            MenuSelected(bediaitem);

                            bediaitem = null;
                        }
                        else if (properties.IsRightButtonPressed)
                        {
                            //RIGHT
                            MenuBack();
                        }
                        else if (properties.IsXButton1Pressed)
                        {
                            XButton1();
                        }
                        else if (properties.IsXButton2Pressed)
                        {
                            XButton2();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void MenuSelected(BediaItem bediaitem)
        {
            try
            {
                switch (bediaitem.MenuType)
                {
                    //Some items should not be added to the breadcrumbs
                    #region Non Bookmark Items
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                    case BediaItem.MenuTypes.YouTubeVideo:
                    case BediaItem.MenuTypes.IPTVVideo:
                    case BediaItem.MenuTypes.AddRemoveHomeFolder:
                    case BediaItem.MenuTypes.AddRemoveHomeFolderSelect:
                    case BediaItem.MenuTypes.EmptyQueue:
                    case BediaItem.MenuTypes.CloseMenu:
                    case BediaItem.MenuTypes.PastItem:
                    case BediaItem.MenuTypes.Play:
                    case BediaItem.MenuTypes.PodcastItem:
                    case BediaItem.MenuTypes.SelectAll:
                    case BediaItem.MenuTypes.SettingsAlbumFolderArt:
                    case BediaItem.MenuTypes.SettingsBubbles:
                    case BediaItem.MenuTypes.SettingsHistoryClear:
                    case BediaItem.MenuTypes.SettingsHistoryClearOK:
                    case BediaItem.MenuTypes.SettingsHistorySync:
                    case BediaItem.MenuTypes.SettingsHistorySave:
                    case BediaItem.MenuTypes.SettingsHistoryClearCancel:
                    case BediaItem.MenuTypes.SettingsIPTVFolder:
                    case BediaItem.MenuTypes.SettingsLateNight:
                    case BediaItem.MenuTypes.SettingsLibrary:
                    case BediaItem.MenuTypes.SettingsMenuColor:
                    case BediaItem.MenuTypes.SettingsMenuCustomColorSelected:
                    case BediaItem.MenuTypes.SettingsMenuFonts:
                    case BediaItem.MenuTypes.SettingsMenuFont:
                    case BediaItem.MenuTypes.SettingsMenuOptionItem:
                    case BediaItem.MenuTypes.SettingsMenuSpeed:
                    case BediaItem.MenuTypes.SettingsMonitorFolders:
                    case BediaItem.MenuTypes.SettingsMultipleMonitorsItem:
                    case BediaItem.MenuTypes.SettingsNavigationSet:
                    case BediaItem.MenuTypes.SettingsNavigationIcons:
                    case BediaItem.MenuTypes.SettingsPartyBubbles:
                    case BediaItem.MenuTypes.SettingsScreenSaverClock:
                    case BediaItem.MenuTypes.SettingsScreenSaverComments:
                    case BediaItem.MenuTypes.SettingsSpeechRecognition:
                    case BediaItem.MenuTypes.SettingsStartWithWindows:
                    case BediaItem.MenuTypes.SettingsTitlebarClock:
                    case BediaItem.MenuTypes.SettingsVolumePercentage:
                    case BediaItem.MenuTypes.SettingsAdvanced:
                    case BediaItem.MenuTypes.SettingsVisualizationItem:
                    case BediaItem.MenuTypes.SettingsVisualizationsMultipleMonitor:
                    case BediaItem.MenuTypes.SettingsWindowsVolume:
                    case BediaItem.MenuTypes.SpaceItem:
                    case BediaItem.MenuTypes.SpaceISSLocationItem:
                    case BediaItem.MenuTypes.StretchVideo:
                    case BediaItem.MenuTypes.TheatreTimesItem:
                    case BediaItem.MenuTypes.TransitItem:
                    case BediaItem.MenuTypes.WeatherItem:
                    case BediaItem.MenuTypes.ExitAppNo:
                    case BediaItem.MenuTypes.ExitAppYes:
                        #endregion
                        break;

                    default:
                        MenuBreadrumbs.Add(bediaitem);
                        break;
                }


                switch (bediaitem.MenuType)
                {
                    #region Base Menus
                    case BediaItem.MenuTypes.Folder:
                        MenuProcessing("Folder", bediaitem.BediaFolder.DisplayName, "Folder");
                        break;

                    case BediaItem.MenuTypes.Libraries:
                        MenuProcessing("LoadLibraries", "Libraries", "Library");
                        break;

                    case BediaItem.MenuTypes.Library:
                        MenuProcessing("LoadLibrary", bediaitem.Title, "Folder");
                        break;
                        
                    case BediaItem.MenuTypes.Podcasts:
                        MenuProcessing("LoadPodcasts", "Podcasts", "Podcast");
                        break;

                    case BediaItem.MenuTypes.Podcast:
                        MenuProcessing("LoadPodcast", bediaitem.Title, "Podcast");
                        break;

                    case BediaItem.MenuTypes.PastHome:
                        MenuProcessing("Past", "Past", "History");
                        break;

                    case BediaItem.MenuTypes.PastList:
                        HackBediaItem = bediaitem;
                        DateTime dtPastDate = Convert.ToDateTime(bediaitem.Value);
                        MenuProcessing("HistoryList", dtPastDate.ToString("MMMM d, yyyy"), "History");
                        break;

                    case BediaItem.MenuTypes.PastAlphabetical:
                        MenuProcessing("PastAlphabetical", "Past", "History");
                        break;

                    case BediaItem.MenuTypes.PastFolder:
                        HackBediaItem = bediaitem;
                        DateTime dtMonthYear = Convert.ToDateTime(bediaitem.Value);
                        MenuProcessing("PastFolder", dtMonthYear.ToString("MMMM yyyy"), "History");
                        break;

                    case BediaItem.MenuTypes.PastItem:
                        PastItem hi = new PastItem();
                        hi.Seconds = Convert.ToDouble(bediaitem.Value);
                        MediaPlay(hi);
                        break;

                    case BediaItem.MenuTypes.PausedHome:
                        MenuProcessing("PausedHome", "Paused", "Pause");
                        break;

                    case BediaItem.MenuTypes.PausedAlphabetical:
                        MenuProcessing("PausedAlphabetical", "Paused", "Pause");
                        break;

                    case BediaItem.MenuTypes.PausedFolder:
                        HackBediaItem = bediaitem;
                        DateTime dtPausedMonthYear = Convert.ToDateTime(bediaitem.Value);
                        MenuProcessing("PausedFolder", dtPausedMonthYear.ToString("MMMM yyyy"), "Pause");
                        break;

                    case BediaItem.MenuTypes.Services:
                        MenuProcessing("Services", "Services", "Services");
                        break;

                    case BediaItem.MenuTypes.Play:
                        MediaPlay();
                        break;

                    case BediaItem.MenuTypes.SelectAll:
                        SelectAll();
                        break;

                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                    case BediaItem.MenuTypes.SpaceItem:
                    case BediaItem.MenuTypes.IPTVVideo:
                    case BediaItem.MenuTypes.PodcastItem:
                    case BediaItem.MenuTypes.YouTubeVideo:
                        MediaSelected(bediaitem);
                        break;

                    case BediaItem.MenuTypes.YouTubePlaylist:
                        HackBediaItem = bediaitem;
                        MenuProcessing("YouTubePlaylist", bediaitem.Title, "YouTube");
                        break;

                    case BediaItem.MenuTypes.PlaylistCurrent:
                        MenuProcessing("Queue", "Queue", "Playlist");
                        break;

                    case BediaItem.MenuTypes.Playlist:
                        MenuProcessing("LoadPlaylist", System.IO.Path.GetFileNameWithoutExtension(bediaitem.BediaFile.Name), "Playlist");
                        break;
                    #endregion

                    #region Services
                    case BediaItem.MenuTypes.IPTVHome:
                        MenuProcessing("IPTV", "IPTV", "IPTV");
                        break;

                    case BediaItem.MenuTypes.IPTVPlaylist:
                        HackBediaItem = bediaitem;
                        MenuProcessing("IPTVPlaylist", bediaitem.Title, "IPTV");
                        break;

                    case BediaItem.MenuTypes.SpaceHome:
                        MenuProcessing("SpaceHome", "Space", "Space");
                        break;

                    case BediaItem.MenuTypes.SpaceISSLocationHome:
                        ISSMapSetup();
                        break;

                    case BediaItem.MenuTypes.TheatreTimesHome:
                        MenuProcessing("MoviesDetais", "Theatre Movie Times", "MovieTimes");
                        break;

                    case BediaItem.MenuTypes.TheatreTimesMovie:
                        HackBediaItem = bediaitem;
                        MenuProcessing("TheatresDetails", bediaitem.Title, "MovieTimes");
                        break;

                    case BediaItem.MenuTypes.TheatreTimesTheatre:
                        string sMovie = HackBediaItem.Title;
                        HackBediaItem = bediaitem;
                        HackBediaItem.Value = sMovie;
                        MenuProcessing("TheatreDateTimes", bediaitem.Title, "MovieTimes");
                        break;

                    case BediaItem.MenuTypes.TransitHome:
                        MenuProcessing("LoadTransit", "Transit", "Transit");
                        break;

                    case BediaItem.MenuTypes.WeatherHome:
                        MenuProcessing("LoadWeather", "Weather", "Weather");
                        break;

                    case BediaItem.MenuTypes.YouTubeHome:
                        MenuProcessing("YouTubeHome", "YouTube", "YouTube");
                        break;

                    case BediaItem.MenuTypes.YouTubeFolder:
                        HackBediaItem = bediaitem;
                        MenuProcessing("YouTubeFolder", bediaitem.Title, "YouTube");
                        break;

                    #endregion

                    #region Settings
                    case BediaItem.MenuTypes.SettingsHome:
                        MenuProcessing("LoadSettings", "Settings", "Settings");
                        break;

                    case BediaItem.MenuTypes.AddRemoveHomeFolders:
                        MenuProcessing("LoadAddRemoveHomeFolders", "Change Home Folders", "Folder");
                        break;

                    case BediaItem.MenuTypes.SettingsLibrary:
                        string sHomePageLibraries = SettingGet("HomePageLibraries");

                        if (bediaitem.Selected == true)
                        {
                            sHomePageLibraries = sHomePageLibraries.Replace(bediaitem.Value + ",", "");
                        }
                        else
                        {
                            //Only add it if it's not already added.
                            if (sHomePageLibraries.IndexOf(bediaitem.Value) == -1)
                            {
                                sHomePageLibraries += bediaitem.Value + ",";
                            }
                        }
                        SettingSave("HomePageLibraries", sHomePageLibraries);

                        bediaitem.Selected = !bediaitem.Selected;
                        MenuSelect(bediaitem.Value, false);
                        break;

                    case BediaItem.MenuTypes.AddRemoveHomeFolder:
                        FolderPicker fp = new FolderPicker();

                        fp.ViewMode = PickerViewMode.List;
                        fp.SuggestedStartLocation = PickerLocationId.Desktop;
                        fp.FileTypeFilter.Add(".txt");
                        StorageFolder SelectedFolder = await fp.PickSingleFolderAsync();
                        if (SelectedFolder != null)
                        {
                            var sHomeToken = StorageApplicationPermissions.FutureAccessList.Add(SelectedFolder);

                            SettingSave("HomeMenus", SettingGet("HomeMenus") + sHomeToken + ",");
                            MenuProcessing("LoadAddRemoveHomeFolders", "Settings", "Settings");
                            SelectedFolder = null;
                        }
                        fp = null;
                        break;

                    case BediaItem.MenuTypes.AddRemoveHomeFolderSelect:
                        string sHomeMenus = SettingGet("HomeMenus");

                        if (bediaitem.Selected == true)
                        {
                            sHomeMenus = sHomeMenus.Replace(bediaitem.Value + ",", "");
                        }
                        else
                        {
                            //Only add it if it's not already added.
                            if (sHomeMenus.IndexOf(bediaitem.BediaFolder.Path) == -1)
                            {
                                sHomeMenus += bediaitem.Value + ",";
                            }
                        }
                        SettingSave("HomeMenus", sHomeMenus);

                        bediaitem.Selected = !bediaitem.Selected;
                        MenuSelect(bediaitem.Value, false);

                        MonitorFoldersStart();
                        break;

                    case BediaItem.MenuTypes.SettingsVisualizations:
                        MenuProcessing("AudioVisualizations", "Audio Visualizations", "Settings");
                        break;
                        
                    case BediaItem.MenuTypes.SettingsVisualizationItem:
                        string sAudioVisualizations = SettingGet("AudioVisualizations");

                        if (bediaitem.Selected == true)
                        {
                            sAudioVisualizations = sAudioVisualizations.Replace(bediaitem.Value + ",", "");
                        }
                        else
                        {
                            //Only add it if it's not already added.
                            if (sAudioVisualizations.IndexOf(bediaitem.Value) == -1)
                            {
                                sAudioVisualizations += bediaitem.Value + ",";
                            }
                        }
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("AudioVisualizations", sAudioVisualizations);
                        MenuSelect(bediaitem.Value, false);
                        break;

                    case BediaItem.MenuTypes.SettingsBubbles:
                        bediaitem.Selected = !bediaitem.Selected;
                        this.Bubbles = bediaitem.Selected;
                        MenuSelect(BediaItem.MenuTypes.SettingsBubbles, false);

                        if (bediaitem.Selected == false)
                        {
                            BubblesStop();
                        }
                        else
                        {
                            BubblesLoad();
                        }
                        SettingSave("Bubbles", bediaitem.Selected);
                        break;
                        
                    case BediaItem.MenuTypes.SettingsHistoryClear:
                        MenuProcessing("ClearHistory", "Clear Past?", "History");
                        break;

                    case BediaItem.MenuTypes.SettingsHistoryClearOK:
                        PastClear();
                        MenuProcessing("LoadSettings", "Settings", "Settings");
                        break;

                    case BediaItem.MenuTypes.SettingsHistoryClearCancel:
                        MenuProcessing("LoadSettings", "Settings", "Settings");
                        break;

                    case BediaItem.MenuTypes.SettingsHistorySync:
                        //TODO:
                        break;

                    case BediaItem.MenuTypes.SettingsHistorySave:
                        bediaitem.Selected = !bediaitem.Selected;
                        this.SavePast = bediaitem.Selected;
                        MenuSelect(BediaItem.MenuTypes.SettingsHistorySave, false);
                        SettingSave("PastSave", bediaitem.Selected);
                        break;

                    case BediaItem.MenuTypes.SettingsIPTVFolder:
                        BediaIO bediaIO = new BediaIO();
                        bool bIPTVFolder = await bediaIO.FolderExists(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);

                        if (bIPTVFolder == true)
                        {
                            StorageFolder IPTVFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\" + msIPTVPath);
                            await Launcher.LaunchFolderAsync(IPTVFolder);
                        }
                        bediaIO = null;
                        break;

                    case BediaItem.MenuTypes.SettingsLateNight:
                        bediaitem.Selected = !bediaitem.Selected;
                        MenuSelect(BediaItem.MenuTypes.SettingsLateNight, false);
                        SettingSave("LateNight", bediaitem.Selected);
                        SetBackground(mMainPage.BediaBase);
                        SetBackground(mMainPage.MediaOverlay);
                        BubblesStop();
                        BubblesLoad();
                        break;

                    case BediaItem.MenuTypes.SettingsMediaDisplayInfo:
                        bediaitem.Selected = !bediaitem.Selected;
                        MenuSelect(BediaItem.MenuTypes.SettingsMediaDisplayInfo, false);
                        SettingSave("MediaDisplayInfo", bediaitem.Selected);
                        break;

                    case BediaItem.MenuTypes.SettingsMenuColors:
                        MenuProcessing("MenuColors", "Menu Colors", "Menu");
                        break;

                    case BediaItem.MenuTypes.SettingsMenuColor:
                        BediaBaseColor = (SolidColorBrush)bediaitem.ValueObject; 
                        SettingSave("BediaBaseColor", BediaBaseColor);
                        
                        bediaTitlebar.SetColors(BediaBaseColor, BediaHighlightColor);
                        mediastatus.SetColors(BediaBaseColor, BediaHighlightColor);
                        SetBackground(mMainPage.BediaBase);
                        SetBackground(mMainPage.MediaOverlay);
                        BubblesStop();
                        BubblesLoad();

                        MenuProcessing("MenuColors", "Menu Colors", "Menu");
                        break;

                    case BediaItem.MenuTypes.SettingsMenuCustomColorSelect:
                        BediaColorsSetup();
                        break;

                    case BediaItem.MenuTypes.SettingsMenuCustomColorSelected:
                        BediaColorSelected();
                        break;

                    case BediaItem.MenuTypes.SettingsOptionalMenus:
                        MenuProcessing("HomePageOptions", "Menu Options", "Menu");
                        break;

                    case BediaItem.MenuTypes.SettingsMenuOptionItem:
                        string sMenuOptions = SettingGet("HomePageMenuOptions");
                        bediaitem.Selected = !bediaitem.Selected;

                        if (bediaitem.Selected == false)
                        {
                            sMenuOptions = sMenuOptions.Replace(bediaitem.Value + ",", "");
                        }
                        else
                        {
                            sMenuOptions += bediaitem.Value + ",";
                        }
                        MenuSelect(bediaitem.Value, false);
                        SettingSave("HomePageMenuOptions", sMenuOptions);
                        break;

                    case BediaItem.MenuTypes.SettingsNavigationSets:
                        MenuProcessing("LoadNavigationSets", "Navigation Sets", "Navigator");
                        break;

                    case BediaItem.MenuTypes.SettingsPartyBubbles:
                        bediaitem.Selected = !bediaitem.Selected;
                        this.BubblesParty = bediaitem.Selected;
                        SettingSave("BubblesParty", bediaitem.Selected);
                        BubblesStop();
                        BubblesLoad();
                        MenuSelect(BediaItem.MenuTypes.SettingsPartyBubbles, false);
                        break;

                    case BediaItem.MenuTypes.SettingsScreenSaverClock:
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("ScreenSaverClock", bediaitem.Selected);
                        MenuSelect(BediaItem.MenuTypes.SettingsScreenSaverComments, false);
                        break;

                    case BediaItem.MenuTypes.SettingsScreenSaverComments:
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("ScreenSaverComments", bediaitem.Selected);
                        MenuSelect(BediaItem.MenuTypes.SettingsScreenSaverComments, false);
                        break;

                    case BediaItem.MenuTypes.StretchVideo:
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("StretchVideo", bediaitem.Selected);
                        mbStretchVideo = bediaitem.Selected;

                        if (mbStretchVideo == true)
                        {
                            mMainPage.BediaMedia.Stretch = Stretch.Fill;
                        }
                        else
                        {
                            mMainPage.BediaMedia.Stretch = Stretch.Uniform; 
                        }
                        MenuSelect(BediaItem.MenuTypes.StretchVideo, false);
                        break;

                    case BediaItem.MenuTypes.SettingsTitlebarClock:
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("TitlebarClock", bediaitem.Selected);
                        SetTitlebarClock();
                        MenuSelect(BediaItem.MenuTypes.SettingsTitlebarClock, false);
                        break;

                    case BediaItem.MenuTypes.SettingsWindowsVolume:
                        bediaitem.Selected = !bediaitem.Selected;
                        SettingSave("WindowsVolume", bediaitem.Selected);
                        MenuSelect(BediaItem.MenuTypes.SettingsWindowsVolume, false);
                        break;

                    case BediaItem.MenuTypes.About:
                        MenuProcessing("LoadAbout", "About " + AppName(), "Info");
                        break;
                    // -- END OF SETTINGS --
                    #endregion

                    #region Additional Menu Items
                    case BediaItem.MenuTypes.EmptyQueue:
                        bool bWait = await EmptyQueue();
                        break;

                    case BediaItem.MenuTypes.CloseMenu:
                        MediaMenu(false);
                        break;

                    case BediaItem.MenuTypes.ExitApp:
                        if (IsPlaying == true)
                        {
                            MenuProcessing("ExitApp", "Exit " + msAppName + "?", "Question");
                        }
                        else
                        {
                            ExitApp();
                        }
                        break;

                    case BediaItem.MenuTypes.ExitAppNo:
                        MenuProcessing("LoadHome", this.HomePageTitle, "Home");
                        break;

                    case BediaItem.MenuTypes.ExitAppYes:
                        ExitApp();
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void MenuBack()
        {
            try
            {
                Window.Current.CoreWindow.PointerCursor = null; //Hide mouse cursor.  This is because in some instances (Customer Bedia Menu Colors) there is a need to show the mouse)

                if (MenuBreadrumbs.Count > 1)
                {
                    BookmarkItem = MenuBreadrumbs[(MenuBreadrumbs.Count - 1)];
                    BediaItem bediaitem = MenuBreadrumbs[(MenuBreadrumbs.Count - 2)];

                    MenuBreadrumbs.RemoveAt(MenuBreadrumbs.Count - 1);
                    MenuBreadrumbs.RemoveAt(MenuBreadrumbs.Count - 1); //The current menu is about to be added back in MenuSelect
                    MenuSelected(bediaitem);

                    bediaitem = null;
                }
                else if (MenuBreadrumbs.Count != 0)  //No need to go back if we are at Home
                {
                    BookmarkItem = null;
                    MenuProcessing("LoadHome", this.HomePageTitle, "Home");
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MenuMove(BediaMenuUI BediaMenu, Int16 From, Int16 To)
        {
            try
            {
                Storyboard sbMove = new Storyboard();
                DoubleAnimation animMove = new DoubleAnimation();
                CompositeTransform compTransform = new CompositeTransform();

                
                animMove.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                animMove.From = From;
                animMove.To = To;

      
                var ease = new ExponentialEase();
                ease.EasingMode = EasingMode.EaseInOut; 
                animMove.EasingFunction = ease;

                Storyboard.SetTarget(animMove, BediaMenu);
                Storyboard.SetTargetProperty(animMove, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
                sbMove.Children.Add(animMove);
                
                BediaMenu.RenderTransform = compTransform;
                
                sbMove.Completed +=
                    (sndr, evnts) =>
                    {
                        if (To >= mMainPage.ActualHeight)
                        {
                            mMainPage.BediaMenus.Children.Remove(BediaMenu);
                        }
                        else
                        {
                            BediaMenu.Top = To;
                        }
                    };
                sbMove.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void PointerWheelChanged(PointerRoutedEventArgs e)
        {
            try
            {
                //The screen saver is running, no need to run this code.  
                //Was > 0.0, but changed to allow movement while the screen saver closes
                if (mMainPage.ScreenSaver.Opacity == 1.0) { return; } 

                var PointerProperties = e.GetCurrentPoint(mMainPage).Properties;

                if ((this.IsPlaying == true || this.IsPaused == true) && this.IsMenuDisplayed == false)
                {
                    //Media Navigation
                    if (PointerProperties.MouseWheelDelta < 0)
                    {
                        NavigationDisplay(-1);
                    }
                    else
                    {
                        NavigationDisplay(1);
                    }
                }
                else
                {
                    //To avoid adjusting the values while the Menus are still being animated
                    if (mbProcessCompleted == false) { return; }

                    if (mbMenuLoadCompleted == false) { return; }

                    ScreenSaverReset();

                    //Menu Navigation
                    if (PointerProperties.MouseWheelDelta > 0)
                    {
                        if (FullPageScroll == true)
                        {
                            MenuScrollDownPage();
                        }
                        else
                        {
                            MenuScrollDown();
                        }
                    }
                    else
                    {
                        if (FullPageScroll == true)
                        {
                            MenuScrollUpPage();
                        }
                        else
                        {
                            MenuScrollUp();
                        }
                    }

                    BediaTransitions bediaTransitions = new BediaTransitions();
                    bediaTransitions.TextChange(bediaTitlebar.TitlebarCountX, (miMenuCurrentPosition + 1).ToString());
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MenuResetTransitions()
        {
            try
            {
                foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                {
                    if (BediaMenu.Transitions != null)
                    {
                        BediaMenu.Transitions.Clear();
                    }

                    TransitionCollection MenuTransitCollection = new TransitionCollection();
                    RepositionThemeTransition MenuRepositionTransition = new RepositionThemeTransition();

                    MenuTransitCollection.Add(MenuRepositionTransition);
                    BediaMenu.Transitions = MenuTransitCollection;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void MenuScrollUp()
        {
            try
            {
                mbProcessCompleted = false;

                miMenuCurrentPosition += 1;

                if (miMenuCurrentPosition >= BediaMenuItems.Count)
                {
                    miMenuCurrentPosition = 0;
                }

                MenuResetTransitions();

                BediaItem bediaitem = await MenuLast(); //HAVE to get this before removing
                mMainPage.BediaMenus.Children.RemoveAt(0);
                bool b = await MenuLoad(bediaitem);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
            finally
            {
                mbProcessCompleted = true;
            }
        }

        private void MenuScrollDown()
        {
            try
            {
                mbProcessCompleted = false;

                miMenuCurrentPosition -= 1;

                if (miMenuCurrentPosition < 0)
                {
                    miMenuCurrentPosition = Convert.ToInt16(BediaMenuItems.Count - 1);
                }

                MenuResetTransitions();

                MenuInsert(BediaMenuItems[miMenuCurrentPosition], 0);

                mMainPage.BediaMenus.Children.RemoveAt(mMainPage.BediaMenus.Children.Count - 1);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
            finally
            {
                mbProcessCompleted = true;
            }
        }

        private async void MenuScrollUpPage()
        {
            try
            {
                byte iMenuCount = await MenuSpaceAvailable();

                if (BediaMenuItems.Count <= iMenuCount)
                {
                    //fewer menus than space, just bump one up
                    MenuScrollUp();
                }
                else
                {
                    MenusClear();

                    BediaItem biMenu;
                    Int16 iTop = 0;
                    Int16 iPosition = 0;
                    bool bAwait = false;

                    miMenuCurrentPosition += iMenuCount;

                    if (miMenuCurrentPosition >= BediaMenuItems.Count)
                    {
                        miMenuCurrentPosition = Convert.ToInt16(miMenuCurrentPosition - BediaMenuItems.Count);
                    }
                    iPosition = miMenuCurrentPosition;

                    for (Int16 iMenu = 0; iMenu < iMenuCount; iMenu++)
                    {
                        biMenu = BediaMenuItems[(iMenu + iPosition)];
                        bAwait = await MenuInsert(biMenu, iTop);

                        if (iTop == 0)
                        {
                            iTop = this.mbMenuSize;
                        }
                        else
                        {
                            iTop += this.mbMenuSize;
                        }

                        //if it passes the number of BediaMenuItems
                        if ((iMenu + iPosition + 1) >= BediaMenuItems.Count)
                        {
                            iPosition = Convert.ToInt16(-iMenu);
                        }
                    }
                    biMenu = null;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void MenuScrollDownPage()
        {
            try
            {
                byte iMenuCount = await MenuSpaceAvailable();

                if (BediaMenuItems.Count <= iMenuCount)
                {
                    //fewer menus than space, just bump one down
                    MenuScrollDown();
                }
                else
                {
                    MenusClear();

                    BediaItem biMenu;
                    Int16 iTop = 0;
                    Int16 iPosition = 0;
                    bool bAwait = false;

                    miMenuCurrentPosition -= iMenuCount;

                    if (miMenuCurrentPosition < 0)
                    {
                        miMenuCurrentPosition = Convert.ToInt16(miMenuCurrentPosition + BediaMenuItems.Count);
                    }
                    iPosition = miMenuCurrentPosition;

                    for (Int16 iMenu = 0; iMenu < iMenuCount; iMenu++)
                    {
                        biMenu = BediaMenuItems[(iMenu + iPosition)];
                        bAwait = await MenuInsert(biMenu, iTop);

                        if (iTop == 0)
                        {
                            iTop = this.MenuSize;
                        }
                        else
                        {
                            iTop += this.MenuSize;
                        }

                        //if it passes the number of BediaMenuItems
                        if ((iMenu + iPosition + 1) >= BediaMenuItems.Count)
                        {
                            iPosition = Convert.ToInt16(-iMenu);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private List<BediaMenuUI> MenusUISorted(bool Descending = false)
        {
            List<BediaMenuUI> BediaMenus = new List<BediaMenuUI>();

            try
            {
                //Vector offset;
                BediaMenus = mMainPage.BediaMenus.Children.OfType<BediaMenuUI>().Cast<BediaMenuUI>().ToList();

                foreach (BediaMenuUI BediaMenu in BediaMenus)
                {
                    var ttv = BediaMenu.TransformToVisual(Window.Current.Content);
                    Point screenCoords = ttv.TransformPoint(new Point(0, 0));

                    BediaMenu.Top = Convert.ToInt16(screenCoords.Y);
                }

                //Then ORDER by Top
                if (Descending == false)
                {
                    BediaMenus = BediaMenus.OrderBy(x => x.Top).ToList();
                }
                else
                {
                    BediaMenus = BediaMenus.OrderByDescending(x => x.Top).ToList();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return BediaMenus;
        }

        private async Task<BediaItem> MenuLast()
        {
            BediaItem biReturn = new BediaItem("", BediaItem.MenuTypes.Info);

            try
            {
                Int16 iMenuItem = 0;
                Int16 iMenuUICount = await MenuSpaceAvailable();

                if (mMainPage.BediaMenus.Children.Count < iMenuUICount)
                {
                    iMenuItem = Convert.ToInt16(miMenuCurrentPosition - 1);

                    if (iMenuItem < 0)
                    {
                        iMenuItem = Convert.ToInt16(BediaMenuItems.Count - 1);
                    }
                }
                else
                {
                    iMenuItem = Convert.ToInt16(miMenuCurrentPosition + (iMenuUICount - 1));

                    if (iMenuItem >= BediaMenuItems.Count)
                    {
                        iMenuItem = Convert.ToInt16(iMenuItem - BediaMenuItems.Count);
                    }
                }


                biReturn = BediaMenuItems[iMenuItem];
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return biReturn;
        }

        private void MenusClear()
        {
            try
            {
                MenuResetTransitions(); 
                
                if (mMainPage.BediaMenus.Children.Count > 0)
                {
                    byte bCount = Convert.ToByte(mMainPage.BediaMenus.Children.Count - 1);


                    for (Int16 b = bCount; b > -1; b--)
                    {
                        BediaMenuUI bi = (BediaMenuUI)mMainPage.BediaMenus.Children[b];
                        
                        mMainPage.BediaMenus.Children.Remove(bi);
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MenuUnload(BediaMenuUI BediaMenu)
        {
            try
            {
                mMainPage.BediaMenus.Children.Remove(BediaMenu);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<bool> MenuLoad()
        {
            try
            {
                if (mbProcessCompleted == false)
                {
                    mbProcessCompleted = true;
                }
                else
                {
                    byte bMenuCount = 0;
                    bool bAwait = false;
                    BediaItem biItem;
                    bool bBookmark = false;
                    byte bVisualMenus = await MenuSpaceAvailable(); 


                    // - In case of empty folder
                    if (BediaMenuItems.Count < 1)
                    {
                        BediaItem biEmpty = new BediaItem("There was nothing found here.", BediaItem.MenuTypes.Info);
                        BediaMenuItems.Add(biEmpty);
                    }

                    //TODO: maybe this should be moved to MenusCreate();
                    if (BediaPlaylist.Count > 0)
                    {
                        if (IsPlaying == true)
                        {
                            biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.EmptyQueue));
                            if (biItem == null)
                            {
                                biItem = new BediaItem("Empty Queue", BediaItem.MenuTypes.EmptyQueue);
                                BediaMenuItems.Add(biItem);
                            }

                            biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.CloseMenu));
                            if (biItem == null)
                            {
                                BediaItem biCloseMenu = new BediaItem("Close Menu", BediaItem.MenuTypes.CloseMenu);
                                BediaMenuItems.Add(biCloseMenu);
                            }
                        }
                        else
                        {
                            biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.Play));
                            if (biItem == null)
                            {
                                biItem = new BediaItem("Play", BediaItem.MenuTypes.Play);
                                BediaMenuItems.Add(biItem);
                            }

                            biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.EmptyQueue));
                            if (biItem == null)
                            {
                                biItem = new BediaItem("Empty Queue", BediaItem.MenuTypes.EmptyQueue);
                                BediaMenuItems.Add(biItem);
                            }
                        }
                    }


                    // - Find the bookmarked item - 
                    if (BookmarkItem != null)
                    {
                        bool bFound = false;
                        Int16 iCount = 0;
                        Int16 iMenuUICount = await MenuSpaceAvailable();

                        foreach (BediaItem bediaitem in BediaMenuItems)
                        {
                            if (bFound == false)
                            {
                                bFound = Compare(bediaitem, BookmarkItem);
                            }

                            if (bFound == true)
                            {
                                miMenuCurrentPosition = iCount; //Only the first time through this loop

                                bBookmark = true;
                                
                                bAwait = await MenuLoad(bediaitem);

                                bMenuCount += 1;
                                if (bMenuCount >= bVisualMenus || bMenuCount >= iMenuUICount)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //This is so that miMenuCurrentPosition does not get affected after the first find
                                iCount += 1;
                            }
                        }


                        //If there is still room, start back at the top.
                        if (bMenuCount < bVisualMenus)
                        {
                            foreach (BediaItem bediaitem in BediaMenuItems)
                            {
                                //The issue is here, if there are less menus than space for them
                                if ((bMenuCount >= bVisualMenus) || (bMenuCount >= BediaMenuItems.Count))
                                {
                                    break;
                                }

                                //This needs to happen after the above check
                                bAwait = await MenuLoad(bediaitem);
                                bMenuCount += 1;
                            }
                        }
                    }
                    else
                    {
                        
                        //If there was no item bookmarked.  Just in case (Home Page)
                        foreach (BediaItem bediaitem in BediaMenuItems)
                        {
                            bAwait = await MenuLoad(bediaitem);
                            bMenuCount += 1;
                            if (bMenuCount >= bVisualMenus)
                            {
                                break;
                            }
                        }
                    }

                    mbMenusCount = bMenuCount; //See MenuLoadCompleted

                    SetCount(Convert.ToInt16(BediaMenuItems.Count));

                    if (bBookmark == true)
                    {
                        BediaTransitions bediaTransitions = new BediaTransitions();
                        await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            bediaTransitions.TextChange(bediaTitlebar.TitlebarCountX, (miMenuCurrentPosition + 1).ToString());
                        });
                    }

                    ScreenSaverReset();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
            finally
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    bediaTitlebar.BediaProgress.IsActive = false;
                });
            }

            return true;
        }

        private async Task<BediaMenuUI> MenuUICreate(BediaItem bediaitem)
        {
            BediaMenuUI uiMenu = new BediaMenuUI(IconByMenuType(bediaitem.MenuType),bediaitem, bediaitem.Title, MenuFontSize, this.MenuSize);

            try
            {
                uiMenu.Width = mMainPage.ActualWidth;
                uiMenu.Height = this.MenuSize;
                uiMenu.Opacity = 1.0;

                if (bediaitem.Selected == true)
                {
                    uiMenu.SetIcon(IconByNameSmall("Selected"));
                }
               

                //After History, in case it has history too, in this we only want to show that it is playing
                if (bediaitem.BediaFile != null)
                {
                    if (IsMediaPlaying(bediaitem.BediaFile) == true)
                    {
                        uiMenu.SetIcon(IconByNameSmall("PlayInverted"), false, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return uiMenu;
        }

        private async void PastSet(BediaItem bediaitem)
        {
            try
            {
                if (this.SavePast == false ) { return; }
                
                if (bediaitem.MenuType == BediaItem.MenuTypes.Audio || bediaitem.MenuType == BediaItem.MenuTypes.Video)
                {
                    PastItem hi = await PastItemGet(bediaitem.BediaFile);

                    if (hi != null)
                    {
                        bool bFound = false;

                        foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                        {
                            bFound = Compare(BediaMenu.Bediaitem, bediaitem);

                            if (bFound == true)
                            {
                                BediaMenu.Bediaitem.HistoryInfo = hi;
                                if (hi.Seconds != 0)
                                {
                                    //This media was paused during playback
                                    BediaMenu.SetIcon(IconByNameSmall("PauseInverted"));
                                }
                                else
                                {
                                    //This media was completely played
                                    BediaMenu.SetIcon(IconByNameSmall("PlayInverted"));
                                }
                                break;
                            }
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<byte> MenuSpaceAvailable()
        {
            byte bReturn = 0;

            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bReturn = Convert.ToByte(Math.Floor((mMainPage.ActualHeight - this.MenuSize) / this.MenuSize));
            });

            return bReturn;    
        }

        private async Task<bool> MenuLoad(BediaItem bediaitem)
        {
            try
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    
                    TransitionCollection MenuTransitCollection = new TransitionCollection();
                    EntranceThemeTransition MenuEntranceTransition = new EntranceThemeTransition();
                    MenuEntranceTransition.FromVerticalOffset = mMainPage.ActualHeight;
                    MenuTransitCollection.Add(MenuEntranceTransition);


                    BediaMenuUI uiMenu = await MenuUICreate(bediaitem);
                    
                    uiMenu.Transitions = MenuTransitCollection;
                    mMainPage.BediaMenus.Children.Add(uiMenu);
                    
                    PastSet(bediaitem); //After the load event
                });
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private async Task<bool> MenuInsert(BediaItem bediaitem, Int16 Position)
        {
            try
            {
                BediaMenuUI BediaMenuUI = await MenuUICreate(bediaitem);
                TransitionCollection MenuInsertTransitCollection = new TransitionCollection();
                PopupThemeTransition MenuInsertTransition = new PopupThemeTransition();


                MenuInsertTransitCollection.Add(MenuInsertTransition);
                BediaMenuUI.Transitions = MenuInsertTransitCollection;
                
                mMainPage.BediaMenus.Children.Insert(Position, BediaMenuUI);

                PastSet(bediaitem); //After the load event
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private void MenuRemove(BediaItem bediaitem)
        {
            try
            {
                switch (bediaitem.MenuType)
                {
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                        BediaPlaylist.RemoveAll(e => (e.BediaFile.Path == bediaitem.BediaFile.Path));
                        break;

                    case BediaItem.MenuTypes.YouTubeVideo:
                        BediaPlaylist.RemoveAll(e => (e.Value == bediaitem.Value));
                        break;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void HideExtraLayers()
        {
            try
            {
                Storyboard sbLayers = new Storyboard();
                if (mMainPage.MediaOverlay.Opacity > 0.0)
                {
                    DoubleAnimation animOverlay = new DoubleAnimation();
                    animOverlay.Duration = new Duration(TimeSpan.FromSeconds(2));
                    animOverlay.From = mMainPage.MediaOverlay.Opacity;
                    animOverlay.To = 0.0;
                    Storyboard.SetTarget(animOverlay, mMainPage.MediaOverlay);
                    Storyboard.SetTargetProperty(animOverlay, "(UIElement.Opacity)");
                    sbLayers.Children.Add(animOverlay);
                }

                if (mMainPage.BediaMedia.Opacity > 0.0)
                {
                    DoubleAnimation animMedia = new DoubleAnimation();
                    animMedia.Duration = new Duration(TimeSpan.FromSeconds(2));
                    animMedia.From = mMainPage.BediaMedia.Opacity;
                    animMedia.To = 0.0;
                    Storyboard.SetTarget(animMedia, mMainPage.BediaMedia);
                    Storyboard.SetTargetProperty(animMedia, "(UIElement.Opacity)");
                    sbLayers.Children.Add(animMedia);
                }

                if (mMainPage.MediaBackground.Opacity > 0.0)
                {
                    DoubleAnimation animBackground = new DoubleAnimation();
                    animBackground.Duration = new Duration(TimeSpan.FromSeconds(2));
                    animBackground.From = mMainPage.MediaBackground.Opacity;
                    animBackground.To = 0.0;
                    Storyboard.SetTarget(animBackground, mMainPage.MediaBackground);
                    Storyboard.SetTargetProperty(animBackground, "(UIElement.Opacity)");
                    sbLayers.Children.Add(animBackground);
                }

                if (mMainPage.Visualizations.Opacity > 0.0)
                {
                    DoubleAnimation animVisualizations = new DoubleAnimation();
                    animVisualizations.Duration = new Duration(TimeSpan.FromSeconds(2));
                    animVisualizations.From = mMainPage.Visualizations.Opacity;
                    animVisualizations.To = 0.0;
                    Storyboard.SetTarget(animVisualizations, mMainPage.Visualizations);
                    Storyboard.SetTargetProperty(animVisualizations, "(Canvas.Opacity)");
                    sbLayers.Children.Add(animVisualizations);
                }

                if (sbLayers.Children.Count > 0)
                {
                    sbLayers.Completed += (sndr, evts) =>
                    {
                        mMainPage.Visualizations.Children.Clear();
                    };
                    sbLayers.Begin();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private PastItem PastItemFromList(Int16 MediaPosition)
        {
            PastItem hiReturn = null;

            try
            {
                BediaItem bi = BediaPlaylist[MediaPosition];
                string sMediaPath = "";

                switch (bi.MenuType)
                {
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                        sMediaPath = bi.BediaFile.Path;
                        break;

                    default:
                        break;
                }

                hiReturn = PastListHistory.Find(e => (e.Path == sMediaPath));
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return hiReturn;
        }
        
        private void VizStart()
        {
            try
            {
                if (mMainPage.Visualizations.Opacity < 1.0)
                {
                    Storyboard sbVisualization = new Storyboard();
                    DoubleAnimation animVisualization = new DoubleAnimation();
                    animVisualization.Duration = new Duration(TimeSpan.FromSeconds(2));
                    animVisualization.From = mMainPage.Visualizations.Opacity;
                    animVisualization.To = 1.0;
                    Storyboard.SetTarget(animVisualization, mMainPage.Visualizations);
                    Storyboard.SetTargetProperty(animVisualization, "(Canvas.Opacity)");
                    sbVisualization.Children.Add(animVisualization);
                    sbVisualization.Begin();
                }

                bool bFloatingBubbles = true;
                Int16 iRange = 64;

                if (bFloatingBubbles == true)
                {
                    bool bFound = false;
                    foreach (Ellipse Bubble in mMainPage.Visualizations.Children)
                    {
                        //No need to reload if they are already loaded
                        if (Bubble.Name.IndexOf("Bubble") > -1)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (bFound == false)
                    {
                        // - Constant moving bubbles
                        for (Int16 i = 0; i < iRange; i++)
                        {
                            BediaBubble bubbledata = new BediaBubble();

                            bubbledata.Name = "Bubble" + i.ToString();
                            bubbledata.Bubble = new Ellipse();
                            bubbledata.Bubble.Name = bubbledata.Name;
                            bubbledata = VizBubbleCreate(bubbledata);

                            mMainPage.Visualizations.Children.Add(bubbledata.Bubble);

                            VizAdd(bubbledata);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void VizAdd(BediaBubble bubbleData)
        {
            try
            {
                byte bSeconds = Convert.ToByte(BediaRandom.Next(8, 28));
                Storyboard sbVisual = new Storyboard();
                QuadraticEase ease = new QuadraticEase();
                CompositeTransform compTransform = new CompositeTransform();


                bubbleData = VizBubbleUpdate(bubbleData);

                if (this.BubblesParty == true)
                {
                    bSeconds = Convert.ToByte(BediaRandom.Next(8, 18));  //Faster, it's a party after all
                    
                    ColorAnimation BubbleColor = new ColorAnimation();
                    BubbleColor.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    BubbleColor.From = Windows.UI.Color.FromArgb(255, bubbleData.BubbleColorStart.Color.R, bubbleData.BubbleColorStart.Color.G, bubbleData.BubbleColorStart.Color.B);
                    BubbleColor.To = Windows.UI.Color.FromArgb(255, bubbleData.BubbleColorEnd.Color.R, bubbleData.BubbleColorEnd.Color.G, bubbleData.BubbleColorEnd.Color.B);
                    Storyboard.SetTarget(BubbleColor, bubbleData.Bubble);
                    Storyboard.SetTargetProperty(BubbleColor, "(UIElement.Fill).(SolidColorBrush.Color)");
                    sbVisual.Children.Add(BubbleColor);
                }
                else
                {
                    //Revert back to BediaBlue
                    bubbleData.Bubble.Fill = this.BediaBaseColor;
                }

                DoubleAnimation animOpacity = new DoubleAnimation();
                animOpacity.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                animOpacity.From = bubbleData.OpacityStart;
                animOpacity.To = bubbleData.OpacityEnd;
                Storyboard.SetTarget(animOpacity, bubbleData.Bubble);
                Storyboard.SetTargetProperty(animOpacity, "(UIElement.Opacity)");
                sbVisual.Children.Add(animOpacity);
                

                DoubleAnimation animLeft = new DoubleAnimation();
                animLeft.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                animLeft.From = bubbleData.LeftStart;
                animLeft.To = bubbleData.LeftEnd;
                animLeft.EasingFunction = ease;
                Storyboard.SetTarget(animLeft, bubbleData.Bubble);
                Storyboard.SetTargetProperty(animLeft, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                sbVisual.Children.Add(animLeft);


                DoubleAnimation animTop = new DoubleAnimation();
                animTop.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                animTop.From = bubbleData.TopStart;
                animTop.To = bubbleData.TopEnd;
                animTop.EasingFunction = ease;
                Storyboard.SetTarget(animTop, bubbleData.Bubble);
                Storyboard.SetTargetProperty(animTop, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
                sbVisual.Children.Add(animTop);


                DoubleAnimation animWidth = new DoubleAnimation();
                animWidth.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                animWidth.From = bubbleData.SizeStart;
                animWidth.To = bubbleData.SizeEnd;
                Storyboard.SetTarget(animWidth, bubbleData.Bubble);
                Storyboard.SetTargetProperty(animWidth, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
                sbVisual.Children.Add(animWidth);


                DoubleAnimation animHeight = new DoubleAnimation();
                animHeight.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                animHeight.From = bubbleData.SizeStart;
                animHeight.To = bubbleData.SizeEnd;
                Storyboard.SetTarget(animHeight, bubbleData.Bubble);
                Storyboard.SetTargetProperty(animHeight, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
                sbVisual.Children.Add(animHeight);

                bubbleData.Bubble.RenderTransform = compTransform;

                sbVisual.Completed += (sndr, evts) =>
                {
                    //Set values for next time
                    bubbleData.TopStart = bubbleData.TopEnd;
                    bubbleData.LeftStart = bubbleData.LeftEnd;
                    bubbleData.OpacityStart = bubbleData.OpacityEnd;
                    bubbleData.SizeStart = bubbleData.SizeEnd;
                    bubbleData.BubbleColorStart = bubbleData.BubbleColorEnd;

                    VizAdd(bubbleData);
                };
                sbVisual.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private BediaBubble VizBubbleUpdate(BediaBubble bubbledata)
        {
            bubbledata.SizeEnd = Convert.ToInt16(BediaRandom.Next(4, 16));
            bubbledata.LeftEnd = Convert.ToInt16(BediaRandom.Next(this.MenuSize, Convert.ToInt16(mMainPage.ActualWidth - (bubbledata.SizeEnd + (this.MenuSize * 2)))));
            bubbledata.TopEnd = Convert.ToInt16(BediaRandom.Next(this.MenuSize, Convert.ToInt16(mMainPage.ActualHeight - (bubbledata.SizeEnd + (this.MenuSize * 2)))));
            bubbledata.OpacityEnd = BediaRandom.NextDouble();

            if (this.BubblesParty == true)
            {
                bubbledata.BubbleColorEnd = new SolidColorBrush(Windows.UI.Color.FromArgb(255,
                    Convert.ToByte(BediaRandom.Next(0, 255)),
                    Convert.ToByte(BediaRandom.Next(0, 255)),
                    Convert.ToByte(BediaRandom.Next(0, 255))));
            }

            return bubbledata;
        }

        private BediaBubble VizBubbleCreate(BediaBubble bubbledata)
        {
            bubbledata.SizeStart = Convert.ToInt16(BediaRandom.Next(4, 16));
            bubbledata.Bubble.Width = bubbledata.SizeStart;
            bubbledata.Bubble.Height = bubbledata.SizeStart;
            bubbledata.LeftStart = Convert.ToInt16(BediaRandom.Next(20, Convert.ToInt16(mMainPage.ActualWidth - (bubbledata.SizeStart + 20))));
            bubbledata.TopStart = Convert.ToInt16(BediaRandom.Next(20, Convert.ToInt16(mMainPage.ActualHeight - (bubbledata.SizeStart + 20))));
            bubbledata.OpacityStart = BediaRandom.NextDouble();


            if (bubbledata.Bubble != null)
            {
                bubbledata.Bubble.Fill = this.BediaBaseColor; 
                bubbledata.Bubble.Stroke = bubbledata.StrokeColor;
                bubbledata.Bubble.StrokeThickness = 0.5; // bubbledata.StrokeStart;
            }

            return bubbledata;
        }

        private void VizStop()
        {
            try
            {
                if (mMainPage.Visualizations.Opacity < 1.0) { return; } //If it's not 1.0, then it is being faded, or is faded. No need to do this again.

                Storyboard sbFade = new Storyboard();
                DoubleAnimation animFade = new DoubleAnimation();


                animFade.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                animFade.From = mMainPage.Visualizations.Opacity;
                animFade.To = 0.0;
                Storyboard.SetTarget(animFade, mMainPage.Visualizations);
                Storyboard.SetTargetProperty(animFade, "(Canvas.Opacity)");
                sbFade.Children.Add(animFade);
                sbFade.Completed += (sndr, evts) =>
                {
                    mMainPage.Visualizations.Children.Clear();
                };
                sbFade.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaNext()
        {
            try
            {
                if (this.IsMenuDisplayed == false)
                {
                    TitlebarHide("");
                }

                PastSave();

                miMediaCurrentPosition += 1;

                if (miMediaCurrentPosition >= BediaPlaylist.Count())
                {
                    miMediaCurrentPosition = 0;
                    miMenuCurrentPosition = 0;

                    MediaStop();
                    if (this.IsMenuDisplayed == true)
                    {
                        BubblesLoad();
                        HideExtraLayers();
                        EmptyQueue();
                        ScreenSaverCancel();
                    }
                    else
                    {
                        MenuBreadrumbs.Clear();
                        ScreenSaverShow(new object());
                    }
                    PlaylistClear();
                }
                else
                {
                    MediaPlay();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaPrior()
        {
            try
            {
                miMediaCurrentPosition -= 1;

                if (miMediaCurrentPosition < 0)
                {
                    miMediaCurrentPosition = 0;
                }

                TitlebarHide("");

                MediaPlay();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaStop()
        {
            try
            {
                mMainPage.BediaMedia.Stop();
                
                this.IsPlaying = false;
                this.IsPaused = false;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }


        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            //https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void TextBlockFade(Canvas parent, TextBlock child, double From, double To)
        {
            try
            {
                Storyboard sbFade = new Storyboard();
                DoubleAnimation aniFade = new DoubleAnimation();


                aniFade.Duration = new Duration(TimeSpan.FromMilliseconds(400));
                aniFade.From = From;
                aniFade.To = To;
                Storyboard.SetTarget(aniFade, child);
                Storyboard.SetTargetProperty(aniFade, "(TextBlock.Opacity)");
                sbFade.Children.Add(aniFade);
                
                sbFade.Completed += (s, e) =>
                {
                    if (To == 0.0)
                    {
                        parent.Children.Remove(child);
                    }
                };
                sbFade.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);

            }
        }

        private void MediaDisplayInfo(BediaItem bediaItem)
        {
            try
            {
                if (SettingGetBool("MediaDisplayInfo") == false) { return; }

                byte bDiff = 4;
                byte bSeconds = 48;
                Int16 iStart = 500;
                Int16 iEnd = -7000;
                Storyboard sbMessage = new Storyboard();
                TextBlock txtMessage = new TextBlock();
                TextBlock txtShadow = new TextBlock();
                DoubleAnimation aniMessage = new DoubleAnimation();
                DoubleAnimation aniShadow = new DoubleAnimation();
                DateTime dtCurrentTime = new DateTime(DateTime.Now.Ticks);
                CompositeTransform compTransform = new CompositeTransform();

                //Check if the control is already loaded, 
                //if so just remove it, they overlay on each other.              
                TextBlock foundMsgBox = FindChild<TextBlock>(mMainPage.Visualizations, msMediaDetails);

                if (foundMsgBox != null)
                {
                    TextBlockFade(mMainPage.Visualizations, foundMsgBox, foundMsgBox.Opacity, 0.0);
                }

                TextBlock foundShadowBox = FindChild<TextBlock>(mMainPage.Visualizations, msMediaDetailsShadow);

                if (foundShadowBox != null)
                {
                    TextBlockFade(mMainPage.Visualizations, foundShadowBox, foundShadowBox.Opacity, 0.0);
                }


                msMediaDetails = "txtMsg" + Guid.NewGuid().ToString().Replace("-", "");
                msMediaDetailsShadow = "txtShadow" + Guid.NewGuid().ToString().Replace("-", "");

                txtMessage.Name = msMediaDetails;
                txtMessage.Text = bediaItem.BediaFile.DisplayName;
                txtMessage.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                txtMessage.FontSize = 80;
                txtMessage.RenderTransform = compTransform;

                txtShadow.Name = msMediaDetailsShadow;
                txtShadow.Text = bediaItem.BediaFile.DisplayName;
                txtShadow.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(148, 0, 0, 0));
                txtShadow.FontSize = 80;
                txtShadow.RenderTransform = compTransform;


                mMainPage.Visualizations.Children.Add(txtMessage);
                Canvas.SetTop(txtMessage, (mMainPage.ActualHeight / 4) * 3);
                Canvas.SetLeft(txtMessage, mMainPage.ActualWidth + 44);
                Canvas.SetZIndex(txtMessage, 50);

                mMainPage.Visualizations.Children.Add(txtShadow);
                Canvas.SetTop(txtShadow, ((mMainPage.ActualHeight / 4) * 3) + bDiff);
                Canvas.SetLeft(txtShadow, mMainPage.ActualWidth + 44 + bDiff);
                Canvas.SetZIndex(txtShadow, 40);

                aniMessage.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                aniMessage.From = iStart;
                aniMessage.To = iEnd;
                aniMessage.AutoReverse = false;
                Storyboard.SetTarget(aniMessage, txtMessage);
                Storyboard.SetTargetProperty(aniMessage, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                sbMessage.Children.Add(aniMessage);

                aniShadow.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                aniShadow.From = iStart;
                aniShadow.To = iEnd;
                aniShadow.AutoReverse = false;
                Storyboard.SetTarget(aniShadow, txtShadow);
                Storyboard.SetTargetProperty(aniShadow, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                sbMessage.Children.Add(aniShadow);


                sbMessage.Completed += (sndr, evts) =>
                {
                    TextBlockFade(mMainPage.Visualizations, txtMessage, txtMessage.Opacity, 0.0);
                    TextBlockFade(mMainPage.Visualizations, txtShadow, txtShadow.Opacity, 0.0);
                };
                sbMessage.Begin();
            }
            catch { } //Don't care if this throws an exception, just move along.... nothing to see here.   An ex could happen if the controls are removed prior to the storyboard completing.  HASN'T... but could
        }

        private async void MediaPlay(PastItem Start = null)
        {
            try
            {
                //Hack: Sometimes, for reasons unknown atm, the value is larger or smaller than the playlist count.
                //Added July 30, 2015
                if (miMediaCurrentPosition < 0 || miMediaCurrentPosition > BediaPlaylist.Count)
                {
                    MediaNext();
                    return;
                }


                if (miMediaCurrentPosition == 0 &&
                    this.IsMenuDisplayed == true &&
                    BediaMenuItems[0].MenuType != BediaItem.MenuTypes.PastItem)  //Check for PastItem to avoid loop when user selects PLAY when History is being shown (they can select the Info item, but NOoooooo, they select Play and we get into this loop
                {

                    PastItem hi = PastItemFromList(0);

                    if (hi != null)
                    {
                        string sIcon = "Video";
                        BediaItem bediaitem = new BediaItem();
                        bediaitem.BediaFile = BediaPlaylist[0].BediaFile;
                        bediaitem.MenuType = BediaItem.MenuTypes.Video;
                        bediaitem.Title = System.IO.Path.GetFileNameWithoutExtension(bediaitem.BediaFile.Path);

                        if (msMediaTypesAudio.IndexOf(bediaitem.BediaFile.FileType.ToLower()) > -1)
                        {
                            bediaitem.MenuType = BediaItem.MenuTypes.Audio;
                            sIcon = "Audio";
                        }

                        bediaitem.HistoryInfo = hi;
                        HackBediaItem = bediaitem;

                        MenuProcessing("HistoryItem", bediaitem.Title, sIcon);
                        return;
                    }
                }

                ScreenSaverCancel();

                NewMediaItemClear(); //Remove current item from the list as soon as it is played
                BediaItem bi = BediaPlaylist[miMediaCurrentPosition];


                if (bi.MenuType == BediaItem.MenuTypes.Audio)
                {
                    VizStart();
                    MediaDisplayInfo(bi);
                }
                else
                {
                    VizStop();
                }

                string sURI = "";

                
                switch (bi.MenuType)
                {
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                        sURI = bi.BediaFile.Path;
                        mMainPage.BediaMedia.SetSource(await bi.BediaFile.OpenAsync(FileAccessMode.Read), bi.BediaFile.ContentType);
                        break;

                    case BediaItem.MenuTypes.SpaceItem:
                        mMainPage.BediaMedia.Source = new Uri(bi.Value);
                        break;

                    case BediaItem.MenuTypes.IPTVVideo:
                    case BediaItem.MenuTypes.PodcastItem:
                        sURI = bi.Value;
                        break;
                }

                this.MediaOpened = false;
                mMainPage.BediaMedia.Volume = this.Volume;
                mMainPage.BediaMedia.Play();

                //This needs to happen after the Play method is called
                if (Start != null)
                {
                    if ((Start.Seconds) > 0)
                    {
                        MediaStartPosiion(Start);
                        //TimeSpan Jump = new TimeSpan(0, 0, Convert.ToInt32(Start.Seconds));
                        //mMainPage.BediaMedia.Position += Jump;
                    }
                }
                
                this.IsPlaying = true;
                this.IsPaused = false;

                ISSLocationStop();
                
                if (mMainPage.BediaMedia.Opacity < 1.0)
                {
                    this.IsMenuDisplayed = false;

                    MenusClear();

                    TitlebarHide("");

                    Storyboard sbMedia = new Storyboard();
                    DoubleAnimation animMedia = new DoubleAnimation();
                    animMedia.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed * 2));
                    animMedia.From = mMainPage.BediaMedia.Opacity;
                    animMedia.To = 1.0;
                    Storyboard.SetTarget(animMedia, mMainPage.BediaMedia);
                    Storyboard.SetTargetProperty(animMedia, "(UIElement.Opacity)");
                    sbMedia.Children.Add(animMedia);


                    DoubleAnimation animBackground = new DoubleAnimation();
                    animBackground.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed * 2));
                    animBackground.From = mMainPage.MediaBackground.Opacity;
                    animBackground.To = 1.0;
                    Storyboard.SetTarget(animBackground, mMainPage.MediaBackground);
                    Storyboard.SetTargetProperty(animBackground, "(Canvas.Opacity)");
                    sbMedia.Children.Add(animBackground);

                    sbMedia.Completed += (sndr, evts) =>
                    {
                        BubblesStop();
                    };
                    sbMedia.Begin();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void MediaStartPosiion(PastItem Start)
        {
            try
            {
                //Can't change the Position until the MediaOpened event has fired
                while (this.MediaOpened == false)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }

                
                TimeSpan Jump = new TimeSpan(0, 0, Convert.ToInt32(Start.Seconds));
                mMainPage.BediaMedia.Position += Jump;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaSelected(BediaItem bediaitem)
        {
            try
            {
                //Select the existing UI menu
                List<BediaMenuUI> UIMenus = MenusUISorted();


                foreach (BediaMenuUI BediaMenu in UIMenus)
                {
                    switch (bediaitem.MenuType)
                    {
                        case BediaItem.MenuTypes.Audio:
                        case BediaItem.MenuTypes.Video:
                            if (BediaMenu.Bediaitem.BediaFile == bediaitem.BediaFile)
                            {
                                BediaMenu.Bediaitem.Selected = !bediaitem.Selected;
                                BediaMenu.SetIcon(IconByNameSmall("Selected"), !BediaMenu.Bediaitem.Selected); 
                                break;
                            }
                            break;

                        case BediaItem.MenuTypes.IPTVVideo:
                        case BediaItem.MenuTypes.PodcastItem:
                        case BediaItem.MenuTypes.SpaceItem:
                        case BediaItem.MenuTypes.YouTubeVideo:
                            if (BediaMenu.Bediaitem.Value == bediaitem.Value)
                            {
                                BediaMenu.Bediaitem.Selected = !bediaitem.Selected;
                                BediaMenu.SetIcon(IconByNameSmall("Selected"), !BediaMenu.Bediaitem.Selected);
                                break;
                            }
                            break;
                    }
                }


                //Add or Remove it from the playlist
                if (bediaitem.Selected == true)
                {
                    BediaPlaylist.Add(bediaitem);
                }
                else
                {
                    MenuRemove(bediaitem);
                }

                MenuPlayCloseClear();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private bool MenuSelected(StorageFile file)
        {
            bool bReturn = false;

            try
            {
                BediaItem FoundFile = BediaPlaylist.Find(e => (e.BediaFile.Path == file.Path));

                if (FoundFile != null)
                {
                    bReturn = true;
                    FoundFile = null;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private async void MenuPlayCloseClear()
        {
            try
            {
                //Add a Play menu if 1 or more media are selected
                //AND play is not already in the list
                //AND media is not already playing (or paused)
                //Load UI menu if there is space
                if (BediaPlaylist.Count > 0)
                {
                    BediaItem biItem;
                    List<BediaItem> NewMenus = new List<BediaItem>();


                    //Check for the existance of menus: PLAY, CLEAR/RECYCLE and/or CLOSE
                    //and adds them to the list and UI if necessary
                    if (this.IsPlaying == false && this.IsPaused == false)
                    {
                        biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.Play));
                        if (biItem == null)
                        {
                            biItem = new BediaItem("Play", BediaItem.MenuTypes.Play);
                            NewMenus.Add(biItem);
                        }
                    }

                    biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.EmptyQueue));
                    if (biItem == null)
                    {
                        biItem = new BediaItem("Empty Queue", BediaItem.MenuTypes.EmptyQueue);
                        NewMenus.Add(biItem);
                    }

                    if (this.IsPlaying == true || this.IsPaused == true)
                    {
                        biItem = BediaMenuItems.Find(e => (e.MenuType == BediaItem.MenuTypes.CloseMenu));
                        if (biItem == null)
                        {
                            biItem = new BediaItem("Close", BediaItem.MenuTypes.CloseMenu);
                            NewMenus.Add(biItem);
                        }
                    }
                    

                    bool bFound = false;                   
                    byte bMenuSpaceAvailable = await MenuSpaceAvailable();
                    byte bCount = 1;
                    biItem = BediaMenuItems[(BediaMenuItems.Count - 1)]; 
                    List<BediaMenuUI> BediaUIMenus = MenusUISorted();


                    foreach(BediaMenuUI bedaiMenuUI in BediaUIMenus)
                    {
                        if (bedaiMenuUI.Bediaitem == biItem)
                        {
                            bFound = true;
                            break;
                        }
                        bCount += 1;
                    }

                    //The position of the last item is less than the number of spaces available
                    //No need to add values to the UI if there is no space to display them
                    if (bFound == true && bCount < bMenuSpaceAvailable)
                    {
                        foreach (BediaItem NewMenu in NewMenus)
                        {
                            if (bCount < (bMenuSpaceAvailable - BediaUIMenus.Count))
                            {
                                bool b = await MenuLoad(NewMenu);
                            }
                            else
                            {
                                bool b = await MenuInsert(NewMenu, bCount);
                                bCount += 1;
                            }

                            //Add New Menus to existing List
                            BediaMenuItems.Add(NewMenu);
                        }
                    }

                    //Remove excess menus
                    for(byte bRemove = Convert.ToByte(mMainPage.BediaMenus.Children.Count); bRemove > bMenuSpaceAvailable; bRemove--)
                    {
                        mMainPage.BediaMenus.Children.RemoveAt(bRemove - 1);
                    }

                    //Cleanup
                    BediaUIMenus.Clear();
                    BediaUIMenus = null;
                    NewMenus.Clear();
                    NewMenus = null;
                    biItem = null;
                }
                else
                {
                    //Remove PLAY, CLEAR/RECYCLE and/or CLOSE
                    MenusAdjust();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void SelectAll()
        {
            try
            {
                //Set the SELECTED value to the oposite of what it currently is
                foreach (BediaItem bediaitem in BediaMenuItems)
                {
                    if (bediaitem.MenuType == BediaItem.MenuTypes.Audio || bediaitem.MenuType == BediaItem.MenuTypes.Video)
                    {
                        bediaitem.Selected = !bediaitem.Selected;

                        foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                        {
                            if (BediaMenu.Title.Text == bediaitem.Title)
                            {
                                BediaMenu.SetIcon(IconByNameSmall("Selected"), !bediaitem.Selected);

                                if (bediaitem.Selected == true)
                                {
                                    BediaPlaylist.Add(bediaitem);
                                }
                                else
                                {
                                    MenuRemove(bediaitem);
                                }
                                break;
                            }
                        }
                    }
                }

                MenuPlayCloseClear();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void MediaPause()
        {
            try
            {
                mMainPage.BediaMedia.Pause();

                IsPaused = true;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<bool> MediaPlayPause()
        {
            try
            {
                if (IsPlaying == false)
                {
                    if (IsPaused == true)
                    {
                        ScreenSaverCancel();

                        ScreenSaverReset();

                        mMainPage.BediaMedia.Play();

                        IsPaused = false;
                    }
                    else
                    {
                        ScreenSaverCancel();

                        BediaItem bi = BediaPlaylist[miMediaCurrentPosition];
                        string sURI = "";


                        switch (bi.MenuType)
                        {
                            case BediaItem.MenuTypes.Audio:
                            case BediaItem.MenuTypes.Video:
                                sURI = bi.BediaFile.Path;
                                break;

                            case BediaItem.MenuTypes.SpaceItem:
                                sURI = bi.Value;
                                break;

                            case BediaItem.MenuTypes.YouTubeVideo:
                                //sURI = await GetYouTubeUri(bi.Value);
                                break;
                        }

                        mMainPage.BediaMedia.Source = new Uri(sURI);
                        mMainPage.BediaMedia.Play();
                    }
                }
                else
                {
                    if (medianavigation.Opacity > 0.0)
                    {
                        ImageChange(medianavigation.NavRightImage, NavigationIcon(NavigationItem.NavigationType.PlayPause).Icon);
                    }

                    MediaJump(new TimeSpan(0, 0, 0)); // to show the current status

                    ScreenSaverReset();

                    MediaPause();
                }

                IsPlaying = !IsPlaying;
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1));

            return true;
        }

        private async Task<bool> PlaylistLoad(BediaItem bediaitem)
        {
            try
            {
                string sLine = "";
                bool bAwait = false;
                StorageFile MediaFile; 
                BediaIO bediaIO = new BediaIO();
                BediaItem biMenuItem = new BediaItem("Select All", BediaItem.MenuTypes.SelectAll);
                BediaMenuItems.Add(biMenuItem);

                
                Stream stream = await bediaitem.BediaFolder.OpenStreamForReadAsync(bediaitem.BediaFile.Name);
                using (StreamReader PlaylistReader = new StreamReader(stream))
                {
                    while ((sLine = PlaylistReader.ReadLine()) != null)
                    {
                        if (sLine.Trim().Length > 0 && sLine.Substring(0, 1) != "#")
                        {
                            if (await bediaIO.FileExists(bediaitem.BediaFolder, sLine) == true)
                            {
                                MediaFile = await bediaitem.BediaFolder.GetFileAsync(sLine);
                                bAwait = await MenuCreate(bediaitem.BediaFolder, MediaFile);
                            }
                            else
                            {
                                //Loop through the Path if there are backslashes, in case the path was full or back a folder or two
                                Int16 bPosition = -1;
                                string sBackslash = "\\";
                                string sValue = "";

                                do
                                {
                                    bPosition = Convert.ToInt16(sLine.IndexOf(sBackslash, (bPosition + 1)));

                                    if (bPosition > -1)
                                    {
                                        sValue = sLine.Substring(bPosition);

                                        if (await bediaIO.FileExists(bediaitem.BediaFolder, sValue) == true)
                                        {
                                            MediaFile = await StorageFile.GetFileFromPathAsync(bediaIO.FolderFilePath(bediaitem.BediaFolder, sValue));
                                            bAwait = await MenuCreate(bediaitem.BediaFolder, MediaFile);
                                            break;
                                        }
                                    }
                                } while (bPosition > -1);
                            }
                        }
                    }
                }

                MediaFile = null;
                bediaIO = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private async Task<bool> PlaylistLoad(StorageFile PlaylistFile)
        {
            try
            {
                string sTitle = "";
                Int16 iComma = 0;
                string sExt;
                bool bFound = false;
                var buffer = await FileIO.ReadBufferAsync(PlaylistFile);
                DataReader reader = DataReader.FromBuffer(buffer);
                byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(fileContent);
                string sEntireFile = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                string[] sLines = sEntireFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);


                foreach(string sLine in sLines)
                {
                    
                    if (sLine.Trim().Length > 0)
                    {
                        if (sLine.Contains("EXTINF"))
                        {
                            iComma = Convert.ToInt16(sLine.IndexOf(","));
                            if (iComma > 0)
                            {
                                sTitle = sLine.Substring(iComma + 1);
                            }
                            else
                            {
                                sTitle = sLine.Substring(8);
                            }
                        }
                        else
                        {
                            sExt = sLine.Substring(sLine.Length - 3, 3);

                            if (sExt.ToLower() == "mp4")
                            {
                                bFound = true;
                            }
                            else if (sExt.ToLower() == "asf")
                            {
                                bFound = true;
                            }
                            else if (sLine.ToLower().Substring(0, 4) == "mms:")
                            {
                                bFound = true;
                            }
                            else if (sLine.ToLower().Contains("m3u8") == true)
                            {
                                bFound = true;
                            }

                            if (bFound == true)
                            {
                                if (sTitle.Trim().Length == 0)
                                {
                                    sTitle = sLine; //TODO: affect this to something more readable
                                }
                                BediaItem biMenuItem = new BediaItem(sTitle.Trim(), BediaItem.MenuTypes.IPTVVideo);
                                biMenuItem.Value = sLine;
                                BediaMenuItems.Add(biMenuItem);
                            }

                            //Reset values
                            sTitle = "";
                            bFound = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private void PlaylistClear()
        {
            try
            {
                BediaPlaylist.Clear();

                mMainPage.BediaMedia.Source = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<bool> EmptyQueue()
        {
            try
            {
                List<BediaMenuUI> UIMenus = MenusUISorted();
                List<BediaItem> RemoveMenus = new List<BediaItem>();
                byte bAddMenus = 0;


                PastSave();

                PlaylistClear();

                MediaStop();

                if (mMainPage.BediaBubbles.Children.Count == 0)
                {
                    BubblesLoad();
                }

                HideExtraLayers();

                //Unselect all UI menu items
                foreach (BediaMenuUI UIMenu in UIMenus)
                {
                    if (UIMenu.Bediaitem.Selected == true)
                    {
                        UIMenu.Bediaitem.Selected = false;
                        UIMenu.SetIcon(null, true, false);
                    }
                }

                //Unselect all menu items
                foreach (BediaItem bi in BediaMenuItems)
                {
                    bi.Selected = false;
                }

                //Remove Empty Queue, Play and Close Menus
                //Move all other menus up to fill the space
                //CLEAR will always be on top, (it was selected, duh!)
                //PLAY or CLOSE will be at the bottom, just remove.  The only way they display is if there are fewer media, so no need to do anything else
                foreach (BediaMenuUI BediaMenu in UIMenus)
                {
                    if (BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.Play ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.EmptyQueue ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.CloseMenu ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.PlaylistCurrent ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.PastItem)
                    {
                        MenuUnload(BediaMenu);
                        bAddMenus += 1;
                    }
                }

                foreach (BediaItem bi in BediaMenuItems)
                {
                    if (bi.MenuType == BediaItem.MenuTypes.Play ||
                        bi.MenuType == BediaItem.MenuTypes.EmptyQueue ||
                        bi.MenuType == BediaItem.MenuTypes.CloseMenu ||
                        bi.MenuType == BediaItem.MenuTypes.PlaylistCurrent ||
                        bi.MenuType == BediaItem.MenuTypes.PastItem)
                    {
                        RemoveMenus.Add(bi);
                    }
                }

                foreach (BediaItem bi in RemoveMenus)
                {
                    BediaMenuItems.Remove(bi);
                }
                
                //Changed Aug 1st, 2016.   I BELIEVE we always end up back at the top
                miMenuCurrentPosition = 0;
                miMediaCurrentPosition = 0; //added Oct 1, 2016

                if (bAddMenus > 0)
                {
                    if (mMainPage.BediaMenus.Children.Count < BediaMenuItems.Count)
                    {
                        byte iMenuCount = Convert.ToByte(mMainPage.BediaMenus.Children.Count());
                        bool bLoad = false;

                        for (byte b = 0; b < bAddMenus; b++)
                        {
                            if ((b + iMenuCount) > BediaMenuItems.Count)
                            {
                                iMenuCount = 0;
                            }

                            bLoad = await MenuLoad(BediaMenuItems[b + iMenuCount]);
                        }
                    }
                }
                

                BediaTransitions bediaTransitions = new BediaTransitions();

                bediaTransitions.TextChange(bediaTitlebar.TitlebarCountX, "1");
                bediaTransitions.TextChange(bediaTitlebar.TitlebarCountY, BediaMenuItems.Count().ToString());

                //Cleanup
                RemoveMenus.Clear();
                RemoveMenus = null;
                UIMenus.Clear();
                UIMenus = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return true;
        }

        private async void MenusAdjust()
        {
            try
            {
                bool bFound = false;
                Int16 iTop = this.MenuSize;
                List<BediaMenuUI> UIMenus = MenusUISorted();
                List<BediaMenuUI> UIMenusRemoved = new List<BediaMenuUI>();
                List<BediaItem> biMenusRemoved = new List<BediaItem>();


                foreach (BediaMenuUI BediaMenu in UIMenus)
                {
                    if (BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.Play ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.EmptyQueue ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.CloseMenu ||
                        BediaMenu.Bediaitem.MenuType == BediaItem.MenuTypes.PlaylistCurrent)
                    {
                        MenuUnload(BediaMenu);
                        UIMenusRemoved.Add(BediaMenu);
                    }
                }

                //Loop through list and remove PLAY, CLEAR, CLOSE, CURRENT_PLAYLIST
                foreach (BediaItem bi in BediaMenuItems)
                {
                    if (bi.MenuType == BediaItem.MenuTypes.Play ||
                        bi.MenuType == BediaItem.MenuTypes.EmptyQueue ||
                        bi.MenuType == BediaItem.MenuTypes.CloseMenu ||
                        bi.MenuType == BediaItem.MenuTypes.PlaylistCurrent)
                    {
                        biMenusRemoved.Add(bi);
                    }
                }

                foreach (BediaItem bi in biMenusRemoved)
                {
                    BediaMenuItems.Remove(bi);
                }

                // If there are any removed menus, remove them from the main list
                if (UIMenusRemoved.Count > 0)
                {
                    //Loop through list and remove PLAY, CLEAR, CLOSE
                    foreach (BediaMenuUI bi in UIMenusRemoved)
                    {
                        UIMenus.Remove(bi);
                    }
                }

                //Add new menus at the bottom
                if (UIMenusRemoved.Count > 0)
                {
                    Int16 iLast = Convert.ToInt16(BediaMenuItems.IndexOf(UIMenus[UIMenus.Count - 1].Bediaitem) + 1);

                    for (byte b = 0; b < UIMenusRemoved.Count; b++)
                    {
                        if ((iLast + b) >= BediaMenuItems.Count)
                        {
                            miMenuCurrentPosition = 0; 
                            break;
                        }


                        BediaItem bi = BediaMenuItems[iLast + b];
                        //Only load the UI if it isn't already loaded.  This can happen
                        //if there are fewer menu items than the max capacity.
                        bFound = false;
                        foreach (BediaMenuUI BediaMenu in UIMenus)
                        {
                            if (BediaMenu.Bediaitem == bi)
                            {
                                bFound = true;
                            }

                        }

                        if (bFound == false)
                        {
                            BediaMenuUI uiMenu = await MenuUICreate(bi);
                            mMainPage.BediaMenus.Children.Add(uiMenu);

                            PastSet(uiMenu.Bediaitem);//After the load event

                            MenuMove(uiMenu, Convert.ToInt16(mMainPage.ActualHeight + uiMenu.ActualHeight), iTop);

                            iTop += this.MenuSize; 
                        }
                    }

                    //Check if this is at the bottom of the list, if so we are now at the top
                    if (miMenuCurrentPosition == (BediaMenuItems.Count + UIMenusRemoved.Count))
                    {
                        miMenuCurrentPosition = 0;
                    }
                    else
                    {
                        miMenuCurrentPosition -= Convert.ToInt16(UIMenusRemoved.Count); //Need to adjust this for the next Scroll event

                        if (miMenuCurrentPosition < 0) { miMenuCurrentPosition = 0; }
                    }
                }
                UIMenusRemoved = null;


                //Move remaining menus up
                if (UIMenus.Count == 0)
                {
                    MenuBack();
                }

                UIMenusRemoved = null;
                biMenusRemoved = null;
                UIMenus = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        public void KeyDown(Windows.UI.Core.KeyEventArgs KeyArgs)
        {  
            try
            {
                //Don't allow user interaction until the menus have completely loaded.  See  MenuLoadCompleted method
                if (mbMenuLoadCompleted == false) { return; }

                if (this.IsMenuDisplayed == true)
                {
                    #region Menu
                    switch (KeyArgs.VirtualKey)
                    {
                        case VirtualKey.P:
                            MediaPlay(new PastItem());
                            break;

                        case VirtualKey.Escape:
                        case VirtualKey.Left:
                        case VirtualKey.Back:
                            MenuBack();
                            break;

                        case VirtualKey.Enter:
                        case VirtualKey.Space:
                        case VirtualKey.Right:
                            BediaItem bediaitem = BediaMenuItems[miMenuCurrentPosition];
                            MenuSelected(bediaitem);
                            bediaitem = null;
                            break;

                        case VirtualKey.Up:
                            MenuScrollUp();
                            break;

                        case VirtualKey.Down:
                            MenuScrollDown();
                            break;

                        case VirtualKey.PageUp:
                            MenuScrollUpPage();
                            break;

                        case VirtualKey.PageDown:
                            MenuScrollDownPage();
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region Playback
                    switch (KeyArgs.VirtualKey)
                    {
                        case VirtualKey.Escape:
                            MediaMenu();
                            break;

                        case VirtualKey.Up:
                            NavigationDisplay(1);
                            break;

                        case VirtualKey.Down:
                            NavigationDisplay(-1);
                            break;

                        case VirtualKey.Left:
                            NavigationItem CurrentNavitaionLeft = NavigationItems[miNavigationCurrentPosition];
                            NavigationSelected(CurrentNavitaionLeft.Left);
                            break;

                        case VirtualKey.Right:
                            NavigationItem CurrentNavitaionRight = NavigationItems[miNavigationCurrentPosition];
                            NavigationSelected(CurrentNavitaionRight.Right);
                            break;

                        case VirtualKey.Space:
                        case VirtualKey.P:
                            MediaPlayPause();
                            break;

                        case VirtualKey.V:
                            MediaVolume(0.05);
                            break;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private void OneMomentPlease()
        {
            try
            {
                BediaTransitions bediaTrans = new BediaTransitions();

                
                bediaTrans.TextChange(bediaTitlebar.TitlebarTitle, "One moment please...");
                 
                bediaTrans.TextChange(bediaTitlebar.TitlebarCountX, "");

                bediaTrans.TextChange(bediaTitlebar.TitlebarCountOf, "");

                bediaTrans.TextChange(bediaTitlebar.TitlebarCountY, "");
                
                ProgressRing(true, "");

                bediaTrans = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
   
        private void SetCount(Int16 Y)
        {
            try
            {
                BediaTransitions bediaTransitions = new BediaTransitions();
                
                bediaTransitions.TextChange(bediaTitlebar.TitlebarCountX, "1");

                bediaTransitions.TextChange(bediaTitlebar.TitlebarCountOf, "of");

                bediaTransitions.TextChange(bediaTitlebar.TitlebarCountY, Y.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private void PageUpDown()
        {
            try
            {
                FullPageScroll = !FullPageScroll;

                if (FullPageScroll == true)
                {
                    ResizeRect(0.0, 90, bediaTitlebar.rectTitlebarPageUpDown, bediaTitlebar.imgTitlebarPageUpDown);
                }
                else
                {
                    ResizeImage(1.0, 0.0, bediaTitlebar.imgTitlebarPageUpDown, bediaTitlebar.rectTitlebarPageUpDown);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ResizeImage(
            double From,
            double To,
            Windows.UI.Xaml.Controls.Image img,
            Windows.UI.Xaml.Shapes.Rectangle rect)
        {
            try
            {
                Storyboard sbResize = new Storyboard();
                DoubleAnimation aniImage = new DoubleAnimation();
                DoubleAnimation aniEllipse = new DoubleAnimation();

                aniImage.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                aniImage.From = From;
                aniImage.To = To;
                Storyboard.SetTarget(aniImage, img);
                Storyboard.SetTargetProperty(aniImage, "(UIElement.Opacity)");
                sbResize.Children.Add(aniImage);

                aniEllipse.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                aniEllipse.From = From;
                aniEllipse.To = To;
                Storyboard.SetTarget(aniEllipse, bediaTitlebar.PageUpBackground);
                Storyboard.SetTargetProperty(aniEllipse, "(UIElement.Opacity)");
                sbResize.Children.Add(aniEllipse);


                if (To > 0)
                {
                    img.Width = this.MenuSize;
                    bediaTitlebar.PageUpBackground.Width = this.MenuSize;
                    bediaTitlebar.PageUpBackground.Height = this.MenuSize;
                }

                sbResize.Completed +=
                    (sndr, evts) =>
                    {
                        if (To == 0.0)
                        {
                            img.Width = 0;
                            bediaTitlebar.PageUpBackground.Width = 0;
                            bediaTitlebar.PageUpBackground.Height = 0;
                            ResizeRect(90, 0, rect, img);
                        }

                    };
                sbResize.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ResizeRect(
            double From,
            double To,
            Windows.UI.Xaml.Shapes.Rectangle rect,
            Windows.UI.Xaml.Controls.Image img)
        {
            try
            {
                Storyboard sbRect = new Storyboard();
                DoubleAnimation animRect = new DoubleAnimation();
                CompositeTransform compTransform = new CompositeTransform();

                rect.RenderTransform = compTransform;
                
                animRect.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                animRect.From = From;
                animRect.To = To;

                Storyboard.SetTarget(animRect, rect);
                Storyboard.SetTargetProperty(animRect, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
                sbRect.Children.Add(animRect);

                if (To > 0)
                {
                    sbRect.Completed +=
                        (sndr, evts) =>
                        {
                           ResizeImage(0.0, 1.0, img, rect);
                        };
                }
                sbRect.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ScreenSaverMessagesLoad()
        {
            try
            {
                ScreenSaverComments.Clear();

                if (SettingGetBool("ScreenSaverComments") == false) { return; }

      
                ScreenSaverComments.Add("Rome wasn't build in a nano-second");
                ScreenSaverComments.Add("Hang on");
                ScreenSaverComments.Add("Time is free, but it is priceless");
                ScreenSaverComments.Add("Patience, grasshopper");
                ScreenSaverComments.Add("Patience is a virtue");
                ScreenSaverComments.Add("What are you looking at?");
                ScreenSaverComments.Add("Is it cartoon time?");
                ScreenSaverComments.Add("Is it movie time?");
                ScreenSaverComments.Add("Is it music time?");
                ScreenSaverComments.Add("Is it veg time?");
                ScreenSaverComments.Add("I could go for some new tunes");
                ScreenSaverComments.Add("Maybe you should go outside");
                ScreenSaverComments.Add("Another movie marathon!");
                ScreenSaverComments.Add("Another media marathon!");
                ScreenSaverComments.Add("Advertise here.  Ask me how");
                ScreenSaverComments.Add("Calibrating enjoyment levels");
                ScreenSaverComments.Add("Psst, psst!  oh nevermind");
                ScreenSaverComments.Add("Behind you!");
                ScreenSaverComments.Add("Made you look!");
                ScreenSaverComments.Add("Carpe diem");
                ScreenSaverComments.Add("In a New York minute");
                ScreenSaverComments.Add("You may delay, but time will not");
                ScreenSaverComments.Add("Better late than never");
                ScreenSaverComments.Add("As time goes by...");
                ScreenSaverComments.Add("intentionally left blank");
                ScreenSaverComments.Add("<insert funny statement here>");
                ScreenSaverComments.Add("Maybe you should read a book");
                ScreenSaverComments.Add("Play it again");
                ScreenSaverComments.Add("One small step for man...");
                ScreenSaverComments.Add("One small step for a man...");
                ScreenSaverComments.Add("Who are your heroes?");
                ScreenSaverComments.Add("Once around the park");
                ScreenSaverComments.Add("Sometimes you've got to wonder");
                ScreenSaverComments.Add("Watch this space");
                ScreenSaverComments.Add("Free Love");

                //Movies & TV
                ScreenSaverComments.Add("Yippee ki-yay!");                        //Die Hard
                ScreenSaverComments.Add("I'm on vacation!");                      //A good day to Die Hard
                ScreenSaverComments.Add("Bring out your dead");                   //Monty Python & the Holy Grail
                ScreenSaverComments.Add("Run away!");                            //Monty Python & the Holy Grail
                ScreenSaverComments.Add("In the not too distant future");        //MST3K
                ScreenSaverComments.Add("Don't Panic");                           //Hitchhikers Guide to the universe
                ScreenSaverComments.Add("Care to play a game?");                  //War Games
                ScreenSaverComments.Add("I'll be back");                          //Terminator
                ScreenSaverComments.Add("Live long and prosper");                 //Star Trek
                ScreenSaverComments.Add("Boldly Go");                             //Star Trek
                ScreenSaverComments.Add("May the force be with you");             //Star Wars
                ScreenSaverComments.Add("There is no spoon");                     //The Matrix
                ScreenSaverComments.Add("Klaatu barada nikto!");                  //The day the earth stood still
                ScreenSaverComments.Add("The answer: 42");                        //Hitchhikers Guide to the universe
                ScreenSaverComments.Add("Don't call me Shirley");                 //Airplane
                ScreenSaverComments.Add("It's alive! It's alive!");               //Frankestein
                ScreenSaverComments.Add("My precious!");                          //LOTR
                ScreenSaverComments.Add("Inconceivable");                         //Princess Bride
                ScreenSaverComments.Add("We're gonna need a bigger boat");        //Jaws
                ScreenSaverComments.Add("Free your mind");                        //The Matrix
                ScreenSaverComments.Add("You had me at hello");                   //Jerry McQuire
                ScreenSaverComments.Add("Back off man. I'm a scientist");         //Ghostbusters
                ScreenSaverComments.Add("Never give up! Never surrender!");       //Galaxy Quest
                ScreenSaverComments.Add("Take the red pill");                     //The Matrix
                ScreenSaverComments.Add("Take the blue pill");                    //The Matrix
                ScreenSaverComments.Add("Take the purple pill");                  //The Matrix via Debra

                //Magic 8 Ball
                ScreenSaverComments.Add("You may rely on it");
                ScreenSaverComments.Add("Reply hazy, ask again later");
                ScreenSaverComments.Add("Signs point to yes");
                ScreenSaverComments.Add("Outlook good");
                ScreenSaverComments.Add("My source says no");
                ScreenSaverComments.Add("Concentrate and ask again");

                //Lyrics
                ScreenSaverComments.Add("I say love is a flower");  //Bette Midler, The Rose
                ScreenSaverComments.Add("Love is all you need");
                ScreenSaverComments.Add("It was the best of times"); 
                ScreenSaverComments.Add("Holding on to you is the best thing I'll ever do"); //Moby, Love Should
                
                //Emoticons
                ScreenSaverComments.Add(";)");
                ScreenSaverComments.Add(":0");
                ScreenSaverComments.Add(":D");
                ScreenSaverComments.Add("-_-");
                ScreenSaverComments.Add("^.^");
                ScreenSaverComments.Add("^-.-^");
                ScreenSaverComments.Add("<|:-)");   //Santa
                ScreenSaverComments.Add("@>--;--"); //Rose
                ScreenSaverComments.Add("=^.^=");   //Cat
                ScreenSaverComments.Add(">^..^<");
                
                //Quotes
                ScreenSaverComments.Add("You're so silly");                 
                ScreenSaverComments.Add("The mind is a hotel at night");    
                ScreenSaverComments.Add("Live a dream");                    
                ScreenSaverComments.Add("We are what we think");
                ScreenSaverComments.Add("I think, therefore I am");
                ScreenSaverComments.Add("Imagination is greater than detail");
                ScreenSaverComments.Add("Fortune favors the brave");
                ScreenSaverComments.Add("Where there is love, there is life");
                ScreenSaverComments.Add("Never laugh at live dragons");
                ScreenSaverComments.Add("Nothing to see here");
                ScreenSaverComments.Add("You know you can do better");
                ScreenSaverComments.Add("N = R* x fp x ne x fe x fi x fc x L");       //Drake Equation
                ScreenSaverComments.Add("If music be the food of love, play on");    //William Shakespear
                ScreenSaverComments.Add("Music is my religion");                     //Jimmy Hendrix
                ScreenSaverComments.Add("Music is the shorthand of emotion");        //Tolstoy
                ScreenSaverComments.Add("When words leave off, music begins");       //Heinrich Heine 
                ScreenSaverComments.Add("Lost time is never found again");           //Ben Franklin
                ScreenSaverComments.Add("Education is the passport to the future");  //Malcolm X
                ScreenSaverComments.Add("Only two things are infinite, the universe and human stupidity");  //Albert Einstien
                ScreenSaverComments.Add("E = MC 2");
                ScreenSaverComments.Add("I have nothing to declare except my genius"); //Oscar Wilde
                ScreenSaverComments.Add("It's kind of fun to do the impossible.");     //Walt Disney
                ScreenSaverComments.Add("If you want to make an apple pie from scratch, you must first create the universe");  //Carl Sagan
                ScreenSaverComments.Add("A witty saying proves nothing"); //Voltaire
                ScreenSaverComments.Add("Life is only a dream and we are the imagination of ourselves");  //Bill Hicks
                ScreenSaverComments.Add("It's just a ride");
                

                //Configure: Folder
                ScreenSaverComments.Add("Anytime is the right time for [FOLDER]");
                ScreenSaverComments.Add("Another [FOLDER] marathon!");
                ScreenSaverComments.Add("Enjoy [FOLDER]");
                ScreenSaverComments.Add("Watch more [FOLDER]!");
                ScreenSaverComments.Add("Tell me more about [FOLDER]");
                ScreenSaverComments.Add("[FOLDER] is the best!");
                ScreenSaverComments.Add("[FOLDER] Rules!");
                ScreenSaverComments.Add("[FOLDER] is my favorite");
                ScreenSaverComments.Add("I really like [FOLDER]");
                ScreenSaverComments.Add("I really love [FOLDER]");
                ScreenSaverComments.Add("[FOLDER] is for lovers");
                ScreenSaverComments.Add("[FOLDER] is amazing");
                ScreenSaverComments.Add("I always love [FOLDER]");
                ScreenSaverComments.Add("[FOLDER] makes me happy");
                ScreenSaverComments.Add("I love [FOLDER]");
                ScreenSaverComments.Add("I like [FOLDER]");
                ScreenSaverComments.Add("I adore [FOLDER]");
                ScreenSaverComments.Add("I desire [FOLDER]");
                ScreenSaverComments.Add("I need [FOLDER]");
                ScreenSaverComments.Add("I want [FOLDER]!");
                ScreenSaverComments.Add("I want my [FOLDER].");
                ScreenSaverComments.Add("Nothing but [FOLDER] all day");
                ScreenSaverComments.Add("Are you really going to watch [FOLDER]?");
                ScreenSaverComments.Add("What I really like is [FOLDER]");
                ScreenSaverComments.Add("More [FOLDER]!");
                ScreenSaverComments.Add("You like [FOLDER]?  Me too!");
                ScreenSaverComments.Add("There is nothing better than [FOLDER].");

                //Advertisting Slogans
                ScreenSaverComments.Add("This space available for advertising");
                ScreenSaverComments.Add("A diamond is forever");
                ScreenSaverComments.Add("Between love and madness lies Obsession");
                ScreenSaverComments.Add("Breakfast of Champions");
                ScreenSaverComments.Add("Do you...Yahoo!?");
                ScreenSaverComments.Add("Got Milk?");
                ScreenSaverComments.Add("I want my MTV");
                ScreenSaverComments.Add("Just Do It");
                ScreenSaverComments.Add("Melts in your mouth, not in your hands");
                ScreenSaverComments.Add("Pork. The Other White Meat");
                ScreenSaverComments.Add("The pause that refreshes");
                ScreenSaverComments.Add("What would you do for a Klondike bar?");
                ScreenSaverComments.Add("Don't leave home without it");
                ScreenSaverComments.Add("Reach out and touch someone");
                ScreenSaverComments.Add("We try harder");
                ScreenSaverComments.Add("The quicker picker upper");
                ScreenSaverComments.Add("When you got it, flaunt it");
                ScreenSaverComments.Add("I'd walk a mile for a Camel");
                ScreenSaverComments.Add("Please don't squeeze the Charmin");
                ScreenSaverComments.Add("The antidote for civilization");
                ScreenSaverComments.Add("Hand-built by robots");
                ScreenSaverComments.Add("Say it with flowers");
                ScreenSaverComments.Add("Guinness is good for you");
                ScreenSaverComments.Add("Snap! Crackle! Pop!");
                ScreenSaverComments.Add("Finger lickin' good");
                ScreenSaverComments.Add("Betcha can't eat just one");
                ScreenSaverComments.Add("Good to the last drop");
                ScreenSaverComments.Add("Tastes so good cats ask for it by name");
                ScreenSaverComments.Add("Where do you want to go today?");
                ScreenSaverComments.Add("I love New York");
                ScreenSaverComments.Add("I <3 New York");
                ScreenSaverComments.Add("All the news that's fit to print");
                ScreenSaverComments.Add("All the news that fits");
                ScreenSaverComments.Add("It takes a licking and keeps on ticking");
                ScreenSaverComments.Add("Where's the beef?");
                ScreenSaverComments.Add("Coke adds life");
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ScreenSaverMessagesReset()
        {
            try
            {
                if (ScreenSaverCommentsTimer != null)
                {
                    ScreenSaverCommentsTimer.Dispose();
                    ScreenSaverCommentsTimer = null;
                }
                
                Int16 iSeconds = Convert.ToInt16(BediaRandom.Next(120, 280));
                ScreenSaverCommentsTimer = new Timer(ScreenSaverMessageShow, null, Convert.ToInt32(TimeSpan.FromSeconds(iSeconds).TotalMilliseconds), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private async Task<string> ScreenSaverFolderName(string sMSG)
        {
            string sReturn = "";

            try
            {
                string sHomeMenus = SettingGet("HomeMenus");

                if (sHomeMenus.Trim().Length > 0)
                {
                    string[] HomeFolders = sHomeMenus.Split(',');
                    string sToken = HomeFolders[BediaRandom.Next(0, (HomeFolders.Count() - 1))];
                    StorageFolder Homefolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(sToken);
                    List<string> Folders = new List<string>();
                    StorageFolder dirFolder = await StorageFolder.GetFolderFromPathAsync(Homefolder.Path);


                    Folders.Add(dirFolder.Name);

                    if (Folders.Count > 0)
                    {
                        string sFolderName = Folders[BediaRandom.Next(0, (Folders.Count - 1))];

                        sReturn = sReturn.Replace("[FOLDER]", sMSG);
                    }
                }
                else
                {
                    sReturn = ""; //In case there are no home folders.
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return sReturn;
        }

        private async void ScreenSaverMessageShow(object state)
        {
            try
            {
                if (SettingGetBool("ScreenSaverComments") == false) { return; }

                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (mMainPage.ScreenSaver.Opacity == 1.0)
                    {
                        string sMSG = "";
                        string sDate = DateTime.Now.ToString("MMdd");

                        //Holidays and other important days
                        switch (sDate)
                        {
                            case "1231":
                                sMSG = "New Years Countdown: ";
                                byte bHours = Convert.ToByte(23 - DateTime.Now.Hour);
                                byte bMinutes = Convert.ToByte(60 - DateTime.Now.Minute);


                                if (bHours > 0)
                                {
                                    sMSG += bHours.ToString() + " hour";
                                    if (bHours > 1)
                                    {
                                        sMSG += "s";
                                    }
                                    sMSG += " ";
                                }

                                if (bMinutes > 0)
                                {
                                    sMSG += bMinutes.ToString() + " minute";
                                    if (bMinutes > 1)
                                    {
                                        sMSG += "s";
                                    }
                                    sMSG += " ";
                                }

                                if (bHours == 0)
                                {
                                    byte bSeconds = Convert.ToByte(60 - DateTime.Now.Second);
                                    sMSG += bSeconds.ToString() + " seconds";
                                    HappyNewYear();
                                }
                                break;

                            case "0101":
                                sMSG = "Happy New Year";
                                HappyNewYear();
                                break;

                            case "0214":
                                sMSG = "Happy Valentine's Day";
                                break;

                            case "0504":
                                sMSG = "May The Force Be With You";
                                break;

                            case "0908":
                                sMSG = "Happy Anniversary";
                                break;
                                
                            case "1224":
                            case "1225":
                                sMSG = "Merry Xmas";
                                break;
                        }


                        if (sMSG.Trim().Length == 0)
                        {
                            Int16 iMsg = Convert.ToInt16(BediaRandom.Next(0, (ScreenSaverComments.Count() - 1)));

                            sMSG = ScreenSaverComments[iMsg];

                            if (sMSG.Contains("[FOLDER]"))
                            {
                                sMSG = await ScreenSaverFolderName(sMSG);
                            }
                        }

                        if (sMSG.Trim().Length > 0)
                        {
                            Storyboard sbMessage = new Storyboard();
                            TextBlock txtMessage = new TextBlock();
                            DoubleAnimation anMessage = new DoubleAnimation();
                            DateTime dtCurrentTime = new DateTime(DateTime.Now.Ticks);
                            CompositeTransform compTransform = new CompositeTransform();


                            txtMessage.Name = "txtMsg" + Guid.NewGuid().ToString().Replace("-", "");
                            txtMessage.Text = sMSG;
                            txtMessage.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                            txtMessage.FontSize = 80;
                            txtMessage.RenderTransform = compTransform;

                            if (dtCurrentTime.Hour > 20 || dtCurrentTime.Hour < 7)
                            {
                                txtMessage.Opacity = 0.5;
                            }
                            else
                            {
                                txtMessage.Opacity = 1.0;
                            }

                            mMainPage.ScreenSaver.Children.Add(txtMessage);
                            Canvas.SetTop(txtMessage, (mMainPage.ActualHeight / 4) * 3);
                            Canvas.SetLeft(txtMessage, mMainPage.ActualWidth + 44);

                            anMessage.Duration = new Duration(TimeSpan.FromSeconds(48));
                            anMessage.From = mMainPage.ActualWidth + 8;
                            anMessage.To = -7000; 
                            anMessage.AutoReverse = false;
                            Storyboard.SetTarget(anMessage, txtMessage);
                            Storyboard.SetTargetProperty(anMessage, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                            sbMessage.Children.Add(anMessage);
                            sbMessage.Completed += (sndr, evts) =>
                            {
                                mMainPage.ScreenSaver.Children.Remove(txtMessage);

                                ScreenSaverMessagesReset();
                            };
                            sbMessage.Begin();
                        }
                    } //If screen saver is at full opacity
                });   //Dispatcher
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private void HappyNewYear()
        {
            try
            {
                byte iBubbles = Convert.ToByte(BediaRandom.Next(88, 244));
                Int16 iTop = 0;
                Int16 iLeft = 0;
                double dSize = 40;
                double dStart = 400;
                byte bSeconds = 30;


                for (byte b = 0; b < iBubbles; b++)
                {
                    Storyboard sbNewYears = new Storyboard();
                    dSize = Convert.ToDouble(BediaRandom.Next(48, 88));
                    dStart = Convert.ToDouble(BediaRandom.Next(Convert.ToInt16(mMainPage.ActualWidth / 4), Convert.ToInt16(mMainPage.ActualWidth / 2)));
                    bSeconds = Convert.ToByte(BediaRandom.Next(2, 16));
                    iTop = Convert.ToInt16(BediaRandom.Next(8, (this.MenuSize * 2)));
                    iLeft = Convert.ToInt16(BediaRandom.Next(100, Convert.ToInt16(mMainPage.ActualWidth - 100)));


                    Ellipse Bubble = new Ellipse();
                    Bubble.Name = "Bubble" + Guid.NewGuid().ToString().Replace("-", "");
                    Bubble.Width = dSize;
                    Bubble.Height = dSize;
                    Bubble.StrokeThickness = BediaRandom.Next(2, 8);
                    Bubble.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                    Bubble.Fill = BediaBaseColor;
                    Bubble.Opacity = BediaRandom.NextDouble();

                    mMainPage.ScreenSaver.Children.Add(Bubble);
                    Canvas.SetTop(Bubble, mMainPage.ActualHeight + 100);
                    Canvas.SetLeft(Bubble, dStart);


                    DoubleAnimation aniTop = new DoubleAnimation();
                    aniTop.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    aniTop.From = (mMainPage.ActualHeight + 100);
                    aniTop.To = iTop;
                    Storyboard.SetTarget(aniTop, Bubble);
                    Storyboard.SetTargetProperty(aniTop, "(Canvas.TopProperty)");
                    sbNewYears.Children.Add(aniTop);

                    DoubleAnimation anLeft = new DoubleAnimation();
                    anLeft.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    anLeft.From = dStart;
                    anLeft.To = iLeft;
                    Storyboard.SetTarget(anLeft, Bubble);
                    Storyboard.SetTargetProperty(anLeft, "(Canvas.LeftProperty)");
                    sbNewYears.Children.Add(anLeft);

                    DoubleAnimation animOpacity = new DoubleAnimation();
                    animOpacity.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    animOpacity.From = Bubble.Opacity;
                    animOpacity.To = 0.0;
                    Storyboard.SetTarget(animOpacity, Bubble);
                    Storyboard.SetTargetProperty(animOpacity, "(Canvas.Opacity)");
                    sbNewYears.Children.Add(animOpacity);

                    DoubleAnimation animWidth = new DoubleAnimation();
                    animWidth.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    animWidth.From = dSize;
                    animWidth.To = (dSize * 2);
                    Storyboard.SetTarget(animWidth, Bubble);
                    Storyboard.SetTargetProperty(animWidth, "(Canvas.WidthProperty)");
                    sbNewYears.Children.Add(animWidth);

                    DoubleAnimation animHeight = new DoubleAnimation();
                    animHeight.Duration = new Duration(TimeSpan.FromSeconds(bSeconds));
                    animHeight.From = dSize;
                    animHeight.To = (dSize * 2);
                    Storyboard.SetTarget(animHeight, Bubble);
                    Storyboard.SetTargetProperty(animHeight, "(Canvas.HeightProperty)");
                    sbNewYears.Children.Add(animHeight);

                    sbNewYears.Completed += (sndr, evts) =>
                    {
                        mMainPage.ScreenSaver.Children.Remove(Bubble);
                    };
                    sbNewYears.Begin();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async void ScreenSaverShow(object state)
        {
            try
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //Just in case
                    //This happens when coming back from music
                    if ((mMainPage.ScreenSaver.Opacity == 1.0) || (IsPlaying == true))
                    {
                        ScreenSaverCancel();
                        return;
                    }

                    mMainPage.ScreenSaver.Children.Clear();

                    if (SettingGetBool("ScreenSaverComments") == true)
                    {
                        ScreenSaverMessagesReset();
                    }

                    //In case the Clock is already loaded (and how did that happen?)
                    Storyboard sbScreenSaver = new Storyboard();
                    

                    if (SettingGetBool("ScreenSaverClock") == true)
                    {
                        Storyboard sbClock = new Storyboard();
                        BediaClock bediaClock = new BediaClock(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)), this.MenuFontSize);
                        CompositeTransform compTransform = new CompositeTransform();

                        bediaClock.Height = this.MenuSize;
                        bediaClock.ClockStart(true);
                        mMainPage.ScreenSaver.Children.Add(bediaClock);
                        Canvas.SetTop(bediaClock, 100);
                        Canvas.SetLeft(bediaClock, 100);
                        
                        // - CLOCK COLON -
                        DoubleAnimation animColon = new DoubleAnimation();
                        animColon.Duration = new Duration(TimeSpan.FromSeconds(1));
                        animColon.From = 0.0;
                        animColon.To = 1.0;
                        animColon.RepeatBehavior = RepeatBehavior.Forever;
                        animColon.AutoReverse = true;
                        Storyboard.SetTarget(animColon, bediaClock.Colon);
                        Storyboard.SetTargetProperty(animColon, "(UIElement.Opacity)");
                        sbClock.Children.Add(animColon);

                        // - CLOCK MOVEMENT -
                        DoubleAnimation animClock = new DoubleAnimation();
                        animClock.Duration = new Duration(TimeSpan.FromSeconds(188));
                        animClock.From = 100;
                        animClock.To = (mMainPage.ActualWidth - 520);
                        animClock.RepeatBehavior = RepeatBehavior.Forever;
                        animClock.AutoReverse = true;
                        Storyboard.SetTarget(animClock, bediaClock);
                        Storyboard.SetTargetProperty(animClock, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                        sbClock.Children.Add(animClock);

                        bediaClock.RenderTransform = compTransform;

                        sbClock.Begin();
                    }

                    // - Screen Saver Background - 
                    DoubleAnimation animFade = new DoubleAnimation();
                    animFade.Duration = new Duration(TimeSpan.FromSeconds(1.4));
                    animFade.From = 0.0;
                    animFade.To = 1.0;
                    Storyboard.SetTarget(animFade, mMainPage.ScreenSaver);
                    Storyboard.SetTargetProperty(animFade, "(Canvas.Opacity)");
                    sbScreenSaver.Children.Add(animFade);
                    
                    sbScreenSaver.Completed += (sndr, evtArgs) =>
                    {
                        if (this.IsPaused == false)
                        {
                            //Only clear this if the user has not paused
                            MenuBreadrumbs.Clear();
                        }
                        
                        HideExtraLayers();
                        VizStop();
                        BubblesStop();
                        ScreenSaverMessagesReset();
                        mMainPage.BediaMenus.Children.Clear();

                        if (FullPageScroll == true)
                        {
                            // Aug 2016: Reset this if the screen saver starts
                            ResizeImage(1.0, 0.0, bediaTitlebar.imgTitlebarPageUpDown, bediaTitlebar.rectTitlebarPageUpDown);
                            FullPageScroll = false;
                        }
                    };
                    sbScreenSaver.Begin();
                    
                }); //Dispatcher
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ScreenSaverStop()
        {
            try
            {
                Storyboard sbFade = new Storyboard();
                
                //Because of the option "LateNight", these two lines need to be set for each load
                SetBackground(mMainPage.BediaBase);
                SetBackground(mMainPage.MediaOverlay);

                if (this.IsPaused == false)
                {
                    //Set titlebar items to blank because coming back from screen saver they show previous values and it just looks odd
                    OneMomentPlease();

                    BubblesLoad();

                    TitlebarShow("Title");

                    MenuProcessing("LoadHome", this.HomePageTitle, "Home");
                }
                else
                {
                    DoubleAnimation animMedia = new DoubleAnimation();
                    animMedia.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    animMedia.From = 0.0;
                    animMedia.To = 1.0;
                    Storyboard.SetTarget(animMedia, mMainPage.BediaMedia);
                    Storyboard.SetTargetProperty(animMedia, "(MediaElement.Opacity)");
                    sbFade.Children.Add(animMedia);

                    DoubleAnimation animMediaBack = new DoubleAnimation();
                    animMediaBack.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                    animMediaBack.From = 0.0;
                    animMediaBack.To = 1.0;
                    Storyboard.SetTarget(animMediaBack, mMainPage.MediaBackground);
                    Storyboard.SetTargetProperty(animMediaBack, "(MediaElement.Opacity)");
                    sbFade.Children.Add(animMediaBack);

                    MediaPlayPause();
                }


                DoubleAnimation animFade = new DoubleAnimation();
                animFade.Duration = new Duration(TimeSpan.FromSeconds(1.8));
                animFade.From = mMainPage.ScreenSaver.Opacity;
                animFade.To = 0.0;
                Storyboard.SetTarget(animFade, mMainPage.ScreenSaver);
                Storyboard.SetTargetProperty(animFade, "(Canvas.Opacity)");
                sbFade.Children.Add(animFade);

                sbFade.Completed += (sndr, evt) =>
                {
                    if (IsPaused == false)
                    {
                        mMainPage.ScreenSaver.Children.Clear();
                    }
                };
                sbFade.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ScreenSaverReset()
        {
            try
            {
                if (ScreenSaverTimer != null)
                {
                    ScreenSaverTimer.Dispose();
                    ScreenSaverTimer = null;
                }

                ScreenSaverTimer = new Timer(ScreenSaverShow, null, Convert.ToInt32(TimeSpan.FromMinutes(mbScreenSaverMinutes).TotalMilliseconds), Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ScreenSaverCancel()
        {
            if (ScreenSaverTimer != null)
            {
                ScreenSaverTimer.Dispose();
                ScreenSaverTimer = null;
            }
        }

        private bool PausedItemsAvailable()
        {
            //This function only check to see if there is ANY HistoryItems where there is a paused media.  Used in the MenusCreate: Home
            bool bReturn = false;

            try
            {
                if (PastListHistory.Count > 0)
                {
                    foreach (PastItem hi in PastListHistory)
                    {
                        if (hi.Seconds > 0)
                        {
                            bReturn = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return bReturn;
        }

        private async Task<PastItem> PastItemGet(StorageFile MediaFile)
        {
            PastItem hiReturn = null;

            try
            {
                //Process:
                //1) Check for file path
                //2) If no path, then check for hash
                hiReturn = PastListHistory.Find(e => (e.Path == MediaFile.Path));

                if (hiReturn == null)
                {
                    string sHash = await PastCreateHash(MediaFile);
                    Windows.Storage.FileProperties.BasicProperties MediaProperties = await MediaFile.GetBasicPropertiesAsync();

                    hiReturn = PastListHistory.Find(e => (e.Hash == sHash) && (e.Size == MediaProperties.Size));
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return hiReturn;
        }

        private async void PastSave()
        {
            try
            {
                if (this.SavePast == false) { return; }
                
                if (BediaPlaylist.Count == 0) { return; }  //Just in case, usually used when the app closes


                bool bFound = false;
                BediaItem bi = BediaPlaylist[miMediaCurrentPosition];

                switch (bi.MenuType)
                {
                    //Only saving the history of these types
                    case BediaItem.MenuTypes.Audio:
                    case BediaItem.MenuTypes.Video:
                        bFound = true;
                        break;
                }


                if (bFound == false) { return; }

                double dPosition = 0;
                double dTotalSeconds = 0;
                string sHash = await PastCreateHash(bi.BediaFile);
                Windows.Storage.FileProperties.BasicProperties MediaProperties = await bi.BediaFile.GetBasicPropertiesAsync();
                PastItem hi;


                if (mMainPage.BediaMedia.NaturalDuration.HasTimeSpan == true)
                {
                    //A value of 0.0 is set so that the media's pause icon will not show if it has been played to completion.
                    //A value is needed in this list because the lack of a value equals the media has not been Played.
                    //TODO: There is a problem, SOMETIMES, where the end of the media is reached and yet it gets saved as not completed.
                    try
                    {
                        dPosition = mMainPage.BediaMedia.Position.TotalSeconds;
                        dTotalSeconds = mMainPage.BediaMedia.NaturalDuration.TimeSpan.TotalSeconds;
                    }
                    catch { }

                    //If the media is close enough to the end, assume the user has skipped end-credits
                    if (dPosition >= (dTotalSeconds * 0.94)) { dPosition = 0.0; }
                }
                else
                {
                    dPosition = 0.0;
                }
                

                //Check the HistoryList for the existance of this path, if found, update it, then delete file and append all data to it
                //Remove ALL instances of this file within the list
                //Then add it back, new
                for (Int32 iHistory = 0; iHistory < PastListHistory.Count(); iHistory++)
                {
                    hi = PastListHistory[iHistory];
                    
                    if (hi.Path == bi.BediaFile.Path)
                    {
                        PastListHistory.Remove(hi);
                    }
                    else if ((hi.Hash == sHash) && (hi.Size.ToString() == MediaProperties.Size.ToString()))
                    {
                        PastListHistory.Remove(hi);
                    }
                }


                //Add new value
                PastItem hiNew = new PastItem();
                hiNew.Path = bi.BediaFile.Path;
                hiNew.Seconds = dPosition;
                hiNew.Size = MediaProperties.Size;
                hiNew.PlayedDate = DateTime.Now;
                hiNew.Hash = sHash;
                PastListHistory.Add(hiNew);
                BediaIO bediaIO = new BediaIO();
                StringBuilder sbPast = new StringBuilder();

                //Delete the existing file
                Windows.Storage.StorageFolder appFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile PastFile = await appFolder.CreateFileAsync(msHistoryFile, CreationCollisionOption.ReplaceExisting);
                
                foreach (PastItem hiMaster in PastListHistory)
                {
                    sbPast.Append(hiMaster.Path + "|" +
                                  hiMaster.Hash + "|" +
                                  hiMaster.Seconds.ToString() + "|" +
                                  hiMaster.Size.ToString() + "|" +
                                  hiMaster.PlayedDate.ToString() +
                                  Environment.NewLine);
                }

                await Windows.Storage.FileIO.WriteTextAsync(PastFile, sbPast.ToString());

                //Cleanup
                bi = null;
                hi = null;
                hiNew = null;
                appFolder = null;
                PastFile = null;
                PastListRefresh();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private async void PastListRefresh()
        {
            try
            {
                string[] Values;
                BediaIO bediaIO = new BediaIO();
                StorageFolder appFolder = ApplicationData.Current.LocalFolder;
                

                if (await bediaIO.FileExists(appFolder, msHistoryFile) == true)
                {
                    PastListHistory.Clear();

                    StorageFile HistoryFile = await appFolder.GetFileAsync(msHistoryFile);
                    var readFile = await Windows.Storage.FileIO.ReadLinesAsync(HistoryFile);
                    foreach(var sLine in readFile)
                    {
                        if (sLine.Trim().Length > 0)
                        {
                            Values = sLine.Split('|');

                            PastItem hi = new PastItem();
                            hi.Path = Values[0];
                            hi.Hash = Values[1];
                            hi.Seconds = Convert.ToDouble(Values[2]);
                            hi.Size = Convert.ToUInt64(Values[3]);
                            hi.PlayedDate = Convert.ToDateTime(Values[4]);
                            hi.PlayState = true;

                            PastListHistory.Add(hi);
                        }
                    }
                }

                bediaIO = null;
                appFolder = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private async void PastClear()
        {
            try
            {
                BediaIO bediaIO = new BediaIO();

                PastListHistory.Clear();

                if (await bediaIO.FileExists(ApplicationData.Current.LocalFolder, msHistoryFile) == true)
                {
                    StorageFile HistoryFile = await ApplicationData.Current.LocalFolder.GetFileAsync(msHistoryFile);
                    bediaIO.FileDelete(HistoryFile);
                }
                bediaIO = null;
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }
        
        private async void PastSync()
        {
			//TODO: Sync with OneDrive
        }

        private IOneDriveClient _client;
        private async Task<int> OneDriveTestBed()
        {
            try
            {
                //https://apps.dev.microsoft.com/#/application/sapi/0000000044183136
                //  https://github.com/OneDrive/onedrive-sdk-csharp
                //  https://dev.onedrive.com/getting-started.htm
                //  https://msdn.microsoft.com/magazine/mt614268
                //  https://msdn.microsoft.com/en-us/magazine/mt632271.aspx
                var uri = new Uri("https://api.onedrive.com/v1.0/drive/special/music");
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "YOUR TRIAL TOKEN");
                var json = await client.GetStringAsync(uri);



                var scopes = new[]
                {
                  "onedrive.readwrite",
                  "onedrive.appfolder",
                  "wl.signin"
                };

                _client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes);
                var session = await _client.AuthenticateAsync();

                Debug.WriteLine($"Token: {session.AccessToken}");

            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return 0;
        }
        
        public void PageSizeChanged(SizeChangedEventArgs e)
        {
            try
            {
                mMainPage.BediaMap.Width = mMainPage.ActualWidth;
                mMainPage.BediaMap.Height = (mMainPage.ActualHeight - this.MenuSize);

                mMainPage.BediaColors.Width = mMainPage.ActualWidth;
                mMainPage.BediaColors.Height = (mMainPage.ActualHeight - this.MenuSize);
                
                foreach (BediaMenuUI BediaMenu in mMainPage.BediaMenus.Children)
                {
                    BediaMenu.Width = mMainPage.ActualWidth;
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private async Task<string> PastCreateHash(StorageFile MediaFile)
        {
            string sReturn = "";

            try
            {
                uint capacity = 100000000;
                HashAlgorithmProvider alg = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                var stream = await MediaFile.OpenStreamForReadAsync();
                var inputStream = stream.AsInputStream();
                Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer(capacity);
                var hash = alg.CreateHash();
                sReturn = CryptographicBuffer.EncodeToHexString(hash.GetValueAndReset()).ToUpper();

                //Cleanup
                inputStream.Dispose();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return sReturn;
        }

        private void TitlebarShow(string ShowControl)
        {
            try
            {
                Storyboard sbShow = new Storyboard();
                DoubleAnimation animShow = new DoubleAnimation();
                animShow.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed));
                animShow.To = 1.0;
                CompositeTransform compTransform = new CompositeTransform();
                

                switch (ShowControl.ToLower())
                {
                    case "title":
                        bediaTitlebar.RenderTransform = compTransform;
                        animShow.From = bediaTitlebar.Opacity;
                        Storyboard.SetTarget(animShow, bediaTitlebar);
                        break;

                    case "navigation":
                        medianavigation.RenderTransform = compTransform;
                        animShow.From = medianavigation.Opacity;
                        Storyboard.SetTarget(animShow, medianavigation);
                        break;

                    case "status":
                    case "volume":
                        mediastatus.RenderTransform = compTransform;
                        animShow.From = mediastatus.Opacity;
                        Storyboard.SetTarget(animShow, mediastatus);
                        break;

                    default:
                        //Do nothing, this is a  'just in case'
                        return;
                }


                Storyboard.SetTargetProperty(animShow, "(UIElement.Opacity)");
                sbShow.Children.Add(animShow);
                sbShow.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void TitlebarHide(string ShowControl)
        {
            try
            {
                Storyboard sbTitlebar = new Storyboard();
                DoubleAnimation animTitlebar = new DoubleAnimation();
                animTitlebar.Duration = new Duration(TimeSpan.FromMilliseconds(this.AnimationSpeed / 2));
                animTitlebar.To = 0.0;

                //VERY unlikely, but just in case.  Also, this method seems sloppy.  Would like to loop through the controls in a grid cell
                if ((bediaTitlebar.Opacity > 0.0) && (ShowControl != "title"))
                {
                    animTitlebar.From = bediaTitlebar.Opacity;
                    Storyboard.SetTarget(animTitlebar, bediaTitlebar);
                    Storyboard.SetTargetProperty(animTitlebar, "(UIElement.Opacity)");
                    sbTitlebar.Children.Add(animTitlebar);
                }

                if ((medianavigation.Opacity > 0.0) && (ShowControl != "navigation"))
                {
                    animTitlebar.From = medianavigation.Opacity;
                    Storyboard.SetTarget(animTitlebar, medianavigation);
                    Storyboard.SetTargetProperty(animTitlebar, "(UIElement.Opacity)");
                    sbTitlebar.Children.Add(animTitlebar);
                }

                if ((mediastatus.Opacity > 0.0) && ((ShowControl != "status") && (ShowControl != "volume")))
                {
                    animTitlebar.From = mediastatus.Opacity;
                    Storyboard.SetTarget(animTitlebar, mediastatus);
                    Storyboard.SetTargetProperty(animTitlebar, "(UIElement.Opacity)");
                    sbTitlebar.Children.Add(animTitlebar);
                }

                if (sbTitlebar.Children.Count > 0)
                {
                    sbTitlebar.Completed +=
                        (sndr, evts) =>
                        {
                            //eg: When NEXT or PREV is selected, just hide the current menus
                            if (ShowControl.Trim().Length > 0)
                            {
                                TitlebarShow(ShowControl);
                            }
                        };
                    sbTitlebar.Begin();
                }
                else
                {
                    if (ShowControl.Trim().Length > 0)
                    {
                        TitlebarShow(ShowControl);
                    }
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void ImageChange(Windows.UI.Xaml.Controls.Image UIImage, Windows.UI.Xaml.Controls.Image Icon)
        {
            try
            {
                Storyboard sbFadeOut = new Storyboard();
                DoubleAnimation animFadeOut = new DoubleAnimation();

                animFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                animFadeOut.From = UIImage.Opacity; //From whatever value it is currently set
                animFadeOut.To = 0.0;

                Storyboard.SetTarget(animFadeOut, UIImage);
                Storyboard.SetTargetProperty(animFadeOut, "(UIElement.Opacity)");

                sbFadeOut.Children.Add(animFadeOut);

                sbFadeOut.Completed +=
                    (sndr, evtArgs) =>
                    {
                        UIImage.Source = Icon.Source;
                        Storyboard sbFadeIn = new Storyboard();
                        DoubleAnimation animFadeIn = new DoubleAnimation();

                        animFadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        animFadeIn.From = 0.0;
                        animFadeIn.To = 1.0;

                        Storyboard.SetTarget(animFadeIn, UIImage);
                        Storyboard.SetTargetProperty(animFadeIn, "(UIElement.Opacity)");

                        sbFadeIn.Children.Add(animFadeIn);
                        sbFadeIn.Begin();
                    };

                sbFadeOut.Begin();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private BediaIcon NavigationIcon(NavigationItem.NavigationType NavType)
        {
            BediaIcon biReturn = new BediaIcon();
            biReturn.Icon = IconByName("Menu");    //Default, just in case
            biReturn.Fill = this.BediaBaseColor;
            biReturn.Stroke = this.BediaHighlightColor;
            biReturn.StrokeThickness = 4;
            biReturn.Size = (this.MenuSize - (biReturn.StrokeThickness * 2)); //Always set size after stroke thickness

            
            try
            {
                string sIcon = "Menu";

                switch (NavType)
                {
                    case NavigationItem.NavigationType.JumpBackSeconds30:
                    case NavigationItem.NavigationType.JumpForwardSeconds30:
                        sIcon = "Nav30Seconds";
                        break;

                    case NavigationItem.NavigationType.JumpBackMinute1:
                    case NavigationItem.NavigationType.JumpForwardMinute1:
                        sIcon = "Nav1Minute";
                        break;

                    case NavigationItem.NavigationType.JumpBackMinute5:
                    case NavigationItem.NavigationType.JumpForwardMinute5:
                        sIcon = "Nav5Minutes";
                        break;

                    
                    case NavigationItem.NavigationType.JumpForwardMinute10:
                    case NavigationItem.NavigationType.JumpBackMinute10:
                        sIcon = "Nav10Minutes";
                        break;

                    case NavigationItem.NavigationType.VolumeDown:
                        sIcon = "NavVolumeDown";
                        break;

                    case NavigationItem.NavigationType.VolumeUp:
                        sIcon = "NavVolumeUp";
                        break;

                    case NavigationItem.NavigationType.PlayPause:
                        if (this.IsPaused == true)
                        {
                            sIcon = "Play";
                        }
                        else
                        {
                            sIcon = "Pause";
                        }
                        break;

                    case NavigationItem.NavigationType.Menu:
                        sIcon = "Menu";
                        break;

                    case NavigationItem.NavigationType.Previous:
                        sIcon = "NavPrior";
                        break;

                    case NavigationItem.NavigationType.Next:
                        sIcon = "NavNext";
                        break;
                }

                biReturn.Icon = IconByName(sIcon);
            }
            catch (Exception ex)
            {
                logException(ex);
            }

            return biReturn;
        }
    }
}
