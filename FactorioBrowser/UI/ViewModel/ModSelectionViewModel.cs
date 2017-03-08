using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Finder;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModListItem {

      public FcModMetaInfo Info { get; }

      public SortStatus AutoSortStatus { get; }

      public bool Enabled { get; set; }

      public ModListItem(FcModMetaInfo modInfo, SortStatus autoSortStatus) {
         Contract.Requires(modInfo != null);
         Contract.Requires(autoSortStatus != null);

         Info = modInfo;
         AutoSortStatus = autoSortStatus;
         Enabled = autoSortStatus.Successful;
      }
   }

   public sealed class ModSelectionViewModel : BindableBase {

      private readonly IFcModFinder _modFinder;
      private readonly IFcModSorter _modSorter;
      private bool _isBusy;

      public ObservableCollection<ModListItem> ModList { get; }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      public ModSelectionViewModel(IFcModFinder modFinder, IFcModSorter modSorter) {
         _modFinder = modFinder;
         _modSorter = modSorter;
         _isBusy = false;
         ModList = new ObservableCollection<ModListItem>();
      }

      public async void RefreshModList() {
         IsBusy = true;
         try {
            var allMods = await Task.Run(() => FindAndSortMods());
            ModList.Clear();
            ModList.AddRange(allMods);
         } finally {
            IsBusy = false;
         }
      }

      private IEnumerable<ModListItem> FindAndSortMods() {
         return  _modSorter.Sort(_modFinder.FindAll())
            .Select(s => new ModListItem(s.ModInfo, s));
      }
   }
}
