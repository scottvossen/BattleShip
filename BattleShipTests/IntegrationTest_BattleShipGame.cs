using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleShip;
using Moq;
using System.Threading;

namespace BattleShipTests
{
    [TestClass]
    public class IntegrationTest_BattleShipGame
    {
        private class ShipPlacementHelperDouble : ShipPlacementHelper
        {
            public ShipPlacementHelperDouble(Coordinate c) : base(c) { }
        }

        private class GameInfoHelperDouble : GameInfoHelper
        {
            public GameInfoHelperDouble() { }

            public override Ship[] GetStartingShips()
            {
                return new Ship[]
                {
                    new Ship("A", "A", new Ship.Section[]
                        {
                            new Ship.Section(new Coordinate(-100, -100)),
                            new Ship.Section(new Coordinate(-100, -100)),
                        }),
                    new Ship("B", "B", new Ship.Section[]
                        {
                            new Ship.Section(new Coordinate(-100, -100)),
                            new Ship.Section(new Coordinate(-100, -100)),
                        })
                };
            }
        }

        private class AI_PlayerDouble : EasyAI
        {
            public AI_PlayerDouble(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc) { }
        }

        private class MainWindowDouble : MainWindow
        {
            public MainWindowDouble(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc) { }
        }

        private class Human_PlayerDouble : Human_Player
        {
            public Human_PlayerDouble(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc)
            {
                this.wdw = new MainWindowDouble(PlayerName, PlayerImgLoc);
            }
        }

        Coordinate testBoardDims = new Coordinate(3, 3);

        [TestMethod]
        public void TestInitialize()
        {
            ShipPlacementHelperDouble sph = new ShipPlacementHelperDouble(testBoardDims);
            GameInfoHelperDouble gih = new GameInfoHelperDouble();
            AI_PlayerDouble ai = new AI_PlayerDouble("playerName", "PlayerImgLoc");
            AI_PlayerDouble ai2 = new AI_PlayerDouble("playerName", "PlayerImgLoc");
            //Human_PlayerDouble p = new Human_PlayerDouble("playerName", "PlayerImgLoc");
            BatteShipGame game = new BatteShipGame(ai, ai2, testBoardDims, sph, gih);

            game.RunGame();
        }
    }
}
