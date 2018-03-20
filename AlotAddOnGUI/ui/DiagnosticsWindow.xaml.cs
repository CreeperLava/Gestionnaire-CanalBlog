using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using SlavaGu.ConsoleAppLauncher;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace AlotAddOnGUI.ui {
    /// <summary>
    /// Interaction logic for DiagnosticsWindow.xaml
    /// </summary>
    public partial class DiagnosticsWindow : MetroWindow
    {
        public const string SHOW_DIALOG_BAD_LOD = "SHOW_DIALOG_BAD_LOD";
        private const string SET_DIAG_TEXT = "SET_DIAG_TEXT";
        private const string SET_DIAGTASK_ICON_WORKING = "SET_DIAGTASK_ICON_WORKING";
        private const string SET_DIAGTASK_ICON_GREEN = "SET_DIAGTASK_ICON_GREEN";
        private const string SET_DIAGTASK_ICON_RED = "SET_DIAGTASK_ICON_RED";
        private const string SET_FULLSCAN_PROGRESS = "SET_STEP_PROGRESS";
        private const string SET_REPLACEDFILE_PROGRESS = "SET_REPLACEDFILE_PROGRESS";
        private const string RESET_REPLACEFILE_TEXT = "RESET_REPLACEFILE_TEXT";
        private const string TURN_OFF_TASKBAR_PROGRESS = "TURN_OFF_TASKBAR_PROGRESS";
        private const string TURN_ON_TASKBAR_PROGRESS = "TURN_ON_TASKBAR_PROGRESS";
        private const string UPLOAD_LINKED_LOG = "UPLOAD_LINKED_LOG";
        private const int CONTEXT_NORMAL = 0;
        private const int CONTEXT_FULLMIPMAP_SCAN = 1;
        private const int CONTEXT_REPLACEDFILE_SCAN = 2;
        private bool TextureCheck = false;
        private static int DIAGNOSTICS_GAME = 0;
        private static ConsoleApp BACKGROUND_MEM_PROCESS;
        private List<string> BACKGROUND_MEM_PROCESS_ERRORS;
        private List<string> BACKGROUND_MEM_PROCESS_PARSED_ERRORS;
        BackgroundWorker diagnosticsWorker;
        private StringBuilder diagStringBuilder;
        private int Context = CONTEXT_NORMAL;
        private bool MEMI_FOUND = true;
        private bool FIXED_LOD_SETTINGS = false;
        private List<string> AddedFiles = new List<string>();
        private string LINKEDLOGURL;

        public DiagnosticsWindow()
        {
            InitializeComponent();
        }

        private void Button_DiagnosticsME3_Click(object sender, RoutedEventArgs e)
        {
            Button_ManualFileME3.ToolTip = "Diagnostic will be run on Mass Effect 3";
            Button_ManualFileME1.Visibility = Visibility.Collapsed;
            Button_ManualFileME2.Visibility = Visibility.Collapsed;
            Button_ManualFileME3.Visibility = Visibility.Collapsed;
            Image_DiagME3.Visibility = Visibility.Visible;
            DIAGNOSTICS_GAME = 3;
            ShowDiagnosticTypes();
        }

        private void Button_DiagnosticsME1_Click(object sender, RoutedEventArgs e)
        {
            Image_DiagME1.ToolTip = "Diagnostic will be run on Mass Effect";
            Button_ManualFileME1.Visibility = Visibility.Collapsed;
            Button_ManualFileME2.Visibility = Visibility.Collapsed;
            Button_ManualFileME3.Visibility = Visibility.Collapsed;
            Image_DiagME1.Visibility = Visibility.Visible;
            DIAGNOSTICS_GAME = 1;
            ShowDiagnosticTypes();
        }

        private void Button_DiagnosticsME2_Click(object sender, RoutedEventArgs e)
        {
            Image_DiagME2.ToolTip = "Diagnostic will be run on Mass Effect 2";
            Button_ManualFileME1.Visibility = Visibility.Collapsed;
            Button_ManualFileME2.Visibility = Visibility.Collapsed;
            Button_ManualFileME3.Visibility = Visibility.Collapsed;
            Image_DiagME2.Visibility = Visibility.Visible;
            DIAGNOSTICS_GAME = 2;
            ShowDiagnosticTypes();
        }

        private void ShowDiagnosticTypes()
        {
            DiagnosticHeader.Text = "Select type of diagnostic.";
            ALOTVersionInfo avi = null;
            if (avi == null)
            {
                Button_FullDiagnostic.IsEnabled = false;
                Button_FullDiagnostic.ToolTip = "MEMI tag missing - full scan won't provide any useful info";
            }
            Panel_DiagnosticsTypes.Visibility = Visibility.Visible;
        }

        private void RunDiagnostics(int game, bool full)
        {
            DiagnosticHeader.Text = "Performing diagnostics...";
            TextureCheck = full;
            if (TextureCheck)
            {
                QuickCheckPanel.Visibility = Visibility.Collapsed;
                FullCheckPanel.Visibility = Visibility.Visible;
            }
            Panel_Progress.Visibility = Visibility.Visible;

            diagnosticsWorker = new BackgroundWorker();
            diagnosticsWorker.WorkerReportsProgress = true;
            diagnosticsWorker.RunWorkerAsync();
        }

        private void addDiagLine(string v)
        {
            if (diagStringBuilder == null)
            {
                diagStringBuilder = new StringBuilder();
            }
            diagStringBuilder.Append(v);
            diagStringBuilder.Append("\n");
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FullDiagnostic_Click(object sender, RoutedEventArgs e)
        {
            Button_FullDiagnostic.Visibility = Visibility.Collapsed;
            Button_QuickDiagnostic.Visibility = Visibility.Collapsed;
            TextBlock_DiagnosticType.Text = "FULL DIAGNOSTIC";
            TextBlock_DiagnosticType.Visibility = Visibility.Visible;
            RunDiagnostics(DIAGNOSTICS_GAME, true);
        }

        private void QuickDiagnostic_Click(object sender, RoutedEventArgs e)
        {
            Button_FullDiagnostic.Visibility = Visibility.Collapsed;
            Button_QuickDiagnostic.Visibility = Visibility.Collapsed;
            TextBlock_DiagnosticType.Text = "QUICK DIAGNOSTIC";
            TextBlock_DiagnosticType.Visibility = Visibility.Visible;
            RunDiagnostics(DIAGNOSTICS_GAME, false);
        }
    }
}
