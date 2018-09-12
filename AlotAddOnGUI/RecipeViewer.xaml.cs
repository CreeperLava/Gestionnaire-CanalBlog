using CanalBlogManager.classes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CanalBlogManager {
    /// <summary>
    /// Interaction logic for RecipeViewer.xaml
    /// </summary>
    public partial class RecipeViewer : MetroWindow {
        private MainWindow windowRef;
        public Recipe recipe { get; set; }
        internal bool SHUTTING_DOWN = false;

        public RecipeViewer(MainWindow windowRef, Recipe recipe) {
            Owner = windowRef;
            InitializeComponent();
            this.windowRef = windowRef;
            this.recipe = recipe;

            if(recipe.imageNom != "") {
                String imageURI = @"data\img\" + recipe.imageNom + ".jpg";
                BitmapImage bitmapImage = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory, imageURI)));
                if(bitmapImage.PixelHeight != 0)
                    photo.Source = bitmapImage;
            }

            String newline = "\n\n";
            TextBlock_intro.Text += newline + recipe.intro;
            TextBlock_ingred.Text += newline + recipe.ingred;
            TextBlock_prepa.Text += newline + recipe.prepa;
        }

        private async void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            string fname = (string)((Hyperlink)e.Source).Tag;

            try {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            } catch (Exception) {
                System.Windows.Clipboard.SetText(e.Uri.ToString());
                await this.ShowMessageAsync("Unable to open web browser", "Unable to open your default web browser. Open your browser and paste the link (already copied to clipboard) into your URL bar. Download the file named " + fname + ", then drag and drop it onto this program's interface.");
            }
        }
    }
}