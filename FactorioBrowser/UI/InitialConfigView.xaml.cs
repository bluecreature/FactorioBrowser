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

      private InitialConfigViewModel GetViewModel() {
         var viewModel = DataContext as InitialConfigViewModel;
         Debug.Assert(viewModel != null);
         return viewModel;
      }

      public InitialConfigView() {
         InitializeComponent();
      }

      private void BrowseDirClick(object sender, RoutedEventArgs e) {
         Debug.Assert(sender is Button);
         Button browseButton = (Button) sender;

         var dialog = new VistaFolderBrowserDialog();
         if (dialog.ShowDialog(null) ?? false) { // TODO: parent window
            if (browseButton.Tag.Equals("1")) {
               GetViewModel().GamePath = dialog.SelectedPath;
            } else {
               GetViewModel().ModsPath = dialog.SelectedPath;
            }
         }
      }
   }
}
