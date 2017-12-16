using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.UI.ViewModel;
using Ookii.Dialogs.Wpf;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for InitialConfigView.xaml
   /// </summary>
   public partial class InitialConfigView {

      private readonly InitialConfigViewModel _viewModel;

      public InitialConfigView(InitialConfigViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
      }

      public delegate void ConfigurationConfirmedEventHandler();

      public event ConfigurationConfirmedEventHandler ConfigurationConfirmed;

      private void BrowseDirClick(object sender, RoutedEventArgs e) {
         Debug.Assert(sender is Button);
         Button browseButton = (Button) sender;

         var dialog = new VistaFolderBrowserDialog();
         if (dialog.ShowDialog(null) ?? false) { // TODO: parent window
            if (browseButton.Tag.Equals("1")) {
               _viewModel.GamePath = dialog.SelectedPath;
            } else {
               _viewModel.ModsPath = dialog.SelectedPath;
            }
         }
      }

      private void OkButton_OnClick(object sender, RoutedEventArgs e) {
         ConfigurationConfirmed?.Invoke();
      }
   }
}
