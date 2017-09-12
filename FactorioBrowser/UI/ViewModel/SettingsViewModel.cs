using System;
using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class FcModSettingValue {

      public FcModSetting Definition { get; }

      public object Value { get; set; }

      public FcModSettingValue(FcModSetting definition, object value) {
         Definition = definition;
         Value = value;
      }
   }

   public sealed class SettingsViewModel : BindableBase {
      private readonly IFcModDataLoader _dataLoader;
      private readonly IList<FcModFileInfo> _modsToLoad;

      private bool _isBusy;

      public SettingsViewModel(IFcModDataLoader dataLoader,
         IEnumerable<FcModFileInfo> modsToLoad) {
         _dataLoader = dataLoader;
         _modsToLoad = new List<FcModFileInfo>(modsToLoad);
         SettingsByMod = new ObservableCollection<IGrouping<string, FcModSettingValue>>();
      }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      public ObservableCollection<IGrouping<string, FcModSettingValue>> SettingsByMod { get; }

      public async Task LoadData() {
         IsBusy = true;
         try {
            var settings = await Task.Factory.StartNew(
               () => _dataLoader.LoadSettings(_modsToLoad));
            var groups = settings
               .Where(s => s.SettingType == SettingTypes.Startup)
               .Select(ToSettingValue)
               .GroupBy(s => s.Definition.History[0]);
            SettingsByMod.Clear();
            SettingsByMod.AddRange(groups);
         } finally {
            IsBusy = false;
         }
      }

      private FcModSettingValue ToSettingValue(FcModSetting definition) {
         Debug.Assert(definition != null);

         object defaultValue;
         if (definition is FcBooleanSetting) {
            defaultValue = ((FcBooleanSetting) definition).DefaultValue;

         } else if (definition is FcIntegerSetting) {
            defaultValue = ((FcIntegerSetting) definition).DefaultValue;

         } else if (definition is FcDoubleSetting) {
            defaultValue = ((FcDoubleSetting) definition).DefaultValue;

         } else if (definition is FcStringSetting) {
            defaultValue = ((FcStringSetting) definition).DefaultValue;

         } else {
            throw new NotImplementedException(
               $"Internal error/incomplete implementation for FcModSetting of type {definition.GetType()}");
         }

         return new FcModSettingValue(definition, defaultValue);
      }
   }
}
