using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class BrowseViewModel : BindableBase {

      private readonly IFcModDataLoader _modLoader;
      private readonly IFcPrototypeUnpacker _prototypeUnpacker;
      private readonly IEnumerable<FcModFileInfo> _modsToLoad;

      private bool _isBusy;

      public BrowseViewModel(IFcModDataLoader modLoader, IFcPrototypeUnpacker prototypeUnpacker,
         IEnumerable<FcModFileInfo> modsToLoad) {

         Debug.Assert(modsToLoad != null);
         _modLoader = modLoader;
         _prototypeUnpacker = prototypeUnpacker;
         _modsToLoad = modsToLoad;
         _isBusy = false;
         Items = new ObservableCollection<FcItem>();
         Recipes = new ObservableCollection<FcRecipe>();
         Technologies = new ObservableCollection<FcTechnology>();
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

      public ObservableCollection<FcRecipe> Recipes { get; }

      public ObservableCollection<FcTechnology> Technologies { get; }

      public async void LoadData() {
         IsBusy = true;
         try {
            var unpackedProtos = await Task.Factory.StartNew(LoadAndUnpackData);
            Items.Clear();
            Items.AddRange(unpackedProtos.Items);

            Recipes.Clear();
            Recipes.AddRange(unpackedProtos.Recipes);

            Technologies.Clear();
            Technologies.AddRange(unpackedProtos.Technologies);

         } finally {
            IsBusy = false;
         }
      }

      private FcPrototypes LoadAndUnpackData() {
         var rawData = _modLoader.LoadRawData(_modsToLoad);
         return _prototypeUnpacker.Unpack(rawData);
      }
   }
}
