using System.Collections.Generic;
using System.Linq;
using BattleShip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BattleShipTests
{
    [TestClass]
    public class UnitTest_ShipPlacementHelper
    {
        private ShipPlacementHelper sph = new ShipPlacementHelper(new Coordinate(2, 2));
        
        /* shipA is the base ship:
         * - Directly adjacent to shipB
         * - No conflict with shipC
         * - Overlap with shipD
         * - No conflict with (outofbounds) shipE
         * - Overlap with (invalidplacement - bunched) shipF
         * - Overlap with (invalidplacement - spread) shipG
        */
        #region Ship Definitions
        Ship shipA = new Ship("shipA", "A",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,1)),
                    new Ship.Section(new Coordinate(0,2)),
                });

        Ship shipB = new Ship("shipB", "B",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(1,0)),
                    new Ship.Section(new Coordinate(1,1)),
                    new Ship.Section(new Coordinate(1,2)),
                });

        Ship shipC = new Ship("shipC", "C",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(2,0)),
                    new Ship.Section(new Coordinate(2,1)),
                    new Ship.Section(new Coordinate(2,2)),
                });

        Ship shipD = new Ship("shipD", "D",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(1,0)),
                    new Ship.Section(new Coordinate(2,0)),
                });

        Ship shipE = new Ship("shipE", "E",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(2,-1)),
                    new Ship.Section(new Coordinate(2,0)),
                    new Ship.Section(new Coordinate(2,1)),
                });

        Ship shipF = new Ship("shipF", "F",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,0)),
                });

        Ship shipG = new Ship("shipG", "G",
            new Ship.Section[]
                { 
                    new Ship.Section(new Coordinate(0,0)),
                    new Ship.Section(new Coordinate(0,2)),
                    new Ship.Section(new Coordinate(2,2)),
                });
        #endregion

        [TestMethod]
        public void TestIsInvalidPlacement()
        {
            Assert.IsFalse(sph.IsInvalidPlacement(shipA));
            Assert.IsFalse(sph.IsInvalidPlacement(shipB));
            Assert.IsFalse(sph.IsInvalidPlacement(shipC));
            Assert.IsFalse(sph.IsInvalidPlacement(shipD));
            Assert.IsTrue(sph.IsInvalidPlacement(shipE));
            Assert.IsTrue(sph.IsInvalidPlacement(shipF));
            Assert.IsTrue(sph.IsInvalidPlacement(shipG));
        }

        [TestMethod]
        public void TestIsOutOfBounds()
        {
            Ship[] ships = new Ship[] { shipA, shipB, shipC, shipD, shipE, shipF, shipD };
            int[] outOfBoundsIndexes = new int[] { 4 };

            for (int i = 0; i < ships.Length; i++)
            {
                bool isOutofBounds = false;

                foreach (var sec in ships[i].Sections)
                {
                    if (sph.IsOutOfBounds(sec.Coord)) { isOutofBounds = true; break; }
                }

                if (outOfBoundsIndexes.Contains(i)) { Assert.IsTrue(isOutofBounds); }
                else { Assert.IsFalse(isOutofBounds); }
            }
        }

        [TestMethod]
        public void TestPlacementCreatesConflict()
        {
            Assert.IsTrue(sph.PlacementCreatesConflict(shipB, new List<Ship> { shipA }));
            Assert.IsFalse(sph.PlacementCreatesConflict(shipC, new List<Ship> { shipA }));
            Assert.IsTrue(sph.PlacementCreatesConflict(shipD, new List<Ship> { shipA }));
            Assert.IsFalse(sph.PlacementCreatesConflict(shipE, new List<Ship> { shipA }));
            Assert.IsTrue(sph.PlacementCreatesConflict(shipF, new List<Ship> { shipA }));
            Assert.IsTrue(sph.PlacementCreatesConflict(shipG, new List<Ship> { shipA }));
        }
    }
}
