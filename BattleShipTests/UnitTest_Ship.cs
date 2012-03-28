using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleShip;

namespace BattleShipTests
{
    [TestClass]
    public class UnitTest_Ship
    {
        [TestMethod]
        public void TestIsDestroyed_True()
        {
            Ship ship = new Ship("testShip", "test ship", 
                new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                });

            foreach (var sec in ship.Sections) { sec.IsDamaged = true; }

            Assert.IsTrue(ship.isDestroyed());
        }

        [TestMethod]
        public void TestIsDestroyed_False1()
        {
            Ship ship = new Ship("testShip", "test ship",
                new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                });

            Assert.IsFalse(ship.isDestroyed());
        }


        [TestMethod]
        public void TestIsDestroyed_False2()
        {
            Ship ship = new Ship("testShip", "test ship",
                new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                });

            for (int i = 0; i < ship.Sections.Length; i++)
            {
                if (i % 2 == 0) { ship.Sections[i].IsDamaged = true; }
            }

            Assert.IsFalse(ship.isDestroyed());
        }
    }
}
