using System.Collections.Immutable;

namespace FactorioBrowser.Prototypes.Unpacker {

   public interface IFcSettingDefsUnpacker {

      IImmutableList<FcModSetting> Unpack(ILuaTable dataRaw);
   }
}
