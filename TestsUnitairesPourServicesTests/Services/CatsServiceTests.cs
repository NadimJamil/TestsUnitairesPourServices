using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Exceptions;
using TestsUnitairesPourServices.Models;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        DbContextOptions<ApplicationDBContext> options;
        public CatsServiceTests()
        {
            options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CatsService")
                .UseLazyLoadingProxies(true)
                .Options;
        }

        [TestInitialize]
        public void Init()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            Cat cats = new Cat
            {
                Id = 1,
                Name = "Garfield",
                Age = 12
            };

            House house1 = new House { Id = 1, OwnerName = "Franky" };
            House house2 = new House { Id = 2, OwnerName = "Maximus" };

            Cat chat2 = new Cat { Id = 2, Name = "Bob", Age = 3, House = house1 };

            db.AddRange(cats, chat2);
            db.House.AddRange(house1, house2);
            db.SaveChanges();
        }

        [TestCleanup]
        public void Dispose()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            db.Cat.RemoveRange(db.Cat);
            db.House.RemoveRange(db.House);
            db.SaveChanges();
        }

        [TestMethod()]
        public void MoveTest()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);
            var maison1 = db.House.Find(1);
            var maison2 = db.House.Find(2);

            var chat1 = db.Cat.Find(1);
            var chat2 = db.Cat.Find(2);

            var chatAvecMaison = service.Move(chat2.Id, maison1, maison2);
            Assert.IsNotNull(chatAvecMaison);
        }

        [TestMethod()]
        public void TestChatIdInconnu()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            var service = new CatsService(db);
            var chatIdInconnu = db.Cat.Find(3);
            var maison1 = db.House.Find(1);
            var maison2 = db.House.Find(2);

            var chatSansId = service.Move(chatIdInconnu.Id, maison1, maison2);
            Assert.IsNull(chatSansId);
        }

        [TestMethod()]
        public void TestChatPasMaison()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            var service = new CatsService(db);
            var chatIdInconnu = db.Cat.Find(1);
            var maison1 = db.House.Find(1);
            var maison2 = db.House.Find(2);

            Exception e = Assert.ThrowsException<WildCatException>(() => service.Move(chatIdInconnu.Id, maison1, maison2));
            Assert.AreEqual("Chat sauvage", e.Message);
        }

        [TestMethod()]
        public void TestMoveBon()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            var service = new CatsService(db);
            var chatIdInconnu = db.Cat.Find(2);
            var maison1 = db.House.Find(1);
            var maison2 = db.House.Find(2);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => service.Move(chatIdInconnu.Id, maison2, maison1));
            Assert.AreEqual("Pas mon chat", e.Message);
        }
    }
}