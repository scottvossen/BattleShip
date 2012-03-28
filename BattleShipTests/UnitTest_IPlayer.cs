using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleShip;
using Moq;

namespace BattleShipTests
{
    [TestClass]
    public class UnitTest_IPlayer
    {
        [TestMethod]
        public void TestIPlayers()
        {
            IPlayer[] players = new IPlayer[] 
            {
                new EasyAI("EasyAI", ""),
                new NormalAI("NormalAI", ""),
                //new HardAI("HardAI", ""),
                //new Human_Player("Human", "")
            };

            foreach (var player in players)
            {
                TestInitialize(player);
                TestPlaceShips(player);
                TestAttack(player);
            }
        }

        public void TestInitialize(IPlayer player)
        {
            String failMsg = player.PlayerName + " Failed TestInitialize ";
            Coordinate maxCoords = getMaxCoords();
            Ship[] ships = getShips();
            player.Initialize(maxCoords, ships);

            Assert.AreEqual(maxCoords, player.MaxCoords, failMsg + "Bad MaxCoords");
            Assert.AreEqual(ships, player.Ships, failMsg + "Bad Ships");
            Assert.AreEqual(maxCoords.X + 1, player.Attacks.GetLength(0), failMsg + "Bad X Length");
            Assert.AreEqual(maxCoords.Y + 1, player.Attacks.GetLength(1), failMsg + "Bad Y Length");
            foreach (var attack in player.Attacks)
            {
                Assert.IsNotNull(attack, failMsg, "Attacks Not Initialized");
            }
        }

        public void TestPlaceShips(IPlayer player)
        {
            String failMsg = player.PlayerName + " Failed TestPlaceShips ";
            var sph = new ShipPlacementHelper(getMaxCoords());
            List<Ship> ships = new List<Ship>();
                
            player.PlaceShips();
                
            foreach (var ship in player.Ships)
            {
                Assert.IsFalse(sph.IsInvalidPlacement(ship), failMsg + "Invalid Placement");
                if (sph.PlacementCreatesConflict(ship, ships)) { Assert.Fail(failMsg + "Placement Conflict"); }
                else { ships.Add(ship); }
            }
        }

        public void TestAttack(IPlayer player)
        {
            String failMsg = player.PlayerName + " Failed TestAttack ";
            List<Coordinate> attacks = new List<Coordinate>();
            for (int i = 0; i < 30; i++)
            {
                Coordinate atk = player.Attack();
                Assert.IsTrue(!attacks.Contains(atk), failMsg + "Repeated Attack");
                attacks.Add(atk);
                if (i % 2 == 0) { player.UpdateAttackResults(atk, AttackResult.Hit, false); }
                else { player.UpdateAttackResults(atk, AttackResult.Miss, false);  }
            }
        }

        private Ship[] getShips()
        {
            Ship.Section[] sections = new Ship.Section[]
            {
                new Ship.Section(new Coordinate(0,0)),
                new Ship.Section(new Coordinate(0,0)),
                new Ship.Section(new Coordinate(0,0))
            };

            return new Ship[] 
            {
                new Ship("Ship1", "Ship1", new Ship.Section[]
                    {
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0))
                    }),
                new Ship("Ship2", "Ship2", new Ship.Section[]
                    {
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0))
                    }),
                new Ship("Ship3", "Ship3", new Ship.Section[]
                    {
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0)),
                        new Ship.Section(new Coordinate(0,0))
                    })
            };
        }

        private Coordinate getMaxCoords()
        {
            return new Coordinate(10, 10);
        }
    }
}
