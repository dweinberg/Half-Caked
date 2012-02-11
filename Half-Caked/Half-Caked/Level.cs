using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public class Level : Sprite
    {
        #region Constants
        public static float METERS_TO_UNITS = 20;
        public static int MAX_LEVELS = 2;
        #endregion

        #region Fields
        public float Gravity;
        public int LevelIdentifier = -1;

        [XmlIgnore]
        public Statistics LevelStatistics;

        public List<Tile> Tiles;

        public List<Obstacle> Obstacles;
        public List<Actor> Actors;
        public PortalGroup Portals;

        protected Vector2 mCenterVector;

        public List<Checkpoint> Checkpoints;
        protected int mCheckpointIndex = 1;

        protected List<TextEffect> mTextEffects;
        protected AudioSettings mAudio;

        protected Character mCharacter;
        protected Sprite mBackground;

        protected SpriteFont mGameFont;
        protected bool mGameOver = false;

        private Vector2 mDimensions;
        #endregion

        #region Initialization
        public Level()
        {
            LevelStatistics = new Statistics();
            mBackground = new Sprite();
            Obstacles = new List<Obstacle>();
            Actors = new List<Actor>();
            Checkpoints = new List<Checkpoint>();
            mTextEffects = new List<TextEffect>();
            Tiles = new List<Tile>();
            Portals = new PortalGroup();

        }

        public virtual void LoadContent(ContentManager theContentManager, Profile activeProfile)
        {
            base.LoadContent(theContentManager, AssetName);
            mBackground.LoadContent(theContentManager, AssetName + "b");

            mDimensions = activeProfile.Graphics.Resolution;
            mCenterVector = new Vector2(mDimensions.X / 2 - 100, mDimensions.Y * 3 / 4 - 100);
            mAudio = activeProfile.Audio;

            Portals.LoadContent(theContentManager);
            
            mCharacter = new Character();
            mCharacter.LoadContent(theContentManager);
            mCharacter.Position = mCharacter.InitialPosition = Checkpoints[0].Location;

            foreach (Obstacle spr in Obstacles)
                spr.LoadContent(theContentManager, spr.AssetName);

            foreach (Actor spr in Actors)
                spr.LoadContent(theContentManager, spr.AssetName);

            mGameFont = theContentManager.Load<SpriteFont>("gamefont");
        }
        #endregion

        #region Update and Draw
        public void Update(GameTime theGameTime, InputState inputState)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            MouseState aCurrentMouseState = Mouse.GetState();
            LevelStatistics.TimeElapsed += theGameTime.ElapsedGameTime.TotalSeconds;

            Portals.ClearSprites();

            foreach (Obstacle spr in Obstacles)
                spr.Update(theGameTime);

            mCharacter.Update(theGameTime, this, inputState);

            foreach (Actor spr in Actors)
            {
                spr.Update(theGameTime);
                spr.CheckCollisions(this);
            }

            mTextEffects = mTextEffects.Where(x => !x.Done).ToList();
            foreach (TextEffect effect in mTextEffects)
            {
                effect.Update(theGameTime);
            }

            Portals.Update(theGameTime);
            
            Position = mCenterVector - mCharacter.Position;
            Position = new Vector2(MathHelper.Clamp(Position.X, mDimensions.X - Size.Width, 0), MathHelper.Clamp(Position.Y, mDimensions.Y - Size.Height, 0));

            mBackground.Position = Position;
            
            if(mCharacter.IsGrounded())
                while (Checkpoints[mCheckpointIndex].InBounds(mCharacter.Position))
                {
                    if (++mCheckpointIndex >= Checkpoints.Count)
                    {
                        GameOver();
                    }
                    else
                    {
                        mTextEffects.Add(new CheckpointNotification(mCharacter.Position+Position));
                    }
                }

        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            mBackground.Draw(theSpriteBatch);
            foreach (Obstacle spr in Obstacles)
                spr.Draw(theSpriteBatch, Position);

            foreach (Sprite spr in Actors)
                spr.Draw(theSpriteBatch, Position);

            mCharacter.Draw(theSpriteBatch, Position);
            Portals.Draw(theSpriteBatch, Position);
            
            base.Draw(theSpriteBatch);
            Portals.DrawPortals(theSpriteBatch, Position);

            foreach (TextEffect effect in mTextEffects)
                effect.Draw(theSpriteBatch, mGameFont);
        }

        #endregion

        #region Public Methods
        public bool IsOver()
        {
            return mGameOver;
        }

        public override void Reset()
        {
            base.Reset();
            LevelStatistics = new Statistics();

            mBackground.Position = Position;
            Portals.Reset();

            foreach (Obstacle spr in Obstacles)
                spr.Reset();

            mCharacter.Reset();

            foreach (Actor spr in Actors)
                spr.Reset();

            mTextEffects.Clear();
            mCheckpointIndex = 1;
        }

        public void PlayerDeath()
        {
            LevelStatistics.Deaths++;
            mCharacter.DeathReset();
            mCharacter.Position = Checkpoints[mCheckpointIndex-1].Location;
        }
        #endregion

        #region Private Methods
        private void GameOver()
        {
            mGameOver = true;
            throw new Exception("LevelComplete");
        }
        #endregion
        
        #region Static Methods
        public static Level LoadLevel(int levelIdentifier)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            try
            {
                FileStream fs = new FileStream(@"Content\Levels\Level" + levelIdentifier + ".xml", FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);

                Level lvl = (Level)serializer.Deserialize(reader);

                fs.Close();
                reader.Close();

                return lvl;
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}
