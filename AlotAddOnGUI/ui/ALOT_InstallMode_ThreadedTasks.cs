using AlotAddOnGUI.classes;
using MahApps.Metro.Controls;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace AlotAddOnGUI {
    public partial class MainWindow : MetroWindow {
        private bool STAGE_DONE_REACHED = false;
        private const int RESULT_UNPACK_FAILED = -40;
        private const int RESULT_SCAN_REMOVE_FAILED = -41;
        private const int RESULT_ME1LAA_FAILED = -43;
        private const int RESULT_TEXTUREINSTALL_NO_TEXTUREMAP = -44;
        private const int RESULT_TEXTUREINSTALL_INVALID_TEXTUREMAP = -45;
        private const int RESULT_TEXTUREINSTALL_GAME_FILE_REMOVED = -47;
        private const int RESULT_TEXTUREINSTALL_GAME_FILE_ADDED = -48;
        private const int RESULT_TEXTUREINSTALL_FAILED = -42;

        private const int RESULT_SAVING_FAILED = -49;
        private const int RESULT_REMOVE_MIPMAPS_FAILED = -50;
        private const int RESULT_REPACK_FAILED = -46;
        private const int RESULT_UNKNOWN_ERROR = -51;
        private const int RESULT_SCAN_FAILED = -52;

        private void MusicIcon_Click(object sender, RoutedEventArgs e) {
        }

        private void InstallALOT(int game, List<AddonFile> filesToInstall) {
        }

        private async void InstallWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        }


        private void InstallALOTContextBased(object sender, DoWorkEventArgs e) {
        }

        private void MusicPlaybackStopped(object sender, StoppedEventArgs e) {
        }

        private void newTipTimer_Tick(object sender, EventArgs e) {
        }

        private string GetMusicDirectory() {
            return EXE_DIRECTORY + "Data\\music\\";
        }

        private void InstallCompleted(object sender, RunWorkerCompletedEventArgs e) {
        }

        private void RunAndTimeMEMContextBased_Install(string exe, string args, BackgroundWorker installWorker) {
        }

        private void runMEM_InstallContextBased(string exe, string args, BackgroundWorker worker, List<string> acceptedIPC = null) {
        }

        private void RunAndTimeMEM_Install(string exe, string args, BackgroundWorker installWorker) {
        }

        private void RunAndTimeMEM_InstallContextBased(string exe, string args, BackgroundWorker installWorker) {
        }

        private void runMEM_Install(string exe, string args, BackgroundWorker worker, List<string> acceptedIPC = null) {
        }
    }
}