using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactorioBrowser.Mod.Finder {

   public interface ISortProblem { }

   public sealed class MissingDependency : ISortProblem {
      public string Dependency { get; }

      public MissingDependency(string dependency) {
         Dependency = dependency;
      }
   }

   public sealed class DependencyVersionMismatch : ISortProblem {
      public string Dependency { get; }

      public FcVersionRequirement ExpectedVersion { get; }

      public FcVersion ActualVersion { get; }

      public DependencyVersionMismatch(string dependency, FcVersionRequirement expected,
         FcVersion actual) {

         Dependency = dependency;
         ExpectedVersion = expected;
         ActualVersion = actual;
      }
   }

   public sealed class CyclicDependency : ISortProblem {
      public IImmutableList<string> Cycle { get; }

      public CyclicDependency(IImmutableList<string> cycle) {
         Cycle = cycle;
      }
   }


   public sealed class SortStatus {
      public FcModMetaInfo ModInfo { get; }

      public bool Successful { get; }
      public IImmutableList<ISortProblem> Problems { get; }

      public SortStatus(FcModMetaInfo modInfo, IList<ISortProblem> problems) {
         ModInfo = modInfo;
         Successful = problems.Count == 0;
         Problems = ImmutableList.CreateRange(problems);
      }

      public static SortStatus Create(FcModMetaInfo modInfo, params ISortProblem[] problems) {
         return new SortStatus(modInfo, ImmutableList.CreateRange(problems));
      }
   }


   public interface IFcModSorter {

      IImmutableList<SortStatus> Sort(IEnumerable<FcModMetaInfo> modList);
   }
}
