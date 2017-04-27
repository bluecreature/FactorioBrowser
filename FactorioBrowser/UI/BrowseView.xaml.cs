using System.Collections.Generic;
using System.Windows;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   public interface IBrowseViewFactory {

      BrowseView Create(IEnumerable<FcModFileInfo> selectedMods);
   }

   /// <summary>
   /// Interaction logic for BrowseWindow.xaml
   /// </summary>
   public partial class BrowseView {

      private readonly BrowseViewModel _viewModel;

      internal BrowseView(BrowseViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
      }

      private void BrowseView_OnLoaded(object sender, RoutedEventArgs e) {
         _viewModel.LoadData();
      }
   }
}
