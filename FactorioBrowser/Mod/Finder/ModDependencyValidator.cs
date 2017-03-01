using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace FactorioBrowser.Mod.Finder {

   internal sealed class ModDependencyValidator {
      private sealed class ValidationState {
         public readonly LinkedList<string> WalkPath = new LinkedList<string>();
         public readonly IDictionary<string, SortStatus> CompleteNodes = new Dictionary<string, SortStatus>();
         public readonly IDictionary<string, IList<string>> CyclesByNode = new Dictionary<string, IList<string>>();
      }

      private readonly IDictionary<string, FcModMetaInfo> _modsByName;
      public ModDependencyValidator(IDictionary<string, FcModMetaInfo> modsByName) {
         _modsByName = modsByName;
      }

      public IDictionary<string, SortStatus> Validate() {
         var state = new ValidationState();
         foreach (var modName in _modsByName.Keys) {
            var status = Validate(modName, state);
            Debug.Assert(status != null, "expected validation status != null for found mods");
         }

         Debug.Assert(state.CompleteNodes.Count == _modsByName.Count,
            "expected ValidatedMods.Count == AllMods.Count");

         return state.CompleteNodes;
      }

      private SortStatus Validate(string mod, ValidationState state) {
         SortStatus status;
         if (state.CompleteNodes.TryGetValue(mod, out status)) {
            return status;
         }

         FcModMetaInfo modInfo;
         if (!_modsByName.TryGetValue(mod, out modInfo)) {
            return null;
         }

         if (DetectCycle(mod, state)) {
            return null;
         }

         IList<ISortProblem> problems = new List<ISortProblem>();
         foreach (var dep in modInfo.Dependencies) {
            var depStatus = Validate(dep.ModName, state);
            if (depStatus == null) {
               // Not found or unresolvable
               if (!dep.Optional) {
                  problems.Add(new MissingDependency(dep.ModName));
               }

            } else if (!VersionMatches(dep.RequiredVersion, depStatus.ModInfo.Version)) {
               // Found, but with wrong version. Error even if dependency is optional.
               problems.Add(new DependencyVersionMismatch(
                  dep.ModName, dep.RequiredVersion, depStatus.ModInfo.Version));
            }
         }

         IList<string> foundCycle;
         if (state.CyclesByNode.TryGetValue(mod, out foundCycle)) {
            problems.Add(new CyclicDependency(foundCycle.ToImmutableArray()));
         }

         status = new SortStatus(modInfo, problems);
         state.CompleteNodes.Add(mod, status);
         return status;
      }

      private bool DetectCycle(string currentNode, ValidationState state) {
         LinkedListNode<string> onCurrentPath = state.WalkPath.Find(currentNode);
         if (onCurrentPath == null) {
            return false;
         }

         // currentNode is on the active path => cycle detected
         var cycle = new List<string>();
         while (onCurrentPath != null) {
            cycle.Add(onCurrentPath.Value);
            onCurrentPath = onCurrentPath.Next;
         }

         cycle.Add(currentNode);
         state.CyclesByNode.Add(currentNode, cycle);
         return true;
      }

      private bool VersionMatches(FcVersionRequirement requiredVersion, FcVersion actualVersion) {

         if (requiredVersion == null) {
            return true;

         } else {
            int cmp = actualVersion.CompareTo(requiredVersion.Version);
            switch (requiredVersion.Range) {
               case FcVersionRange.After:
                  return cmp > 0;

               case FcVersionRange.AtLeast:
                  return cmp >= 0;

               case FcVersionRange.Exactly:
                  return cmp == 0;

               default:
                  throw new NotImplementedException(
                     $"Incomplete {typeof(FcVersionRange).Name} " +
                     $"implementation for {requiredVersion.Range}");

            }
         }
      }
   }
}
