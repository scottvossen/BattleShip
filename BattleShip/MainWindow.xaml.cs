using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattleShip
{
    public partial class MainWindow : Window
    {
        protected enum UIState
        {
            WaitingToPlace = 0,
            Placing,
            WaitingToAttack,
            Attacking,
            GameFinished
        }

        #region Public Members
        // ===============================================================================================
        public string PlayerName { get; set; }
        public string PlayerImgLoc { get; set; }
        public Ship[] Ships { get; set; }
        public Attack[,] Attacks { get; set; }
        public Coordinate MaxCoords { get; set; }
        // ===============================================================================================
        #endregion

        #region Private Members
        // ===============================================================================================
        protected IPlayer opponent;
        protected ShipPlacementHelper sph;
        protected GameInfoHelper gih;
        protected bool isReady = false;
        protected UIState uiState = UIState.WaitingToPlace;
        protected List<Image> placedShips = new List<Image>();
        protected Border selectedBorder;
        protected Rectangle highLightGridImg;
        protected Image HighLightImgGridCenteredImg;
        protected bool shipOrientIsVert = true;
        // ===============================================================================================
        #endregion

        #region Event Declarations
        // ===============================================================================================
        public delegate void ShipsPlacedEventHandler(object sender, ShipsPlacedEventArgs e);
        public delegate void AttackMadeEventHandler(object sender, AttackMadeEventArgs e);

        public event ShipsPlacedEventHandler ShipsPlaced;
        public event AttackMadeEventHandler AttackMade;
        // ===============================================================================================
        #endregion

        #region Constructors and Initializers
        // ===============================================================================================

        public MainWindow(string PlayerName, string PlayerImgLoc)
        {
            InitializeComponent();
            this.PlayerName = PlayerName;
            this.PlayerImgLoc = PlayerImgLoc;
        }
        
        public void SetOpponent(ref IPlayer opponent) { this.opponent = opponent; }

        public void Initialize(Coordinate maxCoords, Ship[] startingShips)
        {
            this.Ships = startingShips;
            this.MaxCoords = maxCoords;
            this.Attacks = new Attack[maxCoords.X + 1, maxCoords.Y + 1];
            for (int x = 0; x <= maxCoords.X; x++) { for (int y = 0; y <= maxCoords.Y; y++) { Attacks[x, y] = new Attack(); } }
            this.sph = new ShipPlacementHelper(maxCoords);
            this.gih = new GameInfoHelper();

            this.resetShips.IsEnabled = false;
            this.resetShips.Visibility = Visibility.Hidden;

            InitializeGrids();
            InitializeShips();
            InitializePlayerInfo();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) { isReady = true; }

        private void InitializeGrids()
        {
            Grid[] grids = { shipBoard, hitBoard };
            List<Attack[,]> attackTables = new List<Attack[,]> { opponent.Attacks, Attacks };

            for (int i = 0; i < grids.Length; i++)
            {
                #region Set Rows and Columns
                for (int j = 0; j <= MaxCoords.Y; j++)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    grids[i].RowDefinitions.Add(rd);
                }
                for (int j = 0; j <= MaxCoords.X; j++)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grids[i].ColumnDefinitions.Add(cd);
                }
                #endregion
            
                #region Fill the grid with images
                for (int x = 0; x <= MaxCoords.X; x++)
                {
                    for (int y = 0; y <= MaxCoords.Y; y++)
                    {
                        Binding bindToAttackResult = new Binding("Result");
                        bindToAttackResult.Source = attackTables[i][x,y];
                        bindToAttackResult.Mode = BindingMode.OneWay;
                        bindToAttackResult.Converter = new AttackResultConverter();

                        Image img = new Image();
                        img.SetBinding(Image.SourceProperty, bindToAttackResult);
                        img.HorizontalAlignment = HorizontalAlignment.Center;
                        img.VerticalAlignment = VerticalAlignment.Center;
                        img.Stretch = Stretch.UniformToFill;
                        Grid.SetRow(img, y);
                        Grid.SetColumn(img, x);

                        // Set GridImg Events
                        img.MouseEnter += GridImg_MouseEnter;
                        
                        grids[i].Children.Add(img);
                    }
                }
                #endregion

                // Set Grid Events
                grids[i].MouseLeave += Grid_MouseLeave;
            }
        }

        private void InitializeShips()
        {
            StackPanel[] shipPanels = { player1_Ships, player2_Ships };
            Ship[][] shipLists = { Ships, opponent.Ships };

            for (int i = 0; i < shipPanels.Length; i++)
            {
                foreach (Ship s in shipLists[i])
                {
                    #region Create Border for Grid
                    Border b = new Border();
                    b.Name = s.Name;
                    b.BorderThickness = new Thickness(3);
                    b.CornerRadius = new CornerRadius(3);

                    // Set Events
                    if (shipPanels[i] == player1_Ships)
                    {
                        b.MouseLeftButtonUp += ShipBorder_MouseLeftButtonUp;
                        b.MouseEnter += Border_MouseEnter;
                        b.MouseLeave += Border_MouseLeave;
                    }
                    #endregion

                    #region Create Grid as Ship
                    Grid ship = new Grid();
                    ship.HorizontalAlignment = HorizontalAlignment.Left;
                    ship.Width = 35 * s.Sections.Length;

                    // Set Columns
                    foreach (var sec in s.Sections)
                    {
                        ColumnDefinition stdColumn = new ColumnDefinition();
                        stdColumn.Width = new GridLength(1, GridUnitType.Star);
                        ship.ColumnDefinitions.Add(stdColumn);
                    }

                    // add ship image
                    Image shipImg = new Image();
                    shipImg.Source = new BitmapImage(GetImageForShip(s));
                    shipImg.Stretch = Stretch.UniformToFill;
                    shipImg.HorizontalAlignment = HorizontalAlignment.Center;
                    shipImg.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetRow(shipImg, 0);
                    Grid.SetColumn(shipImg, 0);
                    Grid.SetColumnSpan(shipImg, s.Sections.Length);
                    ship.Children.Add(shipImg);

                    // add attackResult images
                    for (int j = 0; j < s.Sections.Length; j++)
                    {
                        Binding bindToIsDamaged = new Binding("IsDamaged");
                        bindToIsDamaged.Source = s.Sections[j];
                        bindToIsDamaged.Converter = new SectionIsDamagedConverter();
                        bindToIsDamaged.Mode = BindingMode.OneWay;

                        Image img = new Image();
                        img.SetBinding(Image.SourceProperty, bindToIsDamaged);
                        img.Stretch = Stretch.UniformToFill;
                        img.HorizontalAlignment = HorizontalAlignment.Center;
                        img.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetRow(img, 0);
                        Grid.SetColumn(img, j);
                        ship.Children.Add(img);
                    }
                    #endregion

                    b.Child = ship;
                    shipPanels[i].Children.Add(b);
                }
            }
        }

        private void InitializePlayerInfo()
        {
            player1_Name.Content = PlayerName;
            player1_Image.Source = new BitmapImage(new Uri(PlayerImgLoc, UriKind.Relative));
            player2_Name.Content = opponent.PlayerName;
            player2_Image.Source = new BitmapImage(new Uri(opponent.PlayerImgLoc, UriKind.Relative));
        }

        // ===============================================================================================
        #endregion

        #region IPlayer calls
        // ===============================================================================================

        public bool IsReady() { return isReady; }

        public void PlaceShips()
        {
            switch (uiState)
            {
                case UIState.WaitingToPlace:
                    uiState = UIState.Placing;
                    ShowNotification("Place your ships on the board:\n" +
                        "Select a ship and place it on your board. Right click to rotate.\n" +
                        "Ships must be placed at least one square apart.");
                    break;
                case UIState.Placing:
                    ShowNotification("Bad Placements! Try again...\n" +
                        "Remember: ships must be placed at least one square apart.");
                    break;
                default:
                    HandleBadUIState();
                    break;
            }

            resetShips.IsEnabled = true;
            resetShips.Visibility = Visibility.Visible;
            ResetShipPlacement();
        }

        public void Attack()
        {
            switch (uiState)
            {
                case UIState.Placing:
                    placeShips.IsEnabled = false;
                    placeShips.Visibility = Visibility.Hidden;
                    uiState = UIState.Attacking;
                    break;
                case UIState.WaitingToAttack:
                    uiState = UIState.Attacking;
                    break;
                case UIState.Attacking:
                    return;
                default:
                    HandleBadUIState();
                    break;
            }
            ShowNotification(string.Format("Make an attack by clicking on {0}'s board...", opponent.PlayerName));
        }

        public void UpdateAttackResults(Coordinate lastAttack, AttackResult result, bool sunkShip)
        {
            if (uiState == UIState.Attacking)
            { 
                uiState = UIState.WaitingToAttack;
                ShowNotification(string.Format("Waiting for {0} to attack...", opponent.PlayerName));
            }
            else { HandleBadUIState(); }

            // update attacks and ui elements
            Attacks[lastAttack.X, lastAttack.Y].Result = result;
        }

        public void WinnerNotification(string winnerName)
        {
            if (uiState == UIState.WaitingToAttack) { uiState = UIState.GameFinished; }
            if (uiState != UIState.GameFinished) { HandleBadUIState(); }
            
            if (winnerName.Equals(PlayerName, StringComparison.Ordinal))
            {
                ShowNotification(string.Format("Congratulations! You won the game!!", winnerName));
            }
            else
            {
                ShowNotification(string.Format("Sorry, {0} won the game. Try Again!", winnerName));
            }
        }

        // ===============================================================================================
        #endregion

        #region UI Events
        // ===============================================================================================
        
        private void HighLightGridImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (sender as Rectangle);
            Grid grid = (Grid)rect.Parent;

            #region PlacingShips
            if (grid == shipBoard)
            {
                if (uiState == UIState.Placing && selectedBorder != null)
                {
                    #region Place a single ship
                    Ship selectedShip = Ships.First(s => s.Name.Equals(selectedBorder.Name, StringComparison.Ordinal));
                    int col = Grid.GetColumn(highLightGridImg);
                    int row = Grid.GetRow(highLightGridImg);
                    int colSpan = Grid.GetColumnSpan(highLightGridImg);
                    int rowSpan = Grid.GetRowSpan(highLightGridImg);

                    #region create shipImg, place on grid, add to placedShips
                    Image shipImg = new Image();
                    shipImg.Name = selectedBorder.Name;
                    if (shipOrientIsVert) { shipImg.Source = RotateImg(new BitmapImage(GetImageForShip(selectedShip))); }
                    else { shipImg.Source = new BitmapImage(GetImageForShip(selectedShip)); }
                    shipImg.Stretch = Stretch.UniformToFill;
                    shipImg.HorizontalAlignment = HorizontalAlignment.Center;
                    shipImg.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetRow(shipImg, row);
                    Grid.SetColumn(shipImg, col);
                    if (colSpan > 1) { Grid.SetColumnSpan(shipImg, selectedShip.Sections.Length); }
                    else { Grid.SetRowSpan(shipImg, selectedShip.Sections.Length); }

                    grid.Children.Insert(0, shipImg);
                    placedShips.Add(shipImg);
                    #endregion

                    // set sections of selectedShip
                    for (int i = 0; i < selectedShip.Sections.Length; i++)
                    {
                        if (colSpan > 1)
                        { 
                            selectedShip.Sections[i].Coord.X = col + i;
                            selectedShip.Sections[i].Coord.Y = row;
                        }
                        else
                        {
                            selectedShip.Sections[i].Coord.X = col;
                            selectedShip.Sections[i].Coord.Y = row + i;
                        }
                    }
                    #endregion

                    // Unselect Border
                    UnHighlightBorder(selectedBorder);
                    selectedBorder = null;

                    // Check for time to SendPlaceShips
                    foreach (Ship s in Ships)
                    {
                        try
                        {
                            Image i = placedShips.Single(ps => ps.Name.Equals(s.Name, StringComparison.Ordinal));
                            if (i == null) { return; }
                        }
                        catch (Exception) { return; }
                    }

                    placeShips.IsEnabled = true;
                    placeShips.Visibility = Visibility.Visible;
                    ShowNotification("Click \"Place Ships\" to continue...");
                }
            }
            #endregion

            #region Attacking

            if (grid == hitBoard)
            {
                if (uiState == UIState.Attacking)
                {
                    SendAttackMade(new AttackMadeEventArgs(new Coordinate(Grid.GetColumn(rect), Grid.GetRow(rect))));
                }
            }
            #endregion
        }
        
        private void HighLightGridImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)(sender as Rectangle).Parent;
            bool isPlacing = (uiState == UIState.Placing);
            bool shipSelected = (selectedBorder != null);
            bool isShipBoard = (grid == shipBoard);

            if (isPlacing && shipSelected && isShipBoard)
            {
                UnHighlightImgGrid(grid);
                shipOrientIsVert = !shipOrientIsVert;
                HighlightImgGrid(HighLightImgGridCenteredImg);
            }
        }

        private void GridImg_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            Grid grid = (Grid)img.Parent;
            bool isShipBoard = (grid == shipBoard);
            bool isPlacing = (uiState == UIState.Placing);
            bool isAttacking = (uiState == UIState.Attacking);
            bool shipSelected = (selectedBorder != null);

            if (isShipBoard && isPlacing && shipSelected)
            {
                UnHighlightImgGrid(grid);
                HighlightImgGrid(img);
            }
            else if (!isShipBoard && isAttacking)
            {
                HighlightImgGrid(img);
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            UnHighlightImgGrid((Grid)sender);
        }

        private void ShipBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            bool isPlacing = (uiState == UIState.Placing);
            bool isPlaced = placedShips.Any(i => i.Name.Equals(b.Name, StringComparison.Ordinal));
            bool isSelected = (selectedBorder != null) && (selectedBorder.Name.Equals(b.Name, StringComparison.Ordinal));

            if (isPlacing && !isPlaced && !isSelected)
            {
                if (selectedBorder != null) { UnHighlightBorder(selectedBorder); }
                selectedBorder = b;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = (Border)sender;
            bool isPlacing = (uiState == UIState.Placing);
            bool isPlaced = placedShips.Any(i => i.Name.Equals(b.Name, StringComparison.Ordinal));
            bool isSelected = (selectedBorder != null) && (selectedBorder.Name.Equals(b.Name, StringComparison.Ordinal));

            if (isPlacing && !isPlaced && !isSelected) { HighlightBorder(b); }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = (Border)sender;
            bool isPlacing = (uiState == UIState.Placing);
            bool isPlaced = placedShips.Any(i => i.Name.Equals(b.Name, StringComparison.Ordinal));
            bool isSelected = (selectedBorder != null) && (selectedBorder.Name.Equals(b.Name, StringComparison.Ordinal));
            
            if (isPlacing && !isPlaced && !isSelected) { UnHighlightBorder(b); }
        }

        private void resetShips_Click(object sender, RoutedEventArgs e)
        {
            ResetShipPlacement();
        }

        private void placeShips_Click(object sender, RoutedEventArgs e)
        {
            resetShips.IsEnabled = false;
            resetShips.Visibility = Visibility.Hidden;
            SendShipsPlaced(new ShipsPlacedEventArgs());
        }
        
        // TODO NewGame Button
        private void newGame_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // ===============================================================================================
        #endregion

        #region Private Helper Methods
        // ===============================================================================================

        private void ShowNotification(string notify)
        {
            //<TextBlock x:Name="notification" HorizontalAlignment="Center" FontSize="20" FontFamily="Arial">Waiting For Startup...</TextBlock>
            notificationPanel.Children.Clear();
            var notifications = notify.Split('\n');
            
            // make new textbox for each token and add it to the notifcationPanel
            int fontSize = Convert.ToInt32(Math.Round(((notificationPanel.Height - 5) * .75) / notifications.Length));
            if (fontSize > 20) { fontSize = 20; }

            for (int i = 0; i < notifications.Length; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = notifications[i];
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.FontSize = fontSize;
                tb.FontFamily = new FontFamily("Arial");
                if (i == 0) { tb.Margin = new Thickness(0, 5, 0, 0); }
                notificationPanel.Children.Add(tb);
            }
        }
        
        private void HighlightBorder(Border b)
        {
            LinearGradientBrush borderBrush = new LinearGradientBrush();
            borderBrush.GradientStops.Add(new GradientStop(Colors.CadetBlue, 0.0));
            borderBrush.GradientStops.Add(new GradientStop(Colors.Beige, 0.5));
            borderBrush.GradientStops.Add(new GradientStop(Colors.CadetBlue, 1.0));
            b.BorderBrush = borderBrush;
            b.Background = new SolidColorBrush(Colors.WhiteSmoke);
            b.Background.Opacity = 0.3;
        }

        private void UnHighlightBorder(Border b)
        {
            b.BorderBrush = null;
            b.Background = null;
        }

        private void HighlightImgGrid(Image i)
        {
            try
            {
                Grid grid = (Grid)i.Parent;
                int startCol;
                int startRow;
                int colSpan = 1;
                int rowSpan = 1;

                #region Ensure highLightGridImg is set/reset
                if (highLightGridImg == null || HighLightImgGridCenteredImg == null)
                {
                    highLightGridImg = new Rectangle();
                    highLightGridImg.Fill = new SolidColorBrush(Colors.WhiteSmoke);
                    highLightGridImg.Opacity = 0.6;
                    highLightGridImg.Stretch = Stretch.Fill;
                    highLightGridImg.MouseRightButtonUp += HighLightGridImg_MouseRightButtonUp;
                    highLightGridImg.MouseLeftButtonUp += HighLightGridImg_MouseLeftButtonUp;

                    HighLightImgGridCenteredImg = i;
                }
                #endregion

                #region Determine placement
                if (grid == shipBoard)
                {
                    // highlight ship's area with this gridImg as approximate center
                    Coordinate c = new Coordinate(Grid.GetColumn(i), Grid.GetRow(i));
                    Ship selectedShip = Ships.First(s => s.Name.Equals(selectedBorder.Name, StringComparison.Ordinal));
                    double halfLength = selectedShip.Sections.Length / 2;
                    if (shipOrientIsVert)
                    {
                        startCol = c.X;
                        startRow = Convert.ToInt32(Math.Round(c.Y - halfLength));
                        rowSpan = selectedShip.Sections.Length; 
                    }
                    else 
                    {
                        startCol = Convert.ToInt32(Math.Round(c.X - halfLength));
                        startRow = c.Y;
                        colSpan = selectedShip.Sections.Length; 
                    }
                }
                else
                {
                    // only highlight the one gridImg
                    Coordinate c = new Coordinate(Grid.GetColumn(i), Grid.GetRow(i));
                    startCol = c.X;
                    startRow = c.Y;
                }
                #endregion

                Grid.SetColumn(highLightGridImg, startCol);
                Grid.SetRow(highLightGridImg, startRow);
                Grid.SetColumnSpan(highLightGridImg, colSpan);
                Grid.SetRowSpan(highLightGridImg, rowSpan);

                grid.Children.Add(highLightGridImg);
            }
            catch (Exception) { }
        }

        private void UnHighlightImgGrid(Grid g)
        {
            try
            {
                g.Children.Remove(highLightGridImg);
                highLightGridImg = null;
                HighLightImgGridCenteredImg = null;
            }
            catch (Exception) { }
        }

        private void ResetShipPlacement()
        {
            ClearShipBoard();
            placeShips.IsEnabled = false;
            placeShips.Visibility = Visibility.Hidden;
        }

        private bool PlaceShipOnBoard(Ship s)
        {
            if (sph.IsInvalidPlacement(s)) { return false; }
            
            // Setup image
            Image ship = new Image();
            ship.Name = s.Name;
            ship.Source = new BitmapImage(GetImageForShip(s));
            ship.HorizontalAlignment = HorizontalAlignment.Center;
            ship.VerticalAlignment = VerticalAlignment.Center;
            ship.Stretch = Stretch.UniformToFill;

            // Determine coords and span
            int maxX = s.Sections.Max(sec => sec.Coord.X);
            int minX = s.Sections.Min(sec => sec.Coord.X);
            int maxY = s.Sections.Max(sec => sec.Coord.Y);
            int minY = s.Sections.Min(sec => sec.Coord.Y);
            Grid.SetColumn(ship, minX);
            Grid.SetColumnSpan(ship, maxX - minX + 1);
            Grid.SetRow(ship, minY);
            Grid.SetRowSpan(ship, maxY - minY + 1);

            shipBoard.Children.Add(ship);
            placedShips.Add(ship);
            return true;
        }

        private void ClearShipBoard()
        {
            foreach (Ship s in Ships)
            {
                foreach (var sec in s.Sections)
                {
                    sec.Coord.X = -100;
                    sec.Coord.Y = -100;
                }
            }

            foreach (var s in placedShips) { ((Grid)s.Parent).Children.Remove(s); }
            placedShips.Clear();
        }

        private Uri GetImageForShip(Ship s) { return new Uri(gih.shipImageLoc[s.Name], UriKind.Relative); }

        private Uri GetImageForAttackResult(AttackResult result)
        {
            switch (result)
            {
                default:
                case AttackResult.Unknown:
                    return new Uri("resources/images/attacks/attackResult_Unknown.png", UriKind.Relative);
                case AttackResult.Miss:
                    return new Uri("resources/images/attacks/attackResult_Miss.png", UriKind.Relative);
                case AttackResult.Hit:
                    return new Uri("resources/images/attacks/attackResult_Hit.png", UriKind.Relative);
            }
        }

        private TransformedBitmap RotateImg(BitmapImage bi)
        {
            // Properties must be set between BeginInit and EndInit calls.
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bi;
            // Set image rotation.
            RotateTransform transform = new RotateTransform(270);
            tb.Transform = transform;
            tb.EndInit();
            return tb;
        }

        private void HandleBadUIState() { }

        // Invoke the PiecePlaced event
        private void SendShipsPlaced(ShipsPlacedEventArgs e) { if (ShipsPlaced != null) { ShipsPlaced(this, e); } }
        
        // Invoke the AttackMade event
        private void SendAttackMade(AttackMadeEventArgs e) { if (AttackMade != null) { AttackMade(this, e); } }

        // ===============================================================================================
        #endregion
    }

    public class ShipsPlacedEventArgs : EventArgs { }

    public class AttackMadeEventArgs : EventArgs
    {
        public Coordinate attack;
        public AttackMadeEventArgs(Coordinate attack) { this.attack = attack; }
    }

    [ValueConversion(typeof(bool), typeof(ImageSource))]
    public class SectionIsDamagedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool isDamaged = System.Convert.ToBoolean(value);
                if (isDamaged) { return new BitmapImage(new Uri("resources/images/attacks/attackResult_Hit.png", UriKind.Relative)); }
                else { return new BitmapImage(new Uri("resources/images/attacks/attackResult_Unknown.png", UriKind.Relative)); }
            }
            catch (Exception) { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(AttackResult), typeof(ImageSource))]
    public class AttackResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                AttackResult atk = (AttackResult)value;

                switch (atk)
                {
                    default:
                    case AttackResult.Unknown:
                        return new Uri("resources/images/attacks/attackResult_Unknown.png", UriKind.Relative);
                    case AttackResult.Miss:
                        return new Uri("resources/images/attacks/attackResult_Miss.png", UriKind.Relative);
                    case AttackResult.Hit:
                        return new Uri("resources/images/attacks/attackResult_Hit.png", UriKind.Relative);
                }
            }
            catch (Exception) { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
