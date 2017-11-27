using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class FcModSettingValue {

      public FcModSetting Definition { get; }

      public string LocalizedName { get; }

      public string LocalizedDescription { get; }

      public object Value { get; set; }

      public FcModSettingValue(FcModSetting definition, object value,
         string localizedName, string localizedDescription) {

         Definition = definition;
         Value = value;
         LocalizedDescription = localizedDescription;
         LocalizedName = localizedName;
      }
   }

   public sealed class SettingsViewModel : BindableBase {
      private static readonly IImmutableList<string> DefaultLocalePreference = new[] { "en" }.ToImmutableList();

      private readonly IFcModDataLoader _dataLoader;
      private readonly IFcLocalizationLoader _localizationLoader;
      private readonly IImmutableList<FcModFileInfo> _modsToLoad;

      private ILocalizationDirectory _localizationDirectory;
      private bool _isBusy;

      public SettingsViewModel(IFcModDataLoader dataLoader,
         IFcLocalizationLoader localizationLoader, IImmutableList<FcModFileInfo> modsToLoad) {
         _dataLoader = dataLoader;
         _localizationLoader = localizationLoader;
         _modsToLoad = modsToLoad;
         SettingsByMod = new ObservableCollection<IGrouping<string, FcModSettingValue>>();
      }

      public IImmutableDictionary<string, object> GetSettingsValues() {
         return SettingsByMod
            .SelectMany(group => group.Select(sv => new { sv.Definition.Name, sv.Value }))
            .ToImmutableDictionary(sv => sv.Name, sv => sv.Value);
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
            _localizationDirectory = await Task.Factory.StartNew(
               () => _localizationLoader.LoadLocalizationTables(_modsToLoad));
            var settings = await Task.Factory.StartNew(
               () => _dataLoader.LoadSettings(_modsToLoad));
            var groups = settings
               .Where(s => s.SettingType == SettingTypes.Startup)
               .Select(ToSettingValue)
               .GroupBy(s => s.Definition.SourceMod);
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

         string localizedName = _localizationDirectory.GetLocalizedName(
            "mod-setting-name", definition.Name, DefaultLocalePreference);
         string localizedDescription = _localizationDirectory.GetLocalizedName(
            "mod-setting-description", definition.Name, DefaultLocalePreference);

         return new FcModSettingValue(definition, defaultValue,
            localizedName: localizedName ?? definition.Name,
            localizedDescription: localizedDescription);
      }
   }
}
