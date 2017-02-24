using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [TestFixture]
   public class AtomicUnpackerTest {

      [Test]
      public void TestUnpackString() {
         string value = "value";
         string unpacked = Unpack<string>("value");
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackInteger() {
         int value = 100;
         int unpacked = Unpack<int>(value);
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackFloatingPoint() {
         double value = 100.5;
         double unpacked = Unpack<double>(value);
         Assert.AreEqual(value, unpacked);
      }

      [TestCase(false)]
      [TestCase(true)]
      public void TestUnpackBoolean(bool value) {
         bool unpacked = Unpack<bool>(value);
         Assert.AreEqual(value, unpacked);
      }

      private T Unpack<T>(object data) {
         return new AtomicValueUnpacker().Unpack<T>(new ObjectLuaVariant(data), "root");
      }
   }
}
