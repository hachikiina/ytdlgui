using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using VideoLibrary;
using ytdlgui.Properties;

public class UrlStuffLol
{
    public string ytTitle { get; set; }
    public string Status { get; set; }
    public string videoID { get; set; }
}

namespace ytdlgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        string Placeholder = "Enter the url here...";
        string Trial = "";
        string Ffmpegpath = "";
        bool Playlist = false;

        public List<UrlStuffLol> items = new List<UrlStuffLol>();


        public MainWindow()
        {
            //this integrates the ookii dll at compile
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            InitializeComponent();

            urlBox.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128));

            outputPathBox.Text = Directory.GetCurrentDirectory();

            OnStartup();

            outputPath.Click += SelectPath;
            ffmpegD.Click += FFmpegCheck;
            ffmpegButton.Click += FfmpegButton_Click;
            donwload.Click += Download_Click;
            clearlist.Click += Clearlist_Click;
            updatebtn.Click += Updatebtn_Click;

            plRanRevReset.Click += PlRanRevReset_Click;

            plStartNum.Click += PlCheckEndStartItem;
            plEndNum.Click += PlCheckEndStartItem;
            plSpecify.Click += PlCheckEndStartItem;

            urlBox.GotFocus += RemoveText;
            urlBox.LostFocus += AddText;
            urlBox.TextChanged += CheckPL;

            urlFetch.Click += RegexUrl;
        }

        private async void Updatebtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var MyClass in items)
            {
                var yt = YouTube.Default;
                try
                {
                    var video = await yt.GetVideoAsync(MyClass.ytTitle);
                    MyClass.ytTitle = video.Title;
                    lvUrls.Items.Refresh();
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("BRUHHHHHH");
                }
            }
        }

        private void Clearlist_Click(object sender, RoutedEventArgs e)
        {
            lvUrls.ItemsSource = null;
            items.Clear();
        }

        //list stuff pls fix
        private void Download_Click(object sender, EventArgs args)
        {
            //items.Add(new UrlStuffLol() { ytUrl = "xd", Status = "xd" });
            //items.Add(new UrlStuffLol() { ytUrl = "xd", Status = "xd" });
            foreach (var myClass in items)
            {
                //MessageBox.Show(myClass.ytUrl + " " + myClass.Status);
                //myClass.Status = "lol";
                lvUrls.Items.Refresh();
                CmdStuff(myClass.videoID, false);
            }
            lvUrls.ItemsSource = items;
        }

        /// <summary>
        /// Called upon the "Select" button's click, opens the dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FfmpegButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select the ffmpeg path.",
                UseDescriptionForTitle = true
            };

            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show("Your operating system is older than Vista, falling back to old folder selector.");

            lol:
            if ((bool)dialog.ShowDialog(this))
            {
                if (File.Exists(dialog.SelectedPath + "\\ffmpeg.exe") && File.Exists(dialog.SelectedPath + "\\ffprobe.exe"))
                {
                    Ffmpegpath = dialog.SelectedPath;
                    ffmpegButton.ToolTip = Ffmpegpath;
                }
                else
                {
                    MessageBox.Show("Please select the correct path!\nMake sure it includes ffmpeg and ffprobe.");
                    goto lol;
                }
            }
        }

        /// <summary>
        /// Checks whether the ffmpeg tick is ticked or not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FFmpegCheck(object sender, RoutedEventArgs e)
        {
            if (ffmpegD.IsChecked ?? false)
            {
                ffmpegButton.IsEnabled = true;
            }
            else
            {
                ffmpegButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Gets called upon closing the application.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.directory = outputPathBox.Text;

            Settings.Default.ffmpegpath = Ffmpegpath;

            Settings.Default.StartString = plStart.Text;

            Settings.Default.EndString = plEnd.Text;

            Settings.Default.ItemString = plItems.Text;

            bool ffmpeglol = ffmpegD.IsChecked ?? false;
            Settings.Default.ffmpegd = ffmpeglol;

            bool ffmpegBBool = ffmpegButton.IsEnabled;
            Settings.Default.ffmpegbutton = ffmpegBBool;

            bool forcepls = forcePlaylist.IsChecked ?? false;
            Settings.Default.forcepl = forcepls;

            bool geobps = geoBypass.IsChecked ?? false;
            Settings.Default.geobp = geobps;

            bool thumbns = thumbnail.IsChecked ?? false;
            Settings.Default.thumbn = thumbns;

            bool metas = metadata.IsChecked ?? false;
            Settings.Default.meta = metas;

            bool timestamps = changeDate.IsChecked ?? false;
            Settings.Default.timestamp = timestamps;

            bool startC = plStartNum.IsChecked ?? false;
            Settings.Default.StartCheck = startC;

            bool endC = plEndNum.IsChecked ?? false;
            Settings.Default.EndCheck = endC;

            bool itemC = plSpecify.IsChecked ?? false;
            Settings.Default.ItemCheck = itemC;

            bool startendE = !plSpecify.IsChecked ?? false;
            Settings.Default.StartEndE = startendE;

            bool itemE = (!plStartNum.IsChecked ?? false) && (!plEndNum.IsChecked ?? false);
            Settings.Default.ItemE = itemE;

            Settings.Default.Save();
        }

        /// <summary>
        /// Gets called upon the start of the program, reloads the settings.
        /// </summary>
        public void OnStartup()
        {
            plStart.Text = Settings.Default.StartString;
            plEnd.Text = Settings.Default.EndString;
            plItems.Text = Settings.Default.ItemString;
            outputPathBox.Text = Settings.Default.directory;
            Ffmpegpath = Settings.Default.ffmpegpath;
            ffmpegButton.ToolTip = Ffmpegpath;
            ffmpegD.IsChecked = Settings.Default.ffmpegd;
            ffmpegButton.IsEnabled = Settings.Default.ffmpegbutton;
            forcePlaylist.IsChecked = Settings.Default.forcepl;
            geoBypass.IsChecked = Settings.Default.geobp;
            thumbnail.IsChecked = Settings.Default.thumbn;
            metadata.IsChecked = Settings.Default.meta;
            changeDate.IsChecked = Settings.Default.timestamp;
            plStartNum.IsChecked = Settings.Default.StartCheck;
            plEndNum.IsChecked = Settings.Default.EndCheck;
            plSpecify.IsChecked = Settings.Default.ItemCheck;
            plStartNum.IsEnabled = Settings.Default.StartEndE;
            plEndNum.IsEnabled = Settings.Default.StartEndE;
            plSpecify.IsEnabled = Settings.Default.ItemE;
        }

        /// <summary>
        /// Removes the placeholder text from urlBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RemoveText(object sender, EventArgs args)
        {
            if (urlBox.Text == Placeholder)
            {
                urlBox.Text = "";
                urlBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        /// <summary>
        /// Adds the placeholder text to urlBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AddText(object sender, EventArgs args)
        {
            if (string.IsNullOrWhiteSpace(urlBox.Text))
            {
                urlBox.Text = Placeholder;
                urlBox.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            }
        }

        /// <summary>
        /// Summons a folder select dialog to set output path for youtube-dl to output files into.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SelectPath(object sender, EventArgs args)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true
            };

            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show("Your operating system is older than Vista, falling back to old folder selector.");

            if ((bool)dialog.ShowDialog(this))
                outputPathBox.Text = dialog.SelectedPath;

        }

        /// <summary>
        /// Feeds the youtube url with specified settings.
        /// </summary>
        /// <param name="videoID"></param>
        /// <param name="pl"></param>
        public void CmdStuff(string videoID, bool pl)
        {
            //checks if any of the playlist options are checked and if they have empty strings. very long if
            if (((plStartNum.IsChecked ?? false) && (String.IsNullOrEmpty(plStartN) || String.IsNullOrWhiteSpace(plStartN))) || ((plEndNum.IsChecked ?? false) && (String.IsNullOrEmpty(plEndN) || String.IsNullOrWhiteSpace(plEndN))) || ((plSpecify.IsChecked ?? false) && ((String.IsNullOrEmpty(plItemN) || String.IsNullOrWhiteSpace(plItemN)))))
            {
                MessageBox.Show("Please enter values for the:\nPlaylist Start\nPlaylist End\nor\nPlaylist Items");
                return;
            }

            cmdOutput.Content = "Processing...";

            outputPath.IsEnabled = false;
            outputPathBox.IsEnabled = false;

            //this makes cmd run in the background
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                WorkingDirectory = $@"{outputPathBox.Text}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
            cmdProcess.ErrorDataReceived += cmd_Error;
            cmdProcess.OutputDataReceived += cmd_DataReceived;
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            //starts the procedure
            if (forcePlaylist.IsChecked ?? false)
            {
                //if the force playlist is checked, it passes the regex control and directly comes here (for another regex control)
                //because the regex i used there only gets video and playlist id's,
                //with video id's being the priority.
                //so i had to pass the whole url for the youtube-dl to see 
                //so it can download the playlist. duh xd
                //this brings some problems of course, but i left youtube-dl to deal with them.
                string ffmpegoutput = "";
                if (ffmpegD.IsChecked ?? false)
                {
                    ffmpegoutput = $"--ffmpeg-location {Ffmpegpath}";
                }

                string geo = "--no-geo-bypass";
                if (geoBypass.IsChecked ?? false)
                {
                    geo = "--geo-bypass";
                }

                string tnail = "";
                if (thumbnail.IsChecked ?? false)
                    tnail = "--embed-thumbnail";

                string mdata = "";
                if (metadata.IsChecked ?? false)
                    mdata = "--add-metadata";

                string chDate = "--no-mtime";
                if (changeDate.IsChecked ?? false)
                    chDate = "";

                string RevRan = "";
                if (plReverse.IsChecked ?? false)
                    RevRan = "--playlist-reverse";
                else if (plRandom.IsChecked ?? false)
                    RevRan = "--playlist-random";

                string StartNumber = "";
                if (plStartNum.IsChecked ?? false)
                    StartNumber = "--playlist-start " + plStartN;

                string EndNumber = "";
                if (plEndNum.IsChecked ?? false)
                    EndNumber = "--playlist-end " + plEndN;

                string Items = "";
                if (plSpecify.IsChecked ?? false)
                    Items = "--playlist-items " + plItemN;

                if (urlBox.Text.ToLower().Contains("youtube") || urlBox.Text.ToLower().Contains("youtu.be"))
                {
                    //from stackoverflow, replaces '&'s with '^&' (i dont like regex xd)
                    string fixedamps = Regex.Replace(urlBox.Text, @" 
                            # Match & that is not part of an HTML entity.
                            &                  # Match literal &.
                            (?!                # But only if it is NOT...
                            \w+;               # an alphanumeric entity,
                            | \#[0-9]+;        # or a decimal entity,
                            | \#x[0-9A-F]+;    # or a hexadecimal entity.
                            )                  # End negative lookahead.",
                    "^&",
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    //do it!!!
                    cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --output %(title)s.%(ext)s {geo} --extract-audio --audio-format mp3 --audio-quality 0 {RevRan} {StartNumber} {EndNumber} {Items} {tnail} {chDate} {mdata} --prefer-ffmpeg {ffmpegoutput} {fixedamps}");
                }
                else
                    MessageBox.Show("Please enter a valid URL!\n(This might have happened because you ticked\n\"Force Playlist\" box. Try to download the whole playlist.)");

            }
            else
            {
                string ffmpegoutput = "";
                if (ffmpegD.IsChecked ?? false)
                {
                    ffmpegoutput = $"--ffmpeg-location {Ffmpegpath}";
                }

                string geo = "--no-geo-bypass";
                if (geoBypass.IsChecked ?? false)
                    geo = "--geo-bypass";

                string tnail = "";
                if (thumbnail.IsChecked ?? false)
                    tnail = "--embed-thumbnail";

                string mdata = "";
                if (metadata.IsChecked ?? false)
                    mdata = "--add-metadata";

                string chDate = "--no-mtime";
                if (changeDate.IsChecked ?? false)
                    chDate = "";

                string RevRan = "";
                if (plReverse.IsChecked ?? false)
                    RevRan = "--playlist-reverse";
                else if (plRandom.IsChecked ?? false)
                    RevRan = "--playlist-random";

                string StartNumber = "";
                if (plStartNum.IsChecked ?? false)
                    StartNumber = "--playlist-start " + plStartN;

                string EndNumber = "";
                if (plEndNum.IsChecked ?? false)
                    EndNumber = "--playlist-end " + plEndN;

                string Items = "";
                if (plSpecify.IsChecked ?? false)
                    Items = "--playlist-items " + plItemN;

                if (pl == true)
                {
                    cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --output %(title)s.%(ext)s {geo} --extract-audio --audio-format mp3 --audio-quality 0 {RevRan} {StartNumber} {EndNumber} {Items} {tnail} {chDate} {mdata} --prefer-ffmpeg {ffmpegoutput} {videoID}");
                }
                else if (pl == false)
                {
                    cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --output %(title)s.%(ext)s {geo} --no-playlist --extract-audio --audio-format mp3 --audio-quality 0 {tnail} {chDate} {mdata} --prefer-ffmpeg {ffmpegoutput} {videoID}");
                }
            }

            void cmd_DataReceived(object sender1, DataReceivedEventArgs e)
            {
                //this does the checks for the status thingie
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //if it stays null it throws ArgumentNullException.
                    Trial = e.Data;
                    if (Trial == null)
                    {
                        Trial = "";
                    }
                    else
                    {
                        if (Trial.ToLower().Contains("adding thumbnail to"))
                        {
                            cmdOutput.Content = "Done!";
                            outputPath.IsEnabled = true;
                            outputPathBox.IsEnabled = true;
                        }
                        else if (Trial.Contains("webpage"))
                        {
                            cmdOutput.Content = "Getting information...";
                        }
                        else if (Trial.ToLower().Contains("download") == true && Trial.ToLower().Contains("downloading") == false)
                        {
                            cmdOutput.Content = "Downloading...";
                        }
                        else if (Trial.ToLower().Contains("download") == false && Trial.ToLower().Contains("downloading") == true)
                        {

                        }
                        else if (Trial.ToLower().Contains("destination:") && Trial.ToLower().Contains(".mp3"))
                        {
                            cmdOutput.Content = "Converting...";
                        }
                        else if (Trial.ToLower().Contains("deleting original file") && (!thumbnail.IsChecked ?? false))
                        {
                            cmdOutput.Content = "Done!";
                            outputPath.IsEnabled = true;
                            outputPathBox.IsEnabled = true;
                        }
                    }
                }));
            }

            //this function catches errors from cmd
            void cmd_Error(object sender2, DataReceivedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //if it stays null it throws ArgumentNullException.
                    Trial = e.Data;
                    if (Trial == null)
                    {
                        Trial = "";
                    }
                    else if (Trial.ToLower().Contains("this playlist does not exist"))
                    {
                        //this else if in here works if the force playlist is ticked.
                        //catches wrong playlist id's.
                        cmdOutput.Content = Trial;
                        outputPath.IsEnabled = true;
                        outputPathBox.IsEnabled = true;
                    }
                    else if (Trial.ToLower().Contains("this video is unavailable"))
                    {
                        //catches bad video id's if force playlist is checked. 
                        cmdOutput.Content = Trial;
                        outputPath.IsEnabled = true;
                        outputPathBox.IsEnabled = true;
                    }
                    else if (Trial.ToLower().Contains("is not recognized"))
                    {
                        cmdOutput.Content = "You dont have youtube-dl installed!";
                        MessageBox.Show("You dont have youtube-dl installed.\nPlease install it from their github page.", "Error");
                    }
                    else
                    {
                        //this catches any error thrown from anything in cmd and says error. lol
                        cmdOutput.Content = Trial; //sometimes it wont fit :<
                                                   //MessageBox.Show(trial);
                        outputPath.IsEnabled = true;
                        outputPathBox.IsEnabled = true;
                    }
                }));
            }
        }

        /// <summary>
        /// Seperates the video or playlist ID's from the videos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RegexUrl(object sender, EventArgs args)
        {
            PlCheckESI();

            if (urlBox.Text == Placeholder || urlBox.Text == "")
            {
                MessageBox.Show("Please enter a URL!", "Error");
            }
            else
            {
                Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?.*?(?:v|list)=(.*?)(?:&|$)|^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?(?:(?!=).)*\/(.*)$");
                Match match = regex.Match(urlBox.Text);

                //if the match succeeded, checks the length and feeds them accordingly. 11 = video id, 34 = playlist id
                if (match.Success)
                {
                    if (match.Groups[1].Value.Length == 11)
                    {
                        if (MessageBox.Show("Do you want to download for sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Playlist = false;
                            CmdStuff(match.Groups[1].Value, Playlist);
                        }
                        else
                        {

                        }
                    }
                    else if (match.Groups[1].Value.Length == 34)
                    {
                        if (MessageBox.Show("Do you want to download for sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Playlist = true;
                            CmdStuff(match.Groups[1].Value, Playlist);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    MessageBox.Show("Couldn't parse that.\nAre you sure that's the right URL?", "Error");
                }
            }
        }

        /// <summary>
        /// Checks the entered URL for it being a playlist or not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void CheckPL(object sender, EventArgs args)
        {
            Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?.*?(?:v|list)=(.*?)(?:&|$)|^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?(?:(?!=).)*\/(.*)$");
            Match match = regex.Match(urlBox.Text);

            if (match.Groups[1].Value.Length == 11)
            {
                correctbox.IsChecked = true;
                playlistbox.IsChecked = false;
                //items.Add(new UrlStuffLol() { ytUrl = "https://www.youtube.com/watch?v=" + match.Groups[1].Value, Status = "Waiting" });
                //lvUrls.ItemsSource = items;
                try
                {
                    await GetVideoTitleAsync(match.Groups[1].Value, false);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Please enter a valid URL!", "Error");
                }
            }
            else if (match.Groups[1].Value.Length == 34)
            {
                correctbox.IsChecked = true;
                playlistbox.IsChecked = true;
                //items.Add(new UrlStuffLol() { ytTitle = "Playlist" + match.Groups[1].Value, Status = "Waiting", videoID = match.Groups[1].Value });
                //lvUrls.ItemsSource = items;
                try
                {
                    await GetVideoTitleAsync(match.Groups[1].Value, true);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Please enter a valid URL!", "Error");
                }
            }
            else
            {
                correctbox.IsChecked = false;
                playlistbox.IsChecked = false;
            }
        }

        /// <summary>
        /// Resets the radio buttons plReset and plReverse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PlRanRevReset_Click(object sender, RoutedEventArgs e)
        {
            plRandom.IsChecked = false;
            plReverse.IsChecked = false;
        }

        /// <summary>
        /// Makes sure that you only input numbers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            PlCheckESI();
        }

        /// <summary>
        /// Makes sure that you only input numbers and 2 special chars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationv2(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,-]+");
            e.Handled = regex.IsMatch(e.Text);
            PlCheckESI();
        }

        string plStartN = "";
        string plEndN = "";
        string plItemN = "";
        /// <summary>
        /// Event handler version of PlCheckESI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlCheckEndStartItem(object sender, RoutedEventArgs e)
        {
            PlCheckESI();
        }

        /// <summary>
        /// Makes sure you cant select them all
        /// </summary>
        private void PlCheckESI()
        {
            if ((plStartNum.IsChecked ?? false) || (plEndNum.IsChecked ?? false))
            {
                plSpecify.IsEnabled = false;
                plSpecify.IsChecked = false;
                if (plStartNum.IsChecked ?? false)
                {
                    plStartN = plStart.Text;
                }
                else
                {
                    plStartN = "";
                }

                if (plEndNum.IsChecked ?? false)
                {
                    plEndN = plEnd.Text;
                }
                else
                {
                    plEndN = "";
                }
            }
            else if ((!plStartNum.IsChecked ?? false) || (!plEndNum.IsChecked ?? false))
            {
                plSpecify.IsEnabled = true;
            }

            if (plSpecify.IsChecked ?? false)
            {
                plStartNum.IsChecked = false;
                plStartNum.IsEnabled = false;
                plEndNum.IsChecked = false;
                plEndNum.IsEnabled = false;

                plItemN = plItems.Text;
            }
            else if (!plSpecify.IsChecked ?? false)
            {
                plStartNum.IsEnabled = true;
                plEndNum.IsEnabled = true;

                plItemN = "";
            }
        }

        private async Task GetVideoTitleAsync(string videoID, bool playlist)
        {
            //items.Add(new UrlStuffLol() {ytUrl = "Getting the title...", Status = "Waiting" });
            string titleurl = "https://www.youtube.com/watch?v=" + videoID;

            var youtube = YouTube.Default;

            if (!playlist)
            {
                var video = await youtube.GetVideoAsync(titleurl);

                //await Task.Delay(2000);

                foreach (var item in items)
                    if (item.ytTitle.Contains(video.Title.Replace(" - YouTube", ""))) return;

                items.Add(new UrlStuffLol() { ytTitle = video.Title.Replace(" - YouTube", ""), Status = "Waiting", videoID = videoID });
                lvUrls.ItemsSource = items;
                lvUrls.Items.Refresh();
            }
            else
            {
                items.Add(new UrlStuffLol() { ytTitle = "Playlist", Status = "Waiting", videoID = videoID });
                lvUrls.ItemsSource = items;
                lvUrls.Items.Refresh();
            }
        }
    }
}

namespace FixedWidthColumn
{
    public class FixedWidthColumn : GridViewColumn
    {
        #region Constructor

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            FixedWidthColumn fwc = o as FixedWidthColumn;
            if (fwc != null)
                return fwc.FixedWidth;
            return 0.0;
        }

        #endregion

        #region FixedWidth

        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        public static readonly DependencyProperty FixedWidthProperty =
            DependencyProperty.Register("FixedWidth", typeof(double), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FixedWidthColumn fwc = o as FixedWidthColumn;

            if (fwc != null)
                fwc.CoerceValue(WidthProperty);
        }

        #endregion
    }
}