using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MAVI.ARCH.UMS.DAL_NS;

namespace UmsHelper_UnitTest
{
    [TestClass]
    public class CRUD_Tesztek
    {
        [TestMethod]
        public void CD_teszt()
        {
            UmsHelper uh = new UmsHelper("UMSDB");

            uh.addUserInfo("ID1", "P2Lajos", "DEV", "64-31-50-41-5C-B0");

            UserInfo info = uh.getUserInfoByConnection("ID1");

            Assert.AreEqual("ID1", info.Id, "Nem jó valami! Nem azt kapjuk amit keresünk!");

            uh.delUserInfo("ID1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException),"Nana, ilyen adatnak nem kellene lenni!")]    
        public void Delete_Teszt()
        {
            UmsHelper uh = new UmsHelper("UMSDB");
            UserInfo info = uh.getUserInfoByConnection("ID1");
        }


        [TestMethod]
        public void SetPass2_teszt()
        {
            UmsHelper uh = new UmsHelper("UMSDB");

            uh.addUserInfo("ID2", "URES", "URES", "64-31-50-41-5C-B0");

            uh.setPass2Info("ID2", "PLajos", "ENV", Environment.MachineName, "64-31-50-41-5C-B0");
            
            UserInfo info = uh.getUserInfoByConnection("ID2");

            Assert.AreEqual("PLajos", info.Pass2Id, "Nem jó valami! Nem azt kapjuk amit keresünk!");

            uh.delUserInfo("ID2");
        }

        [TestMethod]
        public void MAC_teszt()
        {
            UmsHelper uh = new UmsHelper("UMSDB");

            uh.addUserInfo("ID2", "URES", "URES", "64-31-50-41-5C-B0");

            uh.setPass2Info("ID2", "PLajos", "ENV", Environment.MachineName, "64-31-50-41-5C-B0");

            UserInfo info = uh.getUserInfoByMacAddress("64-31-50-41-5C-B0");

            Assert.AreEqual("PLajos", info.Pass2Id, "Nem jó valami! Nem azt kapjuk amit keresünk!");

            uh.delUserInfo("ID2");
        }



    }
}
