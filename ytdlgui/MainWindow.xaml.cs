using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ytdlgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private string Placeholder = "Enter the url here...";
        string trial = "";
        bool playlist = false;

        public MainWindow()
        {
            InitializeComponent();

            urlBox.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128));

            urlBox.GotFocus += RemoveText;
            urlBox.LostFocus += AddText;
            urlBox.TextChanged += CheckPL;

            urlFetch.Click += RegexUrl;
        }

        

        public void RemoveText(object sender, EventArgs args)
        {
            if (urlBox.Text == Placeholder)
            {
                urlBox.Text = "";
                urlBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        public void AddText(object sender, EventArgs args)
        {
            if (string.IsNullOrWhiteSpace(urlBox.Text))
            {
                urlBox.Text = Placeholder;
                urlBox.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            }
        }

        //                                                          //i thought that it would work. it works but cant give the output to an element
        public void CmdStuff(string videoID, bool pl)
        {
            cmdOutput.Content = "Processing...";

            ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError = true;
            cmdStartInfo.RedirectStandardInput = true;
            cmdStartInfo.UseShellExecute = false;
            cmdStartInfo.CreateNoWindow = true;

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
            cmdProcess.ErrorDataReceived += cmd_Error;
            cmdProcess.OutputDataReceived += cmd_DataReceived;
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            if (pl == true)
            {
                cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --geo-bypass --extract-audio --audio-format mp3 --audio-quality 0 --embed-thumbnail --no-mtime --prefer-ffmpeg {videoID}");
            }
            else if (pl == false)
            {
                cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --geo-bypass --no-playlist --extract-audio --audio-format mp3 --audio-quality 0 --embed-thumbnail --no-mtime --prefer-ffmpeg {videoID}");
            }

            void cmd_DataReceived(object sender1, DataReceivedEventArgs e)
            {

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    trial = e.Data;
                    if (trial == null)
                    {
                        trial = "";
                    }
                    else
                    {
                        if (trial.ToLower().Contains("adding thumbnail to"))
                        {
                            cmdOutput.Content = "Done!";
                        }
                        else if (trial.Contains("webpage"))
                        {
                            cmdOutput.Content = "Getting information...";
                        }
                        else if (trial.ToLower().Contains("download") == true && trial.ToLower().Contains("downloading") == false)
                        {
                            cmdOutput.Content = "Downloading...";
                        }
                        else if (trial.ToLower().Contains("download") == false && trial.ToLower().Contains("downloading") == true)
                        {
                            
                        }
                        else if (trial.ToLower().Contains("destination:") && trial.ToLower().Contains(".mp3"))
                        {
                            cmdOutput.Content = "Converting...";
                        }
                    }
                }));
            }

            void cmd_Error(object sender2, DataReceivedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    trial = e.Data;
                    if (trial == null)
                    {
                        trial = "";
                    }
                    else
                    {
                        cmdOutput.Content = "Error";
                    }
                }));
            }
        }

        public void RegexUrl(object sender, EventArgs args)
        {
            if (urlBox.Text == Placeholder || urlBox.Text == "")
            {
                MessageBox.Show("Please enter a URL!", "Error");
            }
            else
            {
                Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?.*?(?:v|list)=(.*?)(?:&|$)|^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?(?:(?!=).)*\/(.*)$");
                Match match = regex.Match(urlBox.Text);

                if (match.Success)
                {
                    if (match.Groups[1].Value.Length == 11)
                    {
                        if (MessageBox.Show("Do you want to download for sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            playlist = false;
                            CmdStuff(match.Groups[1].Value, playlist);
                        }
                        else
                        {

                        }
                    }
                    else if (match.Groups[1].Value.Length == 34)
                    {
                        if (MessageBox.Show("Do you want to download for sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            playlist = true;
                            CmdStuff(match.Groups[1].Value, playlist);
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

        public void CmdUrl(object sender, EventArgs args)
        {

        }

        public void CheckPL(object sender, EventArgs args)
        {
            Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?.*?(?:v|list)=(.*?)(?:&|$)|^(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?(?:(?!=).)*\/(.*)$");
            Match match = regex.Match(urlBox.Text);

            if (match.Groups[1].Value.Length == 11)
            {
                correctbox.IsChecked = true;
                playlistbox.IsChecked = false;
            }
            else if (match.Groups[1].Value.Length == 34)
            {
                correctbox.IsChecked = true;
                playlistbox.IsChecked = true;
            }
            else
            {
                correctbox.IsChecked = false;
                playlistbox.IsChecked = false;
            }
        }
    }
}
