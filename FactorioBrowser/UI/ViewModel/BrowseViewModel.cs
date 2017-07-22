using System.Collections.Generic;
using System.Collections.Generics;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;

using ItemGraphVertex = FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcItem>;
using ItemGraphEdge = FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcItem>;
using ItemGraph = QuickGraph.BidirectionalGraph<
   FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcItem>,
   FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcItem>>;

using TechnologyGraphVertex = FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraphEdge = FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraph = QuickGraph.BidirectionalGraph<
   FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>,
   FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>>;

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

      public ItemGraph ItemGraph { get; private set; }

      public TechnologyGraph TechnologyGraph { get; private set; }

      public async Task LoadData() {
         IsBusy = true;
         try {
            var unpackedProtos = await Task.Factory.StartNew(LoadAndUnpackData);
            Items.Clear();
            Items.AddRange(unpackedProtos.Items);
            ItemGraph = BuildItemGraph(unpackedProtos.Items, unpackedProtos.Recipes);

            Recipes.Clear();
            Recipes.AddRange(unpackedProtos.Recipes);

            Technologies.Clear();
            Technologies.AddRange(unpackedProtos.Technologies);
            TechnologyGraph = BuildTechnologyGraph(unpackedProtos.Technologies);

         } finally {
            IsBusy = false;
         }
      }

      private ItemGraph BuildItemGraph(IImmutableList<FcItem> items,
         IImmutableList<FcRecipe> recipes) {

         var graph = new ItemGraph(allowParallelEdges: true, vertexCapacity: items.Count);

         IDictionary<string, ItemGraphVertex> vertexByName = new Dictionary<string, ItemGraphVertex>();
         foreach (var item in items) {
            var vertex = new ItemGraphVertex(item);
            vertexByName.Add(item.Name, vertex);
            graph.AddVertex(vertex);
         }

         foreach (var recipe in recipes) {
            if (recipe.Ingredients == null || recipe.Results == null) {
               continue;
            }

            foreach (var ingredient in recipe.Ingredients) {
               foreach (var result in recipe.Results) {
                  ItemGraphVertex ingredientItem;
                  ItemGraphVertex resultItem;

                  if (vertexByName.TryGetValue(ingredient.Item, out ingredientItem)
                     && vertexByName.TryGetValue(result.Item, out resultItem)) {

                     var edge = new ItemGraphEdge(ingredientItem, resultItem);
                     graph.AddEdge(edge);
                  }
               }
            }
         }

         return graph;
      }

      private TechnologyGraph BuildTechnologyGraph(IImmutableList<FcTechnology> technologies) {
         TechnologyGraph graph = new TechnologyGraph(allowParallelEdges: false,
            vertexCapacity: technologies.Count);

         IDictionary<string, TechnologyGraphVertex> vertexByName =
            new Dictionary<string, TechnologyGraphVertex>(technologies.Count);
         foreach (var tech in technologies) {
            var vertex = new TechnologyGraphVertex(tech);
            graph.AddVertex(vertex);
            vertexByName.Add(tech.Name, vertex);
         }

         foreach (var tech in technologies) {
            if (tech.Prerequisites == null) { // TODO : guarantee non-null for collection fields
               continue;
            }

            foreach (var prerequisite in tech.Prerequisites) {
               var techVertex = vertexByName[tech.Name];
               TechnologyGraphVertex prereqVertex;
               if (vertexByName.TryGetValue(prerequisite, out prereqVertex)) {
                  var edge = new TechnologyGraphEdge(prereqVertex, techVertex);
                  graph.AddEdge(edge);
               }
            }
         }

         return graph;
      }

      private FcPrototypes LoadAndUnpackData() {
         var rawData = _modLoader.LoadPrototypes(_modsToLoad, ImmutableDictionary<string, object>.Empty); // TODO
         return _prototypeUnpacker.Unpack(rawData);
      }
   }
}
