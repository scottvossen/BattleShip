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
using System.Windows.Shapes;
using System.IO;

namespace BattleShip
{
    /// <summary>
    /// Interaction logic for SetupGame.xaml
    /// </summary>
    public partial class SetupGame : Window
    {
        private const string PLAYER_IMG_PATH = @"resources\images\players";
        private Dictionary<ListBoxItem, Opponent> opponentMap = new Dictionary<ListBoxItem, Opponent>();

        public SetupGame()
        {
            InitializeComponent();
            PopulateOpponents();
            PopulatePlayerImages();
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) 
        {
            if (DialogResult != true) { Environment.Exit(0); }
        }

        public AI_Player getOpponent()
        {
            ListBoxItem selectedItem = ((ListBoxItem)chooseOpponent.SelectedItem);
            
            switch(opponentMap[selectedItem].Difficulty)
            {
                case AIDifficulty.NORMAL:
                    return new NormalAI(opponentMap[selectedItem].Name,
                        PLAYER_IMG_PATH + "\\" + opponentMap[selectedItem].ImgLoc);
                case AIDifficulty.HARD:
                    return new HardAI(opponentMap[selectedItem].Name,
                        PLAYER_IMG_PATH + "\\" + opponentMap[selectedItem].ImgLoc);
                default:
                    return new EasyAI(opponentMap[selectedItem].Name,
                        PLAYER_IMG_PATH + "\\" + opponentMap[selectedItem].ImgLoc);
            }
        }

        public Human_Player getHumanPlayer()
        {
            BitmapImage bi = ((Image)((ListBoxItem)choosePlayerImage.SelectedItem).Content).Source as BitmapImage;
            Uri uri = bi.UriSource;
            return new Human_Player(playerNameTextBox.Text, uri.OriginalString);
        }

        private void PopulateOpponents()
        {
            Opponent[] opponents = Opponent.GetOpponents();
            foreach (Opponent opp in opponents)
            {
                ListBoxItem item = GetOpponentItem(opp);
                opponentMap.Add(item, opp);
                chooseOpponent.Items.Add(item);
            }
        }

        private void PopulatePlayerImages()
        {
            string[] imgs = Array.FindAll(Directory.GetFiles(PLAYER_IMG_PATH), isPlayerImg);

            foreach (string img in imgs)
            {
                choosePlayerImage.Items.Add(GetImageItem(img));
            }
        }

        private bool isPlayerImg(string str)
        {
            string subStr = str.Substring(str.LastIndexOf("\\") + 1);
            return subStr.StartsWith("player_") && subStr.EndsWith(".png");
        }

        private ListBoxItem GetOpponentItem(Opponent opp)
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(PLAYER_IMG_PATH + "\\" + opp.ImgLoc, UriKind.Relative));
            img.Stretch = Stretch.Uniform;

            Label lbl = new Label();
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.FontSize = 18;
            lbl.FontWeight = FontWeights.Bold;
            lbl.Content = opp.Name;

            DockPanel dock = new DockPanel();
            dock.Height = 50;
            dock.Margin = new Thickness(0,2,0,2);
            dock.Children.Add(img);
            dock.Children.Add(lbl);

            ListBoxItem imgItem = new ListBoxItem();
            imgItem.Background = new SolidColorBrush(GetOpponentColor(opp.Difficulty));
            imgItem.Content = dock;
            return imgItem;
        }

        private Color GetOpponentColor(AIDifficulty diff)
        {
            Color easy = Color.FromRgb(70, 242, 51);
            Color norm = Color.FromRgb(242, 163, 51);
            Color hard = Color.FromRgb(255, 0, 0);

            switch(diff)
            {
                case AIDifficulty.NORMAL:
                    return Color.FromScRgb(0.7f, norm.ScR, norm.ScG, norm.ScB);
                case AIDifficulty.HARD:
                    return Color.FromScRgb(0.7f, hard.ScR, hard.ScG, hard.ScB);
                default:
                    return Color.FromScRgb(0.7f, easy.ScR, easy.ScG, easy.ScB);
            }
        }

        private ListBoxItem GetImageItem(string imageLoc)
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(imageLoc, UriKind.Relative));
            img.Stretch = Stretch.Uniform;

            ListBoxItem imgItem = new ListBoxItem();
            imgItem.Height = 120;
            imgItem.Content = img;
            return imgItem;
        }
        
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormIsValid())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool FormIsValid()
        {
            String errorMsg = "";
            if (String.IsNullOrEmpty(playerNameTextBox.Text)) { errorMsg += "\nMissing Player Name"; }
            if (chooseOpponent.SelectedItem == null) { errorMsg += "\nMissing Player Selection"; }
            if (choosePlayerImage.SelectedItem == null) { errorMsg += "\nMissing Opponent Selection"; }

            if (String.IsNullOrEmpty(errorMsg)) { return true; }

            MessageBox.Show(this, "To continue, the following errors must be addressed:" 
                                  + errorMsg, "Insufficient Data");
            return false;
        }
    }
}
