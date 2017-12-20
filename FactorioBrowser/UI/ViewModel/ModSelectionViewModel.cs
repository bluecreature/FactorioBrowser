using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FactorioBrowser.Mod.Finder;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModSelectionStep : BindableBase, IBrowserStep<FcModList> {
      private readonly IFcModFinder _modFinder;
      private readonly IFcModSorter _modSorter;
      private readonly GameDirectories _gameDirectories;

      private bool _isBusy;
      private object _viewModel;

      private sealed class DiscoveryResult {

         public DiscoveryResult(FcModMetaInfo coreData,
            IImmutableList<SortStatus> selectableMods) {
            CoreData = coreData;
            SelectableMods = selectableMods;
         }

         public FcModMetaInfo CoreData { get; }

         public IImmutableList<SortStatus> SelectableMods { get; }
      }

      public ModSelectionStep(IFcModFinder modFinder, IFcModSorter modSorter,
         GameDirectories gameDirectories) {
         _modFinder = modFinder;
         _modSorter = modSorter;
         _gameDirectories = gameDirectories;
         _isBusy = false;
         _viewModel = null;
      }

      public async Task<FcModList> Run() {
         var discovery = await Refresh();

         if (discovery.SelectableMods.Count > 1) {
            Func<Task<IImmutableList<SortStatus>>> refreshFuncion = () => {
               return Refresh().ContinueWith(t => t.Result.SelectableMods);
            };

            TaskCompletionSource<FcModList> runTaskSource = new TaskCompletionSource<FcModList>();
            Action<IImmutableList<FcModMetaInfo>> submitAction = selectedMods => {
               runTaskSource.SetResult(new FcModList(discovery.CoreData, selectedMods));
            };

            var viewModel = new ModSelectionViewModel(discovery.SelectableMods, refreshFuncion,
               submitAction);
            ViewModel = viewModel;
            return await runTaskSource.Task;

         } else {
            return new FcModList(discovery.CoreData,
               discovery.SelectableMods.Select(s => s.ModInfo).ToImmutableList());
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

         set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      private async Task<DiscoveryResult> Refresh() {
         IsBusy = true;
         try {
            return await Task.Factory.StartNew(DiscoverAndSort);
         } finally {
            IsBusy = false;
         }
      }

      private DiscoveryResult DiscoverAndSort() {
         var fcMods = _modFinder.FindAll(_gameDirectories.GamePath, _gameDirectories.ModsPath);
         var coreMod = fcMods.CoreMod;
         var sortedSelectableMods = _modSorter.Sort(fcMods.SelectableMods);
         return new DiscoveryResult(coreMod, sortedSelectableMods);
      }
   }

   public sealed class ModListItem : BindableBase {
      private bool _enabled;
      private bool _selected;

      public FcModMetaInfo Info { get; }

      public SortStatus SortStatus { get; }

      public bool Selectable { get; }

      public bool Selected {
         get {
            return _selected;
         }

         set {
            UpdateProperty(ref _selected, value);
         }
      }

      public bool Enabled {
         get {
            return _enabled;
         }

         set {
            UpdateProperty(ref _enabled, value);
         }
      }

      public ModListItem(SortStatus sortStatus) {
         Debug.Assert(sortStatus != null);

         Info = sortStatus.ModInfo;
         SortStatus = sortStatus;
         Enabled = sortStatus.Successful;
         Selectable = sortStatus.Successful;
      }
   }

   public sealed class ModSelectionViewModel : BindableBase {
      private readonly Func<Task<IImmutableList<SortStatus>>> _refreshFunction;
      private readonly Action<IImmutableList<FcModMetaInfo>> _submitAction;


      // TODO: unify the first two parameters
      public ModSelectionViewModel(IImmutableList<SortStatus> selectableMods,
         Func<Task<IImmutableList<SortStatus>>> refreshFunction,
         Action<IImmutableList<FcModMetaInfo>> submitAction) {

         _refreshFunction = refreshFunction;
         _submitAction = submitAction;

         RefreshCommand = new ActionCommand(Refresh);
         SubmitCommand = new ActionCommand(Submit);
         ModList = new ObservableCollection<ModListItem>();
         ModList.AddRange(selectableMods.Select(s => new ModListItem(s)));
         DependencyGraph = BuildDependencyGraph(ModList);
      }

      public ICommand RefreshCommand { get; }

      public ICommand SubmitCommand { get; }

      public ObservableCollection<ModListItem> ModList { get; }

      public BidirectionalGraph<ModGraphVertex, ModGraphEdge> DependencyGraph { get; private set; }

      private async void Refresh() {
         var refreshed = await _refreshFunction.Invoke();
         ModList.Clear();
         ModList.AddRange(refreshed.Select(s => new ModListItem(s)));
         DependencyGraph = BuildDependencyGraph(ModList);
      }

      private void Submit() {
         var selectedMods = ModList
            .Where(i => i.Enabled)
            .Select(i => i.SortStatus.ModInfo)
            .ToImmutableList();
         _submitAction.Invoke(selectedMods);
      }

      private BidirectionalGraph<ModGraphVertex, ModGraphEdge> BuildDependencyGraph(
         IList<ModListItem> modList) {

         BidirectionalGraph<ModGraphVertex, ModGraphEdge> graph =
            new BidirectionalGraph<ModGraphVertex, ModGraphEdge>(allowParallelEdges: false);

         IDictionary<string, ModGraphVertex> vertexByName = new Dictionary<string, ModGraphVertex>(modList.Count);
         foreach (var mod in modList) {
            if (mod.SortStatus.Successful) {
               var vertex = new ModGraphVertex(mod);
               vertexByName[mod.Info.Name] = vertex;
               graph.AddVertex(vertex);
            }
         }

         foreach (var mod in modList) {
            foreach (var dependency in mod.Info.Dependencies) {
               ModGraphVertex depVertex;
               if (vertexByName.TryGetValue(dependency.ModName, out depVertex)) {
                  graph.AddEdge(
                     new ModGraphEdge(vertexByName[mod.Info.Name], depVertex, dependency.Optional));
               }
            }
         }

         return graph;
      }
   }
}
