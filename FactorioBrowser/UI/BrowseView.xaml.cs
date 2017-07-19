using System.Collections.Generic;
using System.Windows;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.UI.ViewModel;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI {

   public interface IBrowseViewFactory {

      BrowseView Create(IEnumerable<FcModFileInfo> selectedMods);
   }

   /// <summary>
   /// Interaction logic for BrowseWindow.xaml
   /// </summary>
   public partial class BrowseView {

      private readonly BrowseViewModel _viewModel;

      internal BrowseView(BrowseViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;

         TechGraph.LogicCore = CreateTechGraphLogic();
         TechGraph.SetVerticesDrag(true);

         ItemGraph.LogicCore = CreateItemRecipeGraphLogic<ItemGraphLogic, FcItem>();
         ItemGraph.SetVerticesDrag(true);

         RecipeGraph.LogicCore = CreateItemRecipeGraphLogic<RecipeGraphLogic, FcRecipe>();
         RecipeGraph.SetVerticesDrag(true);
      }

      private TechnologyGraphLogic CreateTechGraphLogic() {
         var logic = new TechnologyGraphLogic();
         var layoutAlgo = LayoutAlgorithmTypeEnum.EfficientSugiyama;
         logic.DefaultLayoutAlgorithm = layoutAlgo;

         var layoutParams = (EfficientSugiyamaLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         layoutParams.Direction = LayoutDirection.TopToBottom;
         layoutParams.LayerDistance = 60;
         layoutParams.VertexDistance = 20;
         layoutParams.EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
         layoutParams.MinimizeEdgeLength = true;
         layoutParams.OptimizeWidth = true;
         logic.DefaultLayoutAlgorithmParams = layoutParams;

         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 20;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 60;

         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
         logic.EdgeCurvingEnabled = false;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private TLogic CreateItemRecipeGraphLogic<TLogic, TCategory>()
         where TLogic : GXLogicCore<
            StructureGraphVertex<TCategory>,
            StructureGraphEdge<TCategory>,
            BidirectionalGraph<StructureGraphVertex<TCategory>, StructureGraphEdge<TCategory>>>, new()
         where TCategory : FcPrototype {

         var logic = new TLogic();

         var layoutAlgo = LayoutAlgorithmTypeEnum.ISOM;
         logic.DefaultLayoutAlgorithm = layoutAlgo;

         var layoutParams = (ISOMLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         layoutParams.MinRadius = 10;
         layoutParams.InitialRadius = 20;
         logic.DefaultLayoutAlgorithmParams = layoutParams;

         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 60;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 20;

         EdgeRoutingAlgorithmTypeEnum edgeRouting = EdgeRoutingAlgorithmTypeEnum.PathFinder;
         logic.DefaultEdgeRoutingAlgorithm = edgeRouting;

         var edgeRoutingParams = (PathFinderEdgeRoutingParameters) logic.AlgorithmFactory.CreateEdgeRoutingParameters(edgeRouting);
         edgeRoutingParams.PathFinderAlgorithm = PathFindAlgorithm.EuclideanNoSQR;
         edgeRoutingParams.UseDiagonals = true;
         edgeRoutingParams.UseHeavyDiagonals = true;
         edgeRoutingParams.SearchTriesLimit = 100;

         logic.DefaultEdgeRoutingAlgorithmParams = edgeRoutingParams;

         logic.EdgeCurvingEnabled = false;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private async void BrowseView_OnLoaded(object sender, RoutedEventArgs e) {
         await _viewModel.LoadData();
         TechGraph.GenerateGraph(_viewModel.TechnologyGraph);
         TechGraphZoom.ZoomToFill();

         ItemGraph.GenerateGraph(_viewModel.ItemGraph);
         ItemGraphZoom.ZoomToFill();
      }
   }
}
