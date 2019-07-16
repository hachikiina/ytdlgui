using Ookii.Dialogs.Wpf;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        Methods MainMethods = new Methods();

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

            MainMethods.OnStartup();

            outputPath.Click += MainMethods.SelectPath;
            ffmpegD.Click += MainMethods.FFmpegCheck;
            ffmpegButton.Click += MainMethods.FfmpegButton_Click;

            urlBox.GotFocus += MainMethods.RemoveText;
            urlBox.LostFocus += MainMethods.AddText;
            urlBox.TextChanged += MainMethods.CheckPL;

            urlFetch.Click += MainMethods.RegexUrl;
        }

        
    }
}
