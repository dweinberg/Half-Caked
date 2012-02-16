using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

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
        public List<Checkpoint> Checkpoints;

        [XmlIgnore]
        public PortalGroup Portals;
        [XmlIgnore]
        public Character Player;

        private Vector2 mDimensions;
        private Vector2 mCenterVector;
        private int mCheckpointIndex = 1;

        private List<TextEffect> mTextEffects;
        private AudioSettings mAudio;
        private Song mExitReached;
        private Song mBackgroundMusic;
        private SoundEffect mCheckpointSound;

        private Sprite mBackground;        
        private SpriteFont mGameFont;

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
            AssetName = "Levels\\" + AssetName;
            base.LoadContent(theContentManager, AssetName);
            mBackground.LoadContent(theContentManager, AssetName + "b");

            mDimensions = activeProfile.Graphics.Resolution;
            mCenterVector = new Vector2(mDimensions.X / 2 - 100, mDimensions.Y * 3 / 4 - 100);

            mAudio = activeProfile.Audio;
            SoundEffect.MasterVolume = mAudio.MasterVolume / 100f;
            MediaPlayer.Volume = mAudio.MasterVolume * mAudio.MusicVolume / 10000f;
            mExitReached = theContentManager.Load<Song>("Sounds\\ExitReached");
            mBackgroundMusic = theContentManager.Load<Song>("Sounds\\Level");
            mCheckpointSound = theContentManager.Load<SoundEffect>("Sounds\\Checkpoint");

            Portals.LoadContent(theContentManager);
            
            Player = new Character();
            Player.LoadContent(theContentManager);
            Player.Position = Player.InitialPosition = Checkpoints[0].Location;

            foreach (Obstacle spr in Obstacles)
                spr.LoadContent(theContentManager, spr.AssetName);

            foreach (Actor spr in Actors)
                spr.LoadContent(theContentManager, spr.AssetName);

            mGameFont = theContentManager.Load<SpriteFont>("Fonts\\gamefont");
        }
        #endregion

        #region Update and Draw
        public void Update(GameTime theGameTime, InputState inputState)
        {
            if (MediaPlayer.State == MediaState.Stopped)
                MediaPlayer.Play(mBackgroundMusic);

            KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            MouseState aCurrentMouseState = Mouse.GetState();
            LevelStatistics.TimeElapsed += theGameTime.ElapsedGameTime.TotalSeconds;

            Portals.ClearSprites();

            foreach (Obstacle spr in Obstacles)
                spr.Update(theGameTime);

            Player.Update(theGameTime, this, inputState);

            foreach (Actor spr in Actors)
            {
                spr.Update(theGameTime, this);
            }

            mTextEffects = mTextEffects.Where(x => !x.Done).ToList();
            foreach (TextEffect effect in mTextEffects)
            {
                effect.Update(theGameTime);
            }

            Portals.Update(theGameTime);
            
            Position = mCenterVector - Player.Position;
            Position = new Vector2(MathHelper.Clamp(Position.X, mDimensions.X - Size.Width, 0), MathHelper.Clamp(Position.Y, mDimensions.Y - Size.Height, 0));

            mBackground.Position = Position;
            
            if(Player.IsGrounded())
                while (Checkpoints[mCheckpointIndex].InBounds(Player.Position))
                {
                    if (++mCheckpointIndex >= Checkpoints.Count)
                    {
                        GameOver();
                    }
                    else
                    {
                        mTextEffects.Add(new CheckpointNotification(Player.Position+Position));
                        PlaySoundEffect(mCheckpointSound);
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

            Player.Draw(theSpriteBatch, Position);
            Portals.Draw(theSpriteBatch, Position);
            
            base.Draw(theSpriteBatch);
            Portals.DrawPortals(theSpriteBatch, Position);

            foreach (TextEffect effect in mTextEffects)
                effect.Draw(theSpriteBatch, mGameFont);
        }

        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            LevelStatistics = new Statistics();

            mBackground.Position = Position;
            Portals.Reset();

            foreach (Obstacle spr in Obstacles)
                spr.Reset();

            Player.Reset();

            foreach (Actor spr in Actors)
                spr.Reset();

            mTextEffects.Clear();
            mCheckpointIndex = 1;
        }

        public void PlayerDeath()
        {
            LevelStatistics.Deaths++;
            Player.DeathReset();
            Player.Position = Checkpoints[mCheckpointIndex-1].Location;
        }

        public void PlaySoundEffect(SoundEffect sfx)
        {
            sfx.Play(mAudio.SoundEffectsVolume / 100f, 0f, 0f);
        }
        #endregion

        #region Private Methods
        private void GameOver()
        {
            MediaPlayer.Play(mExitReached);
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
