using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModSettingsStep : BindableBase, IBrowserStep<IImmutableDictionary<string, object>> {
      private readonly IFcModDataLoader _dataLoader;
      private readonly ILocalizationDirectory _localizationDirectory;
      private readonly FcModList _modsToLoad;

      private bool _isBusy;
      private object _viewModel;

      public ModSettingsStep(IFcModDataLoader dataLoader,
         ILocalizationDirectory localizationDirectory, FcModList modsToLoad) {
         _dataLoader = dataLoader;
         _localizationDirectory = localizationDirectory;
         _modsToLoad = modsToLoad;
      }

      public async Task<IImmutableDictionary<string, object>> Run() {
         var startupSettings = await LoadStartupSettingDefinitions();
         if (startupSettings.Count > 0) {
            var runTaskSource = new TaskCompletionSource<IImmutableDictionary<string, object>>();
            var viewModel = new SettingsViewModel(_localizationDirectory, startupSettings,
               runTaskSource.SetResult);
            ViewModel = viewModel;
            return await runTaskSource.Task;

         } else {
            return ImmutableDictionary.Create<string, object>();
         }
      }

      private async Task<IList<FcModSetting>> LoadStartupSettingDefinitions() {
         IsBusy = true;
         try {
            var settings = await Task.Factory.StartNew(
               () => _dataLoader.LoadSettings(
                  _modsToLoad.SelectableMods.Select(FcModFileInfo.FromMetaInfo)));
            return settings
               .Where(s => s.SettingType == SettingTypes.Startup)
               .ToList();

         } finally {
            IsBusy = false;
         }
      }

      public object ViewModel {
         get {
            return _viewModel;
         }

         private set {
            UpdateProperty(ref _viewModel, value);
         }
      }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }
   }

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

      private readonly ILocalizationDirectory _localizationDirectory;
      private readonly Action<IImmutableDictionary<string, object>> _submitAction;

      public SettingsViewModel(ILocalizationDirectory localizationDirectory,
         IEnumerable<FcModSetting> settingDefs,
         Action<IImmutableDictionary<string, object>> submitAction) {

         _localizationDirectory = localizationDirectory;
         _submitAction = submitAction;
         SettingsByMod = new ObservableCollection<IGrouping<string, FcModSettingValue>>();
         SettingsByMod.AddRange(settingDefs
            .Select(ToSettingValue)
            .GroupBy(s => s.Definition.SourceMod));
         SubmitCommand = new ActionCommand(Submit);
      }

      public ICommand SubmitCommand { get; }

      public ObservableCollection<IGrouping<string, FcModSettingValue>> SettingsByMod { get; }

      private void Submit() {
         var settings = SettingsByMod
            .SelectMany(group => group.Select(sv => new { sv.Definition.Name, sv.Value }))
            .ToImmutableDictionary(sv => sv.Name, sv => sv.Value);
         _submitAction.Invoke(settings);
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
