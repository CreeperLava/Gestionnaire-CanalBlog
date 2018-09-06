using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SlavaGu.ConsoleAppLauncher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Data;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Net;

namespace AlotAddOnGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged {
        public ConsoleApp BACKGROUND_MEM_PROCESS = null;
        public bool BACKGROUND_MEM_RUNNING = false;
        public const string UPDATE_ADDONUI_CURRENTTASK = "UPDATE_OPERATION_LABEL";
        public const string HIDE_TIPS = "HIDE_TIPS";
        public const string UPDATE_PROGRESSBAR_INDETERMINATE = "SET_PROGRESSBAR_DETERMINACY";
        public const string INCREMENT_COMPLETION_EXTRACTION = "INCREMENT_COMPLETION_EXTRACTION";
        public const string SHOW_DIALOG = "SHOW_DIALOG";
        public const string ERROR_OCCURED = "ERROR_OCCURED";
        public static string EXE_DIRECTORY = System.AppDomain.CurrentDomain.BaseDirectory;
        public static string BINARY_DIRECTORY = EXE_DIRECTORY + "Data\\bin\\";
        private bool PreventFileRefresh = false;
        public const string REGISTRY_KEY = @"SOFTWARE\ALOTAddon";
        public const string ME3_BACKUP_REGISTRY_KEY = @"SOFTWARE\Mass Effect 3 Mod Manager";

        private BackgroundWorker BuildWorker = new BackgroundWorker();
        private BackgroundWorker BackupWorker = new BackgroundWorker();
        private BackgroundWorker InstallWorker = new BackgroundWorker();
        private BackgroundWorker ImportWorker = new BackgroundWorker();
        public event PropertyChangedEventHandler PropertyChanged;
        List<string> PendingUserFiles = new List<string>();
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        NotifyIcon nIcon = new NotifyIcon();
        private const string SETTINGSTR_DONT_FORCE_UPGRADES = "DontForceUpgrades";
        private const string SETTINGSTR_REPACK = "RepackGameFiles";
        private const string SETTINGSTR_IMPORTASMOVE = "ImportAsMove";
        public const string SETTINGSTR_BETAMODE = "BetaMode";
        public const string SETTINGSTR_DOWNLOADSFOLDER = "DownloadsFolder";
        private const string SHOW_DIALOG_YES_NO = "SHOW_DIALOG_YES_NO";
        public bool USING_BETA { get; private set; }
        public bool SOUND_SETTING { get; private set; }
        public StringBuilder BACKGROUND_MEM_STDOUT { get; private set; }
        public int BACKUP_THREAD_GAME { get; private set; }
        private List<FrameworkElement> currentFadeInItems = new List<FrameworkElement>();
        internal AddonDownloadAssistant DOWNLOAD_ASSISTANT_WINDOW;
        public static string DOWNLOADS_FOLDER;
        public static bool DEBUG_LOGGING;

        // my vars
        private BindingList<AddonFile> allRecipes;
        string imgPath = @"data\img\";
        string database = @"data\recipes.db";
        string xml = @"data\recipes.xml";
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

