using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;
using System.Net.Http;
using Windows.UI;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO.Compression;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.ApplicationModel.Activation;
using System.ComponentModel;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.OpenApi.Writers;
using Windows.Devices.Enumeration;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace iShell
{
    public sealed partial class MainPage : Page
    {
        int refreshTime = 0;
        int activeItem = 0;
        Windows.UI.Xaml.Media.Brush internetFail = null;
        Windows.UI.Xaml.Media.Brush internetSuccess = null;
        Windows.UI.Xaml.Media.AcrylicBrush tileBorder = new Windows.UI.Xaml.Media.AcrylicBrush();
        Boolean internet = true;
        List<ListView> lvWidgets = new System.Collections.Generic.List<ListView>();
        Boolean firstLoad = true;
        TextBlock clockTime;

        public MainPage()
        {
            this.InitializeComponent();
            tbTimeDate.Visibility = Visibility.Collapsed;
            cmdRefresh.Visibility = Visibility.Collapsed;
            recTopBar.Visibility = Visibility.Collapsed;
            lvMain.Visibility = Visibility.Collapsed;
            pgbRefresh.Visibility = Visibility.Collapsed;
            networkStatus.Visibility = Visibility.Collapsed;
            tbLanguage.Visibility = Visibility.Collapsed;
            splashText.Text = "";
            loadSpinner.IsActive = true;
            loadText.Text = "Willkommen \n" + WindowsIdentity.GetCurrent().Name.Substring(WindowsIdentity.GetCurrent().Name.IndexOf("\\") + 1) + "";
            tbVer.Text = SetVersionLabel();

            DispatcherTimer tClock = new DispatcherTimer();
            tClock.Tick += tClock_Tick;
            tClock.Interval = TimeSpan.FromSeconds(1);
            tClock.Start();

            Windows.UI.Xaml.Media.AcrylicBrush internetFailBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
            internetFailBrush.TintColor = Color.FromArgb(200, 255, 165, 70);
            internetFail = internetFailBrush;
            internetSuccess = recTopBar.Fill;
            tbLanguage.Text = "DE";
            testInternet();
        }

        private void testInternet()
        {


            HttpClient client = new HttpClient();
            try
            {
                if (client.GetAsync(new Uri("https://one.one.one.one/").AbsoluteUri).Result.IsSuccessStatusCode)
                {
                    internet = true;
                    recTopBar.Fill = internetSuccess;
                    var b = new Binding
                    {
                        Source = "M 26,24L 29,24L 29,33L 26,33L 26,24 Z M 31,25L 34,25L 34,39L 31,39L 29,41L 29,51L 26,51L 26,41L 24,39L 21,39L 21,25L 24,25L 24,35L 31,35L 31,25 Z M 30,48L 30,44L 51,44L 51,30L 35,30L 35,26L 56,26L 56,48L 42,48L 42,51L 46,51L 47,54L 33,54L 34,51L 38,51L 38,48L 30,48 Z"
                    };
                    BindingOperations.SetBinding(networkStatus, Windows.UI.Xaml.Shapes.Path.DataProperty, b);
                    if (splashText.Text.Equals("⚠️ Kein Netzwerkzugriff"))
                        splashText.Text = "Übersicht";
                }
            }
            catch (System.AggregateException e)
            {
                internet = false;
                recTopBar.Fill = internetFail;
                System.Console.WriteLine(e.Message);
                var b = new Binding
                {
                    Source = "M 48.0542,39.5833L 53.0417,44.5708L 58.0291,39.5834L 60.1666,41.7209L 55.1792,46.7083L 60.1667,51.6958L 58.0292,53.8333L 53.0417,48.8458L 48.0542,53.8333L 45.9167,51.6958L 50.9042,46.7083L 45.9167,41.7208L 48.0542,39.5833 Z M 24,24L 27,24L 27,33L 24,33L 24,24 Z M 29,25L 32,25L 32,39L 29,39L 27,41L 27,51L 24,51L 24,41L 22,39L 19,39L 19,25L 22,25L 22,35L 29,35L 29,25 Z M 28,48L 28,44L 45.5,44L 48.0541,46.7083L 47,48L 40,48L 40,51L 44,51L 45,54L 31,54L 32,51L 36,51L 36,48L 28,48 Z M 49,30L 33,30L 33,26L 54,26L 54,40.75L 53.0416,41.7209L 49,37.75L 49,30 Z "
                };
                BindingOperations.SetBinding(networkStatus, Windows.UI.Xaml.Shapes.Path.DataProperty, b);
                if (splashText.Text.Equals("Übersicht"))
                    splashText.Text = "⚠️ Kein Netzwerkzugriff";
                    
            }
            ToolTip toolTip = new ToolTip();
            toolTip.Placement = PlacementMode.Bottom;
            if (internet)
                toolTip.Content = "Verbunden";
            if (!internet)
                toolTip.Content = "Nicht verbunden\nPing an '1.1.1.1' nicht erfolgreich";
            ToolTipService.SetToolTip(networkStatusCanvas, toolTip);
        }

        private void loadItems()
        {
            ElementsVisible();

            if (internet)
            {
                lvMain.Items.Clear();
                //Add tiles here to load

                if (firstLoad)
                {
                    lvMain.Items.Add(Clock());
                    lvMain.Items.Add(CalendarTile());
                    lvMain.Items.Add(DWDWeather(0, "Q055", null));
                    lvMain.Items.Add(DWDWeather(1, "49.5182", "9.3213"));
                    lvMain.Items.Add(DBInfo(0));
                    lvMain.Items.Add(DBInfo(1));
                    lvMain.Items.Add(DBInfo(2));
                    lvMain.Items.Add(DBInfo(3));
                    
                    foreach (ListView item in lvMain.Items)
                    {
                        lvWidgets.Add(item);
                    }
                }
                if (!firstLoad)
                {
                    List<ListView> reloadWidgets = new System.Collections.Generic.List<ListView>();
                    reloadWidgets.Add(DBInfo(0));
                    reloadWidgets.Add(DBInfo(1));
                    reloadWidgets.Add(DBInfo(2));
                    reloadWidgets.Add(DBInfo(3));
                    reloadWidgets.Add(DWDWeather(0, "Q055", null));
                    reloadWidgets.Add(DWDWeather(1, "49.5182", "9.3213"));
                    reloadWidgets.Add(CalendarTile());
                    reloadWidgets.Add(Clock());
                    for (int i = 0; i < reloadWidgets.Count; i++)
                    {
                        for (int j = 0; j < reloadWidgets.Count; j++) { 
                            if (lvWidgets[i].Name.Equals(reloadWidgets[j].Name)) {
                                lvWidgets[i]=reloadWidgets[j];
                                lvMain.Items.Add(lvWidgets[i]);
                                break;
                            }   
                        }
                    }
                }

                //Design for widgets
                foreach (ListView item in lvMain.Items)
                {
                    if (item.Background == null)
                    item.Background = recTopBar.Fill;
                    item.PointerEntered += lvItem_PointerEntered;
                    item.PointerExited += lvItem_PointerExited;
                    item.CornerRadius = new CornerRadius(20,20,20,20);
                    item.ShowsScrollingPlaceholders = false;

                }
                firstLoad = false;
            }

            InitAnim(0);
        }

        private String SetVersionLabel()
        {
            var ProcessArch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            var BuildDate = GetLinkerTimestampUtc(Assembly.GetExecutingAssembly()).ToString();
            BuildDate = BuildDate.Replace(":", "");
            BuildDate = BuildDate.Replace(".", "");
            BuildDate = BuildDate.Replace(" ", "_");

            return Assembly.GetExecutingAssembly().GetName().Name + "\nInternal" + " \nBuild 8." + ProcessArch + ".iot." + BuildDate + "\n" + System.Environment.OSVersion;
        }

        private void tClock_Tick(object sender, object e)
        {
            tbTimeDate.Text = System.DateTime.Now.ToString("HH:mm") + " Uhr" + System.Environment.NewLine +
                System.DateTime.Now.Date.ToString("d");

            if (clockTime!=null)
            clockTime.Text = System.DateTime.Now.ToString("HH:mm:ss") + " Uhr\n" + "---------------\n"+
                    System.DateTime.Now.ToString("dddd") + System.Environment.NewLine + System.DateTime.Now.Date.ToString("d");

            refreshTime++;
            pgbRefresh.Value = refreshTime/3;
            if (refreshTime > 300)
            {
                refreshTime = 0;
                splashText.Text = "";
                ElementsNotVisible();
                loadSpinner.IsActive = true;
                loadText.Text = "Aktualisiere Inhalte...";
            }
            if (refreshTime % 10 == 0)
            {
                activeItem++;
                if (activeItem==lvMain.Items.Count)
                    activeItem = 0;
                lvMain.ScrollIntoView(lvMain.Items[activeItem]);
                bool internetBefore = internet;
                testInternet();
                if (internet && !internetBefore)
                {
                    refreshTime = 0;
                    splashText.Text = "";
                    loadSpinner.IsActive = true;
                    loadText.Text = "Aktualisiere Inhalte...";
                }

                }
            if (refreshTime == 1 && (splashText.Text.Equals(""))) {
                if (internet == true)
                    splashText.Text = "Übersicht";
                else
                    splashText.Text = "⚠️ Kein Netzwerkzugriff";
                refreshTime = 0;
                loadSpinner.IsActive = false;
                loadText.Text = "";
                loadItems();
                }

        }

        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampUtc(location);
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970 + 7200);
        }

        private async void pageScroll(WebView2 webView)
        {
            if (webView != null) {
                await webView.EnsureCoreWebView2Async();
                await Task.Delay(3000);
                await webView.ExecuteScriptAsync("window.scroll(0,340);");
                await webView.ExecuteScriptAsync("document.getElementById('app-header').style.display = 'none';");
            }
        }

        private void InitAnim(int direct)
        {
            TranslateTransform moveTransform = new TranslateTransform();

            CompositeTransform3D rotateProject = new CompositeTransform3D();

            ScaleTransform scaleTransformI = new ScaleTransform();

            if (direct == 0)
            {
                rotateProject.RotationX = -10;
                rotateProject.RotationY = -95;
                moveTransform.X = -100;
                moveTransform.Y = -20;
                scaleTransformI.ScaleX = 1.5;
            }
            if (direct == 1)
            {
                rotateProject.RotationX = -10;
                rotateProject.RotationY = -95;
                moveTransform.X = 100;
                moveTransform.Y = 20;
                scaleTransformI.ScaleX = 0.5;
            }


            // Objects for Anim.
            splashText.RenderTransform = moveTransform;
            splashText.Transform3D = rotateProject;
            splashText.RenderTransform = scaleTransformI;

            lvMain.RenderTransform = moveTransform;
            lvMain.Transform3D = rotateProject;
            lvMain.RenderTransform = scaleTransformI;

            recTopBar.RenderTransform = moveTransform;
            recTopBar.Transform3D = rotateProject;
            recTopBar.RenderTransform = scaleTransformI;

            tbTimeDate.RenderTransform = moveTransform;
            tbTimeDate.Transform3D= rotateProject;
            tbTimeDate.RenderTransform= scaleTransformI;

            networkStatus.RenderTransform = moveTransform;
            networkStatus.Transform3D = rotateProject;
            networkStatus.RenderTransform = scaleTransformI;

            tbLanguage.RenderTransform = moveTransform;
            tbLanguage.Transform3D = rotateProject;
            tbLanguage.RenderTransform= scaleTransformI;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.5));
            DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
            DoubleAnimation myDoubleAnimationY = new DoubleAnimation();
            DoubleAnimation my3dRoationX = new DoubleAnimation();
            DoubleAnimation my3dRoationY = new DoubleAnimation();
            DoubleAnimation scaleTransformA = new DoubleAnimation();
            myDoubleAnimationX.Duration = duration;
            myDoubleAnimationY.Duration = duration;
            my3dRoationX.Duration = duration;
            my3dRoationY.Duration = duration;
            scaleTransformA.Duration = duration;
            Storyboard justintimeStoryboard = new Storyboard();
            justintimeStoryboard.Duration = duration;
            justintimeStoryboard.Children.Add(myDoubleAnimationX);
            justintimeStoryboard.Children.Add(myDoubleAnimationY);
            justintimeStoryboard.Children.Add(my3dRoationX);
            justintimeStoryboard.Children.Add(my3dRoationY);
            justintimeStoryboard.Children.Add(scaleTransformA);
            Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
            Storyboard.SetTarget(myDoubleAnimationY, moveTransform);
            Storyboard.SetTarget(my3dRoationX, rotateProject);
            Storyboard.SetTarget(my3dRoationY, rotateProject);
            Storyboard.SetTarget(scaleTransformA, scaleTransformI);
            Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
            Storyboard.SetTargetProperty(myDoubleAnimationY, "Y");
            Storyboard.SetTargetProperty(my3dRoationX, "rotateProject.RotationX");
            Storyboard.SetTargetProperty(my3dRoationY, "rotateProject.RotationY");
            Storyboard.SetTargetProperty(scaleTransformA, "scaleTransformI.ScaleX");

            myDoubleAnimationX.To = 0;
            myDoubleAnimationY.To = 0;
            my3dRoationX.To = 0;
            my3dRoationY.To = 0;
            scaleTransformA.To = 1;


            justintimeStoryboard.Begin();
        }

        private void ElementsNotVisible()
        {
            tbTimeDate.Visibility = Visibility.Collapsed;
            cmdRefresh.Visibility = Visibility.Collapsed;
            recTopBar.Visibility = Visibility.Collapsed;
            lvMain.Visibility = Visibility.Collapsed;
            pgbRefresh.Visibility = Visibility.Collapsed;
            networkStatus.Visibility = Visibility.Collapsed;
            tbLanguage.Visibility = Visibility.Collapsed;
        }

        private void ElementsVisible() 
        {
            tbTimeDate.Visibility = Visibility.Visible;
            cmdRefresh.Visibility = Visibility.Visible;
            recTopBar.Visibility = Visibility.Visible;
            lvMain.Visibility = Visibility.Visible;
            pgbRefresh.Visibility = Visibility.Visible;
            networkStatus.Visibility = Visibility.Visible;
            tbLanguage.Visibility = Visibility.Visible;
        }

        //---------------------------------------------------------------------------------------
        //Tiles
        public ListView DBInfo(int route)
        {
            ListView grid = new ListView();
            TextBlock textBlock = new TextBlock();
            String StartStation = "";
            String EndStation = "";
            String URLString = "";
            Windows.UI.Xaml.Media.AcrylicBrush acrylicBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
            acrylicBrush.TintColor = Colors.OrangeRed;
            acrylicBrush.TintOpacity = 0.5;
            acrylicBrush.Opacity = 0.5;


            //Eicholzheim - Eberbach
            if (route == 0)
            {
                StartStation = "Eicholzheim";
                EndStation = "Eberbach";
                URLString = "https://www.bahn.de/buchung/fahrplan/suche#sts=true&so=Eicholzheim&zo=Eberbach&kl=2&r=13:16:KLASSENLOS:1&soid=A%3D1%40O%3DEicholzheim%40X%3D9290316%40Y%3D49432437%40U%3D80%40L%3D8001707%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014153%40&zoid=A%3D1%40O%3DEberbach%40X%3D8984152%40Y%3D49465769%40U%3D80%40L%3D8000369%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014121%40&sot=ST&zot=ST&soei=8001707&zoei=8000369";
            }
            //Eberbach - Eicholzheim
            if (route == 1)
            {
                StartStation = "Eberbach";
                EndStation = "Eicholzheim";
                URLString = "https://www.bahn.de/buchung/fahrplan/suche#sts=true&so=Eberbach&zo=Eicholzheim&kl=2&r=13:16:KLASSENLOS:1&soid=A%3D1%40O%3DEberbach%40X%3D8984152%40Y%3D49465769%40U%3D80%40L%3D8000369%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014121%40&zoid=A%3D1%40O%3DEicholzheim%40X%3D9290316%40Y%3D49432437%40U%3D80%40L%3D8001707%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014153%40&sot=ST&zot=ST&soei=8000369&zoei=8001707";
            }

            //Mosbach - Eicholzheim
            if (route == 2)
            {
                StartStation = "Mosbach (Baden)";
                EndStation = "Eicholzheim";
                URLString = "https://www.bahn.de/buchung/fahrplan/suche#sts=true&so=Mosbach(Baden)&zo=Eicholzheim&kl=2&r=13:16:KLASSENLOS:1&soid=A%3D1%40O%3DMosbach(Baden)%40X%3D9143657%40Y%3D49352559%40U%3D80%40L%3D8004094%40B%3D1%40p%3D1723671445%40i%3DU%C3%97008014136%40&zoid=A%3D1%40O%3DEicholzheim%40X%3D9290316%40Y%3D49432437%40U%3D80%40L%3D8001707%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014153%40&sot=ST&zot=ST&soei=8004094&zoei=8001707";
            }

            //Eicholzheim - Mosbach
            if (route == 3)
            {
                StartStation = "Eicholzheim";
                EndStation = "Mosbach (Baden)";
                URLString = "https://www.bahn.de/buchung/fahrplan/suche#sts=true&so=Eicholzheim&zo=Mosbach(Baden)&kl=2&r=13:16:KLASSENLOS:1&soid=A%3D1%40O%3DEicholzheim%40X%3D9290316%40Y%3D49432437%40U%3D80%40L%3D8001707%40B%3D1%40p%3D1723490478%40i%3DU%C3%97008014153%40&zoid=A%3D1%40O%3DMosbach(Baden)%40X%3D9143657%40Y%3D49352559%40U%3D80%40L%3D8004094%40B%3D1%40p%3D1723671445%40i%3DU%C3%97008014136%40&sot=ST&zot=ST&soei=8001707&zoei=8004094";
            }


            Uri uri = new Uri(URLString);
            textBlock.Text = "DB: Von " + StartStation + " nach " + EndStation + ":";
            textBlock.FontWeight = FontWeights.Bold;
            grid.Items.Add(textBlock);
            TextBlock seperator = new TextBlock();
            seperator.Text = "---------------------------------------------------------------------------";
            grid.Items.Add(seperator);



            WebView2 webView = new WebView2();
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.Source = uri;
            webView.Width = 350;
            webView.Height = 200;
            webView.IsHitTestVisible = false;

            grid.Items.Add(webView);
            grid.Height = 300;
            grid.Name = "DB" + route;
            grid.Background = acrylicBrush;

            return grid;

        }

        public ListView DWDWeather(int Option, String arg1, String arg2)
        {

            //Station IDs:https://www.dwd.de/DE/leistungen/klimadatendeutschland/statliste/statlex_html.html?view=nasPublication&nn=16102
            Dictionary<int,String> map = new Dictionary<int,String>();
            Dictionary<int, String> mapIcon = new Dictionary<int, String>();

            //Weather Ressources
            {
                map.Add(1, "Sonne"); mapIcon.Add(1, "M481.653-54.919v125.932c0 16.797 13.617 30.414 30.414 30.414s30.414-13.617 30.414-30.414v0-125.932c0-16.797-13.617-30.414-30.414-30.414s-30.414 13.617-30.414 30.414v0zM831.093 64.733v0l-89.024 89.024c-5.783 5.546-9.377 13.336-9.377 21.965 0 16.797 13.617 30.414 30.414 30.414 8.629 0 16.419-3.594 21.955-9.367l0.010-0.011 88.944-89.131c4.55-5.285 7.321-12.215 7.321-19.792 0-16.797-13.617-30.414-30.414-30.414-7.644 0-14.629 2.82-19.972 7.476l0.036-0.031zM150.039 64.733c-5.501 5.503-8.903 13.105-8.903 21.501s3.402 15.998 8.903 21.501l89.050 89.024c5.505 5.508 13.112 8.915 21.514 8.915 16.798 0 30.416-13.618 30.416-30.416 0-8.396-3.402-15.997-8.902-21.501l-89.050-89.024c-5.505-5.509-13.112-8.916-21.514-8.916s-16.010 3.408-21.514 8.916v0zM269.637 426.761c0 133.905 108.552 242.457 242.457 242.457s242.457-108.552 242.457-242.457c0-133.905-108.552-242.457-242.457-242.457v0c-133.905 0-242.457 108.552-242.457 242.457v0zM330.465 426.761c0-100.37 81.366-181.736 181.736-181.736s181.736 81.366 181.736 181.736c0 100.37-81.366 181.736-181.736 181.736v0c-100.37 0-181.736-81.366-181.736-181.736v0zM867.654 396.347c-16.797 0-30.414 13.617-30.414 30.414s13.617 30.414 30.414 30.414v0h125.932c16.797 0 30.414-13.617 30.414-30.414s-13.617-30.414-30.414-30.414v0zM30.414 396.347c-16.797 0-30.414 13.617-30.414 30.414s13.617 30.414 30.414 30.414v0h126.012c16.797 0 30.414-13.617 30.414-30.414s-13.617-30.414-30.414-30.414v0zM741.989 656.736c-5.509 5.505-8.916 13.112-8.916 21.514s3.408 16.010 8.916 21.514v0l89.024 89.050c5.506 5.506 13.112 8.912 21.514 8.912 16.804 0 30.426-13.622 30.426-30.426 0-8.402-3.406-16.008-8.912-21.514l-89.050-89.050c-5.508-5.502-13.114-8.905-21.514-8.905s-16.007 3.403-21.515 8.906v0zM239.009 656.736l-88.97 88.944c-5.506 5.506-8.912 13.112-8.912 21.514 0 16.804 13.622 30.426 30.426 30.426 8.402 0 16.008-3.406 21.514-8.912l89.050-89.050c5.508-5.505 8.915-13.112 8.915-21.514 0-16.798-13.618-30.416-30.416-30.416-8.396 0-15.997 3.402-21.501 8.902v0zM481.573 782.348v125.905c0 16.797 13.617 30.414 30.414 30.414s30.414-13.617 30.414-30.414v0-125.959c0-16.797-13.617-30.414-30.414-30.414s-30.414 13.617-30.414 30.414v0z");
                map.Add(2, "Sonne, leicht bewölkt"); mapIcon.Add(2, "M260.522-85.27c-0.855-0.010-1.864-0.016-2.875-0.016-143.883 0-260.522 116.64-260.522 260.522s116.64 260.522 260.522 260.522c1.011 0 2.021-0.006 3.029-0.017l-0.154 0.001c4.030 0 8.061-0.317 12.091-0.508 61.189 108.579 175.745 180.706 307.147 180.706 55.384 0 107.774-12.813 154.37-35.637l-2.073 0.917c24.089 26.012 53.339 46.869 86.165 61l1.649 0.632c29.159 12.674 63.125 20.048 98.813 20.048 140.027 0 253.54-113.514 253.54-253.54 0-96.009-53.365-179.555-132.051-222.592l-1.323-0.662c17.178-26.787 27.383-59.473 27.383-94.542 0-97.698-79.2-176.898-176.898-176.898-0.099 0-0.197 0-0.296 0h0.015zM72.232 175.285c0.126-103.939 84.352-188.164 188.279-188.291h628.547c57.952 0.015 104.926 46.997 104.926 104.952 0 57.963-46.988 104.952-104.952 104.952-25.871 0-49.555-9.361-67.851-24.88l0.152 0.126c-6.332-5.603-14.708-9.023-23.882-9.023-19.946 0-36.116 16.17-36.116 36.116 0 11.284 5.175 21.359 13.281 27.982l0.065 0.051c23.11 19.623 51.683 33.507 83.126 39.1l1.071 0.158c-1.045 153.306-125.566 277.181-279.019 277.181-96.011 0-180.696-48.492-230.898-122.321l-0.623-0.972c28.679-10.365 53.48-24.462 75.375-42.033l-0.509 0.395c8.429-6.673 13.787-16.904 13.787-28.387 0-19.946-16.17-36.116-36.116-36.116-8.676 0-16.637 3.059-22.865 8.158l0.064-0.051c-31.677 25.697-72.486 41.258-116.929 41.258-0.174 0-0.348 0-0.522-0.001h0.027c-104.023-0.036-188.345-84.336-188.418-188.347v-0.007zM847.959 576.335v0c-20.342-8.864-37.783-20.5-52.802-34.675l0.088 0.083c83.037-64.796 135.934-164.874 135.958-277.307v-0.004q0-0.317 0-0.666c20.472-5.105 38.469-13.116 54.622-23.707l-0.67 0.412c67.612 27.355 114.44 92.473 114.44 168.527 0 96.707-75.713 175.73-171.096 181.026l-0.47 0.021q-5.078 0.286-10.156 0.286c-0.127 0-0.277 0.001-0.427 0.001-25.055 0-48.907-5.147-70.558-14.441l1.165 0.445zM1221.145 17.302l-86.259 96.002c-5.806 6.385-9.361 14.907-9.361 24.259 0 19.946 16.17 36.116 36.116 36.116 10.726 0 20.36-4.676 26.974-12.1l0.032-0.036 86.259-96.129c5.806-6.385 9.361-14.907 9.361-24.259 0-19.946-16.17-36.116-36.116-36.116-10.726 0-20.36 4.676-26.974 12.1l-0.032 0.036zM1409.468 346.279l-128.976 6.982c-19.091 1.065-34.169 16.806-34.169 36.068 0 19.949 16.172 36.121 36.121 36.121 0.687 0 1.369-0.019 2.046-0.057l-0.094 0.004 128.976-6.982c19.008-1.158 33.984-16.856 33.984-36.053 0-19.877-16.058-36.004-35.91-36.115h-0.011zM1162.846 628.731c-5.721 6.364-9.22 14.825-9.22 24.104 0 10.668 4.625 20.256 11.98 26.867l0.033 0.029 96.129 86.259c6.372 5.716 14.837 9.21 24.12 9.21 19.985 0 36.186-16.201 36.186-36.186 0-10.703-4.647-20.321-12.033-26.946l-0.034-0.030-96.161-86.386c-6.366-5.73-14.833-9.235-24.119-9.235-10.66 0-20.241 4.618-26.852 11.963l-0.029 0.033zM647.672 656.659l-86.227 96.161c-5.806 6.385-9.361 14.907-9.361 24.259 0 19.946 16.17 36.116 36.116 36.116 10.726 0 20.36-4.676 26.974-12.1l0.032-0.036 86.164-96.319c5.806-6.385 9.361-14.907 9.361-24.259 0-19.946-16.17-36.116-36.116-36.116-10.726 0-20.36 4.676-26.974 12.1l-0.032 0.036zM935.964 737.396c-19.092 1.060-34.173 16.801-34.173 36.063 0 0.711 0.021 1.416 0.061 2.117l-0.004-0.097 6.982 129.008c1.065 19.091 16.806 34.169 36.068 34.169 19.949 0 36.121-16.172 36.121-36.121 0-0.687-0.019-1.369-0.057-2.046l0.004 0.094-6.982-129.008c-1.056-19.092-16.792-34.174-36.052-34.18h-0.001z");
                map.Add(3, "Sonne, bewölkt"); mapIcon.Add(3, "M280.781 666.829c0.24 0.001 0.523 0.002 0.807 0.002 47.855 0 91.802-16.734 126.308-44.671l-0.377 0.296c6.586-5.305 15.053-8.516 24.271-8.516 12.244 0 23.164 5.665 30.284 14.516l0.059 0.075c5.305 6.586 8.516 15.053 8.516 24.271 0 12.244-5.665 23.164-14.516 30.284l-0.075 0.059c-23.059 18.528-49.801 33.737-78.778 44.299l-1.948 0.621c54.902 80.534 146.087 132.803 249.5 133.12h0.049c165.352-0.208 299.469-133.642 300.782-298.712l0.001-0.125c-35.048-6.179-65.852-21.139-91.035-42.518l0.274 0.227c-8.41-7.164-13.71-17.763-13.71-29.6 0-9.557 3.454-18.307 9.183-25.070l-0.047 0.057c7.17-8.402 17.769-13.696 29.604-13.696 9.553 0 18.301 3.449 25.066 9.169l-0.057-0.047c19.567 16.607 45.108 26.711 73.008 26.726h0.003c62.475-0.058 113.108-50.679 113.186-113.144v-0.008c-0.078-62.48-50.706-113.108-113.179-113.186h-677.179c-112.035 0.136-202.821 90.921-202.957 202.944v0.013c0.31 111.896 91.034 202.5 202.946 202.615h0.011zM280.781 183.228h677.274c105.354 0.155 190.718 85.52 190.874 190.859v0.015c-0.202 89.206-61.48 164.043-144.225 184.925l-1.32 0.282v0.785c-0.175 209.009-169.563 378.398-378.556 378.573h-0.017c-141.518-0.266-264.864-77.813-330.11-192.678l-0.984-1.882c-4.369 0.205-8.704 0.546-13.073 0.546-154.913-0.175-280.45-125.7-280.644-280.591v-0.019c0.175-154.932 125.715-280.484 280.625-280.678h0.019z");
                map.Add(4, "Sonne, Wolken"); mapIcon.Add(4, "M260.522-85.27c-0.855-0.010-1.864-0.016-2.875-0.016-143.883 0-260.522 116.64-260.522 260.522s116.64 260.522 260.522 260.522c1.011 0 2.021-0.006 3.029-0.017l-0.154 0.001c4.030 0 8.061-0.317 12.091-0.508 61.189 108.579 175.745 180.706 307.147 180.706 55.384 0 107.774-12.813 154.37-35.637l-2.073 0.917c24.089 26.012 53.339 46.869 86.165 61l1.649 0.632c29.159 12.674 63.125 20.048 98.813 20.048 140.027 0 253.54-113.514 253.54-253.54 0-96.009-53.365-179.555-132.051-222.592l-1.323-0.662c17.178-26.787 27.383-59.473 27.383-94.542 0-97.698-79.2-176.898-176.898-176.898-0.099 0-0.197 0-0.296 0h0.015zM72.232 175.285c0.126-103.939 84.352-188.164 188.279-188.291h628.547c57.952 0.015 104.926 46.997 104.926 104.952 0 57.963-46.988 104.952-104.952 104.952-25.871 0-49.555-9.361-67.851-24.88l0.152 0.126c-6.332-5.603-14.708-9.023-23.882-9.023-19.946 0-36.116 16.17-36.116 36.116 0 11.284 5.175 21.359 13.281 27.982l0.065 0.051c23.11 19.623 51.683 33.507 83.126 39.1l1.071 0.158c-1.045 153.306-125.566 277.181-279.019 277.181-96.011 0-180.696-48.492-230.898-122.321l-0.623-0.972c28.679-10.365 53.48-24.462 75.375-42.033l-0.509 0.395c8.429-6.673 13.787-16.904 13.787-28.387 0-19.946-16.17-36.116-36.116-36.116-8.676 0-16.637 3.059-22.865 8.158l0.064-0.051c-31.677 25.697-72.486 41.258-116.929 41.258-0.174 0-0.348 0-0.522-0.001h0.027c-104.023-0.036-188.345-84.336-188.418-188.347v-0.007zM847.959 576.335v0c-20.342-8.864-37.783-20.5-52.802-34.675l0.088 0.083c83.037-64.796 135.934-164.874 135.958-277.307v-0.004q0-0.317 0-0.666c20.472-5.105 38.469-13.116 54.622-23.707l-0.67 0.412c67.612 27.355 114.44 92.473 114.44 168.527 0 96.707-75.713 175.73-171.096 181.026l-0.47 0.021q-5.078 0.286-10.156 0.286c-0.127 0-0.277 0.001-0.427 0.001-25.055 0-48.907-5.147-70.558-14.441l1.165 0.445zM1221.145 17.302l-86.259 96.002c-5.806 6.385-9.361 14.907-9.361 24.259 0 19.946 16.17 36.116 36.116 36.116 10.726 0 20.36-4.676 26.974-12.1l0.032-0.036 86.259-96.129c5.806-6.385 9.361-14.907 9.361-24.259 0-19.946-16.17-36.116-36.116-36.116-10.726 0-20.36 4.676-26.974 12.1l-0.032 0.036zM1409.468 346.279l-128.976 6.982c-19.091 1.065-34.169 16.806-34.169 36.068 0 19.949 16.172 36.121 36.121 36.121 0.687 0 1.369-0.019 2.046-0.057l-0.094 0.004 128.976-6.982c19.008-1.158 33.984-16.856 33.984-36.053 0-19.877-16.058-36.004-35.91-36.115h-0.011zM1162.846 628.731c-5.721 6.364-9.22 14.825-9.22 24.104 0 10.668 4.625 20.256 11.98 26.867l0.033 0.029 96.129 86.259c6.372 5.716 14.837 9.21 24.12 9.21 19.985 0 36.186-16.201 36.186-36.186 0-10.703-4.647-20.321-12.033-26.946l-0.034-0.030-96.161-86.386c-6.366-5.73-14.833-9.235-24.119-9.235-10.66 0-20.241 4.618-26.852 11.963l-0.029 0.033zM647.672 656.659l-86.227 96.161c-5.806 6.385-9.361 14.907-9.361 24.259 0 19.946 16.17 36.116 36.116 36.116 10.726 0 20.36-4.676 26.974-12.1l0.032-0.036 86.164-96.319c5.806-6.385 9.361-14.907 9.361-24.259 0-19.946-16.17-36.116-36.116-36.116-10.726 0-20.36 4.676-26.974 12.1l-0.032 0.036zM935.964 737.396c-19.092 1.060-34.173 16.801-34.173 36.063 0 0.711 0.021 1.416 0.061 2.117l-0.004-0.097 6.982 129.008c1.065 19.091 16.806 34.169 36.068 34.169 19.949 0 36.121-16.172 36.121-36.121 0-0.687-0.019-1.369-0.057-2.046l0.004 0.094-6.982-129.008c-1.056-19.092-16.792-34.174-36.052-34.18h-0.001z");
                map.Add(5, "Nebel"); mapIcon.Add(5, "M234.92-85.333c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h293.249c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM374.129 148.937c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h415.066c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM287.095 383.169c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h571.705c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM130.494 617.592c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h658.701c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM43.498 851.671c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h989.293c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0z");
                map.Add(6, "Nebel, rutschgefahr"); mapIcon.Add(6, "M234.92-85.333c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h293.249c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM374.129 148.937c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h415.066c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM287.095 383.169c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h571.705c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM130.494 617.592c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h658.701c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0zM43.498 851.671c-24.023 0-43.498 19.475-43.498 43.498s19.475 43.498 43.498 43.498v0h989.293c24.023 0 43.498-19.475 43.498-43.498s-19.475-43.498-43.498-43.498v0z");
                map.Add(7, "leichter Regen"); mapIcon.Add(7, "M218.416-75.254c-6.213 6.219-10.055 14.807-10.055 24.292s3.842 18.073 10.055 24.292v0l177.738 177.708c6.262 6.53 15.057 10.588 24.801 10.588 18.966 0 34.341-15.375 34.341-34.341 0-9.744-4.058-18.539-10.576-24.789l-0.012-0.011-177.708-177.738c-6.219-6.213-14.807-10.055-24.292-10.055s-18.073 3.842-24.292 10.055v0zM580.017 92.827c-6.218 6.211-10.065 14.794-10.065 24.277s3.847 18.066 10.065 24.276l130.512 130.392h-362.688l-179.096-178.975c-6.214-6.21-14.797-10.051-24.277-10.051-18.967 0-34.343 15.376-34.343 34.343 0 9.487 3.847 18.076 10.066 24.292l130.543 130.543h-3.018c-0.813-0.010-1.773-0.015-2.734-0.015-136.81 0-247.717 110.907-247.717 247.717s110.907 247.717 247.717 247.717c0.961 0 1.921-0.005 2.88-0.016l-0.146 0.001c3.832 0 7.665-0.302 11.497-0.483 58.199 103.234 167.129 171.807 292.076 171.807 184.541 0 334.145-149.587 334.172-334.122v-0.003q0-0.302 0-0.634c74.243-18.625 128.35-84.771 128.35-163.555 0-92.996-75.388-168.383-168.383-168.383-0.004 0-0.007 0-0.011 0h-37.72l-179.096-179.126c-6.214-6.211-14.797-10.052-24.277-10.052s-18.063 3.841-24.277 10.053v0zM68.651 519.67c0.103-98.843 80.195-178.946 179.024-179.066h597.501c55.104 0.014 99.768 44.687 99.768 99.793 0 55.114-44.679 99.793-99.793 99.793-24.599 0-47.119-8.901-64.516-23.657l0.144 0.119c-5.956-5.082-13.744-8.174-22.255-8.174-18.989 0-34.382 15.393-34.382 34.382 0 10.478 4.687 19.862 12.079 26.168l0.048 0.040c21.974 18.659 49.143 31.86 79.040 37.178l1.018 0.15c-1.218 145.598-119.531 263.158-265.301 263.158-91.122 0-171.515-45.938-219.283-115.919l-0.589-0.915c27.281-9.849 50.874-23.255 71.7-39.967l-0.484 0.375c8.014-6.345 13.11-16.073 13.11-26.991 0-18.966-15.375-34.341-34.341-34.341-8.25 0-15.82 2.909-21.741 7.757l0.061-0.048c-30.123 24.434-68.93 39.23-111.192 39.23-0.162 0-0.324 0-0.485-0.001h0.025c-98.9-0.034-179.070-80.173-179.156-179.058v-0.008z");
                map.Add(8, "Regen"); mapIcon.Add(8, "M491.685-75.028c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l182.266 182.235c6.421 6.696 15.441 10.858 25.433 10.858 19.449 0 35.215-15.766 35.215-35.215 0-9.992-4.161-19.012-10.845-25.421l-182.278-182.278c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM854.576 71.372c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l133.868 133.868h-392.197l-183.658-183.689c-6.421-6.696-15.441-10.858-25.433-10.858-19.449 0-35.215 15.766-35.215 35.215 0 9.992 4.161 19.012 10.845 25.421l0.012 0.012 133.868 133.713h-242.64c-0.833-0.010-1.818-0.015-2.804-0.015-140.295 0-254.027 113.732-254.027 254.027s113.732 254.027 254.027 254.027c0.986 0 1.97-0.006 2.953-0.017l-0.15 0.001c3.93 0 7.86-0.309 11.79-0.495 59.658 105.895 171.37 176.242 299.512 176.242 160.797 0 295.723-110.769 332.635-260.163l0.492-2.353c42.774 38.905 99.87 62.727 162.53 62.727 130.817 0 237.383-103.829 241.786-233.573l0.011-0.403c52.412-16.786 89.691-65.079 89.691-122.079 0-70.652-57.275-127.927-127.927-127.927-0.179 0-0.357 0-0.536 0.001h-175.956l-183.658-183.504c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM70.431 509.089v0c0.106-101.35 82.22-183.487 183.552-183.627h613.189c56.403 0.152 102.068 45.911 102.068 102.335 0 56.518-45.817 102.335-102.335 102.335-25.242 0-48.35-9.139-66.195-24.289l0.147 0.122c-6.095-5.187-14.059-8.343-22.76-8.343-19.45 0-35.218 15.768-35.218 35.218 0 10.75 4.816 20.374 12.408 26.834l0.050 0.041c22.534 19.133 50.395 32.67 81.053 38.125l1.044 0.154c-1.002 149.497-122.424 270.302-272.062 270.302-93.617 0-176.19-47.283-225.141-119.271l-0.608-0.947c27.976-10.1 52.17-23.847 73.526-40.985l-0.496 0.385c8.218-6.506 13.444-16.482 13.444-27.679 0-19.449-15.766-35.215-35.215-35.215-8.46 0-16.223 2.983-22.294 7.955l0.062-0.050c-30.887 25.057-70.679 40.229-114.014 40.229-0.17 0-0.34 0-0.509-0.001h0.026c-101.419-0.035-183.632-82.216-183.72-183.619v-0.008zM918.323 592.64c70.809-22.503 121.214-87.664 121.214-164.599 0-38.616-12.699-74.267-34.149-103.003l0.324 0.454h258.267c31.731 0.031 57.441 25.761 57.441 57.496 0 31.754-25.742 57.496-57.496 57.496-14.169 0-27.141-5.125-37.163-13.623l0.083 0.069c-6.2-5.544-14.43-8.933-23.45-8.933-19.466 0-35.246 15.78-35.246 35.246 0 11.109 5.139 21.018 13.17 27.478l0.068 0.053c14.159 12.028 31.231 21.088 49.987 25.983l0.886 0.196c-5.57 90.368-80.223 161.549-171.5 161.549-59.003 0-111.059-29.743-141.993-75.054l-0.382-0.592z");
                map.Add(9, "starker Regen"); mapIcon.Add(9, "M491.685-75.028c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l182.266 182.235c6.421 6.696 15.441 10.858 25.433 10.858 19.449 0 35.215-15.766 35.215-35.215 0-9.992-4.161-19.012-10.845-25.421l-182.278-182.278c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM854.576 71.372c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l133.868 133.868h-392.197l-183.658-183.689c-6.421-6.696-15.441-10.858-25.433-10.858-19.449 0-35.215 15.766-35.215 35.215 0 9.992 4.161 19.012 10.845 25.421l0.012 0.012 133.868 133.713h-242.64c-0.833-0.010-1.818-0.015-2.804-0.015-140.295 0-254.027 113.732-254.027 254.027s113.732 254.027 254.027 254.027c0.986 0 1.97-0.006 2.953-0.017l-0.15 0.001c3.93 0 7.86-0.309 11.79-0.495 59.658 105.895 171.37 176.242 299.512 176.242 160.797 0 295.723-110.769 332.635-260.163l0.492-2.353c42.774 38.905 99.87 62.727 162.53 62.727 130.817 0 237.383-103.829 241.786-233.573l0.011-0.403c52.412-16.786 89.691-65.079 89.691-122.079 0-70.652-57.275-127.927-127.927-127.927-0.179 0-0.357 0-0.536 0.001h-175.956l-183.658-183.504c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM70.431 509.089v0c0.106-101.35 82.22-183.487 183.552-183.627h613.189c56.403 0.152 102.068 45.911 102.068 102.335 0 56.518-45.817 102.335-102.335 102.335-25.242 0-48.35-9.139-66.195-24.289l0.147 0.122c-6.095-5.187-14.059-8.343-22.76-8.343-19.45 0-35.218 15.768-35.218 35.218 0 10.75 4.816 20.374 12.408 26.834l0.050 0.041c22.534 19.133 50.395 32.67 81.053 38.125l1.044 0.154c-1.002 149.497-122.424 270.302-272.062 270.302-93.617 0-176.19-47.283-225.141-119.271l-0.608-0.947c27.976-10.1 52.17-23.847 73.526-40.985l-0.496 0.385c8.218-6.506 13.444-16.482 13.444-27.679 0-19.449-15.766-35.215-35.215-35.215-8.46 0-16.223 2.983-22.294 7.955l0.062-0.050c-30.887 25.057-70.679 40.229-114.014 40.229-0.17 0-0.34 0-0.509-0.001h0.026c-101.419-0.035-183.632-82.216-183.72-183.619v-0.008zM918.323 592.64c70.809-22.503 121.214-87.664 121.214-164.599 0-38.616-12.699-74.267-34.149-103.003l0.324 0.454h258.267c31.731 0.031 57.441 25.761 57.441 57.496 0 31.754-25.742 57.496-57.496 57.496-14.169 0-27.141-5.125-37.163-13.623l0.083 0.069c-6.2-5.544-14.43-8.933-23.45-8.933-19.466 0-35.246 15.78-35.246 35.246 0 11.109 5.139 21.018 13.17 27.478l0.068 0.053c14.159 12.028 31.231 21.088 49.987 25.983l0.886 0.196c-5.57 90.368-80.223 161.549-171.5 161.549-59.003 0-111.059-29.743-141.993-75.054l-0.382-0.592z");
                map.Add(10, "leichter Regen, rutschgefahr"); mapIcon.Add(10, "M218.416-75.254c-6.213 6.219-10.055 14.807-10.055 24.292s3.842 18.073 10.055 24.292v0l177.738 177.708c6.262 6.53 15.057 10.588 24.801 10.588 18.966 0 34.341-15.375 34.341-34.341 0-9.744-4.058-18.539-10.576-24.789l-0.012-0.011-177.708-177.738c-6.219-6.213-14.807-10.055-24.292-10.055s-18.073 3.842-24.292 10.055v0zM580.017 92.827c-6.218 6.211-10.065 14.794-10.065 24.277s3.847 18.066 10.065 24.276l130.512 130.392h-362.688l-179.096-178.975c-6.214-6.21-14.797-10.051-24.277-10.051-18.967 0-34.343 15.376-34.343 34.343 0 9.487 3.847 18.076 10.066 24.292l130.543 130.543h-3.018c-0.813-0.010-1.773-0.015-2.734-0.015-136.81 0-247.717 110.907-247.717 247.717s110.907 247.717 247.717 247.717c0.961 0 1.921-0.005 2.88-0.016l-0.146 0.001c3.832 0 7.665-0.302 11.497-0.483 58.199 103.234 167.129 171.807 292.076 171.807 184.541 0 334.145-149.587 334.172-334.122v-0.003q0-0.302 0-0.634c74.243-18.625 128.35-84.771 128.35-163.555 0-92.996-75.388-168.383-168.383-168.383-0.004 0-0.007 0-0.011 0h-37.72l-179.096-179.126c-6.214-6.211-14.797-10.052-24.277-10.052s-18.063 3.841-24.277 10.053v0zM68.651 519.67c0.103-98.843 80.195-178.946 179.024-179.066h597.501c55.104 0.014 99.768 44.687 99.768 99.793 0 55.114-44.679 99.793-99.793 99.793-24.599 0-47.119-8.901-64.516-23.657l0.144 0.119c-5.956-5.082-13.744-8.174-22.255-8.174-18.989 0-34.382 15.393-34.382 34.382 0 10.478 4.687 19.862 12.079 26.168l0.048 0.040c21.974 18.659 49.143 31.86 79.040 37.178l1.018 0.15c-1.218 145.598-119.531 263.158-265.301 263.158-91.122 0-171.515-45.938-219.283-115.919l-0.589-0.915c27.281-9.849 50.874-23.255 71.7-39.967l-0.484 0.375c8.014-6.345 13.11-16.073 13.11-26.991 0-18.966-15.375-34.341-34.341-34.341-8.25 0-15.82 2.909-21.741 7.757l0.061-0.048c-30.123 24.434-68.93 39.23-111.192 39.23-0.162 0-0.324 0-0.485-0.001h0.025c-98.9-0.034-179.070-80.173-179.156-179.058v-0.008z");
                map.Add(11, "starker Regen, rutschgefahr"); mapIcon.Add(11, "M491.685-75.028c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l182.266 182.235c6.421 6.696 15.441 10.858 25.433 10.858 19.449 0 35.215-15.766 35.215-35.215 0-9.992-4.161-19.012-10.845-25.421l-182.278-182.278c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM854.576 71.372c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l133.868 133.868h-392.197l-183.658-183.689c-6.421-6.696-15.441-10.858-25.433-10.858-19.449 0-35.215 15.766-35.215 35.215 0 9.992 4.161 19.012 10.845 25.421l0.012 0.012 133.868 133.713h-242.64c-0.833-0.010-1.818-0.015-2.804-0.015-140.295 0-254.027 113.732-254.027 254.027s113.732 254.027 254.027 254.027c0.986 0 1.97-0.006 2.953-0.017l-0.15 0.001c3.93 0 7.86-0.309 11.79-0.495 59.658 105.895 171.37 176.242 299.512 176.242 160.797 0 295.723-110.769 332.635-260.163l0.492-2.353c42.774 38.905 99.87 62.727 162.53 62.727 130.817 0 237.383-103.829 241.786-233.573l0.011-0.403c52.412-16.786 89.691-65.079 89.691-122.079 0-70.652-57.275-127.927-127.927-127.927-0.179 0-0.357 0-0.536 0.001h-175.956l-183.658-183.504c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM70.431 509.089v0c0.106-101.35 82.22-183.487 183.552-183.627h613.189c56.403 0.152 102.068 45.911 102.068 102.335 0 56.518-45.817 102.335-102.335 102.335-25.242 0-48.35-9.139-66.195-24.289l0.147 0.122c-6.095-5.187-14.059-8.343-22.76-8.343-19.45 0-35.218 15.768-35.218 35.218 0 10.75 4.816 20.374 12.408 26.834l0.050 0.041c22.534 19.133 50.395 32.67 81.053 38.125l1.044 0.154c-1.002 149.497-122.424 270.302-272.062 270.302-93.617 0-176.19-47.283-225.141-119.271l-0.608-0.947c27.976-10.1 52.17-23.847 73.526-40.985l-0.496 0.385c8.218-6.506 13.444-16.482 13.444-27.679 0-19.449-15.766-35.215-35.215-35.215-8.46 0-16.223 2.983-22.294 7.955l0.062-0.050c-30.887 25.057-70.679 40.229-114.014 40.229-0.17 0-0.34 0-0.509-0.001h0.026c-101.419-0.035-183.632-82.216-183.72-183.619v-0.008zM918.323 592.64c70.809-22.503 121.214-87.664 121.214-164.599 0-38.616-12.699-74.267-34.149-103.003l0.324 0.454h258.267c31.731 0.031 57.441 25.761 57.441 57.496 0 31.754-25.742 57.496-57.496 57.496-14.169 0-27.141-5.125-37.163-13.623l0.083 0.069c-6.2-5.544-14.43-8.933-23.45-8.933-19.466 0-35.246 15.78-35.246 35.246 0 11.109 5.139 21.018 13.17 27.478l0.068 0.053c14.159 12.028 31.231 21.088 49.987 25.983l0.886 0.196c-5.57 90.368-80.223 161.549-171.5 161.549-59.003 0-111.059-29.743-141.993-75.054l-0.382-0.592z");
                map.Add(12, "Regen, vereinzelt Schneefall"); mapIcon.Add(12, "M519.698-53.587c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM610.54 37.254c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM866.963 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM378.518 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM701.382 128.096c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM957.805 169.522c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM469.391 169.522c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM860.939 259.657h-608.666c-139.327 0-252.274 112.947-252.274 252.274s112.947 252.274 252.274 252.274v0c3.903 0 7.806-0.307 11.709-0.522 59.254 105.133 170.179 174.97 297.414 174.97 159.734 0 293.76-110.068 330.371-258.497l0.487-2.336c42.478 38.637 99.181 62.294 161.409 62.294 129.903 0 235.727-103.095 240.116-231.929l0.011-0.402c52.102-16.639 89.172-64.624 89.172-121.267 0-70.164-56.879-127.043-127.043-127.043-0.212 0-0.425 0.001-0.637 0.002h0.033zM912.107 595.060c70.307-22.366 120.35-87.078 120.35-163.477 0-38.41-12.649-73.865-34.008-102.429l0.321 0.448h256.515c31.511 0.031 57.045 25.583 57.045 57.099 0 31.535-25.564 57.099-57.099 57.099-14.071 0-26.953-5.090-36.906-13.529l0.083 0.068c-5.932-4.774-13.557-7.662-21.856-7.662-19.332 0-35.003 15.671-35.003 35.003 0 10.373 4.512 19.692 11.681 26.102l0.034 0.030c14.051 11.952 30.995 20.959 49.611 25.834l0.881 0.196c-5.506 89.75-79.64 160.454-170.288 160.454-58.599 0-110.298-29.547-141.010-74.555l-0.379-0.589zM70.068 512.085c0.122-100.648 81.681-182.207 182.317-182.329h608.493c0.003 0 0.008 0 0.012 0 56.128 0 101.629 45.501 101.629 101.629s-45.501 101.629-101.629 101.629c-25.068 0-48.017-9.076-65.738-24.122l0.146 0.121c-6.132-5.425-14.242-8.738-23.126-8.738-19.315 0-34.972 15.658-34.972 34.972 0 10.927 5.011 20.683 12.86 27.096l0.063 0.050c22.384 19.010 50.065 32.456 80.526 37.862l1.035 0.152c-1.098 148.387-121.642 268.253-270.183 268.253-92.972 0-174.976-46.958-223.589-118.45l-0.603-0.94c27.771-10.036 51.787-23.688 72.988-40.702l-0.493 0.383c8.162-6.461 13.351-16.368 13.351-27.488 0-19.315-15.658-34.972-34.972-34.972-8.401 0-16.111 2.962-22.141 7.9l0.062-0.049c-30.674 24.884-70.191 39.951-113.227 39.951-0.169 0-0.337 0-0.506-0.001h0.026c-100.719-0.035-182.365-81.648-182.452-182.352v-0.008z");
                map.Add(13, "Regen, vermehrt Schneefall"); mapIcon.Add(13, "M519.698-53.587c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM610.54 37.254c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM866.963 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM378.518 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM701.382 128.096c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM957.805 169.522c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM469.391 169.522c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM860.939 259.657h-608.666c-139.327 0-252.274 112.947-252.274 252.274s112.947 252.274 252.274 252.274v0c3.903 0 7.806-0.307 11.709-0.522 59.254 105.133 170.179 174.97 297.414 174.97 159.734 0 293.76-110.068 330.371-258.497l0.487-2.336c42.478 38.637 99.181 62.294 161.409 62.294 129.903 0 235.727-103.095 240.116-231.929l0.011-0.402c52.102-16.639 89.172-64.624 89.172-121.267 0-70.164-56.879-127.043-127.043-127.043-0.212 0-0.425 0.001-0.637 0.002h0.033zM912.107 595.060c70.307-22.366 120.35-87.078 120.35-163.477 0-38.41-12.649-73.865-34.008-102.429l0.321 0.448h256.515c31.511 0.031 57.045 25.583 57.045 57.099 0 31.535-25.564 57.099-57.099 57.099-14.071 0-26.953-5.090-36.906-13.529l0.083 0.068c-5.932-4.774-13.557-7.662-21.856-7.662-19.332 0-35.003 15.671-35.003 35.003 0 10.373 4.512 19.692 11.681 26.102l0.034 0.030c14.051 11.952 30.995 20.959 49.611 25.834l0.881 0.196c-5.506 89.75-79.64 160.454-170.288 160.454-58.599 0-110.298-29.547-141.010-74.555l-0.379-0.589zM70.068 512.085c0.122-100.648 81.681-182.207 182.317-182.329h608.493c0.003 0 0.008 0 0.012 0 56.128 0 101.629 45.501 101.629 101.629s-45.501 101.629-101.629 101.629c-25.068 0-48.017-9.076-65.738-24.122l0.146 0.121c-6.132-5.425-14.242-8.738-23.126-8.738-19.315 0-34.972 15.658-34.972 34.972 0 10.927 5.011 20.683 12.86 27.096l0.063 0.050c22.384 19.010 50.065 32.456 80.526 37.862l1.035 0.152c-1.098 148.387-121.642 268.253-270.183 268.253-92.972 0-174.976-46.958-223.589-118.45l-0.603-0.94c27.771-10.036 51.787-23.688 72.988-40.702l-0.493 0.383c8.162-6.461 13.351-16.368 13.351-27.488 0-19.315-15.658-34.972-34.972-34.972-8.401 0-16.111 2.962-22.141 7.9l0.062-0.049c-30.674 24.884-70.191 39.951-113.227 39.951-0.169 0-0.337 0-0.506-0.001h0.026c-100.719-0.035-182.365-81.648-182.452-182.352v-0.008z");
                map.Add(14, "leichter Schneefall"); mapIcon.Add(14, "M519.698-53.587c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM610.54 37.254c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM866.963 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM378.518 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM701.382 128.096c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM957.805 169.522c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM469.391 169.522c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM860.939 259.657h-608.666c-139.327 0-252.274 112.947-252.274 252.274s112.947 252.274 252.274 252.274v0c3.903 0 7.806-0.307 11.709-0.522 59.254 105.133 170.179 174.97 297.414 174.97 159.734 0 293.76-110.068 330.371-258.497l0.487-2.336c42.478 38.637 99.181 62.294 161.409 62.294 129.903 0 235.727-103.095 240.116-231.929l0.011-0.402c52.102-16.639 89.172-64.624 89.172-121.267 0-70.164-56.879-127.043-127.043-127.043-0.212 0-0.425 0.001-0.637 0.002h0.033zM912.107 595.060c70.307-22.366 120.35-87.078 120.35-163.477 0-38.41-12.649-73.865-34.008-102.429l0.321 0.448h256.515c31.511 0.031 57.045 25.583 57.045 57.099 0 31.535-25.564 57.099-57.099 57.099-14.071 0-26.953-5.090-36.906-13.529l0.083 0.068c-5.932-4.774-13.557-7.662-21.856-7.662-19.332 0-35.003 15.671-35.003 35.003 0 10.373 4.512 19.692 11.681 26.102l0.034 0.030c14.051 11.952 30.995 20.959 49.611 25.834l0.881 0.196c-5.506 89.75-79.64 160.454-170.288 160.454-58.599 0-110.298-29.547-141.010-74.555l-0.379-0.589zM70.068 512.085c0.122-100.648 81.681-182.207 182.317-182.329h608.493c0.003 0 0.008 0 0.012 0 56.128 0 101.629 45.501 101.629 101.629s-45.501 101.629-101.629 101.629c-25.068 0-48.017-9.076-65.738-24.122l0.146 0.121c-6.132-5.425-14.242-8.738-23.126-8.738-19.315 0-34.972 15.658-34.972 34.972 0 10.927 5.011 20.683 12.86 27.096l0.063 0.050c22.384 19.010 50.065 32.456 80.526 37.862l1.035 0.152c-1.098 148.387-121.642 268.253-270.183 268.253-92.972 0-174.976-46.958-223.589-118.45l-0.603-0.94c27.771-10.036 51.787-23.688 72.988-40.702l-0.493 0.383c8.162-6.461 13.351-16.368 13.351-27.488 0-19.315-15.658-34.972-34.972-34.972-8.401 0-16.111 2.962-22.141 7.9l0.062-0.049c-30.674 24.884-70.191 39.951-113.227 39.951-0.169 0-0.337 0-0.506-0.001h0.026c-100.719-0.035-182.365-81.648-182.452-182.352v-0.008z");
                map.Add(15, "Schneefall"); mapIcon.Add(15, "M519.698-53.587c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM610.54 37.254c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM866.963 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM378.518 78.68c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM701.382 128.096c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM957.805 169.522c0.018 17.519 14.224 31.715 31.745 31.715 17.533 0 31.746-14.213 31.746-31.746s-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746 0 0.011 0 0.022 0 0.032v-0.002zM469.391 169.522c0 17.533 14.213 31.746 31.746 31.746s31.746-14.213 31.746-31.746c0-17.533-14.213-31.746-31.746-31.746v0c-17.533 0-31.746 14.213-31.746 31.746v0zM860.939 259.657h-608.666c-139.327 0-252.274 112.947-252.274 252.274s112.947 252.274 252.274 252.274v0c3.903 0 7.806-0.307 11.709-0.522 59.254 105.133 170.179 174.97 297.414 174.97 159.734 0 293.76-110.068 330.371-258.497l0.487-2.336c42.478 38.637 99.181 62.294 161.409 62.294 129.903 0 235.727-103.095 240.116-231.929l0.011-0.402c52.102-16.639 89.172-64.624 89.172-121.267 0-70.164-56.879-127.043-127.043-127.043-0.212 0-0.425 0.001-0.637 0.002h0.033zM912.107 595.060c70.307-22.366 120.35-87.078 120.35-163.477 0-38.41-12.649-73.865-34.008-102.429l0.321 0.448h256.515c31.511 0.031 57.045 25.583 57.045 57.099 0 31.535-25.564 57.099-57.099 57.099-14.071 0-26.953-5.090-36.906-13.529l0.083 0.068c-5.932-4.774-13.557-7.662-21.856-7.662-19.332 0-35.003 15.671-35.003 35.003 0 10.373 4.512 19.692 11.681 26.102l0.034 0.030c14.051 11.952 30.995 20.959 49.611 25.834l0.881 0.196c-5.506 89.75-79.64 160.454-170.288 160.454-58.599 0-110.298-29.547-141.010-74.555l-0.379-0.589zM70.068 512.085c0.122-100.648 81.681-182.207 182.317-182.329h608.493c0.003 0 0.008 0 0.012 0 56.128 0 101.629 45.501 101.629 101.629s-45.501 101.629-101.629 101.629c-25.068 0-48.017-9.076-65.738-24.122l0.146 0.121c-6.132-5.425-14.242-8.738-23.126-8.738-19.315 0-34.972 15.658-34.972 34.972 0 10.927 5.011 20.683 12.86 27.096l0.063 0.050c22.384 19.010 50.065 32.456 80.526 37.862l1.035 0.152c-1.098 148.387-121.642 268.253-270.183 268.253-92.972 0-174.976-46.958-223.589-118.45l-0.603-0.94c27.771-10.036 51.787-23.688 72.988-40.702l-0.493 0.383c8.162-6.461 13.351-16.368 13.351-27.488 0-19.315-15.658-34.972-34.972-34.972-8.401 0-16.111 2.962-22.141 7.9l0.062-0.049c-30.674 24.884-70.191 39.951-113.227 39.951-0.169 0-0.337 0-0.506-0.001h0.026c-100.719-0.035-182.365-81.648-182.452-182.352v-0.008z");
                map.Add(16, "starker Schneefall"); mapIcon.Add(16, "M283.796-53.588c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM374.635 37.251c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM631.050 78.675c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM142.62 78.675c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM465.474 128.090c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM721.889 169.515c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.532 0-31.745 14.213-31.745 31.745v0zM233.459 169.515c0 17.532 14.213 31.745 31.745 31.745s31.745-14.213 31.745-31.745c0-17.532-14.213-31.745-31.745-31.745v0c-17.506 0.035-31.683 14.234-31.683 31.744 0 0 0 0 0 0v0zM252.266 259.647c-0.828-0.010-1.805-0.015-2.784-0.015-139.323 0-252.266 112.943-252.266 252.266s112.943 252.266 252.266 252.266c0.979 0 1.957-0.006 2.933-0.017l-0.149 0.001c3.903 0 7.806-0.307 11.708-0.492 59.256 105.147 170.191 174.994 297.44 174.994 187.93 0 340.278-152.348 340.278-340.278 0-0.005 0-0.011 0-0.016v0.001q0-0.338 0-0.645c75.607-18.967 130.707-86.328 130.707-166.559 0-94.704-76.772-171.476-171.476-171.476-0.004 0-0.008 0-0.011 0h0.001zM69.943 511.944c0.105-100.659 81.668-182.232 182.312-182.354h608.475c56.116 0.014 101.601 45.508 101.601 101.626 0 56.126-45.499 101.626-101.626 101.626-25.051 0-47.985-9.064-65.701-24.091l0.147 0.122c-6.069-5.182-14.006-8.334-22.679-8.334-19.343 0-35.024 15.681-35.024 35.024 0 10.67 4.771 20.225 12.296 26.649l0.049 0.040c22.381 18.994 50.047 32.436 80.489 37.86l1.039 0.153c-1.283 148.24-121.752 267.916-270.173 267.916-92.777 0-174.632-46.763-223.281-118.004l-0.6-0.931c27.771-10.033 51.786-23.684 72.986-40.701l-0.492 0.382c8.161-6.461 13.35-16.368 13.35-27.487 0-19.314-15.657-34.971-34.971-34.971-8.401 0-16.11 2.962-22.14 7.899l0.062-0.049c-30.677 24.883-70.196 39.95-113.234 39.95-0.165 0-0.329 0-0.494-0.001h0.026c-100.716-0.035-182.359-81.646-182.447-182.346v-0.008z");
                map.Add(17, "Wolken, (Hagel)"); mapIcon.Add(17, "M491.685-75.028c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l182.266 182.235c6.421 6.696 15.441 10.858 25.433 10.858 19.449 0 35.215-15.766 35.215-35.215 0-9.992-4.161-19.012-10.845-25.421l-182.278-182.278c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM854.576 71.372c-6.378 6.374-10.324 15.181-10.324 24.911s3.946 18.537 10.324 24.91l133.868 133.868h-392.197l-183.658-183.689c-6.421-6.696-15.441-10.858-25.433-10.858-19.449 0-35.215 15.766-35.215 35.215 0 9.992 4.161 19.012 10.845 25.421l0.012 0.012 133.868 133.713h-242.64c-0.833-0.010-1.818-0.015-2.804-0.015-140.295 0-254.027 113.732-254.027 254.027s113.732 254.027 254.027 254.027c0.986 0 1.97-0.006 2.953-0.017l-0.15 0.001c3.93 0 7.86-0.309 11.79-0.495 59.658 105.895 171.37 176.242 299.512 176.242 160.797 0 295.723-110.769 332.635-260.163l0.492-2.353c42.774 38.905 99.87 62.727 162.53 62.727 130.817 0 237.383-103.829 241.786-233.573l0.011-0.403c52.412-16.786 89.691-65.079 89.691-122.079 0-70.652-57.275-127.927-127.927-127.927-0.179 0-0.357 0-0.536 0.001h-175.956l-183.658-183.504c-6.372-6.369-15.174-10.309-24.895-10.309s-18.523 3.939-24.895 10.309v0zM70.431 509.089v0c0.106-101.35 82.22-183.487 183.552-183.627h613.189c56.403 0.152 102.068 45.911 102.068 102.335 0 56.518-45.817 102.335-102.335 102.335-25.242 0-48.35-9.139-66.195-24.289l0.147 0.122c-6.095-5.187-14.059-8.343-22.76-8.343-19.45 0-35.218 15.768-35.218 35.218 0 10.75 4.816 20.374 12.408 26.834l0.050 0.041c22.534 19.133 50.395 32.67 81.053 38.125l1.044 0.154c-1.002 149.497-122.424 270.302-272.062 270.302-93.617 0-176.19-47.283-225.141-119.271l-0.608-0.947c27.976-10.1 52.17-23.847 73.526-40.985l-0.496 0.385c8.218-6.506 13.444-16.482 13.444-27.679 0-19.449-15.766-35.215-35.215-35.215-8.46 0-16.223 2.983-22.294 7.955l0.062-0.050c-30.887 25.057-70.679 40.229-114.014 40.229-0.17 0-0.34 0-0.509-0.001h0.026c-101.419-0.035-183.632-82.216-183.72-183.619v-0.008zM918.323 592.64c70.809-22.503 121.214-87.664 121.214-164.599 0-38.616-12.699-74.267-34.149-103.003l0.324 0.454h258.267c31.731 0.031 57.441 25.761 57.441 57.496 0 31.754-25.742 57.496-57.496 57.496-14.169 0-27.141-5.125-37.163-13.623l0.083 0.069c-6.2-5.544-14.43-8.933-23.45-8.933-19.466 0-35.246 15.78-35.246 35.246 0 11.109 5.139 21.018 13.17 27.478l0.068 0.053c14.159 12.028 31.231 21.088 49.987 25.983l0.886 0.196c-5.57 90.368-80.223 161.549-171.5 161.549-59.003 0-111.059-29.743-141.993-75.054l-0.382-0.592z");
                map.Add(18, "Sonne, leichter Regen"); mapIcon.Add(18, "M160.005-77.578c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l136.762 136.739c4.818 5.025 11.586 8.147 19.083 8.147 14.593 0 26.424-11.83 26.424-26.424 0-7.497-3.122-14.265-8.138-19.075l-136.771-136.748c-4.782-4.786-11.391-7.747-18.692-7.747s-13.909 2.96-18.691 7.746v0zM438.243 51.824c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l100.447 100.447h-279.098l-137.807-137.807c-4.783-4.786-11.391-7.746-18.692-7.746-14.595 0-26.426 11.831-26.426 26.426 0 7.294 2.955 13.898 7.734 18.68l100.958 100.981c-102.967 2.856-185.348 87.007-185.348 190.399 0 105.193 85.275 190.468 190.468 190.468 1.8 0 3.595-0.025 5.383-0.075l-0.263 0.006c2.972 0 5.898-0.232 8.847-0.372 44.954 79.294 128.692 131.98 224.755 132.211h0.033c0.112 0 0.244 0 0.377 0 40.402 0 78.614-9.375 112.579-26.070l-1.502 0.667c17.618 19.029 39.010 34.288 63.019 44.63l1.206 0.462c21.249 9.194 45.994 14.542 71.987 14.542 102.423 0 185.454-83.030 185.454-185.454 0-70.095-38.888-131.107-96.266-162.639l-0.961-0.484c12.568-19.598 20.035-43.513 20.035-69.171 0-71.48-57.946-129.425-129.425-129.425-0.072 0-0.144 0-0.217 0h-42.155l-137.807-137.831c-4.781-4.779-11.385-7.735-18.68-7.735s-13.899 2.956-18.68 7.735v0zM57.863 380.286c0.079-76.048 61.693-137.678 137.727-137.784h459.894c0.003 0 0.006 0 0.009 0 42.408 0 76.787 34.379 76.787 76.787s-34.379 76.787-76.787 76.787c-18.941 0-36.28-6.858-49.669-18.226l0.11 0.091c-4.574-3.892-10.549-6.26-17.078-6.26-14.595 0-26.426 11.831-26.426 26.426 0 8.066 3.614 15.288 9.311 20.135l0.037 0.031c16.912 14.369 37.826 24.536 60.841 28.63l0.783 0.116c-0.765 112.165-91.869 202.797-204.141 202.797-70.245 0-132.204-35.479-168.934-89.495l-0.456-0.711c20.973-7.591 39.11-17.904 55.125-30.754l-0.374 0.29c6.073-4.883 9.927-12.313 9.927-20.642 0-14.594-11.831-26.425-26.425-26.425-6.264 0-12.020 2.18-16.549 5.822l0.051-0.040c-23.179 18.801-53.039 30.186-85.558 30.186-0.124 0-0.249 0-0.373-0.001h0.019c-76.091-0.026-137.775-61.677-137.854-137.753v-0.007zM625.415 673.71v0c-14.869-6.466-27.622-14.948-38.614-25.281l0.069 0.065c60.753-47.407 99.455-120.628 99.472-202.889v-0.003q0-0.232 0-0.488c14.978-3.729 28.146-9.591 39.962-17.344l-0.489 0.301c49.423 20.042 83.645 67.667 83.645 123.284 0 70.73-55.346 128.532-125.092 132.463l-0.348 0.016q-3.622 0.186-7.244 0.186c-0.101 0-0.221 0-0.341 0-18.423 0-35.961-3.791-51.875-10.637l0.854 0.327zM898.453 264.677l-63.11 70.262c-4.248 4.671-6.849 10.906-6.849 17.749 0 14.593 11.83 26.424 26.424 26.424 7.847 0 14.896-3.421 19.736-8.853l0.023-0.026 63.087-70.332c4.248-4.671 6.849-10.906 6.849-17.749 0-14.593-11.83-26.424-26.424-26.424-7.847 0-14.896 3.421-19.736 8.853l-0.023 0.026zM1036.237 505.369l-94.364 5.108c-13.968 0.779-24.999 12.296-24.999 26.389 0 14.595 11.832 26.427 26.427 26.427 0.502 0 1.001-0.014 1.497-0.042l-0.069 0.003 94.341-5.108c13.907-0.847 24.864-12.333 24.864-26.378 0-14.543-11.749-26.342-26.273-26.423h-0.008zM855.798 711.906c-4.192 4.657-6.756 10.853-6.756 17.647 0 7.799 3.379 14.809 8.753 19.646l0.024 0.021 70.332 63.11c4.671 4.248 10.906 6.849 17.749 6.849 14.593 0 26.424-11.83 26.424-26.424 0-7.847-3.421-14.896-8.853-19.736l-0.026-0.023-70.332-63.11c-4.657-4.192-10.853-6.756-17.647-6.756-7.799 0-14.809 3.379-19.646 8.753l-0.021 0.024zM478.877 732.339l-63.11 70.378c-4.191 4.658-6.754 10.853-6.754 17.647 0 14.598 11.834 26.432 26.432 26.432 7.804 0 14.819-3.382 19.657-8.761l0.021-0.024 63.041-70.378c4.182-4.662 6.739-10.856 6.739-17.647 0-14.622-11.853-26.475-26.475-26.475-7.831 0-14.867 3.4-19.715 8.804l-0.022 0.025zM689.826 791.433c-13.969 0.776-25.002 12.292-25.002 26.385 0 0.52 0.015 1.036 0.045 1.549l-0.003-0.071 5.108 94.387c0.779 13.968 12.296 24.999 26.389 24.999 14.595 0 26.427-11.832 26.427-26.427 0-0.502-0.014-1.001-0.042-1.497l0.003 0.069-5.108-94.387c-0.785-13.958-12.293-24.98-26.377-24.984v0z");
                map.Add(19, "Sonne, starker Regen"); mapIcon.Add(19, "M160.005-77.578c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l136.762 136.739c4.818 5.025 11.586 8.147 19.083 8.147 14.593 0 26.424-11.83 26.424-26.424 0-7.497-3.122-14.265-8.138-19.075l-136.771-136.748c-4.782-4.786-11.391-7.747-18.692-7.747s-13.909 2.96-18.691 7.746v0zM438.243 51.824c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l100.447 100.447h-279.098l-137.807-137.807c-4.783-4.786-11.391-7.746-18.692-7.746-14.595 0-26.426 11.831-26.426 26.426 0 7.294 2.955 13.898 7.734 18.68l100.958 100.981c-102.967 2.856-185.348 87.007-185.348 190.399 0 105.193 85.275 190.468 190.468 190.468 1.8 0 3.595-0.025 5.383-0.075l-0.263 0.006c2.972 0 5.898-0.232 8.847-0.372 44.954 79.294 128.692 131.98 224.755 132.211h0.033c0.112 0 0.244 0 0.377 0 40.402 0 78.614-9.375 112.579-26.070l-1.502 0.667c17.618 19.029 39.010 34.288 63.019 44.63l1.206 0.462c21.249 9.194 45.994 14.542 71.987 14.542 102.423 0 185.454-83.030 185.454-185.454 0-70.095-38.888-131.107-96.266-162.639l-0.961-0.484c12.568-19.598 20.035-43.513 20.035-69.171 0-71.48-57.946-129.425-129.425-129.425-0.072 0-0.144 0-0.217 0h-42.155l-137.807-137.831c-4.781-4.779-11.385-7.735-18.68-7.735s-13.899 2.956-18.68 7.735v0zM57.863 380.286c0.079-76.048 61.693-137.678 137.727-137.784h459.894c0.003 0 0.006 0 0.009 0 42.408 0 76.787 34.379 76.787 76.787s-34.379 76.787-76.787 76.787c-18.941 0-36.28-6.858-49.669-18.226l0.11 0.091c-4.574-3.892-10.549-6.26-17.078-6.26-14.595 0-26.426 11.831-26.426 26.426 0 8.066 3.614 15.288 9.311 20.135l0.037 0.031c16.912 14.369 37.826 24.536 60.841 28.63l0.783 0.116c-0.765 112.165-91.869 202.797-204.141 202.797-70.245 0-132.204-35.479-168.934-89.495l-0.456-0.711c20.973-7.591 39.11-17.904 55.125-30.754l-0.374 0.29c6.073-4.883 9.927-12.313 9.927-20.642 0-14.594-11.831-26.425-26.425-26.425-6.264 0-12.020 2.18-16.549 5.822l0.051-0.040c-23.179 18.801-53.039 30.186-85.558 30.186-0.124 0-0.249 0-0.373-0.001h0.019c-76.091-0.026-137.775-61.677-137.854-137.753v-0.007zM625.415 673.71v0c-14.869-6.466-27.622-14.948-38.614-25.281l0.069 0.065c60.753-47.407 99.455-120.628 99.472-202.889v-0.003q0-0.232 0-0.488c14.978-3.729 28.146-9.591 39.962-17.344l-0.489 0.301c49.423 20.042 83.645 67.667 83.645 123.284 0 70.73-55.346 128.532-125.092 132.463l-0.348 0.016q-3.622 0.186-7.244 0.186c-0.101 0-0.221 0-0.341 0-18.423 0-35.961-3.791-51.875-10.637l0.854 0.327zM898.453 264.677l-63.11 70.262c-4.248 4.671-6.849 10.906-6.849 17.749 0 14.593 11.83 26.424 26.424 26.424 7.847 0 14.896-3.421 19.736-8.853l0.023-0.026 63.087-70.332c4.248-4.671 6.849-10.906 6.849-17.749 0-14.593-11.83-26.424-26.424-26.424-7.847 0-14.896 3.421-19.736 8.853l-0.023 0.026zM1036.237 505.369l-94.364 5.108c-13.968 0.779-24.999 12.296-24.999 26.389 0 14.595 11.832 26.427 26.427 26.427 0.502 0 1.001-0.014 1.497-0.042l-0.069 0.003 94.341-5.108c13.907-0.847 24.864-12.333 24.864-26.378 0-14.543-11.749-26.342-26.273-26.423h-0.008zM855.798 711.906c-4.192 4.657-6.756 10.853-6.756 17.647 0 7.799 3.379 14.809 8.753 19.646l0.024 0.021 70.332 63.11c4.671 4.248 10.906 6.849 17.749 6.849 14.593 0 26.424-11.83 26.424-26.424 0-7.847-3.421-14.896-8.853-19.736l-0.026-0.023-70.332-63.11c-4.657-4.192-10.853-6.756-17.647-6.756-7.799 0-14.809 3.379-19.646 8.753l-0.021 0.024zM478.877 732.339l-63.11 70.378c-4.191 4.658-6.754 10.853-6.754 17.647 0 14.598 11.834 26.432 26.432 26.432 7.804 0 14.819-3.382 19.657-8.761l0.021-0.024 63.041-70.378c4.182-4.662 6.739-10.856 6.739-17.647 0-14.622-11.853-26.475-26.475-26.475-7.831 0-14.867 3.4-19.715 8.804l-0.022 0.025zM689.826 791.433c-13.969 0.776-25.002 12.292-25.002 26.385 0 0.52 0.015 1.036 0.045 1.549l-0.003-0.071 5.108 94.387c0.779 13.968 12.296 24.999 26.389 24.999 14.595 0 26.427-11.832 26.427-26.427 0-0.502-0.014-1.001-0.042-1.497l0.003 0.069-5.108-94.387c-0.785-13.958-12.293-24.98-26.377-24.984v0z");
                map.Add(20, "Sonne, Regen, vereinzelt Schneefall"); mapIcon.Add(20, "M250.678-61.007c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM320.336 8.604c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM516.828 40.347c0.082 13.307 10.889 24.063 24.208 24.063 13.37 0 24.208-10.838 24.208-24.208s-10.838-24.208-24.208-24.208c-6.698 0-12.76 2.72-17.143 7.115l-0.001 0.001c-4.366 4.378-7.065 10.42-7.065 17.092 0 0.051 0 0.102 0 0.153v-0.008zM142.565 40.347c0.093 13.273 10.875 23.996 24.161 23.996 13.344 0 24.161-10.817 24.161-24.161s-10.817-24.161-24.161-24.161c-6.678 0-12.722 2.709-17.096 7.088v0c-4.365 4.371-7.065 10.407-7.065 17.073 0 0.058 0 0.116 0.001 0.174v-0.009zM389.828 78.214c0.088 13.29 10.882 24.030 24.184 24.030 13.357 0 24.185-10.828 24.185-24.185s-10.828-24.185-24.185-24.185c-6.688 0-12.741 2.714-17.119 7.102v0c-4.366 4.375-7.065 10.413-7.065 17.083 0 0.054 0 0.109 0.001 0.163v-0.008zM586.438 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM212.011 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM193.313 179.027c-0.634-0.008-1.383-0.012-2.134-0.012-106.764 0-193.313 86.549-193.313 193.313s86.549 193.313 193.313 193.313c0.75 0 1.499-0.004 2.248-0.013l-0.114 0.001c2.991 0 5.981-0.235 8.972-0.377 45.411 80.564 130.415 134.080 227.919 134.080 41.092 0 79.963-9.505 114.536-26.436l-1.538 0.68c34.476 37.269 83.637 60.525 138.229 60.525 103.889 0 188.109-84.219 188.109-188.109 0-71.098-39.445-132.984-97.644-164.967l-0.975-0.491c12.747-19.876 20.319-44.13 20.319-70.152 0-72.494-58.768-131.262-131.262-131.262-0.073 0-0.146 0-0.22 0h0.011zM53.597 372.363c0.080-77.135 62.582-139.645 139.707-139.739h466.277c0.010 0 0.021 0 0.032 0 43.010 0 77.876 34.866 77.876 77.876s-34.866 77.876-77.876 77.876c-19.209 0-36.794-6.955-50.374-18.484l0.112 0.093c-4.709-4.185-10.947-6.742-17.782-6.742-14.813 0-26.822 12.009-26.822 26.822 0 8.383 3.846 15.867 9.869 20.786l0.048 0.038c17.153 14.572 38.363 24.882 61.704 29.036l0.795 0.117c-0.756 113.771-93.16 205.709-207.038 205.709-71.257 0-134.106-35.997-171.354-90.799l-0.463-0.722c21.279-7.7 39.681-18.16 55.932-31.191l-0.38 0.295c6.149-4.958 10.050-12.491 10.050-20.935 0-14.817-12.012-26.829-26.829-26.829-6.373 0-12.227 2.222-16.83 5.934l0.051-0.040c-23.508 19.068-53.791 30.614-86.772 30.614-0.126 0-0.252 0-0.379-0.001h0.020c-77.080-0.147-139.518-62.626-139.598-139.708v-0.008zM629.25 669.951c-15.081-6.555-28.015-15.158-39.161-25.639l0.070 0.065c61.618-48.077 100.869-122.338 100.883-205.768v-0.003q0-0.235 0-0.495c15.19-3.78 28.546-9.725 40.528-17.59l-0.495 0.305c50.169 20.298 84.917 68.616 84.917 125.051 0 71.758-56.18 130.395-126.956 134.325l-0.348 0.015q-3.65 0.188-7.3 0.188c-0.095 0-0.208 0-0.321 0-18.709 0-36.52-3.845-52.685-10.788l0.869 0.332zM1045.9 499.221l-95.703 5.181c-14.167 0.787-25.357 12.466-25.357 26.76 0 0.527 0.015 1.051 0.045 1.571l-0.003-0.072c0.562 14.135 12.157 25.382 26.379 25.382 0.662 0 1.317-0.024 1.967-0.072l-0.087 0.005 95.703-5.181c14.104-0.859 25.217-12.508 25.217-26.752 0-14.749-11.916-26.716-26.646-26.798h-0.008zM862.926 708.806c-4.252 4.723-6.852 11.007-6.852 17.897 0 7.91 3.427 15.019 8.877 19.924l0.024 0.021 71.306 63.888c4.726 4.257 11.013 6.86 17.909 6.86 14.805 0 26.806-12.002 26.806-26.806 0-7.909-3.425-15.018-8.873-19.925l-0.024-0.021-71.33-64.006c-4.723-4.252-11.007-6.852-17.897-6.852-7.91 0-15.019 3.427-19.924 8.877l-0.021 0.024zM480.656 729.529l-64.006 71.259c-4.25 4.724-6.849 11.007-6.849 17.897 0 14.805 12.002 26.807 26.807 26.807 7.915 0 15.029-3.43 19.936-8.885l0.022-0.024 63.935-71.377c4.25-4.724 6.849-11.007 6.849-17.897 0-14.805-12.002-26.807-26.807-26.807-7.915 0-15.029 3.43-19.936 8.885l-0.022 0.024zM669.613 811.738l-16.249 95.703c-0.302 1.51-0.475 3.245-0.475 5.021 0 14.8 11.998 26.799 26.799 26.799 13.44 0 24.569-9.893 26.501-22.794l0.018-0.147 16.249-95.703c0.242-1.353 0.381-2.911 0.381-4.501 0-13.213-9.562-24.192-22.144-26.396l-0.161-0.023c-1.358-0.238-2.923-0.375-4.52-0.377h-0.001c-13.21 0.012-24.181 9.581-26.375 22.164l-0.023 0.16z");
                map.Add(21, "Sonne, Regen, vermehrt Schneefall"); mapIcon.Add(21, "M250.678-61.007c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM320.336 8.604c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM516.828 40.347c0.082 13.307 10.889 24.063 24.208 24.063 13.37 0 24.208-10.838 24.208-24.208s-10.838-24.208-24.208-24.208c-6.698 0-12.76 2.72-17.143 7.115l-0.001 0.001c-4.366 4.378-7.065 10.42-7.065 17.092 0 0.051 0 0.102 0 0.153v-0.008zM142.565 40.347c0.093 13.273 10.875 23.996 24.161 23.996 13.344 0 24.161-10.817 24.161-24.161s-10.817-24.161-24.161-24.161c-6.678 0-12.722 2.709-17.096 7.088v0c-4.365 4.371-7.065 10.407-7.065 17.073 0 0.058 0 0.116 0.001 0.174v-0.009zM389.828 78.214c0.088 13.29 10.882 24.030 24.184 24.030 13.357 0 24.185-10.828 24.185-24.185s-10.828-24.185-24.185-24.185c-6.688 0-12.741 2.714-17.119 7.102v0c-4.366 4.375-7.065 10.413-7.065 17.083 0 0.054 0 0.109 0.001 0.163v-0.008zM586.438 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM212.011 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM193.313 179.027c-0.634-0.008-1.383-0.012-2.134-0.012-106.764 0-193.313 86.549-193.313 193.313s86.549 193.313 193.313 193.313c0.75 0 1.499-0.004 2.248-0.013l-0.114 0.001c2.991 0 5.981-0.235 8.972-0.377 45.411 80.564 130.415 134.080 227.919 134.080 41.092 0 79.963-9.505 114.536-26.436l-1.538 0.68c34.476 37.269 83.637 60.525 138.229 60.525 103.889 0 188.109-84.219 188.109-188.109 0-71.098-39.445-132.984-97.644-164.967l-0.975-0.491c12.747-19.876 20.319-44.13 20.319-70.152 0-72.494-58.768-131.262-131.262-131.262-0.073 0-0.146 0-0.22 0h0.011zM53.597 372.363c0.080-77.135 62.582-139.645 139.707-139.739h466.277c0.010 0 0.021 0 0.032 0 43.010 0 77.876 34.866 77.876 77.876s-34.866 77.876-77.876 77.876c-19.209 0-36.794-6.955-50.374-18.484l0.112 0.093c-4.709-4.185-10.947-6.742-17.782-6.742-14.813 0-26.822 12.009-26.822 26.822 0 8.383 3.846 15.867 9.869 20.786l0.048 0.038c17.153 14.572 38.363 24.882 61.704 29.036l0.795 0.117c-0.756 113.771-93.16 205.709-207.038 205.709-71.257 0-134.106-35.997-171.354-90.799l-0.463-0.722c21.279-7.7 39.681-18.16 55.932-31.191l-0.38 0.295c6.149-4.958 10.050-12.491 10.050-20.935 0-14.817-12.012-26.829-26.829-26.829-6.373 0-12.227 2.222-16.83 5.934l0.051-0.040c-23.508 19.068-53.791 30.614-86.772 30.614-0.126 0-0.252 0-0.379-0.001h0.020c-77.080-0.147-139.518-62.626-139.598-139.708v-0.008zM629.25 669.951c-15.081-6.555-28.015-15.158-39.161-25.639l0.070 0.065c61.618-48.077 100.869-122.338 100.883-205.768v-0.003q0-0.235 0-0.495c15.19-3.78 28.546-9.725 40.528-17.59l-0.495 0.305c50.169 20.298 84.917 68.616 84.917 125.051 0 71.758-56.18 130.395-126.956 134.325l-0.348 0.015q-3.65 0.188-7.3 0.188c-0.095 0-0.208 0-0.321 0-18.709 0-36.52-3.845-52.685-10.788l0.869 0.332zM1045.9 499.221l-95.703 5.181c-14.167 0.787-25.357 12.466-25.357 26.76 0 0.527 0.015 1.051 0.045 1.571l-0.003-0.072c0.562 14.135 12.157 25.382 26.379 25.382 0.662 0 1.317-0.024 1.967-0.072l-0.087 0.005 95.703-5.181c14.104-0.859 25.217-12.508 25.217-26.752 0-14.749-11.916-26.716-26.646-26.798h-0.008zM862.926 708.806c-4.252 4.723-6.852 11.007-6.852 17.897 0 7.91 3.427 15.019 8.877 19.924l0.024 0.021 71.306 63.888c4.726 4.257 11.013 6.86 17.909 6.86 14.805 0 26.806-12.002 26.806-26.806 0-7.909-3.425-15.018-8.873-19.925l-0.024-0.021-71.33-64.006c-4.723-4.252-11.007-6.852-17.897-6.852-7.91 0-15.019 3.427-19.924 8.877l-0.021 0.024zM480.656 729.529l-64.006 71.259c-4.25 4.724-6.849 11.007-6.849 17.897 0 14.805 12.002 26.807 26.807 26.807 7.915 0 15.029-3.43 19.936-8.885l0.022-0.024 63.935-71.377c4.25-4.724 6.849-11.007 6.849-17.897 0-14.805-12.002-26.807-26.807-26.807-7.915 0-15.029 3.43-19.936 8.885l-0.022 0.024zM669.613 811.738l-16.249 95.703c-0.302 1.51-0.475 3.245-0.475 5.021 0 14.8 11.998 26.799 26.799 26.799 13.44 0 24.569-9.893 26.501-22.794l0.018-0.147 16.249-95.703c0.242-1.353 0.381-2.911 0.381-4.501 0-13.213-9.562-24.192-22.144-26.396l-0.161-0.023c-1.358-0.238-2.923-0.375-4.52-0.377h-0.001c-13.21 0.012-24.181 9.581-26.375 22.164l-0.023 0.16z");
                map.Add(22, "Sonne, vereinzelter Schneefall"); mapIcon.Add(22, "M250.678-61.007c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM320.336 8.604c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM516.828 40.347c0.082 13.307 10.889 24.063 24.208 24.063 13.37 0 24.208-10.838 24.208-24.208s-10.838-24.208-24.208-24.208c-6.698 0-12.76 2.72-17.143 7.115l-0.001 0.001c-4.366 4.378-7.065 10.42-7.065 17.092 0 0.051 0 0.102 0 0.153v-0.008zM142.565 40.347c0.093 13.273 10.875 23.996 24.161 23.996 13.344 0 24.161-10.817 24.161-24.161s-10.817-24.161-24.161-24.161c-6.678 0-12.722 2.709-17.096 7.088v0c-4.365 4.371-7.065 10.407-7.065 17.073 0 0.058 0 0.116 0.001 0.174v-0.009zM389.828 78.214c0.088 13.29 10.882 24.030 24.184 24.030 13.357 0 24.185-10.828 24.185-24.185s-10.828-24.185-24.185-24.185c-6.688 0-12.741 2.714-17.119 7.102v0c-4.366 4.375-7.065 10.413-7.065 17.083 0 0.054 0 0.109 0.001 0.163v-0.008zM586.438 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM212.011 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM193.313 179.027c-0.634-0.008-1.383-0.012-2.134-0.012-106.764 0-193.313 86.549-193.313 193.313s86.549 193.313 193.313 193.313c0.75 0 1.499-0.004 2.248-0.013l-0.114 0.001c2.991 0 5.981-0.235 8.972-0.377 45.411 80.564 130.415 134.080 227.919 134.080 41.092 0 79.963-9.505 114.536-26.436l-1.538 0.68c34.476 37.269 83.637 60.525 138.229 60.525 103.889 0 188.109-84.219 188.109-188.109 0-71.098-39.445-132.984-97.644-164.967l-0.975-0.491c12.747-19.876 20.319-44.13 20.319-70.152 0-72.494-58.768-131.262-131.262-131.262-0.073 0-0.146 0-0.22 0h0.011zM53.597 372.363c0.080-77.135 62.582-139.645 139.707-139.739h466.277c0.010 0 0.021 0 0.032 0 43.010 0 77.876 34.866 77.876 77.876s-34.866 77.876-77.876 77.876c-19.209 0-36.794-6.955-50.374-18.484l0.112 0.093c-4.709-4.185-10.947-6.742-17.782-6.742-14.813 0-26.822 12.009-26.822 26.822 0 8.383 3.846 15.867 9.869 20.786l0.048 0.038c17.153 14.572 38.363 24.882 61.704 29.036l0.795 0.117c-0.756 113.771-93.16 205.709-207.038 205.709-71.257 0-134.106-35.997-171.354-90.799l-0.463-0.722c21.279-7.7 39.681-18.16 55.932-31.191l-0.38 0.295c6.149-4.958 10.050-12.491 10.050-20.935 0-14.817-12.012-26.829-26.829-26.829-6.373 0-12.227 2.222-16.83 5.934l0.051-0.040c-23.508 19.068-53.791 30.614-86.772 30.614-0.126 0-0.252 0-0.379-0.001h0.020c-77.080-0.147-139.518-62.626-139.598-139.708v-0.008zM629.25 669.951c-15.081-6.555-28.015-15.158-39.161-25.639l0.070 0.065c61.618-48.077 100.869-122.338 100.883-205.768v-0.003q0-0.235 0-0.495c15.19-3.78 28.546-9.725 40.528-17.59l-0.495 0.305c50.169 20.298 84.917 68.616 84.917 125.051 0 71.758-56.18 130.395-126.956 134.325l-0.348 0.015q-3.65 0.188-7.3 0.188c-0.095 0-0.208 0-0.321 0-18.709 0-36.52-3.845-52.685-10.788l0.869 0.332zM1045.9 499.221l-95.703 5.181c-14.167 0.787-25.357 12.466-25.357 26.76 0 0.527 0.015 1.051 0.045 1.571l-0.003-0.072c0.562 14.135 12.157 25.382 26.379 25.382 0.662 0 1.317-0.024 1.967-0.072l-0.087 0.005 95.703-5.181c14.104-0.859 25.217-12.508 25.217-26.752 0-14.749-11.916-26.716-26.646-26.798h-0.008zM862.926 708.806c-4.252 4.723-6.852 11.007-6.852 17.897 0 7.91 3.427 15.019 8.877 19.924l0.024 0.021 71.306 63.888c4.726 4.257 11.013 6.86 17.909 6.86 14.805 0 26.806-12.002 26.806-26.806 0-7.909-3.425-15.018-8.873-19.925l-0.024-0.021-71.33-64.006c-4.723-4.252-11.007-6.852-17.897-6.852-7.91 0-15.019 3.427-19.924 8.877l-0.021 0.024zM480.656 729.529l-64.006 71.259c-4.25 4.724-6.849 11.007-6.849 17.897 0 14.805 12.002 26.807 26.807 26.807 7.915 0 15.029-3.43 19.936-8.885l0.022-0.024 63.935-71.377c4.25-4.724 6.849-11.007 6.849-17.897 0-14.805-12.002-26.807-26.807-26.807-7.915 0-15.029 3.43-19.936 8.885l-0.022 0.024zM669.613 811.738l-16.249 95.703c-0.302 1.51-0.475 3.245-0.475 5.021 0 14.8 11.998 26.799 26.799 26.799 13.44 0 24.569-9.893 26.501-22.794l0.018-0.147 16.249-95.703c0.242-1.353 0.381-2.911 0.381-4.501 0-13.213-9.562-24.192-22.144-26.396l-0.161-0.023c-1.358-0.238-2.923-0.375-4.52-0.377h-0.001c-13.21 0.012-24.181 9.581-26.375 22.164l-0.023 0.16z");
                map.Add(23, "Sonne, vermehrter Schnefall"); mapIcon.Add(23, "M250.678-61.007c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM320.336 8.604c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM516.828 40.347c0.082 13.307 10.889 24.063 24.208 24.063 13.37 0 24.208-10.838 24.208-24.208s-10.838-24.208-24.208-24.208c-6.698 0-12.76 2.72-17.143 7.115l-0.001 0.001c-4.366 4.378-7.065 10.42-7.065 17.092 0 0.051 0 0.102 0 0.153v-0.008zM142.565 40.347c0.093 13.273 10.875 23.996 24.161 23.996 13.344 0 24.161-10.817 24.161-24.161s-10.817-24.161-24.161-24.161c-6.678 0-12.722 2.709-17.096 7.088v0c-4.365 4.371-7.065 10.407-7.065 17.073 0 0.058 0 0.116 0.001 0.174v-0.009zM389.828 78.214c0.088 13.29 10.882 24.030 24.184 24.030 13.357 0 24.185-10.828 24.185-24.185s-10.828-24.185-24.185-24.185c-6.688 0-12.741 2.714-17.119 7.102v0c-4.366 4.375-7.065 10.413-7.065 17.083 0 0.054 0 0.109 0.001 0.163v-0.008zM586.438 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM212.011 109.958c0 13.435 10.891 24.326 24.326 24.326s24.326-10.891 24.326-24.326c0-13.435-10.891-24.326-24.326-24.326v0c-13.435 0-24.326 10.891-24.326 24.326v0zM193.313 179.027c-0.634-0.008-1.383-0.012-2.134-0.012-106.764 0-193.313 86.549-193.313 193.313s86.549 193.313 193.313 193.313c0.75 0 1.499-0.004 2.248-0.013l-0.114 0.001c2.991 0 5.981-0.235 8.972-0.377 45.411 80.564 130.415 134.080 227.919 134.080 41.092 0 79.963-9.505 114.536-26.436l-1.538 0.68c34.476 37.269 83.637 60.525 138.229 60.525 103.889 0 188.109-84.219 188.109-188.109 0-71.098-39.445-132.984-97.644-164.967l-0.975-0.491c12.747-19.876 20.319-44.13 20.319-70.152 0-72.494-58.768-131.262-131.262-131.262-0.073 0-0.146 0-0.22 0h0.011zM53.597 372.363c0.080-77.135 62.582-139.645 139.707-139.739h466.277c0.010 0 0.021 0 0.032 0 43.010 0 77.876 34.866 77.876 77.876s-34.866 77.876-77.876 77.876c-19.209 0-36.794-6.955-50.374-18.484l0.112 0.093c-4.709-4.185-10.947-6.742-17.782-6.742-14.813 0-26.822 12.009-26.822 26.822 0 8.383 3.846 15.867 9.869 20.786l0.048 0.038c17.153 14.572 38.363 24.882 61.704 29.036l0.795 0.117c-0.756 113.771-93.16 205.709-207.038 205.709-71.257 0-134.106-35.997-171.354-90.799l-0.463-0.722c21.279-7.7 39.681-18.16 55.932-31.191l-0.38 0.295c6.149-4.958 10.050-12.491 10.050-20.935 0-14.817-12.012-26.829-26.829-26.829-6.373 0-12.227 2.222-16.83 5.934l0.051-0.040c-23.508 19.068-53.791 30.614-86.772 30.614-0.126 0-0.252 0-0.379-0.001h0.020c-77.080-0.147-139.518-62.626-139.598-139.708v-0.008zM629.25 669.951c-15.081-6.555-28.015-15.158-39.161-25.639l0.070 0.065c61.618-48.077 100.869-122.338 100.883-205.768v-0.003q0-0.235 0-0.495c15.19-3.78 28.546-9.725 40.528-17.59l-0.495 0.305c50.169 20.298 84.917 68.616 84.917 125.051 0 71.758-56.18 130.395-126.956 134.325l-0.348 0.015q-3.65 0.188-7.3 0.188c-0.095 0-0.208 0-0.321 0-18.709 0-36.52-3.845-52.685-10.788l0.869 0.332zM1045.9 499.221l-95.703 5.181c-14.167 0.787-25.357 12.466-25.357 26.76 0 0.527 0.015 1.051 0.045 1.571l-0.003-0.072c0.562 14.135 12.157 25.382 26.379 25.382 0.662 0 1.317-0.024 1.967-0.072l-0.087 0.005 95.703-5.181c14.104-0.859 25.217-12.508 25.217-26.752 0-14.749-11.916-26.716-26.646-26.798h-0.008zM862.926 708.806c-4.252 4.723-6.852 11.007-6.852 17.897 0 7.91 3.427 15.019 8.877 19.924l0.024 0.021 71.306 63.888c4.726 4.257 11.013 6.86 17.909 6.86 14.805 0 26.806-12.002 26.806-26.806 0-7.909-3.425-15.018-8.873-19.925l-0.024-0.021-71.33-64.006c-4.723-4.252-11.007-6.852-17.897-6.852-7.91 0-15.019 3.427-19.924 8.877l-0.021 0.024zM480.656 729.529l-64.006 71.259c-4.25 4.724-6.849 11.007-6.849 17.897 0 14.805 12.002 26.807 26.807 26.807 7.915 0 15.029-3.43 19.936-8.885l0.022-0.024 63.935-71.377c4.25-4.724 6.849-11.007 6.849-17.897 0-14.805-12.002-26.807-26.807-26.807-7.915 0-15.029 3.43-19.936 8.885l-0.022 0.024zM669.613 811.738l-16.249 95.703c-0.302 1.51-0.475 3.245-0.475 5.021 0 14.8 11.998 26.799 26.799 26.799 13.44 0 24.569-9.893 26.501-22.794l0.018-0.147 16.249-95.703c0.242-1.353 0.381-2.911 0.381-4.501 0-13.213-9.562-24.192-22.144-26.396l-0.161-0.023c-1.358-0.238-2.923-0.375-4.52-0.377h-0.001c-13.21 0.012-24.181 9.581-26.375 22.164l-0.023 0.16z");
                map.Add(24, "Sonne, (Hagel)"); mapIcon.Add(24, "M160.005-77.578c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l136.762 136.739c4.818 5.025 11.586 8.147 19.083 8.147 14.593 0 26.424-11.83 26.424-26.424 0-7.497-3.122-14.265-8.138-19.075l-136.771-136.748c-4.782-4.786-11.391-7.747-18.692-7.747s-13.909 2.96-18.691 7.746v0zM438.243 51.824c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l100.447 100.447h-279.098l-137.807-137.807c-4.783-4.786-11.391-7.746-18.692-7.746-14.595 0-26.426 11.831-26.426 26.426 0 7.294 2.955 13.898 7.734 18.68l100.958 100.981c-102.967 2.856-185.348 87.007-185.348 190.399 0 105.193 85.275 190.468 190.468 190.468 1.8 0 3.595-0.025 5.383-0.075l-0.263 0.006c2.972 0 5.898-0.232 8.847-0.372 44.954 79.294 128.692 131.98 224.755 132.211h0.033c0.112 0 0.244 0 0.377 0 40.402 0 78.614-9.375 112.579-26.070l-1.502 0.667c17.618 19.029 39.010 34.288 63.019 44.63l1.206 0.462c21.249 9.194 45.994 14.542 71.987 14.542 102.423 0 185.454-83.030 185.454-185.454 0-70.095-38.888-131.107-96.266-162.639l-0.961-0.484c12.568-19.598 20.035-43.513 20.035-69.171 0-71.48-57.946-129.425-129.425-129.425-0.072 0-0.144 0-0.217 0h-42.155l-137.807-137.831c-4.781-4.779-11.385-7.735-18.68-7.735s-13.899 2.956-18.68 7.735v0zM57.863 380.286c0.079-76.048 61.693-137.678 137.727-137.784h459.894c0.003 0 0.006 0 0.009 0 42.408 0 76.787 34.379 76.787 76.787s-34.379 76.787-76.787 76.787c-18.941 0-36.28-6.858-49.669-18.226l0.11 0.091c-4.574-3.892-10.549-6.26-17.078-6.26-14.595 0-26.426 11.831-26.426 26.426 0 8.066 3.614 15.288 9.311 20.135l0.037 0.031c16.912 14.369 37.826 24.536 60.841 28.63l0.783 0.116c-0.765 112.165-91.869 202.797-204.141 202.797-70.245 0-132.204-35.479-168.934-89.495l-0.456-0.711c20.973-7.591 39.11-17.904 55.125-30.754l-0.374 0.29c6.073-4.883 9.927-12.313 9.927-20.642 0-14.594-11.831-26.425-26.425-26.425-6.264 0-12.020 2.18-16.549 5.822l0.051-0.040c-23.179 18.801-53.039 30.186-85.558 30.186-0.124 0-0.249 0-0.373-0.001h0.019c-76.091-0.026-137.775-61.677-137.854-137.753v-0.007zM625.415 673.71v0c-14.869-6.466-27.622-14.948-38.614-25.281l0.069 0.065c60.753-47.407 99.455-120.628 99.472-202.889v-0.003q0-0.232 0-0.488c14.978-3.729 28.146-9.591 39.962-17.344l-0.489 0.301c49.423 20.042 83.645 67.667 83.645 123.284 0 70.73-55.346 128.532-125.092 132.463l-0.348 0.016q-3.622 0.186-7.244 0.186c-0.101 0-0.221 0-0.341 0-18.423 0-35.961-3.791-51.875-10.637l0.854 0.327zM898.453 264.677l-63.11 70.262c-4.248 4.671-6.849 10.906-6.849 17.749 0 14.593 11.83 26.424 26.424 26.424 7.847 0 14.896-3.421 19.736-8.853l0.023-0.026 63.087-70.332c4.248-4.671 6.849-10.906 6.849-17.749 0-14.593-11.83-26.424-26.424-26.424-7.847 0-14.896 3.421-19.736 8.853l-0.023 0.026zM1036.237 505.369l-94.364 5.108c-13.968 0.779-24.999 12.296-24.999 26.389 0 14.595 11.832 26.427 26.427 26.427 0.502 0 1.001-0.014 1.497-0.042l-0.069 0.003 94.341-5.108c13.907-0.847 24.864-12.333 24.864-26.378 0-14.543-11.749-26.342-26.273-26.423h-0.008zM855.798 711.906c-4.192 4.657-6.756 10.853-6.756 17.647 0 7.799 3.379 14.809 8.753 19.646l0.024 0.021 70.332 63.11c4.671 4.248 10.906 6.849 17.749 6.849 14.593 0 26.424-11.83 26.424-26.424 0-7.847-3.421-14.896-8.853-19.736l-0.026-0.023-70.332-63.11c-4.657-4.192-10.853-6.756-17.647-6.756-7.799 0-14.809 3.379-19.646 8.753l-0.021 0.024zM478.877 732.339l-63.11 70.378c-4.191 4.658-6.754 10.853-6.754 17.647 0 14.598 11.834 26.432 26.432 26.432 7.804 0 14.819-3.382 19.657-8.761l0.021-0.024 63.041-70.378c4.182-4.662 6.739-10.856 6.739-17.647 0-14.622-11.853-26.475-26.475-26.475-7.831 0-14.867 3.4-19.715 8.804l-0.022 0.025zM689.826 791.433c-13.969 0.776-25.002 12.292-25.002 26.385 0 0.52 0.015 1.036 0.045 1.549l-0.003-0.071 5.108 94.387c0.779 13.968 12.296 24.999 26.389 24.999 14.595 0 26.427-11.832 26.427-26.427 0-0.502-0.014-1.001-0.042-1.497l0.003 0.069-5.108-94.387c-0.785-13.958-12.293-24.98-26.377-24.984v0z");
                map.Add(25, "Sonne, (starker Hagel)"); mapIcon.Add(25, "M160.005-77.578c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l136.762 136.739c4.818 5.025 11.586 8.147 19.083 8.147 14.593 0 26.424-11.83 26.424-26.424 0-7.497-3.122-14.265-8.138-19.075l-136.771-136.748c-4.782-4.786-11.391-7.747-18.692-7.747s-13.909 2.96-18.691 7.746v0zM438.243 51.824c-4.779 4.781-7.735 11.385-7.735 18.68s2.956 13.899 7.735 18.68l100.447 100.447h-279.098l-137.807-137.807c-4.783-4.786-11.391-7.746-18.692-7.746-14.595 0-26.426 11.831-26.426 26.426 0 7.294 2.955 13.898 7.734 18.68l100.958 100.981c-102.967 2.856-185.348 87.007-185.348 190.399 0 105.193 85.275 190.468 190.468 190.468 1.8 0 3.595-0.025 5.383-0.075l-0.263 0.006c2.972 0 5.898-0.232 8.847-0.372 44.954 79.294 128.692 131.98 224.755 132.211h0.033c0.112 0 0.244 0 0.377 0 40.402 0 78.614-9.375 112.579-26.070l-1.502 0.667c17.618 19.029 39.010 34.288 63.019 44.63l1.206 0.462c21.249 9.194 45.994 14.542 71.987 14.542 102.423 0 185.454-83.030 185.454-185.454 0-70.095-38.888-131.107-96.266-162.639l-0.961-0.484c12.568-19.598 20.035-43.513 20.035-69.171 0-71.48-57.946-129.425-129.425-129.425-0.072 0-0.144 0-0.217 0h-42.155l-137.807-137.831c-4.781-4.779-11.385-7.735-18.68-7.735s-13.899 2.956-18.68 7.735v0zM57.863 380.286c0.079-76.048 61.693-137.678 137.727-137.784h459.894c0.003 0 0.006 0 0.009 0 42.408 0 76.787 34.379 76.787 76.787s-34.379 76.787-76.787 76.787c-18.941 0-36.28-6.858-49.669-18.226l0.11 0.091c-4.574-3.892-10.549-6.26-17.078-6.26-14.595 0-26.426 11.831-26.426 26.426 0 8.066 3.614 15.288 9.311 20.135l0.037 0.031c16.912 14.369 37.826 24.536 60.841 28.63l0.783 0.116c-0.765 112.165-91.869 202.797-204.141 202.797-70.245 0-132.204-35.479-168.934-89.495l-0.456-0.711c20.973-7.591 39.11-17.904 55.125-30.754l-0.374 0.29c6.073-4.883 9.927-12.313 9.927-20.642 0-14.594-11.831-26.425-26.425-26.425-6.264 0-12.020 2.18-16.549 5.822l0.051-0.040c-23.179 18.801-53.039 30.186-85.558 30.186-0.124 0-0.249 0-0.373-0.001h0.019c-76.091-0.026-137.775-61.677-137.854-137.753v-0.007zM625.415 673.71v0c-14.869-6.466-27.622-14.948-38.614-25.281l0.069 0.065c60.753-47.407 99.455-120.628 99.472-202.889v-0.003q0-0.232 0-0.488c14.978-3.729 28.146-9.591 39.962-17.344l-0.489 0.301c49.423 20.042 83.645 67.667 83.645 123.284 0 70.73-55.346 128.532-125.092 132.463l-0.348 0.016q-3.622 0.186-7.244 0.186c-0.101 0-0.221 0-0.341 0-18.423 0-35.961-3.791-51.875-10.637l0.854 0.327zM898.453 264.677l-63.11 70.262c-4.248 4.671-6.849 10.906-6.849 17.749 0 14.593 11.83 26.424 26.424 26.424 7.847 0 14.896-3.421 19.736-8.853l0.023-0.026 63.087-70.332c4.248-4.671 6.849-10.906 6.849-17.749 0-14.593-11.83-26.424-26.424-26.424-7.847 0-14.896 3.421-19.736 8.853l-0.023 0.026zM1036.237 505.369l-94.364 5.108c-13.968 0.779-24.999 12.296-24.999 26.389 0 14.595 11.832 26.427 26.427 26.427 0.502 0 1.001-0.014 1.497-0.042l-0.069 0.003 94.341-5.108c13.907-0.847 24.864-12.333 24.864-26.378 0-14.543-11.749-26.342-26.273-26.423h-0.008zM855.798 711.906c-4.192 4.657-6.756 10.853-6.756 17.647 0 7.799 3.379 14.809 8.753 19.646l0.024 0.021 70.332 63.11c4.671 4.248 10.906 6.849 17.749 6.849 14.593 0 26.424-11.83 26.424-26.424 0-7.847-3.421-14.896-8.853-19.736l-0.026-0.023-70.332-63.11c-4.657-4.192-10.853-6.756-17.647-6.756-7.799 0-14.809 3.379-19.646 8.753l-0.021 0.024zM478.877 732.339l-63.11 70.378c-4.191 4.658-6.754 10.853-6.754 17.647 0 14.598 11.834 26.432 26.432 26.432 7.804 0 14.819-3.382 19.657-8.761l0.021-0.024 63.041-70.378c4.182-4.662 6.739-10.856 6.739-17.647 0-14.622-11.853-26.475-26.475-26.475-7.831 0-14.867 3.4-19.715 8.804l-0.022 0.025zM689.826 791.433c-13.969 0.776-25.002 12.292-25.002 26.385 0 0.52 0.015 1.036 0.045 1.549l-0.003-0.071 5.108 94.387c0.779 13.968 12.296 24.999 26.389 24.999 14.595 0 26.427-11.832 26.427-26.427 0-0.502-0.014-1.001-0.042-1.497l0.003 0.069-5.108-94.387c-0.785-13.958-12.293-24.98-26.377-24.984v0z");
                map.Add(26, "Gewitter"); mapIcon.Add(26, "M282.914-78.394c-4.294 4.289-6.951 10.217-6.951 16.765s2.656 12.476 6.95 16.765l29.029 28.988h-9.482c-1.64 0.004-3.237 0.178-4.777 0.506l0.151-0.027c-0.845 0.196-1.542 0.403-2.221 0.647l0.137-0.043c-0.954 0.24-1.746 0.497-2.516 0.799l0.14-0.048c-0.921 0.415-1.679 0.819-2.41 1.264l0.097-0.055c-0.583 0.313-1.209 0.583-1.771 0.959-2.627 1.759-4.819 3.944-6.532 6.481l-0.053 0.084c-0.375 0.542-0.625 1.146-0.938 1.709-0.388 0.636-0.805 1.415-1.183 2.216l-0.068 0.16c-0.23 0.601-0.473 1.38-0.675 2.176l-0.034 0.158c-0.205 0.539-0.419 1.235-0.596 1.945l-0.029 0.139c-0.296 1.394-0.465 2.996-0.465 4.637s0.169 3.243 0.492 4.788l-0.026-0.152c0.208 0.852 0.422 1.548 0.671 2.227l-0.046-0.143c0.239 0.96 0.482 1.739 0.762 2.5l-0.054-0.166c0.443 0.959 0.867 1.745 1.332 2.503l-0.061-0.106c0.327 0.681 0.633 1.234 0.964 1.769l-0.047-0.081c0.917 1.365 1.898 2.558 2.981 3.648l66.685 66.685c4.291 4.289 10.219 6.941 16.765 6.941 13.098 0 23.717-10.618 23.717-23.717 0-6.552-2.657-12.483-6.952-16.775l-26.237-26.237h9.482c1.648-0.006 3.253-0.18 4.801-0.506l-0.154 0.027c0.709-0.146 1.355-0.396 2.084-0.604 0.964-0.246 1.756-0.503 2.527-0.802l-0.151 0.051c0.929-0.424 1.688-0.827 2.421-1.269l-0.107 0.060c0.583-0.333 1.209-0.583 1.771-0.959 2.614-1.773 4.799-3.964 6.51-6.5l0.054-0.085c0.284-0.454 0.59-1.007 0.867-1.576l0.049-0.112c0.409-0.657 0.833-1.443 1.212-2.254l0.060-0.143c0.227-0.59 0.47-1.362 0.673-2.149l0.036-0.164c0.2-0.53 0.414-1.225 0.593-1.935l0.032-0.149c0.293-1.394 0.461-2.996 0.461-4.637s-0.168-3.243-0.487-4.789l0.026 0.152c-0.146-0.729-0.417-1.396-0.625-2.084-0.249-0.956-0.499-1.726-0.785-2.479l0.055 0.166c-0.44-0.955-0.864-1.741-1.33-2.498l0.058 0.102c-0.313-0.563-0.563-1.146-0.917-1.709-0.919-1.365-1.9-2.557-2.982-3.649l-69.434-69.414c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM188.45 47.205c-4.29 4.295-6.944 10.225-6.944 16.776s2.653 12.481 6.944 16.776v0l74.459 74.459h-53.182c-1.617 0.016-3.185 0.182-4.703 0.485l0.16-0.027c-0.833 0.203-1.528 0.425-2.203 0.687l0.119-0.041c-0.916 0.234-1.666 0.476-2.397 0.758l0.146-0.049c-0.984 0.44-1.798 0.872-2.58 1.349l0.1-0.057c-0.542 0.292-1.084 0.542-1.605 0.875-2.631 1.776-4.83 3.975-6.552 6.521l-0.054 0.085c-0.313 0.458-0.5 0.938-0.771 1.417-0.469 0.742-0.942 1.617-1.358 2.523l-0.059 0.144c-0.271 0.646-0.396 1.334-0.625 2.084-0.238 0.629-0.488 1.443-0.695 2.274l-0.034 0.164c-0.209 1.085-0.344 2.35-0.375 3.64l-0.001 0.028c0 0.313 0 0.604 0 0.938v1c0.005 2.153 0.418 4.208 1.165 6.094l-0.039-0.113c0.208 0.667 0.354 1.334 0.625 2.084 0.475 1.060 0.949 1.942 1.474 2.787l-0.057-0.099c0.271 0.458 0.458 0.959 0.771 1.396 0.92 1.384 1.915 2.591 3.020 3.687l69.834 69.834h-91.693c-94.629 0-171.34 76.712-171.34 171.34s76.712 171.34 171.34 171.34v0c2.751 0 5.46-0.208 8.19-0.354 40.296 71.189 115.411 118.514 201.603 118.784h0.038c0.1 0 0.219 0 0.338 0 36.26 0 70.555-8.414 101.039-23.397l-1.348 0.599c15.829 17.097 35.049 30.806 56.62 40.096l1.084 0.415c18.904 8.087 40.9 12.789 63.995 12.789 91.959 0 166.506-74.547 166.506-166.506 0-62.7-34.656-117.305-85.858-145.706l-0.852-0.434c11.521-17.744 18.371-39.446 18.371-62.747 0-64.256-52.090-116.346-116.346-116.346-0.063 0-0.125 0-0.188 0h-72.698l-19.297-19.297h9.503c1.624-0.003 3.208-0.17 4.737-0.485l-0.152 0.026c0.85-0.206 1.546-0.421 2.225-0.67l-0.141 0.045c0.931-0.233 1.696-0.475 2.441-0.758l-0.148 0.049c0.963-0.439 1.756-0.863 2.52-1.33l-0.103 0.058c0.563-0.313 1.125-0.542 1.667-0.896 2.631-1.769 4.83-3.961 6.552-6.501l0.054-0.084c0.375-0.563 0.625-1.188 0.959-1.771 0.375-0.614 0.778-1.366 1.143-2.139l0.065-0.154c0.249-0.632 0.506-1.438 0.717-2.263l0.033-0.154c0.209-0.555 0.417-1.252 0.581-1.965l0.023-0.119c0.288-1.397 0.452-3.003 0.452-4.647s-0.165-3.25-0.479-4.802l0.026 0.154c-0.146-0.709-0.417-1.355-0.604-2.084-0.245-0.971-0.502-1.77-0.801-2.548l0.051 0.151c-0.418-0.914-0.814-1.659-1.248-2.379l0.060 0.107c-0.333-0.604-0.604-1.23-1-1.834-0.901-1.333-1.861-2.498-2.919-3.565l-69.476-69.455c-4.292-4.295-10.224-6.952-16.776-6.952-13.098 0-23.717 10.618-23.717 23.717 0 6.547 2.653 12.474 6.941 16.765v0l29.050 28.987h-9.419c-1.642 0.007-3.238 0.181-4.78 0.506l0.153-0.027c-0.729 0.125-1.375 0.417-2.084 0.604-0.928 0.226-1.707 0.476-2.461 0.774l0.127-0.044c-0.957 0.442-1.736 0.859-2.489 1.314l0.113-0.064c-0.563 0.313-1.167 0.563-1.709 0.938-2.627 1.766-4.819 3.959-6.532 6.501l-0.054 0.085c-0.375 0.583-0.646 1.209-0.979 1.792-0.379 0.618-0.775 1.363-1.13 2.131l-0.058 0.14c-0.25 0.634-0.514 1.454-0.734 2.291l-0.037 0.168c-0.202 0.553-0.403 1.251-0.561 1.964l-0.022 0.12c-0.29 1.397-0.457 3.003-0.457 4.647s0.166 3.25 0.483 4.801l-0.026-0.154c0.146 0.688 0.396 1.355 0.604 2.084 0.248 0.985 0.505 1.791 0.803 2.577l-0.053-0.16c0.412 0.899 0.808 1.637 1.243 2.349l-0.056-0.098c0.333 0.604 0.604 1.25 1 1.834 0.906 1.332 1.865 2.496 2.92 3.566l26.234 26.254h-114.804l-62.935-62.914h53.119c1.647-0.003 3.252-0.178 4.799-0.506l-0.152 0.027c0.688-0.125 1.292-0.375 1.959-0.563 1.008-0.254 1.835-0.518 2.64-0.825l-0.161 0.054q1.063-0.521 2.084-1.125c0.667-0.375 1.334-0.646 2.084-1.084 1.19-0.816 2.231-1.673 3.194-2.61l-0.005 0.005c0.125-0.125 0.292-0.208 0.417-0.333s0.292-0.396 0.479-0.583c0.869-0.904 1.677-1.884 2.407-2.922l0.052-0.079q0.625-1.021 1.146-2.084c0.353-0.582 0.702-1.267 1.002-1.978l0.040-0.106c0.3-0.743 0.593-1.661 0.824-2.603l0.031-0.148c0.167-0.563 0.375-1.104 0.5-1.688 0.293-1.403 0.461-3.016 0.461-4.668s-0.168-3.265-0.487-4.822l0.026 0.154c-0.125-0.583-0.333-1.104-0.5-1.646-0.273-1.115-0.566-2.039-0.911-2.938l0.056 0.167c-0.292-0.688-0.688-1.313-1.042-2.084-0.383-0.817-0.765-1.5-1.189-2.154l0.043 0.070c-0.896-1.325-1.849-2.483-2.899-3.545l-114.968-114.968c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM47.243 436.899c0.071-68.409 55.503-123.848 123.902-123.931h413.501c0.006 0 0.014 0 0.022 0 38.165 0 69.103 30.938 69.103 69.103s-30.938 69.103-69.103 69.103c-17.044 0-32.647-6.171-44.696-16.399l0.099 0.082c-4.105-3.493-9.468-5.618-15.327-5.618-13.099 0-23.717 10.618-23.717 23.717 0 7.239 3.243 13.721 8.356 18.071l0.034 0.028c15.143 12.835 33.852 21.925 54.434 25.611l0.706 0.105c-0.334 100.947-82.245 182.652-183.239 182.652-63.197 0-118.921-31.992-151.861-80.665l-0.412-0.646c18.814-6.821 35.084-16.076 49.455-27.602l-0.336 0.261c5.451-4.383 8.91-11.050 8.91-18.526 0-13.098-10.618-23.716-23.716-23.716-5.622 0-10.788 1.956-14.852 5.226l0.046-0.036c-20.841 16.875-47.677 27.092-76.898 27.092-0.161 0-0.321 0-0.482-0.001h0.025c-68.408-0.047-123.857-55.467-123.952-123.86v-0.009zM557.305 700.787c-13.357-5.821-24.81-13.461-34.672-22.769l0.058 0.054c54.528-42.545 89.263-108.262 89.275-182.091v-0.752c13.386-3.273 25.166-8.444 35.754-15.299l-0.452 0.274c44.464 17.938 75.271 60.734 75.271 110.725 0 63.506-49.714 115.4-112.347 118.889l-0.309 0.014q-3.251 0.167-6.481 0.167c-0.093 0-0.203 0-0.312 0-16.528 0-32.266-3.387-46.556-9.505l0.772 0.294zM802.354 333.682l-56.641 63.060c-3.812 4.193-6.147 9.788-6.147 15.93 0 13.097 10.618 23.715 23.715 23.715 7.043 0 13.369-3.070 17.712-7.945l0.021-0.024 56.537-62.935c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM925.993 549.702l-84.67 4.605c-12.536 0.699-22.437 11.035-22.437 23.684 0 13.099 10.619 23.718 23.718 23.718 0.451 0 0.899-0.013 1.343-0.037l-0.062 0.003 84.67-4.605c12.481-0.76 22.316-11.069 22.316-23.674 0-13.052-10.545-23.642-23.58-23.715h-0.007zM764.072 735.172c-3.768 4.178-6.072 9.74-6.072 15.839 0 7.001 3.036 13.292 7.864 17.63l0.022 0.019 63.122 56.558c4.193 3.812 9.788 6.147 15.93 6.147 13.097 0 23.715-10.618 23.715-23.715 0-7.043-3.070-13.369-7.945-17.712l-0.024-0.021-63.122-56.641c-4.179-3.757-9.735-6.054-15.827-6.054-7.005 0-13.301 3.037-17.642 7.867l-0.019 0.022zM425.809 753.51l-56.641 63.143c-3.762 4.18-6.064 9.74-6.064 15.838 0 7 3.032 13.291 7.855 17.632l0.021 0.019c4.179 3.757 9.735 6.054 15.827 6.054 7.005 0 13.301-3.037 17.642-7.867l0.019-0.022 56.641-63.122c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM615.113 806.525c-12.537 0.696-22.439 11.032-22.439 23.681 0 0.467 0.013 0.93 0.040 1.39l-0.003-0.064 4.585 84.691c0.699 12.536 11.035 22.437 23.684 22.437 13.099 0 23.718-10.619 23.718-23.718 0-0.451-0.013-0.899-0.037-1.343l0.003 0.062-4.585-84.712c-0.705-12.527-11.033-22.42-23.673-22.423v0z");
                map.Add(27, "Gewitter, Regen"); mapIcon.Add(27, "M282.914-78.394c-4.294 4.289-6.951 10.217-6.951 16.765s2.656 12.476 6.95 16.765l29.029 28.988h-9.482c-1.64 0.004-3.237 0.178-4.777 0.506l0.151-0.027c-0.845 0.196-1.542 0.403-2.221 0.647l0.137-0.043c-0.954 0.24-1.746 0.497-2.516 0.799l0.14-0.048c-0.921 0.415-1.679 0.819-2.41 1.264l0.097-0.055c-0.583 0.313-1.209 0.583-1.771 0.959-2.627 1.759-4.819 3.944-6.532 6.481l-0.053 0.084c-0.375 0.542-0.625 1.146-0.938 1.709-0.388 0.636-0.805 1.415-1.183 2.216l-0.068 0.16c-0.23 0.601-0.473 1.38-0.675 2.176l-0.034 0.158c-0.205 0.539-0.419 1.235-0.596 1.945l-0.029 0.139c-0.296 1.394-0.465 2.996-0.465 4.637s0.169 3.243 0.492 4.788l-0.026-0.152c0.208 0.852 0.422 1.548 0.671 2.227l-0.046-0.143c0.239 0.96 0.482 1.739 0.762 2.5l-0.054-0.166c0.443 0.959 0.867 1.745 1.332 2.503l-0.061-0.106c0.327 0.681 0.633 1.234 0.964 1.769l-0.047-0.081c0.917 1.365 1.898 2.558 2.981 3.648l66.685 66.685c4.291 4.289 10.219 6.941 16.765 6.941 13.098 0 23.717-10.618 23.717-23.717 0-6.552-2.657-12.483-6.952-16.775l-26.237-26.237h9.482c1.648-0.006 3.253-0.18 4.801-0.506l-0.154 0.027c0.709-0.146 1.355-0.396 2.084-0.604 0.964-0.246 1.756-0.503 2.527-0.802l-0.151 0.051c0.929-0.424 1.688-0.827 2.421-1.269l-0.107 0.060c0.583-0.333 1.209-0.583 1.771-0.959 2.614-1.773 4.799-3.964 6.51-6.5l0.054-0.085c0.284-0.454 0.59-1.007 0.867-1.576l0.049-0.112c0.409-0.657 0.833-1.443 1.212-2.254l0.060-0.143c0.227-0.59 0.47-1.362 0.673-2.149l0.036-0.164c0.2-0.53 0.414-1.225 0.593-1.935l0.032-0.149c0.293-1.394 0.461-2.996 0.461-4.637s-0.168-3.243-0.487-4.789l0.026 0.152c-0.146-0.729-0.417-1.396-0.625-2.084-0.249-0.956-0.499-1.726-0.785-2.479l0.055 0.166c-0.44-0.955-0.864-1.741-1.33-2.498l0.058 0.102c-0.313-0.563-0.563-1.146-0.917-1.709-0.919-1.365-1.9-2.557-2.982-3.649l-69.434-69.414c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM188.45 47.205c-4.29 4.295-6.944 10.225-6.944 16.776s2.653 12.481 6.944 16.776v0l74.459 74.459h-53.182c-1.617 0.016-3.185 0.182-4.703 0.485l0.16-0.027c-0.833 0.203-1.528 0.425-2.203 0.687l0.119-0.041c-0.916 0.234-1.666 0.476-2.397 0.758l0.146-0.049c-0.984 0.44-1.798 0.872-2.58 1.349l0.1-0.057c-0.542 0.292-1.084 0.542-1.605 0.875-2.631 1.776-4.83 3.975-6.552 6.521l-0.054 0.085c-0.313 0.458-0.5 0.938-0.771 1.417-0.469 0.742-0.942 1.617-1.358 2.523l-0.059 0.144c-0.271 0.646-0.396 1.334-0.625 2.084-0.238 0.629-0.488 1.443-0.695 2.274l-0.034 0.164c-0.209 1.085-0.344 2.35-0.375 3.64l-0.001 0.028c0 0.313 0 0.604 0 0.938v1c0.005 2.153 0.418 4.208 1.165 6.094l-0.039-0.113c0.208 0.667 0.354 1.334 0.625 2.084 0.475 1.060 0.949 1.942 1.474 2.787l-0.057-0.099c0.271 0.458 0.458 0.959 0.771 1.396 0.92 1.384 1.915 2.591 3.020 3.687l69.834 69.834h-91.693c-94.629 0-171.34 76.712-171.34 171.34s76.712 171.34 171.34 171.34v0c2.751 0 5.46-0.208 8.19-0.354 40.296 71.189 115.411 118.514 201.603 118.784h0.038c0.1 0 0.219 0 0.338 0 36.26 0 70.555-8.414 101.039-23.397l-1.348 0.599c15.829 17.097 35.049 30.806 56.62 40.096l1.084 0.415c18.904 8.087 40.9 12.789 63.995 12.789 91.959 0 166.506-74.547 166.506-166.506 0-62.7-34.656-117.305-85.858-145.706l-0.852-0.434c11.521-17.744 18.371-39.446 18.371-62.747 0-64.256-52.090-116.346-116.346-116.346-0.063 0-0.125 0-0.188 0h-72.698l-19.297-19.297h9.503c1.624-0.003 3.208-0.17 4.737-0.485l-0.152 0.026c0.85-0.206 1.546-0.421 2.225-0.67l-0.141 0.045c0.931-0.233 1.696-0.475 2.441-0.758l-0.148 0.049c0.963-0.439 1.756-0.863 2.52-1.33l-0.103 0.058c0.563-0.313 1.125-0.542 1.667-0.896 2.631-1.769 4.83-3.961 6.552-6.501l0.054-0.084c0.375-0.563 0.625-1.188 0.959-1.771 0.375-0.614 0.778-1.366 1.143-2.139l0.065-0.154c0.249-0.632 0.506-1.438 0.717-2.263l0.033-0.154c0.209-0.555 0.417-1.252 0.581-1.965l0.023-0.119c0.288-1.397 0.452-3.003 0.452-4.647s-0.165-3.25-0.479-4.802l0.026 0.154c-0.146-0.709-0.417-1.355-0.604-2.084-0.245-0.971-0.502-1.77-0.801-2.548l0.051 0.151c-0.418-0.914-0.814-1.659-1.248-2.379l0.060 0.107c-0.333-0.604-0.604-1.23-1-1.834-0.901-1.333-1.861-2.498-2.919-3.565l-69.476-69.455c-4.292-4.295-10.224-6.952-16.776-6.952-13.098 0-23.717 10.618-23.717 23.717 0 6.547 2.653 12.474 6.941 16.765v0l29.050 28.987h-9.419c-1.642 0.007-3.238 0.181-4.78 0.506l0.153-0.027c-0.729 0.125-1.375 0.417-2.084 0.604-0.928 0.226-1.707 0.476-2.461 0.774l0.127-0.044c-0.957 0.442-1.736 0.859-2.489 1.314l0.113-0.064c-0.563 0.313-1.167 0.563-1.709 0.938-2.627 1.766-4.819 3.959-6.532 6.501l-0.054 0.085c-0.375 0.583-0.646 1.209-0.979 1.792-0.379 0.618-0.775 1.363-1.13 2.131l-0.058 0.14c-0.25 0.634-0.514 1.454-0.734 2.291l-0.037 0.168c-0.202 0.553-0.403 1.251-0.561 1.964l-0.022 0.12c-0.29 1.397-0.457 3.003-0.457 4.647s0.166 3.25 0.483 4.801l-0.026-0.154c0.146 0.688 0.396 1.355 0.604 2.084 0.248 0.985 0.505 1.791 0.803 2.577l-0.053-0.16c0.412 0.899 0.808 1.637 1.243 2.349l-0.056-0.098c0.333 0.604 0.604 1.25 1 1.834 0.906 1.332 1.865 2.496 2.92 3.566l26.234 26.254h-114.804l-62.935-62.914h53.119c1.647-0.003 3.252-0.178 4.799-0.506l-0.152 0.027c0.688-0.125 1.292-0.375 1.959-0.563 1.008-0.254 1.835-0.518 2.64-0.825l-0.161 0.054q1.063-0.521 2.084-1.125c0.667-0.375 1.334-0.646 2.084-1.084 1.19-0.816 2.231-1.673 3.194-2.61l-0.005 0.005c0.125-0.125 0.292-0.208 0.417-0.333s0.292-0.396 0.479-0.583c0.869-0.904 1.677-1.884 2.407-2.922l0.052-0.079q0.625-1.021 1.146-2.084c0.353-0.582 0.702-1.267 1.002-1.978l0.040-0.106c0.3-0.743 0.593-1.661 0.824-2.603l0.031-0.148c0.167-0.563 0.375-1.104 0.5-1.688 0.293-1.403 0.461-3.016 0.461-4.668s-0.168-3.265-0.487-4.822l0.026 0.154c-0.125-0.583-0.333-1.104-0.5-1.646-0.273-1.115-0.566-2.039-0.911-2.938l0.056 0.167c-0.292-0.688-0.688-1.313-1.042-2.084-0.383-0.817-0.765-1.5-1.189-2.154l0.043 0.070c-0.896-1.325-1.849-2.483-2.899-3.545l-114.968-114.968c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM47.243 436.899c0.071-68.409 55.503-123.848 123.902-123.931h413.501c0.006 0 0.014 0 0.022 0 38.165 0 69.103 30.938 69.103 69.103s-30.938 69.103-69.103 69.103c-17.044 0-32.647-6.171-44.696-16.399l0.099 0.082c-4.105-3.493-9.468-5.618-15.327-5.618-13.099 0-23.717 10.618-23.717 23.717 0 7.239 3.243 13.721 8.356 18.071l0.034 0.028c15.143 12.835 33.852 21.925 54.434 25.611l0.706 0.105c-0.334 100.947-82.245 182.652-183.239 182.652-63.197 0-118.921-31.992-151.861-80.665l-0.412-0.646c18.814-6.821 35.084-16.076 49.455-27.602l-0.336 0.261c5.451-4.383 8.91-11.050 8.91-18.526 0-13.098-10.618-23.716-23.716-23.716-5.622 0-10.788 1.956-14.852 5.226l0.046-0.036c-20.841 16.875-47.677 27.092-76.898 27.092-0.161 0-0.321 0-0.482-0.001h0.025c-68.408-0.047-123.857-55.467-123.952-123.86v-0.009zM557.305 700.787c-13.357-5.821-24.81-13.461-34.672-22.769l0.058 0.054c54.528-42.545 89.263-108.262 89.275-182.091v-0.752c13.386-3.273 25.166-8.444 35.754-15.299l-0.452 0.274c44.464 17.938 75.271 60.734 75.271 110.725 0 63.506-49.714 115.4-112.347 118.889l-0.309 0.014q-3.251 0.167-6.481 0.167c-0.093 0-0.203 0-0.312 0-16.528 0-32.266-3.387-46.556-9.505l0.772 0.294zM802.354 333.682l-56.641 63.060c-3.812 4.193-6.147 9.788-6.147 15.93 0 13.097 10.618 23.715 23.715 23.715 7.043 0 13.369-3.070 17.712-7.945l0.021-0.024 56.537-62.935c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM925.993 549.702l-84.67 4.605c-12.536 0.699-22.437 11.035-22.437 23.684 0 13.099 10.619 23.718 23.718 23.718 0.451 0 0.899-0.013 1.343-0.037l-0.062 0.003 84.67-4.605c12.481-0.76 22.316-11.069 22.316-23.674 0-13.052-10.545-23.642-23.58-23.715h-0.007zM764.072 735.172c-3.768 4.178-6.072 9.74-6.072 15.839 0 7.001 3.036 13.292 7.864 17.63l0.022 0.019 63.122 56.558c4.193 3.812 9.788 6.147 15.93 6.147 13.097 0 23.715-10.618 23.715-23.715 0-7.043-3.070-13.369-7.945-17.712l-0.024-0.021-63.122-56.641c-4.179-3.757-9.735-6.054-15.827-6.054-7.005 0-13.301 3.037-17.642 7.867l-0.019 0.022zM425.809 753.51l-56.641 63.143c-3.762 4.18-6.064 9.74-6.064 15.838 0 7 3.032 13.291 7.855 17.632l0.021 0.019c4.179 3.757 9.735 6.054 15.827 6.054 7.005 0 13.301-3.037 17.642-7.867l0.019-0.022 56.641-63.122c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM615.113 806.525c-12.537 0.696-22.439 11.032-22.439 23.681 0 0.467 0.013 0.93 0.040 1.39l-0.003-0.064 4.585 84.691c0.699 12.536 11.035 22.437 23.684 22.437 13.099 0 23.718-10.619 23.718-23.718 0-0.451-0.013-0.899-0.037-1.343l0.003 0.062-4.585-84.712c-0.705-12.527-11.033-22.42-23.673-22.423v0z");
                map.Add(28, "Gewitter, starker Regen"); mapIcon.Add(28, "M356.019-76.506c-5.407 5.41-8.751 12.882-8.751 21.135s3.344 15.725 8.752 21.135l36.595 36.595h-11.953c-2.066 0.004-4.081 0.215-6.027 0.611l0.195-0.033c-0.919 0.184-1.734 0.525-2.627 0.762-1.176 0.282-2.176 0.606-3.14 0.998l0.145-0.052c-1.164 0.527-2.121 1.036-3.043 1.595l0.127-0.072c-0.736 0.42-1.524 0.736-2.233 1.208-3.312 2.226-6.076 4.989-8.234 8.195l-0.068 0.107c-0.447 0.683-0.762 1.445-1.156 2.154-0.508 0.821-1.034 1.803-1.503 2.818l-0.073 0.177c-0.394 0.972-0.63 1.97-0.919 2.942s-0.604 1.708-0.788 2.627c-0.361 1.757-0.568 3.777-0.568 5.845s0.207 4.088 0.601 6.040l-0.033-0.195c0.184 0.893 0.525 1.76 0.788 2.627 0.304 1.198 0.619 2.179 0.984 3.135l-0.065-0.193c0.535 1.197 1.061 2.188 1.644 3.141l-0.068-0.12c0.394 0.709 0.709 1.445 1.156 2.154 1.169 1.719 2.413 3.222 3.784 4.598l84.065 84.065c5.455 5.682 13.114 9.213 21.598 9.213 16.526 0 29.922-13.397 29.922-29.922 0-8.438-3.493-16.060-9.111-21.5l-33.109-33.109h12.084c2.067-0.006 4.081-0.216 6.028-0.611l-0.196 0.033c0.919-0.184 1.76-0.525 2.627-0.762 1.188-0.298 2.169-0.613 3.124-0.981l-0.182 0.062c1.188-0.539 2.153-1.048 3.087-1.604l-0.145 0.080c0.884-0.426 1.607-0.828 2.305-1.266l-0.099 0.058c1.713-1.144 3.208-2.372 4.572-3.731l-0.001 0.001c1.327-1.345 2.536-2.813 3.608-4.383l0.070-0.109c0.473-0.736 0.814-1.524 1.235-2.286 0.482-0.783 0.982-1.723 1.427-2.692l0.071-0.171c0.309-0.783 0.633-1.791 0.901-2.82l0.044-0.201c0.258-0.691 0.52-1.569 0.73-2.467l0.032-0.16c0.366-1.761 0.575-3.785 0.575-5.858s-0.21-4.097-0.609-6.052l0.033 0.194c-0.251-1.073-0.513-1.951-0.818-2.808l0.056 0.181c-0.309-1.236-0.633-2.252-1.011-3.242l0.065 0.195c-0.526-1.151-1.034-2.099-1.593-3.012l0.070 0.122c-0.42-0.762-0.736-1.55-1.208-2.259-1.162-1.721-2.407-3.224-3.782-4.597v0l-87.612-87.586c-5.41-5.407-12.882-8.751-21.135-8.751s-15.725 3.344-21.135 8.752v0zM236.934 81.853c-5.415 5.411-8.765 12.888-8.765 21.148s3.35 15.737 8.764 21.148l93.891 93.865h-66.964c-2.069 0.009-4.083 0.219-6.031 0.611l0.199-0.033c-0.893 0.184-1.708 0.525-2.627 0.762-1.209 0.306-2.207 0.63-3.178 1.009l0.183-0.063c-1.141 0.516-2.080 1.016-2.985 1.566l0.122-0.069c-0.762 0.42-1.55 0.736-2.286 1.235-1.712 1.146-3.207 2.374-4.572 3.732l0.001-0.001c-1.324 1.346-2.533 2.814-3.607 4.383l-0.071 0.11c-0.473 0.762-0.841 1.55-1.261 2.312-0.479 0.775-0.979 1.705-1.425 2.665l-0.072 0.172c-0.323 0.811-0.647 1.828-0.908 2.869l-0.038 0.178c-0.251 0.679-0.513 1.557-0.727 2.454l-0.035 0.173c-0.374 1.761-0.588 3.785-0.588 5.858s0.214 4.097 0.621 6.050l-0.033-0.192c0.158 0.867 0.499 1.681 0.736 2.627 0.309 1.247 0.633 2.272 1.011 3.271l-0.066-0.197c0.509 1.135 1.009 2.074 1.562 2.978l-0.065-0.114c0.437 0.907 0.848 1.647 1.295 2.363l-0.060-0.103c1.154 1.722 2.391 3.225 3.758 4.599l89.030 89.030h-116.090c-119.103 0-215.655 96.552-215.655 215.655s96.552 215.655 215.655 215.655v0c3.336 0 6.673-0.289 9.983-0.473 50.667 89.872 145.498 149.57 254.273 149.57 160.656 0 290.897-130.226 290.92-290.877v-0.554c64.647-16.246 111.769-73.814 111.86-142.402v-0.010c-0.090-80.972-65.699-146.59-146.659-146.695h-90.433l-25.325-25.325h11.927c2.095-0.005 4.136-0.224 6.105-0.638l-0.194 0.034c0.968-0.236 1.757-0.479 2.527-0.762l-0.163 0.052c1.271-0.307 2.341-0.649 3.377-1.058l-0.172 0.060c1.124-0.522 2.036-1.012 2.92-1.547l-0.135 0.076c0.788-0.42 1.603-0.788 2.364-1.287 1.713-1.144 3.208-2.372 4.572-3.731l-0.001 0.001c1.332-1.343 2.542-2.812 3.609-4.385l0.069-0.107c0.401-0.631 0.813-1.38 1.179-2.155l0.056-0.13c0.481-0.783 0.981-1.722 1.427-2.692l0.071-0.172c0.394-0.972 0.63-2.023 0.946-3.021 0.263-0.699 0.525-1.577 0.733-2.476l0.029-0.151c0.367-1.761 0.577-3.785 0.577-5.858s-0.21-4.097-0.61-6.052l0.033 0.194c-0.184-0.893-0.499-1.681-0.762-2.627-0.319-1.253-0.643-2.269-1.016-3.261l0.071 0.214c-0.533-1.159-1.042-2.106-1.598-3.021l0.074 0.132c-0.42-0.762-0.736-1.55-1.208-2.259-1.152-1.624-2.377-3.050-3.722-4.353l-87.62-87.594c-5.451-5.685-13.109-9.218-21.591-9.218-16.511 0-29.896 13.385-29.896 29.896 0 8.482 3.533 16.14 9.207 21.581l0.011 0.010 36.595 36.595h-11.953c-2.066 0.003-4.080 0.214-6.027 0.611l0.195-0.033c-0.919 0.184-1.734 0.525-2.627 0.762-2.24 0.507-4.216 1.36-5.981 2.512l0.070-0.043c-0.736 0.42-1.524 0.736-2.233 1.235-3.315 2.213-6.079 4.969-8.235 8.17l-0.067 0.105c-0.357 0.577-0.742 1.283-1.093 2.009l-0.063 0.146c-1.174 1.696-2.080 3.68-2.602 5.815l-0.025 0.122c-0.263 0.699-0.525 1.577-0.733 2.476l-0.029 0.151c-0.365 1.757-0.574 3.777-0.574 5.845s0.209 4.088 0.607 6.039l-0.033-0.193c0.262 1.075 0.532 1.952 0.846 2.808l-0.058-0.181c0.293 1.197 0.6 2.179 0.957 3.136l-0.063-0.194c0.542 1.203 1.069 2.194 1.649 3.149l-0.072-0.128c0.394 0.683 0.709 1.445 1.156 2.128 1.162 1.72 2.398 3.222 3.76 4.6l34.149 34.149h-144.751l-80.309-80.309h66.964c2.065-0.001 4.079-0.212 6.025-0.611l-0.193 0.033c0.893-0.184 1.734-0.525 2.627-0.762 1.159-0.276 2.15-0.6 3.103-0.995l-0.135 0.049c1.13-0.503 2.069-1.003 2.971-1.559l-0.107 0.062c0.762-0.42 1.55-0.762 2.286-1.235 1.7-1.15 3.186-2.377 4.546-3.731l-0.001 0.001c1.334-1.342 2.545-2.811 3.61-4.386l0.068-0.106c0.389-0.626 0.8-1.384 1.172-2.165l0.063-0.147c0.478-0.78 0.978-1.719 1.425-2.688l0.073-0.176c0.295-0.771 0.61-1.779 0.873-2.807l0.046-0.214c0.258-0.691 0.52-1.569 0.73-2.467l0.032-0.16c0.367-1.761 0.577-3.785 0.577-5.858s-0.21-4.097-0.61-6.052l0.033 0.194c-0.184-0.867-0.499-1.681-0.762-2.627-0.319-1.253-0.643-2.269-1.016-3.26l0.070 0.213c-0.522-1.148-1.031-2.096-1.591-3.008l0.068 0.118c-0.431-0.913-0.834-1.654-1.272-2.371l0.063 0.111c-1.154-1.722-2.391-3.225-3.758-4.598l-144.776-144.881c-5.41-5.407-12.882-8.751-21.135-8.751s-15.725 3.344-21.135 8.752v0zM59.792 573.769c0.105-86.039 69.824-155.759 155.853-155.863h520.167c47.972 0.012 86.856 38.903 86.856 86.877 0 47.981-38.896 86.877-86.877 86.877-21.415 0-41.021-7.749-56.166-20.595l0.126 0.104c-5.173-4.397-11.928-7.071-19.309-7.071-16.513 0-29.9 13.387-29.9 29.9 0 9.133 4.095 17.309 10.548 22.794l0.043 0.035c19.13 16.236 42.784 27.721 68.811 32.34l0.885 0.13c-0.974 126.82-104.007 229.252-230.964 229.252-79.394 0-149.432-40.058-191.004-101.066l-0.514-0.8c23.739-8.586 44.268-20.255 62.395-34.795l-0.423 0.328c6.868-5.526 11.227-13.932 11.227-23.355 0-16.516-13.389-29.905-29.905-29.905-7.093 0-13.609 2.469-18.736 6.596l0.058-0.045c-26.212 21.272-59.983 34.152-96.763 34.152-0.154 0-0.308 0-0.462-0.001h0.024c-86.099-0.030-155.894-69.797-155.968-155.882v-0.007z");
                map.Add(29, "Gewitter, (Hagel)"); mapIcon.Add(29, "M282.914-78.394c-4.294 4.289-6.951 10.217-6.951 16.765s2.656 12.476 6.95 16.765l29.029 28.988h-9.482c-1.64 0.004-3.237 0.178-4.777 0.506l0.151-0.027c-0.845 0.196-1.542 0.403-2.221 0.647l0.137-0.043c-0.954 0.24-1.746 0.497-2.516 0.799l0.14-0.048c-0.921 0.415-1.679 0.819-2.41 1.264l0.097-0.055c-0.583 0.313-1.209 0.583-1.771 0.959-2.627 1.759-4.819 3.944-6.532 6.481l-0.053 0.084c-0.375 0.542-0.625 1.146-0.938 1.709-0.388 0.636-0.805 1.415-1.183 2.216l-0.068 0.16c-0.23 0.601-0.473 1.38-0.675 2.176l-0.034 0.158c-0.205 0.539-0.419 1.235-0.596 1.945l-0.029 0.139c-0.296 1.394-0.465 2.996-0.465 4.637s0.169 3.243 0.492 4.788l-0.026-0.152c0.208 0.852 0.422 1.548 0.671 2.227l-0.046-0.143c0.239 0.96 0.482 1.739 0.762 2.5l-0.054-0.166c0.443 0.959 0.867 1.745 1.332 2.503l-0.061-0.106c0.327 0.681 0.633 1.234 0.964 1.769l-0.047-0.081c0.917 1.365 1.898 2.558 2.981 3.648l66.685 66.685c4.291 4.289 10.219 6.941 16.765 6.941 13.098 0 23.717-10.618 23.717-23.717 0-6.552-2.657-12.483-6.952-16.775l-26.237-26.237h9.482c1.648-0.006 3.253-0.18 4.801-0.506l-0.154 0.027c0.709-0.146 1.355-0.396 2.084-0.604 0.964-0.246 1.756-0.503 2.527-0.802l-0.151 0.051c0.929-0.424 1.688-0.827 2.421-1.269l-0.107 0.060c0.583-0.333 1.209-0.583 1.771-0.959 2.614-1.773 4.799-3.964 6.51-6.5l0.054-0.085c0.284-0.454 0.59-1.007 0.867-1.576l0.049-0.112c0.409-0.657 0.833-1.443 1.212-2.254l0.060-0.143c0.227-0.59 0.47-1.362 0.673-2.149l0.036-0.164c0.2-0.53 0.414-1.225 0.593-1.935l0.032-0.149c0.293-1.394 0.461-2.996 0.461-4.637s-0.168-3.243-0.487-4.789l0.026 0.152c-0.146-0.729-0.417-1.396-0.625-2.084-0.249-0.956-0.499-1.726-0.785-2.479l0.055 0.166c-0.44-0.955-0.864-1.741-1.33-2.498l0.058 0.102c-0.313-0.563-0.563-1.146-0.917-1.709-0.919-1.365-1.9-2.557-2.982-3.649l-69.434-69.414c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM188.45 47.205c-4.29 4.295-6.944 10.225-6.944 16.776s2.653 12.481 6.944 16.776v0l74.459 74.459h-53.182c-1.617 0.016-3.185 0.182-4.703 0.485l0.16-0.027c-0.833 0.203-1.528 0.425-2.203 0.687l0.119-0.041c-0.916 0.234-1.666 0.476-2.397 0.758l0.146-0.049c-0.984 0.44-1.798 0.872-2.58 1.349l0.1-0.057c-0.542 0.292-1.084 0.542-1.605 0.875-2.631 1.776-4.83 3.975-6.552 6.521l-0.054 0.085c-0.313 0.458-0.5 0.938-0.771 1.417-0.469 0.742-0.942 1.617-1.358 2.523l-0.059 0.144c-0.271 0.646-0.396 1.334-0.625 2.084-0.238 0.629-0.488 1.443-0.695 2.274l-0.034 0.164c-0.209 1.085-0.344 2.35-0.375 3.64l-0.001 0.028c0 0.313 0 0.604 0 0.938v1c0.005 2.153 0.418 4.208 1.165 6.094l-0.039-0.113c0.208 0.667 0.354 1.334 0.625 2.084 0.475 1.060 0.949 1.942 1.474 2.787l-0.057-0.099c0.271 0.458 0.458 0.959 0.771 1.396 0.92 1.384 1.915 2.591 3.020 3.687l69.834 69.834h-91.693c-94.629 0-171.34 76.712-171.34 171.34s76.712 171.34 171.34 171.34v0c2.751 0 5.46-0.208 8.19-0.354 40.296 71.189 115.411 118.514 201.603 118.784h0.038c0.1 0 0.219 0 0.338 0 36.26 0 70.555-8.414 101.039-23.397l-1.348 0.599c15.829 17.097 35.049 30.806 56.62 40.096l1.084 0.415c18.904 8.087 40.9 12.789 63.995 12.789 91.959 0 166.506-74.547 166.506-166.506 0-62.7-34.656-117.305-85.858-145.706l-0.852-0.434c11.521-17.744 18.371-39.446 18.371-62.747 0-64.256-52.090-116.346-116.346-116.346-0.063 0-0.125 0-0.188 0h-72.698l-19.297-19.297h9.503c1.624-0.003 3.208-0.17 4.737-0.485l-0.152 0.026c0.85-0.206 1.546-0.421 2.225-0.67l-0.141 0.045c0.931-0.233 1.696-0.475 2.441-0.758l-0.148 0.049c0.963-0.439 1.756-0.863 2.52-1.33l-0.103 0.058c0.563-0.313 1.125-0.542 1.667-0.896 2.631-1.769 4.83-3.961 6.552-6.501l0.054-0.084c0.375-0.563 0.625-1.188 0.959-1.771 0.375-0.614 0.778-1.366 1.143-2.139l0.065-0.154c0.249-0.632 0.506-1.438 0.717-2.263l0.033-0.154c0.209-0.555 0.417-1.252 0.581-1.965l0.023-0.119c0.288-1.397 0.452-3.003 0.452-4.647s-0.165-3.25-0.479-4.802l0.026 0.154c-0.146-0.709-0.417-1.355-0.604-2.084-0.245-0.971-0.502-1.77-0.801-2.548l0.051 0.151c-0.418-0.914-0.814-1.659-1.248-2.379l0.060 0.107c-0.333-0.604-0.604-1.23-1-1.834-0.901-1.333-1.861-2.498-2.919-3.565l-69.476-69.455c-4.292-4.295-10.224-6.952-16.776-6.952-13.098 0-23.717 10.618-23.717 23.717 0 6.547 2.653 12.474 6.941 16.765v0l29.050 28.987h-9.419c-1.642 0.007-3.238 0.181-4.78 0.506l0.153-0.027c-0.729 0.125-1.375 0.417-2.084 0.604-0.928 0.226-1.707 0.476-2.461 0.774l0.127-0.044c-0.957 0.442-1.736 0.859-2.489 1.314l0.113-0.064c-0.563 0.313-1.167 0.563-1.709 0.938-2.627 1.766-4.819 3.959-6.532 6.501l-0.054 0.085c-0.375 0.583-0.646 1.209-0.979 1.792-0.379 0.618-0.775 1.363-1.13 2.131l-0.058 0.14c-0.25 0.634-0.514 1.454-0.734 2.291l-0.037 0.168c-0.202 0.553-0.403 1.251-0.561 1.964l-0.022 0.12c-0.29 1.397-0.457 3.003-0.457 4.647s0.166 3.25 0.483 4.801l-0.026-0.154c0.146 0.688 0.396 1.355 0.604 2.084 0.248 0.985 0.505 1.791 0.803 2.577l-0.053-0.16c0.412 0.899 0.808 1.637 1.243 2.349l-0.056-0.098c0.333 0.604 0.604 1.25 1 1.834 0.906 1.332 1.865 2.496 2.92 3.566l26.234 26.254h-114.804l-62.935-62.914h53.119c1.647-0.003 3.252-0.178 4.799-0.506l-0.152 0.027c0.688-0.125 1.292-0.375 1.959-0.563 1.008-0.254 1.835-0.518 2.64-0.825l-0.161 0.054q1.063-0.521 2.084-1.125c0.667-0.375 1.334-0.646 2.084-1.084 1.19-0.816 2.231-1.673 3.194-2.61l-0.005 0.005c0.125-0.125 0.292-0.208 0.417-0.333s0.292-0.396 0.479-0.583c0.869-0.904 1.677-1.884 2.407-2.922l0.052-0.079q0.625-1.021 1.146-2.084c0.353-0.582 0.702-1.267 1.002-1.978l0.040-0.106c0.3-0.743 0.593-1.661 0.824-2.603l0.031-0.148c0.167-0.563 0.375-1.104 0.5-1.688 0.293-1.403 0.461-3.016 0.461-4.668s-0.168-3.265-0.487-4.822l0.026 0.154c-0.125-0.583-0.333-1.104-0.5-1.646-0.273-1.115-0.566-2.039-0.911-2.938l0.056 0.167c-0.292-0.688-0.688-1.313-1.042-2.084-0.383-0.817-0.765-1.5-1.189-2.154l0.043 0.070c-0.896-1.325-1.849-2.483-2.899-3.545l-114.968-114.968c-4.291-4.289-10.218-6.942-16.765-6.942s-12.474 2.653-16.765 6.942v0zM47.243 436.899c0.071-68.409 55.503-123.848 123.902-123.931h413.501c0.006 0 0.014 0 0.022 0 38.165 0 69.103 30.938 69.103 69.103s-30.938 69.103-69.103 69.103c-17.044 0-32.647-6.171-44.696-16.399l0.099 0.082c-4.105-3.493-9.468-5.618-15.327-5.618-13.099 0-23.717 10.618-23.717 23.717 0 7.239 3.243 13.721 8.356 18.071l0.034 0.028c15.143 12.835 33.852 21.925 54.434 25.611l0.706 0.105c-0.334 100.947-82.245 182.652-183.239 182.652-63.197 0-118.921-31.992-151.861-80.665l-0.412-0.646c18.814-6.821 35.084-16.076 49.455-27.602l-0.336 0.261c5.451-4.383 8.91-11.050 8.91-18.526 0-13.098-10.618-23.716-23.716-23.716-5.622 0-10.788 1.956-14.852 5.226l0.046-0.036c-20.841 16.875-47.677 27.092-76.898 27.092-0.161 0-0.321 0-0.482-0.001h0.025c-68.408-0.047-123.857-55.467-123.952-123.86v-0.009zM557.305 700.787c-13.357-5.821-24.81-13.461-34.672-22.769l0.058 0.054c54.528-42.545 89.263-108.262 89.275-182.091v-0.752c13.386-3.273 25.166-8.444 35.754-15.299l-0.452 0.274c44.464 17.938 75.271 60.734 75.271 110.725 0 63.506-49.714 115.4-112.347 118.889l-0.309 0.014q-3.251 0.167-6.481 0.167c-0.093 0-0.203 0-0.312 0-16.528 0-32.266-3.387-46.556-9.505l0.772 0.294zM802.354 333.682l-56.641 63.060c-3.812 4.193-6.147 9.788-6.147 15.93 0 13.097 10.618 23.715 23.715 23.715 7.043 0 13.369-3.070 17.712-7.945l0.021-0.024 56.537-62.935c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM925.993 549.702l-84.67 4.605c-12.536 0.699-22.437 11.035-22.437 23.684 0 13.099 10.619 23.718 23.718 23.718 0.451 0 0.899-0.013 1.343-0.037l-0.062 0.003 84.67-4.605c12.481-0.76 22.316-11.069 22.316-23.674 0-13.052-10.545-23.642-23.58-23.715h-0.007zM764.072 735.172c-3.768 4.178-6.072 9.74-6.072 15.839 0 7.001 3.036 13.292 7.864 17.63l0.022 0.019 63.122 56.558c4.193 3.812 9.788 6.147 15.93 6.147 13.097 0 23.715-10.618 23.715-23.715 0-7.043-3.070-13.369-7.945-17.712l-0.024-0.021-63.122-56.641c-4.179-3.757-9.735-6.054-15.827-6.054-7.005 0-13.301 3.037-17.642 7.867l-0.019 0.022zM425.809 753.51l-56.641 63.143c-3.762 4.18-6.064 9.74-6.064 15.838 0 7 3.032 13.291 7.855 17.632l0.021 0.019c4.179 3.757 9.735 6.054 15.827 6.054 7.005 0 13.301-3.037 17.642-7.867l0.019-0.022 56.641-63.122c3.812-4.193 6.147-9.788 6.147-15.93 0-13.097-10.618-23.715-23.715-23.715-7.043 0-13.369 3.070-17.712 7.945l-0.021 0.024zM615.113 806.525c-12.537 0.696-22.439 11.032-22.439 23.681 0 0.467 0.013 0.93 0.040 1.39l-0.003-0.064 4.585 84.691c0.699 12.536 11.035 22.437 23.684 22.437 13.099 0 23.718-10.619 23.718-23.718 0-0.451-0.013-0.899-0.037-1.343l0.003 0.062-4.585-84.712c-0.705-12.527-11.033-22.42-23.673-22.423v0z");
                map.Add(30, "Gewitter, (starker Hagel)"); mapIcon.Add(30, "M539.825-76.549v0c-5.419 5.413-8.772 12.894-8.772 21.158s3.352 15.745 8.772 21.158l36.636 36.583h-11.966c-2.074 0.013-4.089 0.233-6.035 0.639l0.197-0.034c-0.92 0.184-1.736 0.526-2.63 0.789-1.201 0.305-2.183 0.62-3.14 0.986l0.194-0.065c-1.203 0.553-2.186 1.080-3.136 1.656l0.138-0.078c-0.71 0.394-1.473 0.71-2.157 1.157-3.31 2.244-6.076 5.018-8.242 8.229l-0.068 0.108c-0.473 0.684-0.763 1.446-1.157 2.13-0.498 0.811-1.024 1.794-1.498 2.807l-0.080 0.191c-0.298 0.759-0.613 1.741-0.876 2.744l-0.045 0.201c-0.263 0.894-0.605 1.736-0.789 2.63-0.366 1.763-0.576 3.79-0.576 5.865s0.21 4.102 0.609 6.059l-0.033-0.194c0.263 1.078 0.534 1.956 0.847 2.813l-0.059-0.183c0.509 2.262 1.373 4.259 2.541 6.039l-0.042-0.069c0.399 0.846 0.785 1.544 1.208 2.216l-0.051-0.086c1.164 1.723 2.41 3.228 3.787 4.602v0l84.159 84.159c5.457 5.691 13.123 9.228 21.615 9.228 16.529 0 29.929-13.4 29.929-29.929 0-8.492-3.537-16.158-9.217-21.605l-0.011-0.010-33.137-33.137h11.966c2.077-0.003 4.103-0.213 6.060-0.612l-0.196 0.033c1.065-0.255 1.943-0.526 2.798-0.844l-0.168 0.055c1.201-0.295 2.184-0.602 3.143-0.959l-0.197 0.064c1.207-0.557 2.199-1.092 3.155-1.679l-0.131 0.075c0.86-0.413 1.558-0.798 2.233-1.216l-0.103 0.059c3.325-2.228 6.1-4.995 8.269-8.205l0.067-0.106c0.351-0.571 0.728-1.269 1.070-1.987l0.061-0.143c0.516-0.829 1.051-1.821 1.529-2.845l0.075-0.18c0.294-0.758 0.6-1.732 0.853-2.727l0.041-0.192c0.263-0.868 0.605-1.709 0.815-2.63 0.359-1.763 0.564-3.79 0.564-5.865s-0.205-4.102-0.597-6.061l0.033 0.196c-0.262-1.059-0.541-1.937-0.869-2.79l0.054 0.16c-0.294-1.187-0.6-2.161-0.957-3.111l0.063 0.191c-0.559-1.21-1.094-2.201-1.68-3.158l0.076 0.133c-0.413-0.859-0.798-1.557-1.216-2.233l0.059 0.102c-1.163-1.723-2.409-3.228-3.787-4.602l-87.657-87.684c-5.413-5.419-12.894-8.772-21.158-8.772s-15.745 3.352-21.158 8.772v0zM420.609 81.985c-5.419 5.413-8.772 12.894-8.772 21.158s3.352 15.745 8.772 21.158v0l93.968 93.995h-66.985c-2.077 0.004-4.103 0.214-6.061 0.612l0.196-0.033c-0.92 0.184-1.762 0.526-2.63 0.789-1.205 0.307-2.187 0.622-3.144 0.987l0.199-0.066c-1.203 0.553-2.186 1.080-3.136 1.656l0.138-0.078c-0.71 0.394-1.473 0.71-2.157 1.183-3.312 2.233-6.078 4.999-8.243 8.204l-0.068 0.107c-0.447 0.684-0.789 1.446-1.183 2.157-0.505 0.818-1.032 1.802-1.502 2.816l-0.076 0.182c-0.303 0.768-0.619 1.75-0.878 2.754l-0.042 0.191c-0.256 0.687-0.518 1.567-0.73 2.465l-0.033 0.165c-0.37 1.763-0.582 3.789-0.582 5.865s0.212 4.102 0.616 6.058l-0.033-0.193c0.25 1.073 0.512 1.952 0.819 2.81l-0.056-0.18c0.297 1.188 0.612 2.17 0.982 3.125l-0.061-0.18c0.536 1.199 1.063 2.191 1.646 3.145l-0.068-0.12c0.394 0.684 0.71 1.446 1.157 2.13 1.157 1.725 2.404 3.231 3.785 4.6l88.184 88.184h-298.737c-119.234 0-215.893 96.659-215.893 215.893s96.659 215.893 215.893 215.893v0c3.34 0 6.68-0.263 10.020-0.447 50.716 89.945 145.63 149.689 254.497 149.689 136.738 0 251.463-94.25 282.755-221.33l0.416-1.998c36.352 33.048 84.868 53.282 138.11 53.282 111.137 0 201.682-88.165 205.51-198.369l0.010-0.35c44.327-14.396 75.807-55.332 75.807-103.62 0-60.046-48.677-108.722-108.722-108.722-0.004 0-0.009 0-0.013 0h-245.479l-24.406-24.406h11.966c0.007 0 0.015 0 0.024 0 2.067 0 4.085-0.211 6.033-0.612l-0.192 0.033c1.065-0.256 1.943-0.526 2.798-0.844l-0.168 0.055c1.202-0.296 2.185-0.602 3.143-0.959l-0.198 0.064c1.195-0.543 2.187-1.078 3.139-1.671l-0.115 0.067c0.86-0.413 1.557-0.798 2.233-1.216l-0.103 0.059c3.323-2.23 6.097-4.997 8.269-8.205l0.068-0.106c0.364-0.584 0.741-1.282 1.078-2.004l0.053-0.127c0.516-0.829 1.051-1.821 1.529-2.845l0.075-0.18c0.292-0.755 0.598-1.729 0.852-2.724l0.042-0.195c0.263-0.894 0.605-1.736 0.815-2.63 0.359-1.763 0.565-3.79 0.565-5.865s-0.206-4.101-0.597-6.061l0.033 0.196c-0.255-1.050-0.535-1.927-0.866-2.779l0.051 0.149c-0.294-1.188-0.601-2.162-0.957-3.111l0.063 0.192c-0.563-1.214-1.098-2.205-1.683-3.163l0.079 0.139c-0.413-0.86-0.798-1.557-1.216-2.233l0.059 0.103c-1.163-1.723-2.409-3.228-3.787-4.602l-87.71-87.736c-5.457-5.691-13.123-9.228-21.615-9.228-16.529 0-29.929 13.4-29.929 29.929 0 8.492 3.537 16.158 9.217 21.605l0.011 0.010 36.635 36.583h-11.966c-2.072 0.009-4.087 0.229-6.032 0.639l0.193-0.034c-1.080 0.264-1.958 0.535-2.815 0.848l0.185-0.059c-1.201 0.305-2.183 0.62-3.14 0.986l0.194-0.065c-1.204 0.554-2.187 1.080-3.137 1.656l0.138-0.078c-0.71 0.394-1.446 0.71-2.13 1.157-3.311 2.234-6.077 5-8.243 8.204l-0.068 0.107c-0.358 0.577-0.743 1.284-1.094 2.011l-0.063 0.146c-0.499 0.812-1.025 1.795-1.498 2.808l-0.080 0.19c-0.298 0.759-0.613 1.741-0.876 2.744l-0.045 0.201c-0.263 0.894-0.605 1.709-0.789 2.63-0.363 1.763-0.571 3.79-0.571 5.865s0.208 4.102 0.604 6.060l-0.033-0.195c0.184 0.92 0.526 1.762 0.789 2.63 0.514 2.253 1.377 4.24 2.541 6.013l-0.043-0.069c0.394 0.71 0.684 1.446 1.157 2.157 1.164 1.723 2.41 3.228 3.787 4.602v0l33.137 33.137h-144.884l-79.477-79.451h67.038c2.073-0.012 4.089-0.222 6.040-0.612l-0.201 0.034c1.050-0.255 1.927-0.535 2.778-0.866l-0.149 0.051c1.201-0.295 2.184-0.602 3.143-0.959l-0.197 0.064c1.207-0.557 2.199-1.092 3.155-1.679l-0.131 0.075c0.71-0.394 1.446-0.684 2.13-1.131 3.324-2.238 6.099-5.013 8.269-8.23l0.068-0.107c0.351-0.571 0.728-1.269 1.070-1.987l0.061-0.143c0.523-0.835 1.058-1.827 1.533-2.853l0.071-0.172c0.292-0.755 0.598-1.729 0.852-2.724l0.042-0.196c0.264-0.69 0.535-1.568 0.755-2.466l0.034-0.164c0.367-1.763 0.577-3.789 0.577-5.865s-0.21-4.102-0.61-6.059l0.033 0.194c-0.253-1.060-0.524-1.939-0.842-2.793l0.053 0.164c-0.296-1.191-0.602-2.165-0.958-3.114l0.064 0.195c-0.551-1.202-1.087-2.194-1.676-3.149l0.072 0.125c-0.413-0.86-0.798-1.557-1.216-2.233l0.059 0.103c-1.163-1.723-2.409-3.228-3.787-4.602l-145.096-145.069c-5.413-5.419-12.894-8.772-21.158-8.772s-15.745 3.352-21.158 8.772v0zM780.519 644.559c60.182-19.133 103.021-74.519 103.021-139.91 0-32.855-10.815-63.185-29.078-87.623l0.275 0.384h219.496c26.967 0.026 48.818 21.894 48.818 48.865 0 26.987-21.877 48.865-48.865 48.865-12.042 0-23.066-4.356-31.584-11.578l0.071 0.059c-5.174-4.388-11.928-7.055-19.304-7.055-16.533 0-29.936 13.403-29.936 29.936 0 9.157 4.111 17.354 10.589 22.845l0.043 0.036c12.026 10.219 26.526 17.919 42.457 22.082l0.754 0.167c-4.734 76.802-68.18 137.298-145.755 137.298-50.146 0-94.387-25.278-120.677-63.787l-0.324-0.503zM59.91 573.55c0.105-86.134 69.902-155.931 156.025-156.035h520.742c0.003 0 0.006 0 0.010 0 48.034 0 86.973 38.939 86.973 86.973s-38.939 86.973-86.973 86.973c-21.453 0-41.092-7.767-56.258-20.643l0.125 0.103c-5.18-4.409-11.949-7.090-19.343-7.090-16.531 0-29.931 13.401-29.931 29.931 0 9.136 4.093 17.316 10.546 22.806l0.042 0.035c19.151 16.261 42.83 27.766 68.885 32.402l0.888 0.131c-0.997 126.943-104.136 229.466-231.219 229.466-79.498 0-149.626-40.12-191.242-101.216l-0.515-0.802c23.764-8.602 44.315-20.283 62.466-34.835l-0.425 0.329c6.876-5.533 11.239-13.947 11.239-23.38 0-16.534-13.404-29.938-29.938-29.938-7.101 0-13.624 2.472-18.757 6.603l0.058-0.045c-26.25 21.295-60.068 34.19-96.898 34.19-0.144 0-0.289 0-0.433-0.001h0.022c-86.194-0.030-156.066-69.874-156.141-156.055v-0.007z");
                map.Add(31, "(Wind)"); mapIcon.Add(31, "M359.112 117.499c0 23.691 19.205 42.896 42.896 42.896s42.896-19.205 42.896-42.896v0c0.086-64.647 52.469-117.030 117.108-117.116h0.008c77.533 0.086 140.363 62.915 140.448 140.44v0.008c-0.107 92.29-74.915 167.072-167.205 167.136h-492.368c-23.691 0-42.896 19.205-42.896 42.896s19.205 42.896 42.896 42.896v0h492.361c139.619-0.172 252.756-113.309 252.928-252.911v-0.017c-0.193-124.83-101.335-225.972-226.147-226.165h-0.018c-111.985 0.129-202.736 90.854-202.908 202.816v0.016zM602.391 459.461c-23.691 0-42.896 19.205-42.896 42.896s19.205 42.896 42.896 42.896v0h262.653c92.311 0.107 167.109 74.932 167.174 167.243v0.006c-0.086 77.533-62.915 140.363-140.44 140.448h-0.008c-64.647-0.086-117.030-52.469-117.116-117.108v-0.008c0-23.691-19.205-42.896-42.896-42.896s-42.896 19.205-42.896 42.896v0c0.171 111.979 90.923 202.704 202.896 202.832h0.012c124.83-0.193 225.972-101.335 226.165-226.147v-0.018c-0.172-139.64-113.326-252.794-252.949-252.966h-0.017zM42.896 459.461c-23.691 0-42.896 19.205-42.896 42.896s19.205 42.896 42.896 42.896v0h262.728c92.303 0.107 167.093 74.937 167.136 167.245v0.004c-0.15 77.505-62.972 140.288-140.478 140.373h-0.008c-64.647-0.086-117.030-52.469-117.116-117.108v-0.008c0-23.691-19.205-42.896-42.896-42.896s-42.896 19.205-42.896 42.896v0c0.129 112.011 90.897 202.779 202.896 202.908h0.012c124.83-0.193 225.972-101.335 226.165-226.147v-0.018c-0.15-139.702-113.403-252.901-253.11-252.966h-0.006z");
            }
            
            ListView listView = new ListView();

            TextBlock seperator = new TextBlock();
            TextBlock seperator2 = new TextBlock();
            seperator.Text = "---------------------------------------------------------------------------";
            seperator2.Text = seperator.Text;

            Windows.UI.Xaml.Media.AcrylicBrush acrylicBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
            acrylicBrush.TintColor = Colors.LightSkyBlue;
            acrylicBrush.TintOpacity = 0.5;
            acrylicBrush.Opacity = 0.5;

            //VORHERSAGE

            if (Option == 0)
            {
                Canvas weatherIcon = new Canvas();
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = -1;
                Windows.UI.Xaml.Shapes.Path path = new Windows.UI.Xaml.Shapes.Path();
                path.Height = 50;
                path.Width = 50;
                path.Stretch = Stretch.Uniform;

                var a = new Binding
                {
                    Source = Application.Current.Resources["AppBarItemForegroundThemeBrush"]
                };
                BindingOperations.SetBinding(path, Windows.UI.Xaml.Shapes.Path.FillProperty, a);

                path.StrokeStartLineCap = PenLineCap.Flat;
                path.StrokeEndLineCap = PenLineCap.Flat;
                path.StrokeLineJoin = PenLineJoin.Miter;
                path.RenderTransform = scaleTransform;
                path.RenderTransformOrigin = new Point(0.5, 0.5);


                Uri uri = new Uri("https://app-prod-ws.warnwetter.de/v30/stationOverviewExtended?stationIds=" + arg1);
                HttpClient client = new HttpClient();
                Task<HttpResponseMessage> httpResponse = client.GetAsync(uri);
                HttpResponseMessage message = httpResponse.Result;
                TextBlock textBox = new TextBlock();

                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                Encoding utf8 = Encoding.UTF8;
                byte[] utfBytes = message.Content.ReadAsByteArrayAsync().Result;
                byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);

                JObject json = JObject.Parse(iso.GetString(isoBytes));
                JToken jToken = new JObject();
                json.TryGetValue(arg1, out jToken);
                if (jToken != null)
                {
                    jToken = jToken.SelectToken("days").First;
                    TextBlock forecast = new TextBlock();
                    TextBlock stationName = new TextBlock();
                    stationName.Text = "Wetter in " + arg1;
                    stationName.FontWeight = FontWeights.Bold;

                    forecast.Text += "Vorhersage für " + jToken.SelectToken("dayDate").ToString() + "\n";

                    String weatherSummary;
                    map.TryGetValue((int)jToken.SelectToken("icon"), out weatherSummary);

                    forecast.Text += weatherSummary + "\n";

                    forecast.Text += "Temperatur: " + ((int)jToken.SelectToken("temperatureMin")) / 10 + " - " + ((int)jToken.SelectToken("temperatureMax")) / 10 + " °C" + "\n";

                    forecast.Text += "Regenwahrscheinlichkeit: " + jToken.SelectToken("precipitation").ToString() + " %" + "\n";

                    forecast.Text += "Wind Geschwindigkeit : " + ((int)jToken.SelectToken("windSpeed")) / 10 + " km/h";

                    String weatherSummaryIcon;
                    mapIcon.TryGetValue((int)jToken.SelectToken("icon"), out weatherSummaryIcon);
                    var b = new Binding
                    {
                        Source = weatherSummaryIcon
                    };
                    BindingOperations.SetBinding(path, Windows.UI.Xaml.Shapes.Path.DataProperty, b);
                    weatherIcon.Children.Add(path);
                    weatherIcon.Height = 80;
                    weatherIcon.Width = 50;
                    listView.Items.Add(stationName);
                    listView.Items.Add(seperator);
                    listView.Items.Add(weatherIcon);
                    listView.Items.Add(forecast);

                }
                listView.Height = 300;
                listView.Name = "DWD," + Option + "," + arg1 + "," + arg2;

                listView.Background = acrylicBrush;
                return listView;
            }

            if (Option == 1)
            {

                //WETTERWARNUNG

                ListView listViewWarn = new ListView();


                Uri uri = new Uri("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16/gemeinde_warnings_v2.json");
                JObject jsonWarn = new JObject();
                JToken jToken = new JObject();

                TextBlock textWarn = new TextBlock();

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.14; rv:89.0) Gecko/20100101 Firefox/89.0");

                    wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                    var responseStream = new GZipStream(wc.OpenRead(uri), CompressionMode.Decompress);
                    var reader = new StreamReader(responseStream);
                    var textResponse = reader.ReadToEnd();
                    JObject json = JObject.Parse(textResponse);
                    json.TryGetValue("warnings", out jToken);
                    foreach (JToken item in jToken)
                    {
                        if (item.SelectToken("regions").ToString().Contains(arg1) &&
                            item.SelectToken("regions").ToString().Contains(arg2))
                        {
                            if (textWarn.Text != "") textWarn.Text += "\n";
                            textWarn.Text += item.SelectToken("event").ToString();
                            textWarn.Text += "\n" + item.SelectToken("description").ToString();
                        }
                    }

                    if (textWarn.Text == "")
                    {
                        textWarn.Text = "Keine Warnungen oder Vorabinformationen für diesen Standort verfügbar";
                        textWarn.FontStyle = FontStyle.Italic;
                    }

                }

                TextBlock warnPos = new TextBlock();
                warnPos.FontWeight = FontWeights.Bold;
                warnPos.Text = "Wetter Warnungen für " + arg1 + " " + arg2;

                listViewWarn.Items.Add(warnPos);
                listViewWarn.Items.Add(seperator2);
                listViewWarn.Items.Add(textWarn);
                textWarn.TextWrapping = TextWrapping.Wrap;
                listViewWarn.Height = 300;
                listViewWarn.Name = "DWD," + Option + "," + arg1 + "," + arg2;
                listViewWarn.Background = acrylicBrush;
                return listViewWarn;
            }

            return null;


        }

        public ListView CalendarTile()
        {
            ListView listView = new ListView();
            listView.Name = "Calendar";
            listView.Height = 300;
            CalendarView calendarView = new CalendarView();
            calendarView.Background = null;
            calendarView.Height = 200;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Kalender";
            textBlock.FontWeight = FontWeights.Bold;

            TextBlock seperator = new TextBlock();
            seperator.Text = "---------------------------------------------------------------------------";

            listView.Items.Add(textBlock);
            listView.Items.Add(seperator);
            listView.Items.Add(calendarView);

            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = 10;

            ((CalendarView) listView.Items[2]).RenderTransform = translateTransform;
            return listView;
        }

        public ListView Clock()
        {
            ListView listView = new ListView();
            listView.Name = "Clock Date";
            listView.Height = 300;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Uhrzeit und Datum";
            textBlock.FontWeight = FontWeights.Bold;

            TextBlock seperator = new TextBlock();
            seperator.Text = "---------------------------------------------------------------------------";

            
            listView.Items.Add(textBlock);
            listView.Items.Add(seperator);
            listView.Items.Add(clockTime = new TextBlock()
            {
                FontWeight = FontWeights.SemiLight,
                FontSize = 37,
                CharacterSpacing = 30,
                TextAlignment = TextAlignment.Center,


            });

            return listView;

        }

        //---------------------------------------------------------------------------------------
        //Events
        private void loadText_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void lvMain_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            lvWidgets.Clear();
            foreach (ListView item in lvMain.Items)
            {
                lvWidgets.Add(item);
            }
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            ElementsNotVisible();
            loadSpinner.IsActive = true;
            testInternet();
            loadText.Text = "Aktualisiere Inhalte...";
            splashText.Text = "";
            refreshTime = 0;
        }

        private void splashText_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            pageScroll(sender);
        }

        private void lvItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((ListView)sender).BorderThickness = new Thickness(0);
        }

        private void lvItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((ListView)sender).BorderBrush = cmdRefresh.Background;
            ((ListView)sender).BorderThickness = new Thickness(2);
        }

        private void tbTimeDate_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            tbTimeDate.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
        }

        private void tbTimeDate_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            tbTimeDate.TextDecorations = Windows.UI.Text.TextDecorations.None;
        }

        private void tbTimeDate_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void tbTimeDate_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
