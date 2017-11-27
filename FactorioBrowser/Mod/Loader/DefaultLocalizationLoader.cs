using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal sealed class LocaleTable {

      public LocaleTable() {
         Contents = new Dictionary<string, IDictionary<string, string>>();
      }

      public IDictionary<string, IDictionary<string, string>> Contents { get; }
   }

   internal sealed class DefaultLocalizationDirectory : ILocalizationDirectory {
      private readonly IDictionary<string, LocaleTable> _tableByLocale;

      internal DefaultLocalizationDirectory(IDictionary<string, LocaleTable> tableByLocale) {
         _tableByLocale = tableByLocale;
      }

      public string GetLocalizedName(string section, string name,
         IImmutableList<string> localePreference) {

         foreach (var locale in localePreference) {
            var localized = TryGetNameInLocale(locale: locale, section: section, name: name);
            if (localized != null) {
               return localized;
            }
         }

         return null;
      }

      public IImmutableList<string> GetAvailableLanguages() {
         return _tableByLocale.Keys.OrderBy(x => x).ToImmutableList();
      }

      private string TryGetNameInLocale(string locale, string section, string name) {
         LocaleTable localeTable;
         if (_tableByLocale.TryGetValue(locale, out localeTable)) {
            IDictionary<string, string> sectionTable;
            if (localeTable.Contents.TryGetValue(section, out sectionTable)) {
               string localizedName;
               if (sectionTable.TryGetValue(name, out localizedName)) {
                  return localizedName;
               }
            }
         }

         return null;
      }
   }

   internal sealed class DefaultLocalizationLoader : IFcLocalizationLoader {

      private const string LocalesDirectory = "locale";
      private const string DefaultLocale = "en";

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      public ILocalizationDirectory LoadLocalizationTables(IEnumerable<FcModFileInfo> mods) {
         IDictionary<string, LocaleTable> tableByLocale = new Dictionary<string, LocaleTable>();
         LocaleTable defaultLocaleTable = new LocaleTable();  // support for other locales is TODO
         tableByLocale.Add(DefaultLocale, defaultLocaleTable);

         foreach (var modInfo in mods) {
            LoadLocaleFiles(modInfo, defaultLocaleTable);
         }

         return new DefaultLocalizationDirectory(tableByLocale);
      }

      private void LoadLocaleFiles(FcModFileInfo mod, LocaleTable localeTable) {
         string localeDir = LocalesDirectory + "/" + DefaultLocale;
         using (var resolver = ModFileResolverFactory.CreateResolver(mod)) {
            foreach (var localeFile in resolver.ListDirectory(localeDir)) {
               string localFileFriendlyName = resolver.FriendlyName(localeFile);
               Stream localeStream = null;
               try {
                  localeStream = resolver.Open(localeFile);
                  LocaleStreamParser.Parse(localFileFriendlyName, localeStream, localeTable);
               } catch (FileNotFoundException e) {
                  Log.Error(e, $"Error while opening locale file {localFileFriendlyName}");

               } finally {
                  localeStream?.Close();
               }
            }
         }
      }
   }

   internal static class LocaleStreamParser {

      private const string CommentStart = "#";

      private static readonly Regex SectionHeaderPattern =
         new Regex(@"^\[([\w\d-]+)\]$", RegexOptions.Compiled);
      private static readonly Regex GlobalKeyValuePattern =
         new Regex(@"^(?<section>[\w\d-]+)\.(?<key>[\w\d-]+)=(?<value>.+)$", RegexOptions.Compiled);
      private static readonly Regex SectionLocalKeyValuePattern =
         new Regex(@"^(?<key>[\w\d-]+)=(?<value>.+)$", RegexOptions.Compiled);

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      public static void Parse(string filename, Stream stream, LocaleTable localeTable) {
         Parse(filename, new StreamReader(stream, Encoding.UTF8), localeTable);
      }

      private static void Parse(string filename, TextReader reader, LocaleTable localeTable) {

         int lineNumber = 0;
         string line;
         string currentSection = null;
         while ((line = reader.ReadLine()) != null) {
            ++lineNumber;
            line = line.Trim();

            string sectionDef = null;
            if (line.Length == 0 || line.StartsWith(CommentStart)) {
               continue;

            } else if ((sectionDef = TryParseSectionHeader(line)) != null) {
               currentSection = sectionDef;

            } else {
               string section;
               string key;
               string value;
               if (TryParseDefinition(line, currentSection, out section, out key, out value)) {
                  AddEntry(localeTable.Contents, section, key, value);

               } else {
                  Log.Warn($"Unparsable localization entry {filename}:{lineNumber}: {line}");
               }
            }
         }
      }

      private static string TryParseSectionHeader(string line) {
         var match = SectionHeaderPattern.Match(line);
         return match.Success ? match.Groups[1].Value : null;
      }

      private static bool TryParseDefinition(string line, string currentSection,
         out string section, out string key, out string value) {

         var pattern = currentSection == null ? GlobalKeyValuePattern : SectionLocalKeyValuePattern;
         var match = pattern.Match(line);
         if (match.Success) {
            section = currentSection ?? match.Groups["section"].Value;
            key = match.Groups["key"].Value;
            value = match.Groups["value"].Value;
            return true;

         } else {
            section = null;
            key = null;
            value = null;
            return false;
         }
      }

      private static void AddEntry(IDictionary<string, IDictionary<string, string>> table,
         string section, string key, string value) {

         Debug.Assert(section != null);
         Debug.Assert(key != null);
         Debug.Assert(value != null);

         var sectionTable = table.GetOrAdd(section, () => new Dictionary<string, string>());
         sectionTable.Remove(key);
         sectionTable.Add(key, value);
      }
   }
}