            string sql = "CREATE TABLE recipes (title VARCHAR, url VARCHAR, date VARCHAR, imageUrl VARCHAR, imageNom VARCHAR, intro VARCHAR, prepa VARCHAR, ingred VARCHAR)";
            (new SQLiteCommand(sql, dbConnection)).ExecuteNonQuery();
        }

        public void OpenDatabase() {
            dbConnection = new SQLiteConnection(String.Format("Data Source={0};", database));
            dbConnection.Open();
        }

        public void UpdateDatabase() {
            int count = 0;
            foreach (var urlMonth in GetUrls(urlIndex, month, "//a[@href]", "href")) {
                foreach (var urlRecipe in GetUrls(urlMonth, recipe, "//meta[@content]", "content")) {
                    var sql = String.Format("SELECT * FROM recipes WHERE url='{0}' LIMIT 1", urlRecipe);
                    var result = (new SQLiteCommand(sql, dbConnection)).ExecuteScalar();
                    if (result != null) // check if recipe in db
                        continue; // if yes, skip it

                    var err = ExtractRecipe(urlRecipe);
                    if (err == 0) // TODO replace with real log
                        count++; // TODO display how many updated in GUI
                    else {
                        Debug.Write("Error selecting ");
                        switch (err) {
                            case -1:
                                Debug.Write("date");
                                break;
                            case -2:
                                Debug.Write("title");
                                break;
                            case -3:
                                Debug.Write("articleNode");
                                break;
                            case -4:
                                Debug.Write("imgNode");
                                break;
                            case -5:
                                Debug.Write("imageUrl");
                                break;
                        }

                        Debug.WriteLine(String.Format(" for recipe {0} : node is null", urlRecipe));
                    }
                }
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

        private void Button_TODO_Click(object sender, RoutedEventArgs e) {
            Debug.Write("click");
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
                        stringDate = (string)e.Element("date"),
                        imageUrl = (string)e.Element("imageUrl"),
                        imageNom = (string)e.Element("imageNom"),
                        intro = (string)e.Element("intro"),
                        prepa = (string)e.Element("prepa"),
                        ingred = (string)e.Element("ingred"),
                        Enabled = true,
                        Ready = true,
                }).ToList();
            } catch (Exception e) {
                Debug.Write("Error has occured parsing the XML!");
                Debug.Write(App.FlattenException(e));
                MessageDialogResult result = await this.ShowMessageAsync("Error reading file manifest", "An error occured while reading the manifest file for installation.\n\n" + e.Message, MessageDialogStyle.Affirmative);
                return;
            }

            linqlist = linqlist.OrderByDescending(o => o.annee).ThenByDescending(x => x.date).ThenBy(x => x.title).ToList();
            allRecipes = new BindingList<AddonFile>(linqlist);

            var set = new HashSet<AddonFile>(allRecipes);
            if (ListView_Files.Items.Count == 0) { // refresh ui
                ListView_Files.ItemsSource = allRecipes;
                CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(ListView_Files.ItemsSource);
                view.GroupDescriptions.Add(new PropertyGroupDescription("annee"));
            }
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

        public void SaveImage(string url, string name) {
            WebClient webClient = new WebClient();
            string path = imgPath + name + ".jpg";
            if (!File.Exists(path))
                using (webClient)
                    try {
                        Debug.WriteLine("Start download of " + url);
                        webClient.DownloadFileAsync(new Uri(url), path);
                    } catch (Exception e) {
                        Debug.Write("File " + url + " could not be downloaded : " + e.Message);
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
            var date = tmp.InnerText;

            tmp = htmlDoc.SelectSingleNode("//h2[@class='articletitle']");
            if (tmp == null)
                return -2;
            var title = String.Join("", tmp.InnerText.Split(Path.GetInvalidFileNameChars())).Replace("&quot;", @"'").Replace("&nbsp;", @"&"); // delete illegal characters

            // article itself (minus metadata)
            HtmlNode articleNode = htmlDoc.SelectSingleNode("//div[@class='articlebody']");
            if (articleNode == null)
                return -3;

            HtmlNode imgNode = articleNode.SelectSingleNode("//div[@class='articlebody']/p/a");
            if (imgNode == null)
                return -4;
            var imageUrl = imgNode.GetAttributeValue("href", "").Replace("_o", "").Replace(".to_resize_150x3000", "");
            if (!picture.Match(imageUrl).Success) { 
                Debug.WriteLine("No valid image for " + url);
                return -5;
            }

            var imageName = imgNode.GetAttributeValue("name", ""); // get full quality picture
            SaveImage(imageUrl, imageName);

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

            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "INSERT INTO recipes VALUES(@title, @url, @date, @imageUrl, @imageName, @texte0, @texte1, @texte2)";
            command.Parameters.AddWithValue("@title", title).DbType = DbType.String;
            command.Parameters.AddWithValue("@url", url).DbType = DbType.String;
            command.Parameters.AddWithValue("@date", date).DbType = DbType.String;
            command.Parameters.AddWithValue("@imageUrl", imageUrl).DbType = DbType.String;
            command.Parameters.AddWithValue("@imageName", imageName).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte0", texte[0]).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte1", texte[1]).DbType = DbType.String;
            command.Parameters.AddWithValue("@texte2", texte[2]).DbType = DbType.String;
            command.ExecuteNonQuery();
            command.Dispose();

            return 0;
        }

        private void InitializeContextMenu(object sender, ContextMenuEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            System.Windows.Controls.ContextMenu cm = row.ContextMenu;

            foreach (System.Windows.Controls.MenuItem mi in cm.Items)
                mi.Visibility = Visibility.Visible;
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
            } catch (Exception de) {
                Debug.Write("File " + imgPath + recipe.imageNom + ".jpg" + " not found");
            }
        }

        /// <summary>
        /// Executes a task in the future
        /// </summary>
        /// <param name="action">Action to run</param>
        /// <param name="timeoutInMilliseconds">Delay in ms</param>
        /// <returns></returns>
        /// 
        public async Task Execute(Action action, int timeoutInMilliseconds) {
            await Task.Delay(timeoutInMilliseconds);
            action();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LoadRecipes();
        }

        /// <summary>
        /// Returns the mem output dir with a \ on the end.
        /// </summary>
        /// <param name="game">Game number to get path for</param>
        /// <returns></returns>

        private async void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            string fname = null;
            if (e.Source is Hyperlink) {
                fname = (string)((Hyperlink)e.Source).Tag;
            }
            try {
                System.Diagnostics.Process.Start(e.Uri.ToString());
                if (fname != null) {
                    this.nIcon.Visible = true;
                    //this.WindowState = System.Windows.WindowState.Minimized;
                    this.nIcon.ShowBalloonTip(14000, "Directions", "Download the file titled: \"" + fname + "\"", ToolTipIcon.Info);
                }
            } catch (Exception other) {
                System.Windows.Clipboard.SetText(e.Uri.ToString());
                await this.ShowMessageAsync("Unable to open web browser", "Unable to open your default web browser. Open your browser and paste the link (already copied to clipboard) into your URL bar." + fname != null ? " Download the file named " + fname + ", then drag and drop it onto this program's interface." : "");
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e) {
            bool isClosing = true;
            if (true) {
                e.Cancel = true;

                MetroDialogSettings mds = new MetroDialogSettings();
                mds.AffirmativeButtonText = "Yes";
                mds.NegativeButtonText = "No";
                mds.DefaultButtonFocus = MessageDialogResult.Negative;

                MessageDialogResult result = await this.ShowMessageAsync("Closing ALOT Installer may leave game in a broken state", "MEM is currently installing textures. Closing the program will likely leave your game in an unplayable, broken state. Are you sure you want to exit?", MessageDialogStyle.AffirmativeAndNegative, mds);
                if (result == MessageDialogResult.Affirmative) {
                    Close();
                } else {
                    isClosing = false;
                }
            }

            if (isClosing) {
                if (DOWNLOAD_ASSISTANT_WINDOW != null) {
                    DOWNLOAD_ASSISTANT_WINDOW.SHUTTING_DOWN = true;
                    DOWNLOAD_ASSISTANT_WINDOW.Close();
                }

                if (BuildWorker.IsBusy || InstallWorker.IsBusy) {
                    //We should add indicator that we closed while busy
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            ((Expander)sender).BringIntoView();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e) {
            switch (this.WindowState) {
                case WindowState.Maximized:
                case WindowState.Normal:
                    break;
            }
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            System.Windows.Controls.ContextMenu cm = row.ContextMenu;
            AddonFile af = (AddonFile)row.DataContext;

            //Reset
            foreach (System.Windows.Controls.MenuItem mi in cm.Items) {
                mi.Visibility = Visibility.Visible;
            }

            int i = 0;
            while (i < cm.Items.Count) {
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)cm.Items[i];
                switch (i) {
                    case 0: //Visit download
                        mi.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        if (!af.Ready || PreventFileRefresh)
                            mi.Visibility = Visibility.Collapsed;
                        break;
                    case 2: //Toggle on/off
                        if (!af.Ready || PreventFileRefresh) {
                            mi.Visibility = Visibility.Collapsed;
                            break;
                        }
                        if (af.Enabled) {
                            mi.Header = "Disable file";
                            mi.ToolTip = "Click to disable file for this session.\nThis file will not be processed when staging files for installation.";
                            mi.ToolTip = "Prevents this file from being used for installation";
                        } else {
                            mi.Header = "Enable file";
                            mi.ToolTip = "Allows this file to be used for installation";
                        }
                        break;
                    case 3: // Remove user file
                        mi.Visibility = Visibility.Collapsed;
                        break;
                }
                i++;
            }
        }

        private void ContextMenu_OpenDownloadPage(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
            // open url
        }

        private void ContextMenu_ViewFile(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
        }

        private void ContextMenu_RemoveFile(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
        }

        private void ListView_Files_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListView_Files.SelectedItem != null) {
                ListView_Files.ScrollIntoView(ListView_Files.SelectedItem);
            }
        }

        private void ListView_Files_Loaded(object sender, RoutedEventArgs e) {
            if (ListView_Files.SelectedItem != null) {
                ListView_Files.ScrollIntoView(ListView_Files.SelectedItem);
            }
        }
    }
}