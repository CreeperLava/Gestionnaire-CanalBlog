using AlotAddOnGUI.classes;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
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

namespace AlotAddOnGUI {
    /// <summary>
    /// Interaction logic for ModConfigurationDialog.xaml
    /// </summary>
    public partial class ModConfigurationDialog : CustomDialog {
        private MainWindow mainWindowRef;

        public ModConfigurationDialog(AddonFile af, MainWindow mainWindow) {
            InitializeComponent();
            mainWindowRef = mainWindow;
            DataContext = af;
        }


        private async void Close_Dialog_Click(object sender, RoutedEventArgs e) {
            await mainWindowRef.HideMetroDialogAsync(this);
        }

        private void Combobox_DropdownClosed(object sender, EventArgs e) {
            if (sender is ComboBox) {
                ComboBox cb = (ComboBox)sender;
                ChoiceFile choisefile = (ChoiceFile)cb.DataContext;
                choisefile.SelectedIndex = cb.SelectedIndex;
            }
        }

        private void Comparisons_Click(object sender, RoutedEventArgs e) {
        }
    }
}