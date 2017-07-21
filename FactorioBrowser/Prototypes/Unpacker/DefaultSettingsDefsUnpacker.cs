using System.Collections.Generic;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class DefaultSettingsDefsUnpacker : IFcSettingsDefsUnpacker {
      public IList<FcModSetting> Unpack(ILuaTable dataRaw) {
         var unpacked = new UnpackerDispatcher().
            Unpack<IDictionary<string, IDictionary<string, FcPrototype>>>(dataRaw.Self(), "data.raw");

         IList<FcModSetting> settings = new List<FcModSetting>();
         foreach (var category in unpacked.Values) {
            foreach (var structure in category.Values) {
               var setting = structure as FcModSetting;
               if (setting != null) {
                  settings.Add(setting);
               }
            }
         }

         return settings;
      }
   }
}
