using System.Collections.Generic;
using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [TestFixture]
   public class ListUnpackerTest {

      [Test]
      public void UnpackEmptyList() {
         var unpacked = Unpack<object>(new Dictionary<object, object>());
         Assert.IsEmpty(unpacked);
      }

      [Test]
      public void UnpackStringList() {
         var unpacked = Unpack<string>(new Dictionary<object, object>() {
            [1] = "item1",
            [2] = "item2",
            [3] = "item3",
         });

         Assert.AreEqual(3, unpacked.Count);
         Assert.AreEqual("item2", unpacked[1]);
      }

      [Test]
      public void UnpackNestedList() {
         var unpacked = Unpack<IList<string>>(new Dictionary<object, object>() {
            [1] = new Dictionary<object, object>() {
               [1] = "value",
            },
         });

         Assert.AreEqual(1, unpacked.Count);
         Assert.AreEqual(1, unpacked[0].Count);
         Assert.AreEqual("value", unpacked[0][0]);
      }

      private IList<T> Unpack<T>(IDictionary<object, object> table) {
         return new ListUnpacker<T>(new UnpackerDispatcher())
            .Unpack(new DictionaryLuaTable(table), "root");
      }
   }
}
