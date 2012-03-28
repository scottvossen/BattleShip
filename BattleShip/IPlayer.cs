using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace BattleShip
{
    public enum AIDifficulty { EASY, NORMAL, HARD }

    public enum AttackResult
    {
        Unknown = 0, // default
        Miss = -1,
        Hit = 1
    }

    public class Attack : INotifyPropertyChanged
    {
        private AttackResult result;
        public AttackResult Result { get { return result; } set { result = value; OnPropertyChanged(Result); } }
        public event PropertyChangedEventHandler PropertyChanged;

        public Attack() { this.result = AttackResult.Unknown; }

        protected void OnPropertyChanged(AttackResult result)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("result"));
            }
        }
    }

    public interface IPlayer
    {
        string PlayerName { get; set; }
        string PlayerImgLoc { get; set; }
        Ship[] Ships { get; set; }
        Attack[,] Attacks { get; set; }
        Coordinate MaxCoords { get; set; } // Inclusive

        void Initialize(Coordinate maxCoords, Ship[] startingShips);
        bool IsReady();
        void PlaceShips();
        Coordinate Attack();
        void UpdateAttackResults(Coordinate lastAttack, AttackResult result, bool sunkShip);
        void WinnerNotification(string winnerName);
    }

    public class Human_Player : IPlayer
    {
        private delegate bool IsReadyDelegate();
        private delegate void placeShipsDelegate();
        private delegate void AttackDelegate();
        private delegate void UpdateAttackResultsDelegate(Coordinate lastAttack, AttackResult result, bool sunkShip);
        private delegate void WinnerNotificationDelegate(string winnerName);

        public bool? DialogResult       { get { return wdw.DialogResult; } }
        public string PlayerName        { get { return wdw.PlayerName; }    set { wdw.PlayerName = value; } }
        public string PlayerImgLoc      { get { return wdw.PlayerImgLoc; }  set { wdw.PlayerImgLoc= value; } }
        public Ship[] Ships             { get { return wdw.Ships; }         set { wdw.Ships = value; } }
        public Attack[,] Attacks        { get { return wdw.Attacks; }       set { wdw.Attacks = value; } }
        public Coordinate MaxCoords     { get { return wdw.MaxCoords; }     set { wdw.MaxCoords = value; } } // Inclusive
        protected const int msToWaitForResponse = 100;
        protected MainWindow wdw;
        protected bool shipsPlaced;
        protected Coordinate attackToBeMade;
        protected ShipPlacementHelper sph;

        public Human_Player(string PlayerName, string PlayerImgLoc) { this.wdw = new MainWindow(PlayerName, PlayerImgLoc); }

        public void Initialize(Coordinate maxCoords, Ship[] startingShips)
        {
            this.wdw.Initialize(maxCoords, startingShips);
            wdw.ShipsPlaced += new MainWindow.ShipsPlacedEventHandler(HandlePiecePlaced);
            wdw.AttackMade += new MainWindow.AttackMadeEventHandler(HandleAttackMade);
            this.sph = new ShipPlacementHelper(maxCoords);
        }

        public void SetOpponent(ref IPlayer opponent) { this.wdw.SetOpponent(ref opponent); }

        public void Show()
        {
            wdw.Show();
        }

        public bool? ShowDialog()
        {
            return wdw.ShowDialog();
        }

        public bool IsReady()
        {
            return Convert.ToBoolean(((MainWindow)wdw).Dispatcher.Invoke(new IsReadyDelegate(wdw.IsReady)));
        }

        public void PlaceShips()
        {
            ((MainWindow)wdw).Dispatcher.BeginInvoke(new placeShipsDelegate(wdw.PlaceShips));
            while (!shipsPlaced) { Thread.Sleep(msToWaitForResponse); }
            shipsPlaced = false;
        }

        private void HandlePiecePlaced(object sender, ShipsPlacedEventArgs e)
        {
            shipsPlaced = true;
        }

        public Coordinate Attack()
        {
            ((MainWindow)wdw).Dispatcher.Invoke(new AttackDelegate(wdw.Attack));
            while (attackToBeMade == null) { Thread.Sleep(msToWaitForResponse); }

            Coordinate attack = attackToBeMade;
            attackToBeMade = null;
            return attack;
        }

        private void HandleAttackMade(object sender, AttackMadeEventArgs e)
        {
            attackToBeMade = e.attack;
        }

        public void UpdateAttackResults(Coordinate lastAttack, AttackResult result, bool sunkShip)
        {
            ((MainWindow)wdw).Dispatcher.Invoke(new UpdateAttackResultsDelegate(wdw.UpdateAttackResults), 
                lastAttack, result, sunkShip);
        }

        public void WinnerNotification(string winnerName)
        {
            ((MainWindow)wdw).Dispatcher.Invoke(new WinnerNotificationDelegate(wdw.WinnerNotification), winnerName);
        }
    }

    public class AI_Player : IPlayer
    {
        protected class AttackInfo
        {
            public Coordinate coord;
            public AttackResult result;
            public bool sunkShip;

            public AttackInfo(Coordinate coord, AttackResult result, bool sunkShip)
            {
                this.coord = coord;
                this.result = result;
                this.sunkShip = sunkShip;
            }
        }

        public string PlayerName { get; set; }
        public string PlayerImgLoc { get; set; }
        public Ship[] Ships { get; set; }
        public Attack[,] Attacks { get; set; }
        public Coordinate MaxCoords { get; set; } // Inclusive
        protected ShipPlacementHelper sph;

        public AI_Player(string PlayerName, string PlayerImgLoc)
        { 
            this.PlayerName = PlayerName;
            this.PlayerImgLoc = PlayerImgLoc;
        }

        public virtual void Initialize(Coordinate maxCoords, Ship[] startingShips)
        {
            this.Ships = startingShips;
            this.Attacks = new Attack[maxCoords.X + 1, maxCoords.Y + 1];
            for (int x = 0; x <= maxCoords.X; x++) { for (int y = 0; y <= maxCoords.Y; y++) { Attacks[x, y] = new Attack(); } }
            this.MaxCoords = maxCoords;
            this.sph = new ShipPlacementHelper(maxCoords);
        }

        public virtual bool IsReady() { return true; }

        public virtual void PlaceShips()
        {
            List<Ship> placedShips = new List<Ship>();
            for (int i = 0; i < Ships.Length; i++)
            {
                Ships[i] = PlaceShip(Ships[i], placedShips);
                placedShips.Add(Ships[i]);
            }
        }

        protected virtual Ship PlaceShip(Ship s, List<Ship> placedShips)
        {
            Random r = new Random();
            bool isSafePlacement = false;

            while (!isSafePlacement)
            {
                isSafePlacement = true;
                int x = r.Next(MaxCoords.X);
                int y = r.Next(MaxCoords.Y);
                int secMod = 0;
                int xMod = 0;
                int yMod = 0;

                #region Pick a random direction
                switch (r.Next(3))
                {
                    case 0:
                        yMod = 1; // Up
                        break;
                    case 1:
                        yMod = -1; // Down
                        break;
                    case 2:
                        xMod = -1; // Left
                        break;
                    case 3:
                        xMod = 1; // Right
                        break;
                    default:
                        break;
                }
                #endregion

                // Place ship
                foreach (var sec in s.Sections)
                {
                    sec.Coord = new Coordinate(x + (secMod * xMod), y + (secMod * yMod));
                    secMod++;
                }

                // Check for safe placement
                if (sph.IsInvalidPlacement(s) || sph.PlacementCreatesConflict(s, placedShips)) { isSafePlacement = false; }
            }
            return s;
        }

        public virtual Coordinate Attack()
        {
            return CalcRandomAttack();
        }

        public virtual void UpdateAttackResults(Coordinate lastAttack, AttackResult result, bool sunkShip)
        {
            Attacks[lastAttack.X, lastAttack.Y].Result = result;
        }

        public virtual void WinnerNotification(string winnerName) { }

        protected virtual Coordinate CalcRandomAttack()
        {
            Random r = new Random();
            Coordinate atk = new Coordinate(r.Next(MaxCoords.X + 1), r.Next(MaxCoords.Y + 1));
            while (AttackExists(atk))
            {
                atk = new Coordinate(r.Next(MaxCoords.X + 1), r.Next(MaxCoords.Y + 1));
            }
            return atk;
        }

        protected virtual bool AttackExists(Coordinate atk)
        {
            if (Attacks[atk.X, atk.Y].Result != AttackResult.Unknown) { return true; }
            else { return false; }
        }

        protected virtual List<Coordinate> CalcAdjCoords(Coordinate c)
        {
            List<Coordinate> adjList = new List<Coordinate>()
            {
                new Coordinate(c.X + 1, c.Y),
                new Coordinate(c.X - 1, c.Y),
                new Coordinate(c.X, c.Y + 1),
                new Coordinate(c.X, c.Y - 1)
            };

            adjList.RemoveAll(x => sph.IsOutOfBounds(x));
            return adjList;
        }

        protected virtual Coordinate ChooseRandomCoordFromListFirst(List<Coordinate> list)
        {
            Random r = new Random();
            list.RemoveAll(x => sph.IsOutOfBounds(x));
            if (list.Count <= 0) { return CalcRandomAttack(); }
            return list[r.Next(list.Count - 1)];
        }
    }

    public class EasyAI : AI_Player
    {
        public EasyAI(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc) { }
    }

    public class NormalAI : AI_Player
    {
        protected enum AtkState
        {
            StabInTheDark = 0,
            FindFollowUp,
            FinishEmOff,
        }

        protected AtkState atkState = AtkState.StabInTheDark;
        protected List<Coordinate> targets = new List<Coordinate>();
        protected AttackInfo firstHit;
        protected AttackInfo secondHit;

        public NormalAI(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc) { }

        public override Coordinate Attack()
        {
            switch (atkState)
            {
                case AtkState.StabInTheDark:
                    return CalcRandomAttack();
                case AtkState.FindFollowUp:
                    if (targets.Count == 0) { atkState = AtkState.StabInTheDark; return CalcRandomAttack(); }
                    Random r = new Random();
                    int i = r.Next(targets.Count);
                    Coordinate target = targets[i];
                    targets.RemoveAt(i);
                    return target;
                case AtkState.FinishEmOff:
                    Coordinate c = FinishEmOff();
                    if (c == null) { atkState = AtkState.StabInTheDark; return CalcRandomAttack(); }
                    return c;
                default:
                    atkState = AtkState.StabInTheDark;
                    return CalcRandomAttack();
            }
        }

        public override void UpdateAttackResults(Coordinate lastAttack, AttackResult result, bool sunkShip)
        {
            switch (atkState)
            {
                case AtkState.StabInTheDark:
                    if (result == AttackResult.Hit)
                    {
                        atkState = AtkState.FindFollowUp;
                        firstHit = new AttackInfo(lastAttack, result, sunkShip);
                        targets = CalcAdjCoords(lastAttack).FindAll(t => Attacks[t.X, t.Y].Result == AttackResult.Unknown);
                    }
                    break;
                case AtkState.FindFollowUp:
                    if (sunkShip) { atkState = AtkState.StabInTheDark; break; }
                    if (result == AttackResult.Hit)
                    {
                        atkState = AtkState.FinishEmOff;
                        secondHit = new AttackInfo(lastAttack, result, sunkShip);
                        targets.Clear();
                    }
                    break;
                case AtkState.FinishEmOff:
                    if (result == AttackResult.Hit && sunkShip)
                    {
                        atkState = AtkState.StabInTheDark;
                        firstHit = null;
                        secondHit = null;
                    }
                    break;
                default:
                    atkState = AtkState.StabInTheDark;
                    break;
            }
            base.UpdateAttackResults(lastAttack, result, sunkShip);
        }

        protected Coordinate FinishEmOff()
        {
            bool isHorizontalChain = (firstHit.coord.Y == secondHit.coord.Y);
            List<Coordinate> coordOptions = new List<Coordinate>();

            if (isHorizontalChain)
            {
                Coordinate nextRight = new Coordinate(firstHit.coord.X + 1, firstHit.coord.Y);
                Coordinate nextLeft = new Coordinate(firstHit.coord.X - 1, firstHit.coord.Y);

                if (!sph.IsOutOfBounds(nextRight))
                {
                    while (Attacks[nextRight.X, nextRight.Y].Result == AttackResult.Hit)
                    {
                        nextRight.X++;
                        if (sph.IsOutOfBounds(nextRight)) { break; }
                    }
                }

                if (!sph.IsOutOfBounds(nextLeft))
                {
                    while (Attacks[nextLeft.X, nextLeft.Y].Result == AttackResult.Hit)
                    {
                        nextLeft.X--;
                        if (sph.IsOutOfBounds(nextLeft)) { break; }
                    }
                }

                coordOptions.Add(nextRight);
                coordOptions.Add(nextLeft);

            }
            else // Vertical Chain
            {
                Coordinate nextUp = new Coordinate(firstHit.coord.X, firstHit.coord.Y + 1);
                Coordinate nextDown = new Coordinate(firstHit.coord.X, firstHit.coord.Y - 1);

                if (!sph.IsOutOfBounds(nextUp))
                {
                    while (Attacks[nextUp.X, nextUp.Y].Result == AttackResult.Hit)
                    {
                        nextUp.Y++;
                        if (sph.IsOutOfBounds(nextUp)) { break; }
                    }
                }

                if (!sph.IsOutOfBounds(nextDown))
                {
                    while (Attacks[nextDown.X, nextDown.Y].Result == AttackResult.Hit)
                    {
                        nextDown.Y--;
                        if (sph.IsOutOfBounds(nextDown)) { break; }
                    }
                }

                coordOptions.Add(nextUp);
                coordOptions.Add(nextDown);
            }

            coordOptions.RemoveAll(x => sph.IsOutOfBounds(x));
            coordOptions.RemoveAll(x => Attacks[x.X, x.Y].Result == AttackResult.Miss);

            // Pick a random coord
            Random r = new Random();
            if (coordOptions.Count <= 0) { return null; }
            return coordOptions[r.Next(coordOptions.Count - 1)];
        }
    }

    // TODO Make HardAI
    public class HardAI : NormalAI
    {
        public HardAI(string PlayerName, string PlayerImgLoc) : base(PlayerName, PlayerImgLoc) { }
    }
}
