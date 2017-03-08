using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.UI.ViewModel;
using Ookii.Dialogs.Wpf;

namespace FactorioBrowser.UI {

   /// <summary>
   ///    Interaction logic for InitialConfigWnd.xaml
   /// </summary>
   public partial class InitialConfigWnd : Window {

      private readonly InitialConfigViewModel _viewModel;

      public InitialConfigWnd(InitialConfigViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
      }

      private void BrowseDirClick(object sender, RoutedEventArgs e) {
         Debug.Assert(sender is Button);
         Button browseButton = (Button) sender;

         var dialog = new VistaFolderBrowserDialog();
         if (dialog.ShowDialog(this) ?? false) {
            if (browseButton.Tag.Equals("1")) {
               _viewModel.GamePath = dialog.SelectedPath;
            } else {
               _viewModel.UserDataPath = dialog.SelectedPath;
            }
         }
      }

      private void SubmitClick(object sender, RoutedEventArgs e) {
         // TODO : validate settings
         DialogResult = true;
         Close();
      }

      private void CancelClick(object sender, RoutedEventArgs e) {
         DialogResult = false;
         Close();
      }
   }
}
