using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.UI.ViewModel {

   public interface IBrowseViewModelFactory {
      BrowseViewModel Create(IEnumerable<FcModFileInfo> modsToLoad);
   }

   public sealed class BrowseViewModel : BindableBase {

      private readonly IFcModDataLoader _modLoader;
      private readonly IEnumerable<FcModFileInfo> _modsToLoad;

      private bool _isBusy;

      public BrowseViewModel(IFcModDataLoader modLoader, IEnumerable<FcModFileInfo> modsToLoad) {
         Debug.Assert(modsToLoad != null);
         _modLoader = modLoader;
         _modsToLoad = modsToLoad;
         _isBusy = false;
         Items = new ObservableCollection<FcItem>();
      }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      public ObservableCollection<FcItem> Items { get; }

      public async void LoadData() {
         IsBusy = true;
         try {
            var items = await Task.Factory.StartNew(LoadAndUnpackData);
            Items.Clear();
            Items.AddRange(items);
         } finally {
            IsBusy = false;
         }
      }

      private IList<FcItem> LoadAndUnpackData() {
         var rawData = _modLoader.LoadRawData(_modsToLoad);
         var unpacked = new UnpackerDispatcher().
            Unpack<IDictionary<string, IDictionary<string, FcDataStructure>>>(rawData.Self(), "data.raw"); // TODO : inject
         IList<FcItem> items = new List<FcItem>();
         foreach (var category in unpacked.Values) {
            foreach (var structure in category.Values) {
               var item = structure as FcItem;
               if (item != null) {
                  items.Add(item);
               }
            }
         }

         return items;
      }
   }
}
