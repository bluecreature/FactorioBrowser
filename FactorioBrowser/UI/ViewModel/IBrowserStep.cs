using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;

namespace FactorioBrowser.UI.ViewModel {

   public interface INestedScreen : INotifyPropertyChanged {

      object ViewModel { get; }

      bool IsBusy { get; }
   }

   public interface IBrowserStep<TResult> : INestedScreen {

      Task<TResult> Run();
   }

   public interface IBrowserStepsFactory {

      IBrowserStep<GameDirectories> CreateInitialConfigStep();

      IBrowserStep<FcModList> CreateModSelectionStep(GameDirectories gameDirectories);

      IBrowserStep<IImmutableDictionary<string, object>> CreateModSettingsStep(
         ILocalizationDirectory localizationDirectory, FcModList modsToLoad);

      IBrowserStep<object> CreateBrowseStep(ILocalizationDirectory localizationDirectory,
         FcModList modsToLoad, IImmutableDictionary<string, object> settings);
   }
}
