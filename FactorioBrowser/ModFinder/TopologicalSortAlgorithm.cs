using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Priority_Queue;

namespace FactorioBrowser.ModFinder {
   internal sealed class TopologicalSortAlgorithm {
      private readonly IDictionary<string, ICollection<string>> _adjList;

      public TopologicalSortAlgorithm(IDictionary<string, ICollection<string>> adjList) {
         _adjList = adjList;
      }

      private IDictionary<string, NodePriority> GetInitialPriorities(IList<string> nodesInAlphaOrder) {
         IDictionary<string, NodePriority> priorityByNode = new Dictionary<string, NodePriority>();

         for (var i = 0; i < nodesInAlphaOrder.Count; ++i)
            priorityByNode.Add(nodesInAlphaOrder[i], new NodePriority(0, i));

         foreach (var node in nodesInAlphaOrder) {
            foreach (var neighbour in _adjList[node]) {
               priorityByNode[neighbour].InDegree++;
            }
         }

         return priorityByNode;
      }

      public IImmutableList<string> Sort() {
         var nodes = new List<string>(_adjList.Keys);
         nodes.Sort();

         var priorities = GetInitialPriorities(nodes);

         var queue = new SimplePriorityQueue<string, NodePriority>();
         foreach (var np in priorities) queue.Enqueue(np.Key, np.Value);

         var ordered = new List<string>(nodes.Count);
         while (queue.Count > 0) {
            var first = queue.Dequeue();
            if (priorities[first].InDegree > 0) {
               throw new ArgumentException("Cycle in the graph detected.");
            }

            ordered.Add(first);

            foreach (var neighbour in _adjList[first]) {
               var newPriorty = priorities[neighbour];
               newPriorty.InDegree--;
               queue.UpdatePriority(neighbour, newPriorty);
            }
         }

         return ordered.ToImmutableArray();
      }

      private class NodePriority : IComparable<NodePriority> {
         public NodePriority(int inDegree, int alphaOrder) {
            InDegree = inDegree;
            AlphaOrder = alphaOrder;
         }

         public int InDegree { get; set; }
         private int AlphaOrder { get; }

         public int CompareTo(NodePriority other) {
            var inDegreeComparison = InDegree.CompareTo(other.InDegree);
            if (inDegreeComparison != 0) return inDegreeComparison;
            return AlphaOrder.CompareTo(other.AlphaOrder);
         }

         public override string ToString() {
            return $"({InDegree}, {AlphaOrder})";
         }
      }
   }
}
