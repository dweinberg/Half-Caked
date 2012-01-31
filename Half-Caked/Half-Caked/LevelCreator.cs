using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public static class LevelCreator
    {
        #region Public Methods

        public static void CreateAndSaveLevel(int levelIdentifier)
        {
            Level lvl;
            switch(levelIdentifier)
            {
                case 0:
                    lvl = CreateLevel0();
                    break;
                case 1:
                    lvl = CreateLevel1();
                    break;
                default:
                    return;
            }
            SaveLevel(levelIdentifier, lvl);
            Statistics.SaveHighScores(levelIdentifier, new Statistics[5]);
        }

        #endregion

        #region Private Methods

        private static Level CreateLevel1()
        {
            Level lvl = new Level();

            lvl.Gravity = 9.80f;
            lvl.InitialPosition = new Vector2(0, -100);

            lvl.AssetName = "Level1";
            lvl.LevelIdentifier = 1;

            lvl.mCheckpoints.Add(new Checkpoint(270, 661 - 134, 0, 0, 4));
            lvl.mCheckpoints.Add(new Checkpoint(680, 661 - 134, 680, 1024, 1));
            lvl.mCheckpoints.Add(new Checkpoint(1180, 661 - 134, 1180, 602, 1));

            lvl.Tiles.Add(new Tile(new Rectangle(0, 0, 301,147), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(301, 0, 361,147), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(662, 0, 49, 4), .40f, Surface.Death));
            lvl.Tiles.Add(new Tile(new Rectangle(711, 0, 569, 147), .40f, Surface.Amplifies));

            lvl.Tiles.Add(new Tile(new Rectangle(0, 490, 215, 174), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 490+174, 215, 363), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(215, 1024 - 363, 458-215, 363), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(936, 1024 - 421, 344, 421), .05f, Surface.Reflects));
            lvl.Tiles.Add(new Tile(new Rectangle(458, 490, 215, 174), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(458, 490+174, 215, 343), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(458+215, 1024 - 363, 830-458-215, 363), .40f, Surface.Normal));
            
            //Boundaries
            lvl.Tiles.Add(new Tile(new Rectangle(0, 1024 - 2, 1280, 2), .40f, Surface.Death));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 0, 1280, 2), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 0, 2, 1024), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(1280 - 2, 0, 2, 1024), .40f, Surface.Absorbs));

            return lvl;
        }

        private static Level CreateLevel0()
        {
            Level lvl = new Level();

            lvl.Gravity = 9.80f;
            lvl.InitialPosition = new Vector2(0, -500);

            lvl.AssetName = "Level0";
            lvl.LevelIdentifier = 0;

            lvl.Tiles.Add(new Tile(new Rectangle(778, 0, 186, 281), .80f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(964, 0, 420, 195), .40f, Surface.Normal));

            lvl.Tiles.Add(new Tile(new Rectangle(0, 473, 301, 151), .80f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(301, 473, 411, 151), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(711, 473, 569, 151), .20f, Surface.Amplifies));

            lvl.Tiles.Add(new Tile(new Rectangle(0, 1500 - 364, 216, 364), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(216, 1500 - 364, 240, 364), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(456, 1500 - 364, 216, 364), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(672, 1500 - 364, 159, 364), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 966, 216, 170), .40f, Surface.Normal));
            lvl.Tiles.Add(new Tile(new Rectangle(456, 966, 215, 170), .40f, Surface.Normal));

            lvl.Tiles.Add(new Tile(new Rectangle(936, 1500 - 421, 485, 421), .05f, Surface.Reflects));

            lvl.Tiles.Add(new Tile(new Rectangle(1718, 915, 282, 585), .80f, Surface.Absorbs));

            lvl.mCheckpoints.Add(new Checkpoint(270, 1136 - 134, 0, 0, 4));
            lvl.mCheckpoints.Add(new Checkpoint(680, 1136 - 134, 680, 1024, 1));
            lvl.mCheckpoints.Add(new Checkpoint(1180, 1079 - 134, 1180, 1500, 1));
            lvl.mCheckpoints.Add(new Checkpoint(1165, 473 - 134, 1275, 473, 2));
            lvl.mCheckpoints.Add(new Checkpoint(75, 473 - 134, 100, 473, 2));

            Switch switch1 = new Switch(System.Guid.NewGuid(), new Vector2(1980, 815), Switch.SwitchState.Active);
            switch1.Actions.Add(new KeyValuePair<Guid, int>(Stickman.mGuid, (int)Switch.SwitchState.Pressed));
            lvl.Obstacles.Add(switch1);

            Platform pf1 = new Platform(System.Guid.NewGuid(), new List<Vector2>() { new Vector2(1435, 1078), new Vector2(1607, 1078), new Vector2(1607, 473), new Vector2(1286, 473) }, 50, Platform.PlatformState.Forward);
            pf1.Actions.Add(new KeyValuePair<Guid, int>(switch1.Guid, (int)Platform.PlatformState.Startionary));
            lvl.Obstacles.Add(pf1);

            Door d1 = new Door(System.Guid.NewGuid(), new Vector2(778, 282), Door.DoorState.Stationary);
            d1.Actions.Add(new KeyValuePair<Guid, int>(switch1.Guid, (int)Door.DoorState.Opening));
            lvl.Obstacles.Add(d1);

            //Boundaries
            lvl.Tiles.Add(new Tile(new Rectangle(0, 1500 - 2, 2000, 2), .40f, Surface.Death));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 0, 1500, 2), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(0, 0, 2, 2000), .40f, Surface.Absorbs));
            lvl.Tiles.Add(new Tile(new Rectangle(2000 - 2, 0, 2, 1500), .40f, Surface.Absorbs));

            return lvl;
        }

        private static void SaveLevel(int identifier, Level lvl)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            TextWriter textWriter = new StreamWriter(@"Content\Levels\Level"+identifier+".xml");
            serializer.Serialize(textWriter, lvl);
            textWriter.Close();
        }

        #endregion
    }
}
