using System.Collections.Generic;
using System.Windows;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;

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
      }

      private TechnologyGraphLogic CreateTechGraphLogic() {
         var logic = new TechnologyGraphLogic();
         var layoutAlgo = LayoutAlgorithmTypeEnum.EfficientSugiyama;
         logic.DefaultLayoutAlgorithm = layoutAlgo;

         var layoutParams = (EfficientSugiyamaLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         layoutParams.Direction = LayoutDirection.TopToBottom;
         layoutParams.LayerDistance = 50;
         layoutParams.VertexDistance = 20;
         layoutParams.EdgeRouting = SugiyamaEdgeRoutings.Traditional;
         layoutParams.PositionMode = 2;
         layoutParams.MinimizeEdgeLength = false;
         layoutParams.OptimizeWidth = true;
         logic.DefaultLayoutAlgorithmParams = layoutParams;

         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 60;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 20;

         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
         logic.EdgeCurvingEnabled = false;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private async void BrowseView_OnLoaded(object sender, RoutedEventArgs e) {
         await _viewModel.LoadData();
         TechGraph.GenerateGraph(_viewModel.TechnologyGraph);
         TechGraphZoom.ZoomToFill();
      }
   }
}
