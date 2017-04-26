using System.Collections.Generic;
using System.Diagnostics;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   public interface IBrowseViewFactory {
      BrowseView CreateBrowseView(IEnumerable<FcModFileInfo> selectedMods);
   }

   /// <summary>
   /// Interaction logic for BrowseWindow.xaml
   /// </summary>
   public partial class BrowseView {

      private readonly BrowseViewModel _viewModel;

      internal BrowseView(IBrowseViewModelFactory viewModelFactory,
         IEnumerable<FcModFileInfo> selectedMods) {

         _viewModel = viewModelFactory.Create(selectedMods);
         InitializeComponent();
         DataContext = _viewModel;
      }

      public void Refresh() {

      }
   }
}
