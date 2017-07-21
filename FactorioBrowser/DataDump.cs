using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;
using NLog;

namespace FactorioBrowser {

   public sealed class DataDump {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private const string GamePath = @"D:\Games\Factorio"; // fill in
      private const string TestModPath = @"D:\Games\Factorio\mods"; // fill in

      private static void DumpTable(int indent, ILuaTable data, TextWriter target) {
         string indentString = new string('\t', indent);
         foreach (var entry in data.Entries()) {
            var key = entry.Key.ToString();
            var value = entry.Value;
            if (value.ValueType == LuaValueType.Table) {
               target.WriteLine($"{indentString}{key} [Table]");
               DumpTable(indent + 1, value.AsTable, target);
            } else {
               target.WriteLine($"{indentString}{key} = [{value.ValueType}] {value}");
            }
         }
      }

      private static void DumpUnpacked(IDictionary<string, IDictionary<string, FcPrototype>> data,
         TextWriter target) {

         foreach (var category in data) {
            if (category.Value.Count > 0) {
               foreach (var entity in category.Value) {
                  if (entity.Value != null) {
                     target.WriteLine($"FOUND ENTITY: {category.Key}:{entity.Key}  -> {entity.Value}");
                  }
               }
            }
         }
      }

      public static void Main(string[] args) {
         var modFinder = new DefaultFcModFinder(GamePath, TestModPath);
         var allMods = modFinder.FindAll();
         var modSorter = new DefaultFcModSorter();
         var sorted = modSorter.Sort(allMods);
         var files = sorted.Where(s => s.Successful).Select(s => FcModFileInfo.FromMetaInfo(s.ModInfo));
         var loader = new DefaultModDataLoader(GamePath, new DefaultSettingsDefsUnpacker());
         Log.Info("Start load data.raw");
         var rawData = loader.LoadRawData(files);
         Log.Info("End load data.raw");

         Log.Info("Start raw dump");
         using (var f = new FileStream("D:\\Temp\\data_dump.txt", FileMode.Create)) {
            DumpTable(0, rawData, new StreamWriter(f));
         }
         Log.Info("End raw dump");

         Log.Info("Start unpacking");
         var unpacked = new UnpackerDispatcher().Unpack<IDictionary<string, IDictionary<string, FcPrototype>>>(rawData.Self(), "data.raw");
         Log.Info("End unpacking");
         DumpUnpacked(unpacked, Console.Out);
      }
   }
}
