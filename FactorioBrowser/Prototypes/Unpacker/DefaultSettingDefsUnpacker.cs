using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class DefaultSettingDefsUnpacker : IFcSettingDefsUnpacker {
      public IImmutableList<FcModSetting> Unpack(ILuaTable dataRaw) {
         var unpacked = new UnpackerDispatcher().
            Unpack<IDictionary<string, IDictionary<string, FcPrototype>>>(dataRaw.ToVariant(), "data.raw");

         IList<FcModSetting> settings = new List<FcModSetting>();
         foreach (var category in unpacked.Values) {
            foreach (var structure in category.Values) {
               var setting = structure as FcModSetting;
               if (setting != null) {
                  settings.Add(setting);
               }
            }
         }

         return settings.ToImmutableList();
      }
   }
}
