using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BattleShip
{
    class Opponent
    {
        public string Name { get; private set; }
        public string ImgLoc { get; private set; }
        public AIDifficulty Difficulty { get; private set; }

        private Opponent() { }

        public static Opponent[] GetOpponents()
        {
            List<Opponent> opponents = new List<Opponent>();

            // Read In XML
            XmlTextReader reader = new XmlTextReader(@"resources/Opponents.xml");
            XmlDocument opponentInfo = new XmlDocument();
            opponentInfo.Load(reader);
            reader.Close();

            // Create Opponents List
            foreach (XmlNode opponentTag in opponentInfo.GetElementsByTagName("Opponent"))
            {
                try
                {
                    Opponent opponent = new Opponent();
                    opponent.Name = opponentTag.Attributes["name"].Value;
                    opponent.ImgLoc = opponentTag.Attributes["img"].Value;

                    switch (opponentTag.Attributes["difficulty"].Value.ToUpper())
                    {
                        case "NORMAL":
                            opponent.Difficulty = AIDifficulty.NORMAL;
                            break;
                        case "HARD":
                            opponent.Difficulty = AIDifficulty.HARD;
                            break;
                        default:
                            opponent.Difficulty = AIDifficulty.EASY;
                            break;
                    }

                    opponents.Add(opponent);
                }
                catch (Exception) { }
            }
            return opponents.ToArray();
        }
    }
}
