using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using Config.Net;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes.Unpacker;
using FactorioBrowser.UI.ViewModel;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace FactorioBrowser.UI {

   public interface IAppSettings {

      [Option(Alias = "game_path")]
      string GamePath { get; set; }

      [Option(Alias = "mods_path")]
      string ModsPath { get; set; }

      [Option(Alias = "use_saved_settings", DefaultValue = false)]
      bool UseSavedSettings { get; set; }

   }

   public static class AppSettingsFactory {

      public static IAppSettings Create() {
         var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         Debug.Assert(exePath != null);
         var cfgPath = Path.Combine(exePath, "config.ini");

         return new ConfigurationBuilder<IAppSettings>()
            .UseIniFile(cfgPath)
            .Build();
      }
   }

   public interface IViewsFactory {

      InitialConfigView CreateInitialConfigView();

      ModSelectionView CreateModSelectionView();

      SettingsView CreateSettingsView(IImmutableList<FcModFileInfo> selectedMods);

      BrowseView CreateBrowseView(IImmutableList<FcModFileInfo> selectedMods,
         IImmutableDictionary<string, object> modSettings);
   }

   public sealed class ComponentContainer {
      private readonly StandardKernel _kernel;

      public ComponentContainer() {
         var krnlConfig = new NinjectSettings {
            LoadExtensions = false,
            InjectNonPublic = true,
         };

         _kernel = new StandardKernel(krnlConfig,
            new ModComponents(),
            new FuncModule()
         );
      }

      public T Get<T>() {
         return _kernel.Get<T>();
      }
   }

   internal sealed class ModComponents : NinjectModule {

      public override void Load() {
         Bind<IAppSettings>().ToMethod((ctx) => AppSettingsFactory.Create()).InSingletonScope();
         Bind<IFcModFinder>().To<DefaultFcModFinder>();
         Bind<IFcModSorter>().To<DefaultFcModSorter>();
         Bind<IFcModDataLoader>().ToMethod(CreateModDataLoader);
         Bind<IFcLocalizationLoader>().To<DefaultLocalizationLoader>();
         Bind<IFcSettingDefsUnpacker>().To<DefaultSettingDefsUnpacker>();
         Bind<IFcPrototypeUnpacker>().To<DefaultPrototypeUnpacker>();
         Bind<IViewModelsFactory>().ToFactory();
         Bind<IViewsFactory>().To<ViewsFactoryImpl>();
      }

      private IFcModDataLoader CreateModDataLoader(IContext ctx) {
         return new DefaultModDataLoader(
            ctx.Kernel.Get<IAppSettings>().GamePath,
            ctx.Kernel.Get<IFcSettingDefsUnpacker>(),
            ctx.Kernel.Get<IFcPrototypeUnpacker>());
      }
   }

   // needs to be public to be visible to the DynamicProxy
   public interface IViewModelsFactory {

      InitialConfigViewModel CreateInitialConfigViewModel();

      ModSelectionViewModel CreateModSelectionViewModel();

      SettingsViewModel CreateSettingsViewModel(IImmutableList<FcModFileInfo> modsToLoad);

      BrowseViewModel CreateBrowseViewModel(IImmutableList<FcModFileInfo> modsToLoad,
         IImmutableDictionary<string, object> modSettings);
   }

   internal sealed class ViewsFactoryImpl : IViewsFactory {

      private readonly IViewModelsFactory _viewModelsFactory;

      public ViewsFactoryImpl(IViewModelsFactory viewModelsFactory) {
         _viewModelsFactory = viewModelsFactory;
      }

      public InitialConfigView CreateInitialConfigView() {
         return new InitialConfigView(_viewModelsFactory.CreateInitialConfigViewModel());
      }

      public ModSelectionView CreateModSelectionView() {
         return new ModSelectionView(_viewModelsFactory.CreateModSelectionViewModel());
      }

      public SettingsView CreateSettingsView(IImmutableList<FcModFileInfo> selectedMods) {
         return new SettingsView(_viewModelsFactory.CreateSettingsViewModel(selectedMods));
      }

      public BrowseView CreateBrowseView(IImmutableList<FcModFileInfo> selectedMods,
         IImmutableDictionary<string, object> modSettings) {
         return new BrowseView(_viewModelsFactory.CreateBrowseViewModel(selectedMods, modSettings));
      }
   }
}
