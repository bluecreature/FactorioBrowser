using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for ModSelectionView.xaml
   /// </summary>
   public partial class ModSelectionView : IDisposable {
      private readonly ModSelectionViewModel _viewModel;

      internal ModSelectionView(ModSelectionViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
         ModGraph.LogicCore = InitModGraphLogic();
         ModGraph.SetVerticesDrag(true);
      }

      public void Dispose() {
         ModGraph?.Dispose();
      }

      internal delegate void SelectionConfirmedEventHandler(IEnumerable<FcModFileInfo> selectedMods);

      internal event SelectionConfirmedEventHandler SelectionConfirmed;

      public async Task Refresh() {
         ModGraph.ClearLayout();
         await _viewModel.RefreshModList();
         ModGraph.GenerateGraph(_viewModel.DependencyGraph);
         ModGraphZoom.ZoomToFill();
      }

      private async void RefreshModList_Click(object sender, RoutedEventArgs e) {
         await Refresh();
      }

      private void Next_Click(object sender, RoutedEventArgs e) {
         var selectedMods = _viewModel.ModList
            .Where(i => i.Enabled)
            .Select(i => FcModFileInfo.FromMetaInfo(i.Info))
            .ToList();
         SelectionConfirmed?.Invoke(selectedMods);
      }

      private ModGraphLogic InitModGraphLogic() {
         var logic = new ModGraphLogic();
         var layoutAlgo = LayoutAlgorithmTypeEnum.BoundedFR;
         var algoParams = (BoundedFRLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         algoParams.CoolingFunction = FRCoolingFunction.Linear;
         logic.DefaultLayoutAlgorithm = layoutAlgo;
         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 60;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 20;

         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private void ModGraph_OnVertexSelected(object sender, VertexSelectedEventArgs args) {
         ModGraphVertex vertex = (ModGraphVertex) args.VertexControl.Vertex;

         foreach (var modListItem in _viewModel.ModList) {
            if (modListItem != vertex.Item) {
               modListItem.Selected = false;
            }
         }

         vertex.Item.Selected = true;
         ModListView.ScrollIntoView(vertex.Item);
      }

      private void ModSelectionView_OnLoaded(object sender, RoutedEventArgs e) {
#pragma warning disable 4014
         Refresh();
#pragma warning restore 4014
      }
   }
}
