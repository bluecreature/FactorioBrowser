using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace FactorioBrowser.UI {

   /// <summary>
   ///    Interaction logic for InitialConfigWnd.xaml
   /// </summary>
   public partial class InitialConfigWnd : Window {
      public InitialConfigWnd() {
         InitializeComponent();
      }

      private void BrowseDirClick(object sender, RoutedEventArgs e) {
         Debug.Assert(sender is Button);
         Button browseButton = (Button) sender;

         TextBox target = browseButton.Tag.Equals("1") ? GameHomeDirEdit : GameDataDirEdit;
         var dialog = new VistaFolderBrowserDialog();
         if (dialog.ShowDialog(this) ?? false) {
            target.Text = dialog.SelectedPath;
         }
      }

      private void SubmitClick(object sender, RoutedEventArgs e) {
         new ModSelectionWnd().Show();
         Close();
      }
   }
}
