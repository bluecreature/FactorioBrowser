using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactorioBrowser.Mod.Loader {

   public interface ILocalizationDirectory {

      string GetLocalizedName(string section, string name, IImmutableList<string> localePreference);

      IImmutableList<string> GetAvailableLanguages();
   }

   public interface IFcLocalizationLoader {

      ILocalizationDirectory LoadLocalizationTables(IEnumerable<FcModFileInfo> mods);
   }
}
