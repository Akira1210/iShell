using System;
using System.Collections.Generic;
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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace iShell
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    /// 
    //Todo Wetter Warnung, 
    public sealed partial class MainPage : Page
    {
        List<object> items = new List<object>();
        int refreshTime = 0;
        int activeItem = 0;
        Windows.UI.Xaml.Media.Brush internetFail = null;
        Windows.UI.Xaml.Media.Brush internetSuccess = null;
        Boolean internet = true;
        public MainPage()
        {

            this.InitializeComponent();
            tbTimeDate.Visibility = Visibility.Collapsed;
            cmdRefresh.Visibility = Visibility.Collapsed;
            recTopBar.Visibility = Visibility.Collapsed;
            lvMain.Visibility = Visibility.Collapsed;
            pgbRefresh.Visibility = Visibility.Collapsed;
            splashText.Text = "";
            loadSpinner.IsActive = true;
            loadText.Text = "Willkommen \n" + WindowsIdentity.GetCurrent().Name.Substring(WindowsIdentity.GetCurrent().Name.IndexOf("\\") + 1) + "";
            tbVer.Text = SetVersionLabel();
            DispatcherTimer tClock = new DispatcherTimer();
            tClock.Tick += tClock_Tick;
            tClock.Interval = TimeSpan.FromSeconds(1);
            tClock.Start();
            AcrylicBrush internetFailBrush = new AcrylicBrush();
            internetFailBrush.TintColor = Color.FromArgb(200, 255, 165, 0);
            internetFail = internetFailBrush;
            internetSuccess = recTopBar.Fill;
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
                    if (splashText.Text.Equals("⚠️ Kein Netzwerkzugriff"))
                        splashText.Text = "Übersicht";
                }
            }
            catch (System.AggregateException e)
            {
                internet = false;
                recTopBar.Fill = internetFail;
                System.Console.WriteLine(e.Message);
                if (splashText.Text.Equals("Übersicht"))
                    splashText.Text = "⚠️ Kein Netzwerkzugriff";
                    
            }
        }

        private void loadItems()
        {
            tbTimeDate.Visibility = Visibility.Visible;
            cmdRefresh.Visibility = Visibility.Visible;
            recTopBar.Visibility = Visibility.Visible;
            lvMain.Visibility = Visibility.Visible;
            pgbRefresh.Visibility = Visibility.Visible;

            if (internet)
            {
                lvMain.Items.Clear();
                DBInfo(0);
                DBInfo(1);
                DBInfo(2);
                DBInfo(3);
                DWDWeather();
            }
            InitAnim(0);
            items.Clear();
            for (int i = 0; i < lvMain.Items.Count; i++)
            {
                items.Add(lvMain.Items[i]);
            }
        }
        private String SetVersionLabel()
        {
            var ProcessArch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            var BuildDate = GetLinkerTimestampUtc(Assembly.GetExecutingAssembly()).ToString();
            BuildDate = BuildDate.Replace(":", "");
            BuildDate = BuildDate.Replace(".", "");
            BuildDate = BuildDate.Replace(" ", "_");

            return Assembly.GetExecutingAssembly().GetName().Name + "\nInternal" + " \nBuild 5." + ProcessArch + ".iot." + BuildDate + "\n" + System.Environment.OSVersion;
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
        private void tClock_Tick(object sender, object e)
        {
            tbTimeDate.Text = System.DateTime.Now.ToString("HH:mm") + " Uhr" + System.Environment.NewLine +
                System.DateTime.Now.Date.ToString("d");
            refreshTime++;
            pgbRefresh.Value = refreshTime/3;
            if (refreshTime > 300)
            {
                refreshTime = 0;
                splashText.Text = "";
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

        public async void checkFileAccess() 
        {
            try
            {
                StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Windows.ApplicationModel.Package.Current.InstalledLocation.Path);
            }
            catch (Exception)
            {
                // prompt user for what action they should do then launch below
                // suggestion could be a message prompt
                await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));
            }

        }

        public void DBInfo(int route)
        {
            ListView grid = new ListView();
            TextBlock textBlock = new TextBlock();
            String StartStation = "";
            String EndStation = "";
            String URLString = "";


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
            textBlock.Text = "DB: Von "+ StartStation + " nach " + EndStation + ":";
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

            lvMain.Items.Add(grid);
            
        }

        public async void DWDWeather()
        {

            //VORHERSAGE

            ListView listView = new ListView();

            //Station IDs:https://www.dwd.de/DE/leistungen/klimadatendeutschland/statliste/statlex_html.html?view=nasPublication&nn=16102
            String station = "Q055";
            Uri uri = new Uri("https://app-prod-ws.warnwetter.de/v30/stationOverviewExtended?stationIds="+station);
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> httpResponse = client.GetAsync(uri);
            HttpResponseMessage message = httpResponse.Result;;
            TextBlock textBox = new TextBlock();
            textBox.IsTextSelectionEnabled=true;
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = message.Content.ReadAsByteArrayAsync().Result;
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            JObject json = JObject.Parse(iso.GetString(isoBytes));
            JToken jToken = new JObject();
            json.TryGetValue(station, out jToken);
            if (jToken != null)
            {
                jToken = jToken.SelectToken("days").First;
                TextBlock stationName = new TextBlock();
                stationName.Text = "Wetter in Buchen";
                stationName.FontWeight = FontWeights.Bold;
                TextBlock date = new TextBlock();
                date.Text = "Vorhersage für " + jToken.SelectToken("dayDate").ToString();
                TextBlock temp = new TextBlock();
                temp.Text = "Temperatur: " + ((int)jToken.SelectToken("temperatureMin")) / 10 + " - " + ((int)jToken.SelectToken("temperatureMax")) / 10 + " °C";
                TextBlock rain = new TextBlock();
                rain.Text = "Regenwahrscheinlichkeit: " + jToken.SelectToken("precipitation").ToString() + " %";
                TextBlock windSpeed = new TextBlock();
                windSpeed.Text = "Wind Geschwindigkeit : " + ((int)jToken.SelectToken("windSpeed")) / 10 + " km/h";

                listView.Items.Add(stationName);
                listView.Items.Add(date);
                listView.Items.Add(temp);
                listView.Items.Add(rain);
                listView.Items.Add(windSpeed);

            }
            listView.Height = 300;
            lvMain.Items.Add(listView);

            //WETTERWARNUNG

            ListView listViewWarn = new ListView();


            uri = new Uri("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16/gemeinde_warnings_v2.json");
            JObject jsonWarn = new JObject();
            TextBox textBox1 = new TextBox();
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.ASCII;
                var jsonStr = wc.DownloadData(uri);
                //jsonWarn = JObject.Parse(System.Text.Encoding.Default.GetString(jsonStr));
                textBox1.Text = System.Text.Encoding.Default.GetString(jsonStr);
            }

            //jsonWarn = JObject.Parse(jsonStr);
            
            
            
            listViewWarn.Items.Add(textBox1);
            lvMain.Items.Add(listViewWarn);


        }




        private void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            pageScroll(sender);
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


        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            tbTimeDate.Visibility = Visibility.Collapsed;
            cmdRefresh.Visibility = Visibility.Collapsed;
            recTopBar.Visibility = Visibility.Collapsed;
            lvMain.Visibility = Visibility.Collapsed;
            pgbRefresh.Visibility = Visibility.Collapsed;
            loadSpinner.IsActive = true;
            loadText.Text = "Aktualisiere Inhalte...";
            splashText.Text = "";
            refreshTime = 0;
        }

        private void splashText_SelectionChanged(object sender, RoutedEventArgs e)
        {

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

        private void lvMain_DragOver(object sender, DragEventArgs e)
        {
   
        }

        private void lvMain_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
        }

        private void lvMain_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void lvMain_Drop(object sender, DragEventArgs e)
        {
            items.Clear();
            for (int i = 0; i < lvMain.Items.Count; i++)
            {
                items.Add(lvMain.Items[i]);
            }
        }

        private void loadText_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }

}
