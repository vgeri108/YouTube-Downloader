using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YouTube_Downloader
{
    /// <summary>
    /// Interaction logic for Progress.xaml
    /// </summary>
    public partial class Progress : Window
    {
        public Progress()
        {
            InitializeComponent();
            TotalProgressBar.Maximum = 100;
        }

        public void SetStatus(string text) => StatusText.Text = text;
        public void SetVideoProgress(double value) => VideoProgress.Value = value;
        public void SetTotalProgress(double value) => TotalProgressBar.Value = value;
        public void SetSpeed(string text) => SpeedText.Text = text;
        public void SetTimeLeft(string text) => TimeLeftText.Text = text;
    }
}
