﻿using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Finder;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModListItem {

      public FcModMetaInfo Info { get; }

      public SortStatus SortStatus { get; }

      public bool Enabled { get; set; }

      public ModListItem(FcModMetaInfo modInfo, SortStatus sortStatus) {
         Contract.Requires(modInfo != null);
         Contract.Requires(sortStatus != null);

         Info = modInfo;
         SortStatus = sortStatus;
         Enabled = sortStatus.Successful;
      }
   }

   internal sealed class ModSelectionViewModel : BindableBase {

      private readonly IFcModFinder _modFinder;
      private readonly IFcModSorter _modSorter;
      private bool _isBusy;

      public ObservableCollection<ModListItem> ModList { get; }

      public BidirectionalGraph<ModGraphVertex, ModGraphEdge> DependencyGraph { get; private set; }

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
         DependencyGraph = null;
      }

      public async Task RefreshModList() {
         IsBusy = true;
         try {
            ModList.Clear();
            var allMods = await Task.Factory.StartNew(FindAndSortMods);
            ModList.AddRange(allMods.Select(m => new ModListItem(m.ModInfo, m)));
            DependencyGraph = BuildDependencyGraph(allMods);
         } finally {
            IsBusy = false;
         }
      }

      private IImmutableList<SortStatus> FindAndSortMods() {
         return _modSorter.Sort(_modFinder.FindAll());
      }

      private BidirectionalGraph<ModGraphVertex, ModGraphEdge> BuildDependencyGraph(
         IImmutableList<SortStatus> modList) {

         BidirectionalGraph<ModGraphVertex, ModGraphEdge> graph =
            new BidirectionalGraph<ModGraphVertex, ModGraphEdge>(allowParallelEdges: false);

         IDictionary<string, ModGraphVertex> vertexByName = new Dictionary<string, ModGraphVertex>(modList.Count);
         foreach (var mod in modList) {
            if (mod.Successful) {
               var vertex = new ModGraphVertex(mod.ModInfo);
               vertexByName[mod.ModInfo.Name] = vertex;
               graph.AddVertex(vertex);
            }
         }

         foreach (var mod in modList) {
            foreach (var dependency in mod.ModInfo.Dependencies) {
               ModGraphVertex depVertex;
               if (vertexByName.TryGetValue(dependency.ModName, out depVertex)) {
                  graph.AddEdge(new ModGraphEdge(vertexByName[mod.ModInfo.Name], depVertex));
               }
            }
         }

         return graph;
      }
   }
}
