using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FactorioBrowser {

   public class VersionFormatException : Exception {
      public VersionFormatException() {
      }

      public VersionFormatException(string message)
         : base(message) {
      }

      public VersionFormatException(string message, Exception inner)
         : base(message, inner) {
      }
   }


   public sealed class FcVersion : IComparable<FcVersion> {
      public int Major { get; }
      public int Minor { get; }
      public int Patch { get; }

      public FcVersion(int major, int minor, int patch) {
         Major = major;
         Minor = minor;
         Patch = patch;
      }

      public string ToDotNotation() {
         return $"{Major}.{Minor}.{Patch}";
      }

      public override string ToString() {
         return "FcVersion(" + ToDotNotation() + ")";
      }

      public override bool Equals(object obj) {
         if (obj is FcVersion) {
            return CompareTo((FcVersion) obj) == 0;

         } else {
            return false;
         }
      }

      public int CompareTo(FcVersion other) {
         if (ReferenceEquals(this, other)) {
            return 0;

         } else if (ReferenceEquals(null, other)) {
            return 1;

         } else {
            int[] leftRepr = new int[] { this.Major, this.Minor, this.Patch };
            int[] rightRepr = new int[] { other.Major, other.Minor, other.Patch };
            return Comparer<int[]>.Default.Compare(leftRepr, rightRepr);
         }
      }

      public override int GetHashCode() {
         unchecked {
            int hash = 17;
            hash = hash * 23 + Major.GetHashCode();
            hash = hash * 23 + Minor.GetHashCode();
            hash = hash * 23 + Patch.GetHashCode();
            return hash;
         }
      }

      public static FcVersion FromDotNotation(string versionString) {
         Match match = DotNotationRegEx.Match(versionString);
         if (!match.Success) {
            throw new VersionFormatException($"FcVersion `{versionString}' does not conform " +
                                             "to the required format <major>.<minor>.<patch>.");
         }

         int major = Int32.Parse(match.Groups[1].Value);
         int minor = Int32.Parse(match.Groups[2].Value);
         string patchStr = match.Groups[4].Value;
         int patch = patchStr.Length > 0 ? Int32.Parse(patchStr) : 0;
         return new FcVersion(major, minor, patch);
      }

      private static readonly Regex DotNotationRegEx = new Regex(
         "^(\\d+)\\.(\\d+)(\\.(\\d+))?$", RegexOptions.Compiled);

   }
}
