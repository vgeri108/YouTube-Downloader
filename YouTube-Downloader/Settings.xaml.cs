using System;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace YouTube_Downloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public string SelectedFolder => Folder.Text;
        public bool IsMp3 => MP3.IsChecked == true;

        public Settings()
        {
            InitializeComponent();
            MP3.IsChecked = true;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                Folder.Text = dialog.SelectedPath;
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Folder.Text))
            {
                MessageBox.Show("Válassz mappát!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }
    }
}
