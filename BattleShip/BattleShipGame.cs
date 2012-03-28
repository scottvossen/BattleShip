using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.Windows;

namespace BattleShip
{
    public class Ship
    {
        public class Section : INotifyPropertyChanged
        {
            private bool isDamaged;
            public Coordinate Coord { get; set; }
            public bool IsDamaged { get { return isDamaged; } set { isDamaged = value; OnPropertyChanged(isDamaged); } }
            public event PropertyChangedEventHandler PropertyChanged;

            public Section(Coordinate Coord)
            {
                this.isDamaged = false;
                this.Coord = Coord;
            }
            
            protected void OnPropertyChanged(bool isDamaged)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("isDamaged"));
                }
            }
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Section[] Sections { get; set; }

        public Ship(string Name, string DisplayName, Section[] Sections)
        {
            this.Name = Name;
            this.DisplayName = DisplayName;
            this.Sections = Sections;
        }

        public bool isDestroyed()
        {
            return Sections.All(x => x.IsDamaged);
        }
    }

    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public class ShipPlacementHelper
    {
        private const int minDistBetweenShips = 2;
        private Coordinate maxCoords;

        public ShipPlacementHelper(Coordinate maxCoords) { this.maxCoords = maxCoords; }

        public bool IsInvalidPlacement(Ship s)
        {
            // Keep everything in bounds
            if (s.Sections.Any(section => IsOutOfBounds(section.Coord))) { return true; }

            // Determine a single orientation
            bool isVertical = s.Sections.All(section => section.Coord.X == s.Sections[0].Coord.X);
            bool isHorizontal = s.Sections.All(section => section.Coord.Y == s.Sections[0].Coord.Y);
            if (!(isVertical ^ isHorizontal)) { return true; }

            // Get a sorted list of the consecutive values along the orientation
            List<int> consecNums = new List<int>();
            foreach (var section in s.Sections)
            {
                if (isVertical) { consecNums.Add(section.Coord.Y); }
                else { consecNums.Add(section.Coord.X); }
            }
            consecNums.Sort();

            // Ensure these values are all adjacent to one another
            for (int i = 0; i < consecNums.Count - 1; i++)
            {
                if (consecNums[i + 1] != (consecNums[i] + 1)) { return true; }
            }

            // Good placement
            return false;
        }

        public bool IsOutOfBounds(Coordinate c)
        {
            if (c.X > maxCoords.X || c.X < 0) { return true; }
            if (c.Y > maxCoords.Y || c.Y < 0) { return true; }
            return false;
        }

        public bool PlacementCreatesConflict(Ship s, List<Ship> placedShips)
        {
            return s.Sections.Any(secOnS =>
                    placedShips.Any(ps =>
                        ps.Sections.Any(secOnPS =>
                            IsPlacementConflict(secOnPS, secOnS))));
        }

        private bool IsPlacementConflict(Ship.Section a, Ship.Section b)
        {
            bool xIsOutsideLimit = (Math.Abs(a.Coord.X - b.Coord.X) >= minDistBetweenShips);
            bool yIsOutsideLimit = (Math.Abs(a.Coord.Y - b.Coord.Y) >= minDistBetweenShips);

            if (xIsOutsideLimit || yIsOutsideLimit) { return false; }
            else return true;
        }

    }

    public class GameInfoHelper
    {
        public class PlayerInfo
        {
            public string Name { get; set; }
            public string playerImageLoc { get; set; }
            public Type aiDifficulty { get; set; }
            
            public PlayerInfo(string Name, string imgLoc, Type difficulty)
            {
                this.Name = Name;
                this.playerImageLoc = imgLoc;
                this.aiDifficulty = difficulty;
            }
        }

        private const string gameInfoPath = @"BattleShipGameInfo.xml";
        private Dictionary<string, int> shipSectionNums;
        private Dictionary<string, string> shipDisplayNames;
        public Dictionary<string, string> shipImageLoc;
        public List<PlayerInfo> players;

        public GameInfoHelper()
        {
            this.shipSectionNums = new Dictionary<string, int>();
            this.shipDisplayNames = new Dictionary<string, string>();
            this.shipImageLoc = new Dictionary<string, string>();
            this.players = new List<PlayerInfo>();

            // Initialize gameInfo
            XmlTextReader reader = new XmlTextReader(gameInfoPath);
            XmlDocument gameInfo = new XmlDocument();
            gameInfo.Load(reader);
            reader.Close();

            PopulateShips(gameInfo);
            PopulatePlayers(gameInfo);
        }

        private void PopulateShips(XmlDocument gameInfo)
        {
            foreach (XmlNode startShipList in gameInfo.GetElementsByTagName("StartingShips"))
            {
                foreach (XmlNode ship in startShipList.SelectNodes("Ship"))
                {
                    try
                    {
                        string name = ship.Attributes["name"].Value;
                        string displayName = ship.Attributes["displayName"].Value;
                        string shipImgLoc = ship.Attributes["shipImage"].Value;
                        int numSections = Convert.ToInt32(ship.Attributes["numSections"].Value);

                        shipSectionNums.Add(name, numSections);
                        shipDisplayNames.Add(name, displayName);
                        shipImageLoc.Add(name, shipImgLoc);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void PopulatePlayers(XmlDocument gameInfo)
        {
            foreach (XmlNode players in gameInfo.GetElementsByTagName("Players"))
            {
                foreach (XmlNode player in players.SelectNodes("Player"))
                {
                    string name = player.Attributes["name"].Value;
                    string playerImage = player.Attributes["playerImage"].Value;
                    string difficulty = player.Attributes["AI_Difficulty"].Value;
                }
            }
        }

        public virtual Ship[] GetStartingShips()
        {
            List<Ship> ships = new List<Ship>();
            int startingX = -100;
            int startingY = -100;

            foreach (var entry in shipSectionNums)
            {
                List<Ship.Section> sections = new List<Ship.Section>();
                for (int i = entry.Value; i > 0; i--) { sections.Add(new Ship.Section(new Coordinate(startingX, startingY))); }
                ships.Add(new Ship(entry.Key, shipDisplayNames[entry.Key], sections.ToArray()));
            }
            return ships.ToArray();
        }
    }

    public class BatteShipGame
    {
        public Coordinate maxCoords { get; private set; }
        private IPlayer A;
        private IPlayer B;
        private ShipPlacementHelper sph;
        private GameInfoHelper gih;
        int playerATurns = 0;
        int playerAMisses = 0;
        int playerAHits = 0;
        int playerBTurns = 0;
        int playerBMisses = 0;
        int playerBHits = 0;

        // TODO: order of constructors is too restricting... fix me!
        public BatteShipGame(IPlayer cpu, IPlayer humanOrCpu, Coordinate boardDims, ShipPlacementHelper sph, GameInfoHelper gih)
        {
            this.maxCoords = boardDims; // ex) 10 x 10 board would have maxCoord of 9, 9
            this.sph = sph;
            this.gih = gih;
            this.A = cpu;
            this.B = humanOrCpu;
            this.A.Initialize(maxCoords, gih.GetStartingShips());
            this.B.Initialize(maxCoords, gih.GetStartingShips());
        }

        public void RunGame()
        {
            // Set play order
            IPlayer currentPlayer = A;
            IPlayer opponent = B;

            // Wait for both players to be ready
            while (!IsReady(A) || !IsReady(B)) { }

            // Place Ships
            PlaceShips(A);
            PlaceShips(B);

            // Begin Game
            while (!isWinner())
            {
                if (currentPlayer == A) { playerATurns++; }
                else { playerBTurns++; }

                // Get an attack that hasn't been made previously
                Coordinate atk = currentPlayer.Attack();
                while (currentPlayer.Attacks[atk.X, atk.Y].Result != AttackResult.Unknown)
                {
                    atk = currentPlayer.Attack();
                }

                #region Calc Attack Result
                AttackResult result = AttackResult.Miss;
                Ship hitShip = null;
                bool sunkShip = false;

                try
                {
                    hitShip = opponent.Ships.First(ship =>
                        ship.Sections.Any(section =>
                            section.Coord.X == atk.X && section.Coord.Y == atk.Y));
                }
                catch (Exception)
                {
                    if (currentPlayer == A) { playerAMisses++; }
                    else { playerBMisses++; }
                }

                if (hitShip != null)
                {
                    hitShip.Sections.First(x => x.Coord.X == atk.X && x.Coord.Y == atk.Y).IsDamaged = true;
                    result = AttackResult.Hit;
                    if (currentPlayer == A) { playerAHits++; }
                    else { playerBHits++; }
                    sunkShip = hitShip.isDestroyed();
                }

                // Let currentPlayer know how the attack went
                currentPlayer.UpdateAttackResults(atk, result, sunkShip);
                #endregion

                // Switch players
                if (currentPlayer.Equals(A)) { currentPlayer = B; opponent = A; }
                else { currentPlayer = A; opponent = B; }
            }

            ResolveWinner();
        }
        
        private bool IsReady(IPlayer p) { return p.IsReady(); }

        private void PlaceShips(IPlayer p)
        {
            bool placementAccepted = false;

            while (!placementAccepted)
            {
                p.PlaceShips();
                placementAccepted = true;
                List<Ship> checkedShips = new List<Ship>();

                foreach (Ship s in p.Ships)
                {
                    if (sph.IsInvalidPlacement(s) || sph.PlacementCreatesConflict(s, checkedShips))
                    {
                        placementAccepted = false;
                        break;
                    }
                    checkedShips.Add(s);
                }
            }
        }

        private bool isWinner()
        {
            return (A.Ships.All(x => x.isDestroyed()) || B.Ships.All(x => x.isDestroyed()));
        }

        private void ResolveWinner()
        {
            string winner = "Error";
            if (A.Ships.All(x => x.isDestroyed())) { winner = B.PlayerName; }
            else if (B.Ships.All(x => x.isDestroyed())) { winner = A.PlayerName; }

            A.WinnerNotification(winner);
            B.WinnerNotification(winner);
        }
    }
}
