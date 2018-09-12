using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CanalBlogManager {
    public partial class App : Application {
        private static bool POST_STARTUP = false;

        [STAThread]
        public static void Main() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            try {
                var application = new App();
                application.InitializeComponent();
                application.Run();
            } catch (Exception e) {
                OnFatalCrash(e);
                throw;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var probingPath = AppDomain.CurrentDomain.BaseDirectory + @"data\lib";
            var assyName = new AssemblyName(args.Name);

            var newPath = Path.Combine(probingPath, assyName.Name);
            if (!newPath.EndsWith(".dll")) {
                newPath = newPath + ".dll";
            }
            if (File.Exists(newPath)) {
                var assy = Assembly.LoadFile(newPath);
                return assy;
            }
            return null;
        }

        public App() : base() {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(UIElement),
            new FrameworkPropertyMetadata(15000));
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
            typeof(Control),
            new FrameworkPropertyMetadata(true));
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            string errorMessage = string.Format("This exception has caused the crash:");
            string st = FlattenException(e.Exception);
        }

        public static void OnFatalCrash(Exception e) {
            if (!POST_STARTUP) {
                string errorMessage = string.Format("Serious fatal startup crash:\n" + FlattenException(e));
                File.WriteAllText("FATAL_STARTUP_CRASH.txt", errorMessage);
            }
        }

        public static string FlattenException(Exception exception) {
            var stringBuilder = new StringBuilder();

            while (exception != null) {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        private void Application_Exit(object sender, ExitEventArgs e) { }
    }
}