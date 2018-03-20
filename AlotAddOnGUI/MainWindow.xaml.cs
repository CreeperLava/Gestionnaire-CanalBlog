using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SlavaGu.ConsoleAppLauncher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AlotAddOnGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private System.Windows.Controls.CheckBox[] buildOptionCheckboxes;
        public ConsoleApp BACKGROUND_MEM_PROCESS = null;
        public bool BACKGROUND_MEM_RUNNING = false;
        ProgressDialogController updateprogresscontroller;
        public const string UPDATE_ADDONUI_CURRENTTASK = "UPDATE_OPERATION_LABEL";
        public const string HIDE_TIPS = "HIDE_TIPS";
        public const string UPDATE_PROGRESSBAR_INDETERMINATE = "SET_PROGRESSBAR_DETERMINACY";
        public const string INCREMENT_COMPLETION_EXTRACTION = "INCREMENT_COMPLETION_EXTRACTION";
        public const string SHOW_DIALOG = "SHOW_DIALOG";
        public const string ERROR_OCCURED = "ERROR_OCCURED";
        public static string EXE_DIRECTORY = System.AppDomain.CurrentDomain.BaseDirectory;
        public static string BINARY_DIRECTORY = EXE_DIRECTORY + "Data\\bin\\";
        private bool errorOccured = false;
        private bool UsingBundledManifest = false;
        private List<string> BlockingMods;
        private AddonFile meuitmFile;
        private DispatcherTimer backgroundticker;
        private DispatcherTimer tipticker;
        private int completed = 0;
        //private int addonstoinstall = 0;
        private int CURRENT_GAME_BUILD = 0; //set when extraction is run/finished
        private int ADDONSTOBUILD_COUNT = 0;
        private bool PreventFileRefresh = false;
        public const string REGISTRY_KEY = @"SOFTWARE\ALOTAddon";
        public const string ME3_BACKUP_REGISTRY_KEY = @"SOFTWARE\Mass Effect 3 Mod Manager";

        private BackgroundWorker BuildWorker = new BackgroundWorker();
        private BackgroundWorker BackupWorker = new BackgroundWorker();
        private BackgroundWorker InstallWorker = new BackgroundWorker();
        private BackgroundWorker ImportWorker = new BackgroundWorker();
        public event PropertyChangedEventHandler PropertyChanged;
        List<string> PendingUserFiles = new List<string>();
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public const string MEM_EXE_NAME = "MassEffectModderNoGui.exe";

        private BindingList<AddonFile> addonfiles;
        NotifyIcon nIcon = new NotifyIcon();
        private const string MEM_OUTPUT_DIR = "Data\\MEM_Packages";
        private const string MEM_OUTPUT_DISPLAY_DIR = "Data\\MEM_Packages";

        private const string ADDON_STAGING_DIR = "ADDON_STAGING";
        private const string USER_STAGING_DIR = "USER_STAGING";
        private const string UPDATE_STAGING_MEMNOGUI_DIR = "Data\\UpdateMemNoGui";
        private const string UPDATE_STAGING_MEM_DIR = "Data\\UpdateMem";

        private string ADDON_FULL_STAGING_DIRECTORY = System.AppDomain.CurrentDomain.BaseDirectory + "Data\\" + ADDON_STAGING_DIR + "\\";
        private string USER_FULL_STAGING_DIRECTORY = System.AppDomain.CurrentDomain.BaseDirectory + "Data\\" + USER_STAGING_DIR + "\\";

        private bool me1Installed;
        private bool me2Installed;
        private bool me3Installed;
        private bool RefreshListOnUserImportClose = false;
        private List<string> musicpackmirrors;
        private BindingList<AddonFile> alladdonfiles;
        private readonly string PRIMARY_HEADER = "Download the listed files for your game as listed below. You can filter per-game in the settings.\nDo not extract or rename any files you download. Drop them onto this interface to import them.";
        private readonly string SETTINGSTR_DEBUGLOGGING = "DebugLogging";
        private const string SETTINGSTR_DONT_FORCE_UPGRADES = "DontForceUpgrades";
        private const string SETTINGSTR_REPACK = "RepackGameFiles";
        private const string SETTINGSTR_IMPORTASMOVE = "ImportAsMove";
        public const string SETTINGSTR_BETAMODE = "BetaMode";
        public const string SETTINGSTR_DOWNLOADSFOLDER = "DownloadsFolder";
        private List<string> BACKGROUND_MEM_PROCESS_ERRORS;
        private List<string> BACKGROUND_MEM_PROCESS_PARSED_ERRORS;
        private const string SHOW_DIALOG_YES_NO = "SHOW_DIALOG_YES_NO";
        private bool CONTINUE_BACKUP_EVEN_IF_VERIFY_FAILS = false;
        private bool ERROR_SHOWING = false;
        private int PREBUILT_MEM_INDEX; //will increment to 10 when run
        private bool SHOULD_HAVE_OUTPUT_FILE;
        public bool USING_BETA { get; private set; }
        public bool SOUND_SETTING { get; private set; }
        public StringBuilder BACKGROUND_MEM_STDOUT { get; private set; }
        public int BACKUP_THREAD_GAME { get; private set; }
        private bool _showME1Files = true;
        private bool _showME2Files = true;
        private bool _showME3Files = true;
        private bool Loading = true;
        //private int LODLIMIT = 0;
        private FrameworkElement[] fadeInItems;
        private List<FrameworkElement> currentFadeInItems = new List<FrameworkElement>();
        private bool ShowReadyFilesOnly = false;
        internal AddonDownloadAssistant DOWNLOAD_ASSISTANT_WINDOW;
        private bool DONT_FORCE_UPGRADES = false;
        public static string DOWNLOADS_FOLDER;
        private int RefreshesUntilRealRefresh;
        private bool ShowBuildingOnly;
        private WebClient downloadClient;
        private string MANIFEST_LOC = EXE_DIRECTORY + @"Data\manifest.xml";
        private string MANIFEST_BUNDLED_LOC = EXE_DIRECTORY + @"Data\manifest-bundled.xml";
        private List<string> COPY_QUEUE = new List<string>();
        private List<string> MOVE_QUEUE = new List<string>();
        private DateTime bootTime;
        public static bool DEBUG_LOGGING;

        public MainWindow() {
            InitializeComponent();

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (!File.Exists(database))
                CreateDatabase();
            else {
                dbConnection = new SQLiteConnection(String.Format("Data Source={0};", database));
                dbConnection.Open();
            }

            UpdateDatabase();
        }

        string database = "Recettes.db";
        string folder = "recettes";
        SQLiteConnection dbConnection;
        enum Mode : int { ADD, UPDATE, NONE };

        private void Button_TODO_Click(object sender, RoutedEventArgs e) {
            // TODO
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
            object recipe = row.DataContext; // recipe.Url
            //System.Diagnostics.Process.Start(recipe.Url);
        }

        private void ContextMenu_Update(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            object recipe = row.DataContext;
            //ExtractRecipe(recipe.Url, true);
        }

        private void ContextMenu_Remove(object sender, RoutedEventArgs e) {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            object recipe = row.DataContext;
            //string path = "recettes\\" + r.Name;
            //System.IO.File.Delete("recettes\\");
        }

        public void CreateDatabase() {
            SQLiteConnection.CreateFile(database);
            dbConnection = new SQLiteConnection(String.Format("Data Source={0};", database));
            dbConnection.Open();

            string sql = "CREATE TABLE recettes (titre VARCHAR, url VARCHAR, date VARCHAR, imageUrl VARCHAR, imageNom VARCHAR, intro VARCHAR, prepa VARCHAR, ingred VARCHAR)";
            (new SQLiteCommand(sql, dbConnection)).ExecuteNonQuery();
        }

        public void UpdateDatabase() {
            string urlIndex = @"http://curcuma67.canalblog.com/archives/index.html";
            Regex month = new Regex(@"^(http:\/\/curcuma67\.canalblog\.com\/archives\/)[0-9]{4}\/[0-9]{2}\/(index\.html)$");
            Regex recipe = new Regex(@"^(http:\/\/curcuma67\.canalblog\.com\/archives\/)[0-9]{4}\/[0-9]{2}\/[0-9]{2}\/[0-9]{8}(\.html)$");

            int count = 0;
            foreach (var urlMonth in GetUrls(urlIndex, month, "//a[@href]", "href"))
                foreach (var urlRecipe in GetUrls(urlMonth, recipe, "//meta[@content]", "content")) {
                    var sql = String.Format("SELECT * FROM recettes WHERE url={0} LIMIT 1", urlRecipe);
                    var result = (new SQLiteCommand(sql, dbConnection)).ExecuteScalar();
                    if (result == null) {
                        // check if recipe in db
                        ExtractRecipe(urlRecipe, true); // if not, add to db
                    } else if (!File.Exists(result.ToString())) { // if yes, check if file is present
                        ExtractRecipe(urlRecipe, false);
                        count++;
                        // TODO display how many updated in GUI
                    }
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

        public void ExtractRecipe(string url, Boolean addToDB) {
            HtmlWeb web = new HtmlWeb();

            // main html document (minus scripts)
            HtmlNode htmlDoc = web.Load(url).DocumentNode.SelectSingleNode("//div[@class='blogbody']");

            var date = htmlDoc.SelectSingleNode("//div[@class='dateheader']").InnerText;
            var title = htmlDoc.SelectSingleNode("//h2[@class='articletitle']").InnerText.Replace(" SANS GLUTEN SANS LAIT SANS OEUF", "");

            // article itself (minus metadata)
            HtmlNode articleNode = htmlDoc.SelectSingleNode("//div[@class='articlebody']");

            HtmlNode imgNode = articleNode.SelectSingleNode("//div[@class='articlebody']/p/a");
            var imageUrl = imgNode.GetAttributeValue("href", "").Replace("_o", "");
            var imageName = imgNode.GetAttributeValue("name", ""); // get full quality picture

            var mode = 0;
            string[] texte = { "", "", "" };

            foreach (HtmlNode pNode in articleNode.SelectNodes("//div[@class='articlebody']/p")) {
                string str = pNode.InnerText.Replace("&nbsp;", ""); // sanitize HTML
                switch (str) {
                    case "": break;
                    case "PRÉPARATION :":
                        mode = 1;
                        break;
                    case "INGRÉDIENTS :":
                        mode = 2;
                        break;
                    default:
                        texte[mode] += str + "\n";
                        break;
                }
            }

            CreateXml("1.0", url, title, date, imageUrl, imageName, texte[0], texte[1], texte[2]);

            if (addToDB) {
                var sql = String.Format("INSERT INTO recettes VALUES ({0},{1},{2},{3},{4},{5},{6},{7})", "1.0", title, url, date, imageUrl, imageName, texte[0], texte[1], texte[2]);
                (new SQLiteCommand(sql, dbConnection)).ExecuteNonQuery();
            }
        }

        public void CreateXml(string version, string url, string titre, string date, string imageUrl, string imageNom, string intro, string prepa, string ingred) {
            XmlDocument doc = new XmlDocument();
            XmlElement recipeElement = doc.CreateElement("recette");
            doc.AppendChild(recipeElement);

            // Create attributes for recipe and append them to the recipe element
            XmlAttribute attribute = doc.CreateAttribute("version");
            attribute.Value = version;
            recipeElement.Attributes.Append(attribute);

            // Create children of recipe element
            XmlElement titreElement = doc.CreateElement("titre");
            titreElement.InnerText = titre;
            recipeElement.AppendChild(titreElement);

            XmlElement dateElement = doc.CreateElement("date");
            dateElement.InnerText = date;
            recipeElement.AppendChild(dateElement);

            XmlElement imageUrlElement = doc.CreateElement("imageUrl");
            imageUrlElement.InnerText = imageUrl;
            recipeElement.AppendChild(imageUrlElement);

            XmlElement imageNomElement = doc.CreateElement("imageNom");
            imageNomElement.InnerText = imageNom;
            recipeElement.AppendChild(imageNomElement);

            XmlElement introElement = doc.CreateElement("introduction");
            introElement.InnerText = intro;
            recipeElement.AppendChild(introElement);

            XmlElement ingredElement = doc.CreateElement("ingredients");
            ingredElement.InnerText = ingred;
            recipeElement.AppendChild(ingredElement);

            XmlElement prepaElement = doc.CreateElement("preparation");
            prepaElement.InnerText = prepa;
            recipeElement.AppendChild(prepaElement);

            doc.Save(String.Format("recettes\\{0}.recette", titre));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ?
        }

        /// <summary>
        /// Returns the mem output dir with a \ on the end.
        /// </summary>
        /// <param name="game">Game number to get path for</param>
        /// <returns></returns>

        private async void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string fname = null;
            if (e.Source is Hyperlink)
            {
                fname = (string)((Hyperlink)e.Source).Tag;
            }
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
                if (fname != null)
                {
                    this.nIcon.Visible = true;
                    //this.WindowState = System.Windows.WindowState.Minimized;
                    this.nIcon.ShowBalloonTip(14000, "Directions", "Download the file titled: \"" + fname + "\"", ToolTipIcon.Info);
                }
            }
            catch (Exception other)
            {
                System.Windows.Clipboard.SetText(e.Uri.ToString());
                await this.ShowMessageAsync("Unable to open web browser", "Unable to open your default web browser. Open your browser and paste the link (already copied to clipboard) into your URL bar." + fname != null ? " Download the file named " + fname + ", then drag and drop it onto this program's interface." : "");
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            bool isClosing = true;
            if (true)
            {
                e.Cancel = true;

                MetroDialogSettings mds = new MetroDialogSettings();
                mds.AffirmativeButtonText = "Yes";
                mds.NegativeButtonText = "No";
                mds.DefaultButtonFocus = MessageDialogResult.Negative;

                MessageDialogResult result = await this.ShowMessageAsync("Closing ALOT Installer may leave game in a broken state", "MEM is currently installing textures. Closing the program will likely leave your game in an unplayable, broken state. Are you sure you want to exit?", MessageDialogStyle.AffirmativeAndNegative, mds);
                if (result == MessageDialogResult.Affirmative)
                {
                    Close();
                }
                else
                {
                    isClosing = false;
                }
            }

            if (isClosing)
            {
                if (DOWNLOAD_ASSISTANT_WINDOW != null)
                {
                    DOWNLOAD_ASSISTANT_WINDOW.SHUTTING_DOWN = true;
                    DOWNLOAD_ASSISTANT_WINDOW.Close();
                }

                if (BuildWorker.IsBusy || InstallWorker.IsBusy)
                {
                    //We should add indicator that we closed while busy
                }
            }

        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            ((Expander)sender).BringIntoView();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Normal:
                    if (COPY_QUEUE.Count > 0)
                    {
                        string detailsMessage = "The following files were just imported to ALOT Installer:";
                        foreach (string af in COPY_QUEUE)
                        {
                            detailsMessage += "\n - " + af;
                        }

                        string originalTitle = COPY_QUEUE.Count + " file" + (COPY_QUEUE.Count != 1 ? "s" : "") + " imported";
                        string originalMessage = COPY_QUEUE.Count + " file" + (COPY_QUEUE.Count != 1 ? "s have" : " has") + " been copied into the Downloaded_Mods directory.";

                        COPY_QUEUE.Clear();
                    }
                    if (MOVE_QUEUE.Count > 0)
                    {
                        string detailsMessage = "The following files were just imported to ALOT Installer. The files have been moved to the Downloaded_Mods folder.";
                        foreach (string af in MOVE_QUEUE)
                        {
                            detailsMessage += "\n - " + af;
                        }
                        string originalTitle = MOVE_QUEUE.Count + " file" + (MOVE_QUEUE.Count != 1 ? "s" : "") + " imported";
                        string originalMessage = MOVE_QUEUE.Count + " file" + (MOVE_QUEUE.Count != 1 ? "s have" : " has") + " been moved into the Downloaded_Mods directory.";
                        MOVE_QUEUE.Clear();
                    }
                    break;
            }
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            System.Windows.Controls.ContextMenu cm = row.ContextMenu;
            AddonFile af = (AddonFile)row.DataContext;

            //Reset
            foreach (System.Windows.Controls.MenuItem mi in cm.Items)
            {
                mi.Visibility = Visibility.Visible;
            }

            int i = 0;
            while (i < cm.Items.Count)
            {
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)cm.Items[i];
                switch (i)
                {
                    case 0: //Visit download
                        if (af.UserFile)
                        {
                            mi.Visibility = Visibility.Collapsed;
                        }
                        break;
                    case 1:
                        if (!af.Ready || PreventFileRefresh)
                        {
                            mi.Visibility = Visibility.Collapsed;
                        }
                        break;
                    case 2: //Toggle on/off
                        if (af.ALOTVersion > 0 || af.ALOTUpdateVersion > 0 || !af.Ready || PreventFileRefresh)
                        {
                            mi.Visibility = Visibility.Collapsed;
                            break;
                        }
                        if (af.Enabled)
                        {
                            mi.Header = "Disable file";
                            mi.ToolTip = "Click to disable file for this session.\nThis file will not be processed when staging files for installation.";
                            mi.ToolTip = "Prevents this file from being used for installation";
                        }
                        else
                        {
                            mi.Header = "Enable file";
                            mi.ToolTip = "Allows this file to be used for installation";
                        }
                        break;
                    case 3: //Remove user file
                        mi.Visibility = af.UserFile ? Visibility.Visible : Visibility.Collapsed;
                        break;
                }
                i++;
            }
        }

        private void ContextMenu_OpenDownloadPage(object sender, RoutedEventArgs e)
        {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
            // open url
        }

        private void ContextMenu_ToggleFile(object sender, RoutedEventArgs e)
        {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;

            if (af.Ready)
            {
                af.Enabled = !af.Enabled;
                if (!af.Enabled)
                {
                    af.ReadyStatusText = "Disabled";
                }
                else
                {
                    af.ReadyStatusText = null;
                }
            }
        }

        private void ContextMenu_ViewFile(object sender, RoutedEventArgs e)
        {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
        }

        private void ContextMenu_RemoveFile(object sender, RoutedEventArgs e)
        {
            var rowIndex = ListView_Files.SelectedIndex;
            var row = (System.Windows.Controls.ListViewItem)ListView_Files.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            AddonFile af = (AddonFile)row.DataContext;
            if (af.UserFile)
            {
                alladdonfiles.Remove(af);
            }
        }

        private void ListView_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListView_Files.SelectedItem != null)
            {
                ListView_Files.ScrollIntoView(ListView_Files.SelectedItem);
            }
        }

        private void ListView_Files_Loaded(object sender, RoutedEventArgs e)
        {
            if (ListView_Files.SelectedItem != null)
            {
                ListView_Files.ScrollIntoView(ListView_Files.SelectedItem);
            }
        }
    }
}