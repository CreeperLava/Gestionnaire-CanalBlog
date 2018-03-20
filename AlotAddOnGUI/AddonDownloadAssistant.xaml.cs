using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlotAddOnGUI
{
    /// <summary>
    /// Interaction logic for AddonDownloadAssistant.xaml
    /// </summary>
    public partial class AddonDownloadAssistant : MetroWindow
    {
        private MainWindow windowRef;
        private List<AddonFile> missingAddonFiles;
        internal bool SHUTTING_DOWN = false;

        public AddonDownloadAssistant(MainWindow windowRef, List<AddonFile> missingAddonFiles) {
            Owner = windowRef;
            InitializeComponent();
            this.windowRef = windowRef;
            this.missingAddonFiles = missingAddonFiles;
        }

        private async void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string fname = (string)((Hyperlink)e.Source).Tag;

            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch (Exception other)
            {
                System.Windows.Clipboard.SetText(e.Uri.ToString());
                await this.ShowMessageAsync("Unable to open web browser", "Unable to open your default web browser. Open your browser and paste the link (already copied to clipboard) into your URL bar. Download the file named " + fname + ", then drag and drop it onto this program's interface.");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!SHUTTING_DOWN)
            {
                windowRef.WindowState = WindowState.Normal;
            }
            else
            {
                windowRef.DOWNLOAD_ASSISTANT_WINDOW = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            windowRef.WindowState = WindowState.Minimized;
        }
    }
}
