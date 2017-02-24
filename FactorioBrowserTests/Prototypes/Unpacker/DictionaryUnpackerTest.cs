using System.Collections.Generic;
using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [TestFixture]
   public class DictionaryUnpackerTest {

      [Test]
      public void TestEmptyDictionary() {
         var unpacked = Unpack<object, object>(new Dictionary<object, object>());
         Assert.IsEmpty(unpacked);
      }

      [Test]
      public void TestStringValues() {
         var unpacked = Unpack<string, string>(new Dictionary<object, object>() {
            ["key1"] = "value1"
         });

         Assert.AreEqual("value1", unpacked["key1"]);
      }

      [Test]
      public void TestNumericValues() { // TODO : move to dedicate test for primitive unpacker
         var unpacked = Unpack<string, int>(new Dictionary<object, object>() {
            ["key1"] = 100
         });

         Assert.AreEqual(100, unpacked["key1"]);
      }

      [Test]
      public void TestNestedDictionary() {
         var unpacked = Unpack<string, IDictionary<string, string>>(new Dictionary<object, object>()
         {
            ["key1"] = new Dictionary<object, object>() {
               ["key2"] = "value"
            }
         });

         Assert.AreEqual("value", unpacked["key1"]["key2"]);
      }

      private IDictionary<TKey, TValue> Unpack<TKey, TValue>(IDictionary<object, object> data) {
         var unpacker = new DictionaryUnpacker<TKey, TValue>(new UnpackerDispatcher());
         return unpacker.Unpack(new DictionaryLuaTable(data), "root");
      }
   }
}
