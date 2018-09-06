using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using SlavaGu.ConsoleAppLauncher;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlotAddOnGUI
{
    public partial class MainWindow : MetroWindow
    {
        static Random RANDOM = new Random();
        public const string UPDATE_CURRENT_STAGE_PROGRESS = "UPDATE_TASK_PROGRESS";
        public const string UPDATE_OVERALL_TASK = "UPDATE_OVERALL_TASK";
        public const string SHOW_ORIGIN_FLYOUT = "SHOW_ORIGIN_FLYOUT";
        private const int INSTALL_OK = 1;

        public const string RESTORE_FAILED_COULD_NOT_DELETE_FOLDER = "RESTORE_FAILED_COULD_NOT_DELETE_FOLDER";
        public string CurrentTask;
        public int CurrentTaskPercent;
        public const string UPDATE_CURRENTTASK_NAME = "UPDATE_SUBTASK";
        public const string STAGE_CONTEXT = "STAGE_CONTEXT";

        public const string UPDATE_STAGE_OF_STAGE_LABEL = "UPDATE_STAGE_LABEL";
        public const string HIDE_STAGES_LABEL = "HIDE_STAGE_LABEL";
        public const string UPDATE_HEADER_LABEL = "UPDATE_HEADER_LABEL";
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME1_ALOT_INFO;
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME2_ALOT_INFO;
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME3_ALOT_INFO;
        private const string SET_OVERALL_PROGRESS = "SET_OVERALL_PROGRESS";
        private const string HIDE_LOD_LIMIT = "HIDE_LOD_LIMIT";
        public bool BUILD_ALOT { get; private set; }

        public ConsoleApp Run7zWithProgressForAddonFile(string args, AddonFile af)
        {
            ConsoleApp ca = new ConsoleApp(MainWindow.BINARY_DIRECTORY + "7z.exe", args);
            ca.ConsoleOutput += (o, args2) =>
            {
                if (args2.IsError && args2.Line.Trim() != "" && !args2.Line.Trim().StartsWith("0M"))
                {
                    int percentIndex = args2.Line.IndexOf("%");
                    string message = "";
                    if (percentIndex > 0)
                    {
                        message = "Extracting - " + args2.Line.Substring(0, percentIndex + 1).Trim();
                        if (message != af.ReadyStatusText)
                        {
                            af.ReadyStatusText = "Extracting - " + args2.Line.Substring(0, percentIndex + 1).Trim();
                        }
                    }
                }
            };
            ca.Run();
            return ca;
        }

        private KeyValuePair<AddonFile, bool> ExtractAddon(AddonFile af)
        {
            return new KeyValuePair<AddonFile, bool>(af, true); //skip
        }

        private void BuildAddon(object sender, DoWorkEventArgs e)
        {
        }

        private bool ExtractAddons(int game)
        {
            return true;
        }

        private async void BackupWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void BackupGame(object sender, DoWorkEventArgs e)
        {
        }

        private void runMEM_BackupAndBuild(string exe, string args, BackgroundWorker worker, List<string> acceptedIPC = null)
        {
        }

        private void runMEM_DetectBadMods(string exe, string args, BackgroundWorker worker, List<string> acceptedIPC = null)
        {
        }

        private void RestoreGame(object sender, DoWorkEventArgs e)
        {
        }
        private async void BuildWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
    }
}
