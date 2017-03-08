using System.Windows;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   public sealed partial class App : Application {

      public App() {
         QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
         QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
      }

      protected override void OnStartup(StartupEventArgs e) {
         base.OnStartup(e);
         AppSettings settings = new AppSettings();
         if (settings.UseSavedSettings) {
            ShowMainWindow(settings);
         } else {
            AskForInitialConfiguration(settings);
         }
      }

      private void AskForInitialConfiguration(AppSettings settings) {
         ShutdownMode = ShutdownMode.OnExplicitShutdown;

         Window configWnd = new InitialConfigWnd(new InitialConfigViewModel(settings));
         if (configWnd.ShowDialog() == true) {
            ShowMainWindow(settings);
            ShutdownMode = ShutdownMode.OnLastWindowClose;
         } else {
            Shutdown(0);
         }
      }

      private void ShowMainWindow(AppSettings settings) {
         var container = new ComponentContainer(settings);
         var window = container.Get<ModSelectionWnd>();
         window.Show();
      }
   }
}
