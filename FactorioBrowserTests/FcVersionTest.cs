using FactorioBrowser;
using NUnit.Framework;

namespace FactorioBrowserTests {

   [TestFixture]
   public class FcVersionTest {

      [Test]
      public void TestParseDotNotation() {
         var v = FcVersion.FromDotNotation("1.2.3");
         Assert.AreEqual(1, v.Major);
         Assert.AreEqual(2, v.Minor);
         Assert.AreEqual(3, v.Patch);
      }

      [Test]
      public void TestOptionalPatchLevel() {
         var v = FcVersion.FromDotNotation("1.2");
         Assert.AreEqual(0, v.Patch);
      }

      [Test]
      public void TestInvalidVersion() {
         Assert.Throws<VersionFormatException>(() => FcVersion.FromDotNotation("a.b"));
      }

      [Test]
      public void TestCreateDotNotation() {
         var v = new FcVersion(1, 2, 3);
         Assert.AreEqual("1.2.3", v.ToDotNotation());
      }
   }
}
