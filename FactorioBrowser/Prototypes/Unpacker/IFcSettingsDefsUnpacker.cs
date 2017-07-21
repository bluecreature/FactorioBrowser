using System.Collections.Generic;

namespace FactorioBrowser.Prototypes.Unpacker {

   interface IFcSettingsDefsUnpacker {

      IList<FcModSetting> Unpack(ILuaTable dataRaw);
   }
}
