﻿using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ytdlgui.Properties;

namespace ytdlgui
{
    public class Methods : MainWindow
    {
        private string Placeholder { get; set; } = "Enter the url here...";
        private string Trial { get; set; } = "";
        private string Ffmpegpath { get; set; } = "";
        private bool Playlist { get; set; } = false;

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

            Settings.Default.Save();
        }

        /// <summary>
        /// Gets called upon the start of the program, reloads the settings.
        /// </summary>
        public void OnStartup()
        {
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
                    cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --output %(title)s.%(ext)s {geo} --extract-audio --audio-format mp3 --audio-quality 0 {tnail} {chDate} {mdata} --prefer-ffmpeg {ffmpegoutput} {fixedamps}");
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

                if (pl == true)
                {
                    cmdProcess.StandardInput.WriteLine($@"youtube-dl --ignore-errors --output %(title)s.%(ext)s {geo} --extract-audio --audio-format mp3 --audio-quality 0 {tnail} {chDate} {mdata} --prefer-ffmpeg {ffmpegoutput} {videoID}");
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