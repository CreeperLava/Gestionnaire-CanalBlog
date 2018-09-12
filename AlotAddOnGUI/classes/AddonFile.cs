using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace AlotAddOnGUI.classes {
    public sealed class AddonFile : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public string title { get; set; }
        public string url { get; set; }
        public string dateString { get; set; }
        public int dateInt { get; set; }
        public string imageUrl { get; set; }
        public string imageNom { get; set; }
        public string intro { get; set; }
        public string prepa { get; set; }
        public string ingred { get; set; }
        public string tag { get; set; }

        public string tagFirst {
            get {
                return tag.Split(' ').First();
            }
        }

        public int annee {
            get {
                return dateInt / 10000;
            }
        }


        public string Tooltipname {
            get {
                return intro + "\n" + prepa + "\n" + ingred;
            }
        }
        private string _readystatustext;
        private string _readyiconpath;

        public string ReadyIconPath {
            get {
                return "images/greencheckmark.png";
                // return "images/greycheckmark.png";
                // return "images/orangedownload.png";
            }

            set {
                _readyiconpath = value;
                OnPropertyChanged("ReadyIconPath");
            }
        }

        public string ReadyStatusText {
            get {
                if (_readystatustext != null)
                    return _readystatustext;
                return "";
            }
            set {
                _readystatustext = value;
                OnPropertyChanged("ReadyStatusText");
            }
        }

        private bool m_ready;
        public bool Ready {
            get { return m_ready; }
            set {
                m_ready = value;
                OnPropertyChanged("LeftBlockColor"); //ui update for tihs property
                OnPropertyChanged("ReadyIconPath");
                OnPropertyChanged("Ready");
            }
        }

        private bool _enabled;
        public bool Enabled {
            get { return _enabled; }
            internal set {
                _enabled = value;
                OnPropertyChanged("LeftBlockColor"); // ui update for this property
                OnPropertyChanged("ReadyIconPath"); // ui update for this property
            }
        }

        public Color LeftBlockColor {
            get {
                return Color.FromRgb((byte)0x31, (byte)0xae, (byte)0x90); // green
                // return Color.FromRgb((byte)0x60, (byte)0x60, (byte)0x60); // gray
                // return Color.FromRgb((byte)0xd9, (byte)0x22, (byte)0x44); // red
            }
        }

        private void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            return title;
        }

        internal void SetWorking() {
            ReadyIconPath = "images/workingicon.png";
        }
        internal void SetError() {
            ReadyIconPath = "images/redx_large.png";
        }
        internal bool IsInErrorState() {
            return ReadyIconPath == "images/redx_large.png";
        }
        internal void SetIdle() {
            ReadyIconPath = null;
        }
    }
}