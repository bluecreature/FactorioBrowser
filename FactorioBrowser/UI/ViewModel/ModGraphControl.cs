using System;
using FactorioBrowser.Mod.Finder;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   internal sealed class ModGraphVertex : VertexBase, IComparable<ModGraphVertex> {
      public FcModMetaInfo ModInfo { get; }

      public ModGraphVertex(FcModMetaInfo modInfo) {
         ModInfo = modInfo;
      }

      public override string ToString() {
         return ModInfo.Name;
      }

      public int CompareTo(ModGraphVertex other) {
         if (ReferenceEquals(this, other)) {
            return 0;

         } else if (ReferenceEquals(null, other)) {
            return 1;

         } else {
            return string.Compare(ModInfo.Name, other.ModInfo.Name, StringComparison.Ordinal);
         }
      }
   }

   internal sealed class ModGraphEdge : EdgeBase<ModGraphVertex> {

      public ModGraphEdge(ModGraphVertex source, ModGraphVertex target, double weight = 1)
         : base(source, target, weight) {
      }
   }

   internal sealed class ModGraphLogic :
      GXLogicCore<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {

   }

   internal sealed class ModGraphControl :
      GraphArea<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {
   }
}
