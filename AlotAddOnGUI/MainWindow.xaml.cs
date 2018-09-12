using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Data;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Net;
using System.Threading;
using System.Globalization;

namespace AlotAddOnGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        NotifyIcon nIcon = new NotifyIcon();

        // my vars
        private BindingList<AddonFile> allRecipes;
        string imgPath = @"data\img\";
        string database = @"data\recipes.db";
        string blacklist = @"data\blacklist.txt";
        List<string> blacklistUrls;
        SQLiteConnection dbConnection;
        StringWriter xmlRecipes = new StringWriter();

        string urlIndex = @"http://curcuma67.canalblog.com/archives/index.html";
        Regex picture = new Regex(@"^https?:\/\/p[0-9]?\.storage\.canalblog\.com\/.*$");
        Regex month = new Regex(@"^(http:\/\/curcuma67\.canalblog\.com\/archives\/)[0-9]{4}\/[0-9]{2}\/(index\.html)$");
        Regex recipe = new Regex(@"^(http:\/\/curcuma67\.canalblog\.com\/archives\/)[0-9]{4}\/[0-9]{2}\/[0-9]{2}\/[0-9]{8}(\.html)$");

        enum Mode : int { ADD, UPDATE, NONE };

        public MainWindow() {
            InitializeComponent();
            Directory.CreateDirectory(imgPath);

            if (!File.Exists(blacklist))
                File.Create(blacklist).Close();
            blacklistUrls = new List<string>(File.ReadAllLines(blacklist)); // get all blacklisted urls, if any

            if (!File.Exists(database))
                CreateDatabase();
            else
                OpenDatabase();
            UpdateDatabase();

            UpdateXml();
        }

        public void CreateDatabase() {
            SQLiteConnection.CreateFile(database);
            dbConnection = new SQLiteConnection(String.Format("Data Source={0};", database));
            dbConnection.Open();

            string sql = "CREATE TABLE recipes (title VARCHAR, url VARCHAR, dateString VARCHAR, dateInt INTEGER, imageUrl VARCHAR, imageNom VARCHAR, intro VARCHAR, prepa VARCHAR, ingred VARCHAR, tag VARCHAR)";
            (new SQLiteCommand(sql, dbConnection)).ExecuteNonQuery();
        }

        public void OpenDatabase() {
            dbConnection = new SQLiteConnection(String.Format("Data Source={0};", database));
            dbConnection.Open();
        }

        public void UpdateDatabase() {
            foreach (var urlMonth in GetUrls(urlIndex, month, "//a[@href]", "href")) {
                Parallel.ForEach(GetUrls(urlMonth, recipe, "//meta[@content]", "content").Except(blacklistUrls), urlRecipe => {
                    var sql = String.Format("SELECT * FROM recipes WHERE url='{0}' LIMIT 1", urlRecipe);
                    var result = (new SQLiteCommand(sql, dbConnection)).ExecuteScalar();
                    if (result != null) // check if recipe in db
                        return; // if yes, skip it

                    var err = ExtractRecipe(urlRecipe);
                    if (err != 0) {
                        String debug = "[ERROR] Error selecting ";
                        switch (err) {
                            case -1:
                                debug += "date";
                                break;
                            case -2:
                                debug += "title";
                                break;
                            case -3:
                                debug += "articleNode";
                                break;
                            case -4:
                                debug += "imgNode";
                                break;
                            case -5:
                                debug += "imageUrl";
                                break;
                        }
                        debug += String.Format(" for recipe {0} : node is null", urlRecipe);
                        Debug.WriteLine(debug);
                        File.AppendAllText(blacklist, urlRecipe + "\n");
                    }
                });
            }
            (new SQLiteCommand("VACUUM", dbConnection)).ExecuteNonQuery();
        }

        private void UpdateXml() {
            string sql = "SELECT * FROM recipes";
            using (SQLiteCommand sqlComm = new SQLiteCommand(sql, dbConnection) { CommandType = CommandType.Text }) {
                SQLiteDataAdapter da = new SQLiteDataAdapter(sqlComm);
                DataSet ds = new DataSet();
                da.Fill(ds);
                ds.Tables[0].WriteXml(xmlRecipes);
            }
        }

        private async void LoadRecipes() {
            List<AddonFile> linqlist = null;
            XElement rootElement = XElement.Load(new StringReader(xmlRecipes.ToString()));

            allRecipes = new BindingList<AddonFile>(); //prevents crashes

            try {
                // SQL like syntax : find all elements named "Table" under root
                // return each of these elements as e
                // create a new AddonFile based on each e
                linqlist =
                (from e in rootElement.Elements("Table")
                    select new AddonFile {
                        title = (string)e.Element("title"),
                        url = (string)e.Element("url"),
                        dateString = (string)e.Element("dateString"),
                        dateInt = (int)e.Element("dateInt"),
                        imageUrl = (string)e.Element("imageUrl"),
                        imageNom = (string)e.Element("imageNom"),
                        intro = (string)e.Element("intro"),
                        prepa = (string)e.Element("prepa"),
                        ingred = (string)e.Element("ingred"),
                        tag = (string)e.Element("tag"),
                        Enabled = true,
                        Ready = true,
                }).ToList();
            } catch (Exception e) {
                Debug.Write("[ERROR] Error has occured parsing the XML!");
                Debug.Write(App.FlattenException(e));
                MessageDialogResult result = await this.ShowMessageAsync("Error reading file manifest", "An error occured while reading the manifest file for installation.\n\n" + e.Message, MessageDialogStyle.Affirmative);
                return;
            }

            linqlist = linqlist.OrderByDescending(x => x.dateInt).ToList();
            allRecipes = new BindingList<AddonFile>(linqlist);

            var set = new HashSet<AddonFile>(allRecipes);
            ListView_Files.ItemsSource = allRecipes; // refresh UI
            CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(ListView_Files.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("annee"));
        }

        public static HashSet<string> GetUrls(string url, Regex r, string n, string attr) {
            HashSet<string> res = new HashSet<string>();
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            // find all href nodes
            IEnumerable<HtmlNode> nodes = htmlDoc.DocumentNode.SelectNodes(n);
            foreach (var node in nodes) {
                var link = node.GetAttributeValue(attr, "");
                if (r.Match(link).Success)
                    res.Add(link);
            }
            return res;
        }

        public String SaveImage(string url, string name) {
            WebClient webClient = new WebClient();
            string path = imgPath + name + ".jpg";
            if (!File.Exists(path)) {
                using (webClient) {
                    try {
                        Debug.WriteLine("[LOG] Start download of " + url);
                        webClient.DownloadFile(new Uri(url), path);

                        if (new FileInfo(path).Length == 0) {
                            Debug.WriteLine("[WARNING] File " + url + " could not be downloaded");
                            File.Delete(path);
                            return "";
                        } else {
                            return name;
                        }
                    } catch (Exception e) {
                        Debug.WriteLine("[WARNING]File " + url + " could not be downloaded : " + e.Message);
                        Thread.Sleep(2000);
                        Debug.WriteLine("[LOG] Restart download of " + url);
                        try {
                            webClient.DownloadFile(new Uri(url), path);
                            return name;
                        } catch (Exception f) {
                            Debug.Write("[WARNING] File " + url + " could not be downloaded : " + f.Message);
                            return "";
                        }
                    }
                }
            } else {
                return name;
            }
        }

        public int ExtractRecipe(string url) {
            HtmlWeb web = new HtmlWeb();
            HtmlNode tmp;

            // main html document (minus scripts)
            HtmlNode htmlDoc = web.Load(url).DocumentNode.SelectSingleNode("//div[@class='blogbody']");

            tmp = htmlDoc.SelectSingleNode("//div[@class='dateheader']"); // check if it is a valid recipe
            if (tmp == null)
                return -1;
            string dateString = tmp.InnerText;
            DateTime dateTime = DateTime.ParseExact(tmp.InnerText, "dd MMMM yyyy", new CultureInfo("fr-FR"));
            int dateInt = int.Parse(dateTime.Year + (dateTime.Month < 10 ? "0" : "") + dateTime.Month + (dateTime.Day < 10 ? "0" : "") + dateTime.Day);

            tmp = htmlDoc.SelectSingleNode("//h2[@class='articletitle']");
            if (tmp == null)
                return -2;
            string title = String.Join("", tmp.InnerText.Split(Path.GetInvalidFileNameChars())).Replace("&quot;", @"'").Replace("&nbsp;", @"&"); // delete illegal characters

            // article itself (minus metadata)
            HtmlNode articleNode = htmlDoc.SelectSingleNode("//div[@class='articlebody']");
            if (articleNode == null)
                return -3;

            HtmlNode imgNode = articleNode.SelectSingleNode("//div[@class='articlebody']/p/a/img");
            if (imgNode == null)
                return -4;
            string imageUrl = imgNode.GetAttributeValue("src", "").Replace("_o", "").Replace(".to_resize_150x3000", "");
            if (!picture.Match(imageUrl).Success) { 
                Debug.WriteLine("[WARNING] No valid image for " + url);
                return -5;
            }

            string imageName = imgNode.ParentNode.GetAttributeValue("name", ""); // get full quality picture
            imageName = SaveImage(imageUrl, imageName);
            if (imageName != "")
                Debug.WriteLine("[LOG] File " + imageUrl + " successfully downloaded");

            var mode = 0;
            string[] texte = { "", "", "" };

            foreach (HtmlNode pNode in articleNode.SelectNodes("//div[@class='articlebody']/p")) {
                string str = pNode.InnerText.Replace("&nbsp;", "").Replace("&quot;", @"'"); // sanitize HTML
                switch (str) {
                    case "": break;
                    case "PRÉPARATION :":
                    case "PREPARATION :":
                    case "PRÉPARATION : ":
                    case "PREPARATION : ":
                        mode = 1;
                        break;
                    case "INGRÉDIENTS :":
                    case "INGREDIENTS :":
                    case "INGRÉDIENTS : ":
                    case "INGREDIENTS : ":
                        mode = 2;
                        break;
                    default:
                        texte[mode] += str + "\n";
                        break;
                }
            }

            HtmlNode footerNode = htmlDoc.SelectSingleNode("//div[@class='itemfooter']");
            HtmlNodeCollection tagNodes = footerNode.SelectNodes("//div[@class='itemfooter']/a[@rel='tag']");
            string tag = "";

            if(tagNodes == null)
                Debug.WriteLine("[WARNING] No valid tags for " + url);
            else
                foreach (HtmlNode t in tagNodes)
                    tag += t.InnerText.Replace("&nbsp;", "").Replace("&quot;", @"'") + " ";

            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "INSERT INTO recipes VALUES(@title, @url, @dateString, @dateInt, @imageUrl, @imageName, @texte0, @texte1, @texte2, @tag)";
            command.Parameters.AddWithValue("@title", title).DbType = DbType.String;
            command.Parameters.AddWithValue("@url", url).DbType = DbType.String;
            command.Parameters.AddWithValue("@dateString", dateString).DbType = DbType.String;
            command.Parameters.AddWithValue("@dateInt", dateInt).DbType = DbType.Int32;
            command.Parameters.AddWithValue("@imageUrl", imageUrl).DbType = DbType.String;
            command.Parameters.AddWithValue("@imageName", imageName).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte0", texte[0]).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte1", texte[1]).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte2", texte[2]).DbType = DbType.String;
            command.Parameters.AddWithValue("@tag", tag).DbType = DbType.String;
            command.ExecuteNonQuery();
            command.Dispose();

            return 0;
        }

        private void Search_Flyout_Click(object sender, RoutedEventArgs e) {
            SettingsFlyout.IsOpen = true;
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e) {
            string keyword = Search.Text;

            CollectionView view;
            if (keyword == null || keyword == "") {
                ListView_Files.ItemsSource = allRecipes;
                view = (CollectionView)CollectionViewSource.GetDefaultView(ListView_Files.ItemsSource);
                return;
            }

            HashSet<AddonFile> newList = new HashSet<AddonFile>();
            CompareInfo compare = CultureInfo.InvariantCulture.CompareInfo;
            CompareOptions options = CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
            foreach (AddonFile af in allRecipes) {
                if (SearchByTitle.IsChecked ?? false)
                    if (compare.IndexOf(af.title, keyword, options) != -1)
                        newList.Add(af);
                
                if (SearchByDescr.IsChecked ?? false)
                    if(compare.IndexOf(af.prepa + af.ingred + af.intro, keyword, options) != -1)
                        newList.Add(af);

                if (SearchByTags.IsChecked ?? false)
                    if (compare.IndexOf(af.tag, keyword, options) != -1)
                        newList.Add(af);
            }

            ListView_Files.ItemsSource = newList;
            view = (CollectionView)CollectionViewSource.GetDefaultView(ListView_Files.ItemsSource);
        }

        private void InitializeContextMenu(object sender, ContextMenuEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            System.Windows.Controls.ContextMenu cm = row.ContextMenu;

            foreach (System.Windows.Controls.MenuItem mi in cm.Items)
                mi.Visibility = Visibility.Visible;
        }

        private void ContextMenu_Recipe(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile recipe = (AddonFile)row.DataContext;
            AddonDownloadAssistant viewRecipe = new AddonDownloadAssistant(this, recipe);
            viewRecipe.Show();
        }

        private void ContextMenu_Open(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile recipe = (AddonFile) row.DataContext;
            Process.Start(recipe.url);
        }

        private void ContextMenu_Update(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile recipe = (AddonFile)row.DataContext;
            try {
                Process.Start(imgPath + recipe.imageNom + ".jpg");
            } catch (Exception) {
                Debug.Write("[ERROR] File " + imgPath + recipe.imageNom + ".jpg" + " not found");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LoadRecipes();
        }
    }
}