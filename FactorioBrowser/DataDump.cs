using System.Linq;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes.Unpacker;
using NLog;

namespace FactorioBrowser {

   public sealed class DataDump {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private const string GamePath = @"D:\Games\Factorio"; // fill in
      private const string TestModPath = @"D:\Games\Factorio\mods"; // fill in

      public static void Main(string[] args) {
         var modFinder = new DefaultFcModFinder(GamePath, TestModPath);
         var allMods = modFinder.FindAll();
         var modSorter = new DefaultFcModSorter();
         var sorted = modSorter.Sort(allMods);
         var files = sorted.Where(s => s.Successful).Select(s => FcModFileInfo.FromMetaInfo(s.ModInfo));
         var loader = new DefaultModDataLoader(GamePath,
            new DefaultSettingDefsUnpacker(), new DefaultPrototypeUnpacker());
      }
   }
}
