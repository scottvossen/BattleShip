using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace BattleShip
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {         
            // Splash Screen
            SplashScreen splashScreen = new SplashScreen("SplashScreen.png");
            splashScreen.Show(false);
            Thread.Sleep(3000);
            splashScreen.Close(TimeSpan.FromMilliseconds(1000));

            bool? playAgain = false;

            do
            {

                // Run Game Setup
                SetupGame setup = new SetupGame();
                setup.ShowDialog();

                if (setup.DialogResult == true)
                {
                    // Setup objects (Order should be maintained)
                    IPlayer ai = setup.getOpponent();
                    Human_Player p = setup.getHumanPlayer();
                    p.SetOpponent(ref ai);
                    Coordinate boardDims = new Coordinate(9, 9);
                    BatteShipGame game = new BatteShipGame(ai, p, boardDims, 
                        new ShipPlacementHelper(boardDims), new GameInfoHelper());

                    // Run the game
                    Thread thread = new Thread(new ThreadStart(game.RunGame));
                    thread.Start();
                    p.ShowDialog();
                    thread.Abort();
                    //thread.Join();

                    playAgain = p.DialogResult;
                }
            } while (playAgain == true);
            Environment.Exit(0);
        }
    }
}
