using AlotAddOnGUI.classes;
using AlotAddOnGUI.music;
using AlotAddOnGUI.ui;
using ByteSizeLib;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SlavaGu.ConsoleAppLauncher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace AlotAddOnGUI
{
    public partial class MainWindow : MetroWindow
    {
        static Random RANDOM = new Random();
        private int INSTALLING_THREAD_GAME;
        private List<AddonFile> ADDONFILES_TO_BUILD;
        private List<AddonFile> ADDONFILES_TO_INSTALL;
        public const string UPDATE_CURRENT_STAGE_PROGRESS = "UPDATE_TASK_PROGRESS";
        public const string UPDATE_OVERALL_TASK = "UPDATE_OVERALL_TASK";
        public const string SHOW_ORIGIN_FLYOUT = "SHOW_ORIGIN_FLYOUT";
        private const int INSTALL_OK = 1;
        private WaveOut waveOut;
        private NAudio.Vorbis.VorbisWaveReader vorbisStream;


        public const string RESTORE_FAILED_COULD_NOT_DELETE_FOLDER = "RESTORE_FAILED_COULD_NOT_DELETE_FOLDER";
        public string CurrentTask;
        public int CurrentTaskPercent;
        public const string UPDATE_CURRENTTASK_NAME = "UPDATE_SUBTASK";
        private int CURRENT_STAGE_NUM = 0;
        private int STAGE_COUNT;
        private string CURRENT_STAGE_CONTEXT = null;
        public const string STAGE_CONTEXT = "STAGE_CONTEXT";

        public const string UPDATE_STAGE_OF_STAGE_LABEL = "UPDATE_STAGE_LABEL";
        public const string HIDE_STAGES_LABEL = "HIDE_STAGE_LABEL";
        public const string UPDATE_HEADER_LABEL = "UPDATE_HEADER_LABEL";
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME1_ALOT_INFO;
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME2_ALOT_INFO;
        public static ALOTVersionInfo CURRENTLY_INSTALLED_ME3_ALOT_INFO;
        int USERFILE_INDEX = 100;
        private bool WARN_USER_OF_EXIT = false;
        private List<string> TIPS_LIST;
        private const string SET_OVERALL_PROGRESS = "SET_OVERALL_PROGRESS";
        private const string HIDE_LOD_LIMIT = "HIDE_LOD_LIMIT";
        Stopwatch stopwatch;
        private string MAINTASK_TEXT;
        private string CURRENT_USER_BUILD_FILE = "";
        public bool BUILD_ALOT { get; private set; }
        private bool BUILD_ADDON_FILES = false;
        private bool BUILD_USER_FILES = false;
        private bool BUILD_ALOT_UPDATE = false;
        private bool BUILD_MEUITM = false;


        private FadeInOutSampleProvider fadeoutProvider;
        private bool MusicPaused;
        private string DOWNLOADED_MODS_DIRECTORY = EXE_DIRECTORY + "Downloaded_Mods";
        private string EXTRACTED_MODS_DIRECTORY = EXE_DIRECTORY + "Data\\Extracted_Mods";
        private bool ERROR_OCCURED_PLEASE_STOP = false;
        private bool REPACK_GAME_FILES;
        private const string ERROR_TEXTURE_MAP_MISSING = "ERROR_TEXTURE_MAP_MISSING";
        private const string ERROR_TEXTURE_MAP_WRONG = "ERROR_TEXTURE_MAP_WRONG";
        private const string ERROR_FILE_ADDED = "ERROR_FILE_ADDED";
        private const string ERROR_FILE_REMOVED = "ERROR_FILE_REMOVED";
        private const string SETTINGSTR_SOUND = "PlayMusic";
        private const string SET_VISIBILE_ITEMS_LIST = "SET_VISIBILE_ITEMS_LIST";
        private const int END_OF_PROCESS_POLL_INTERVAL = 100;

        public bool MusicIsPlaying { get; private set; }

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
