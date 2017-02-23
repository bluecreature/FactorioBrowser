using System;
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


   public sealed class FcVersion {
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

      public static FcVersion FromDotNotation(string versionString) {
         Match match = DotNotationRegEx.Match(versionString);
         if (!match.Success) {
            throw new VersionFormatException($"FcVersion `{versionString}' does not conform " +
                                             "to the required format <major>.<minor>.<patch>.");
         }

         int major = Int32.Parse(match.Groups[1].Value);
         int minor = Int32.Parse(match.Groups[2].Value);
         int patch = Int32.Parse(match.Groups[3].Value);
         return new FcVersion(major, minor, patch);
      }

      private static readonly Regex DotNotationRegEx = new Regex(
         "^(\\d+).(\\d+).(\\d+)$", RegexOptions.Compiled);
   }
}
