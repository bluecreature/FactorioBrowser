using System;
using FactorioBrowser.Prototypes;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class StructureGraphVertex<TCategory> : VertexBase,
      IComparable<StructureGraphVertex<TCategory>> where TCategory : FcDataStructure {

      public StructureGraphVertex(TCategory data) {
         Data = data;
      }

      public TCategory Data { get; }

      public int CompareTo(StructureGraphVertex<TCategory> other) {
         if (ReferenceEquals(this, other)) {
            return 0;

         } else if (ReferenceEquals(null, other)) {
            return 1;

         } else {
            return string.Compare(this.Data.Name, other.Data.Name, StringComparison.Ordinal);
         }
      }
   }

   public sealed class StructureGraphEdge<TCategory> : EdgeBase<StructureGraphVertex<TCategory>>
      where TCategory : FcDataStructure {

      public StructureGraphEdge(StructureGraphVertex<TCategory> source, StructureGraphVertex<TCategory> target) :
         base(source, target, 1) {
      }
   }

   public sealed class ItemGraphLogic : GXLogicCore<
      StructureGraphVertex<FcItem>,
      StructureGraphEdge<FcItem>,
      BidirectionalGraph<StructureGraphVertex<FcItem>, StructureGraphEdge<FcItem>>> {
   }

   public sealed class ItemGraphControl : GraphArea<
      StructureGraphVertex<FcItem>,
      StructureGraphEdge<FcItem>,
      BidirectionalGraph<StructureGraphVertex<FcItem>, StructureGraphEdge<FcItem>>> {
   }

   public sealed class TechnologyGraphLogic : GXLogicCore<
      StructureGraphVertex<FcTechnology>,
      StructureGraphEdge<FcTechnology>,
      BidirectionalGraph<StructureGraphVertex<FcTechnology>, StructureGraphEdge<FcTechnology>>> {
   }

   public sealed class TechnologyGraphControl : GraphArea<
      StructureGraphVertex<FcTechnology>,
      StructureGraphEdge<FcTechnology>,
      BidirectionalGraph<StructureGraphVertex<FcTechnology>, StructureGraphEdge<FcTechnology>>> {
   }
}
