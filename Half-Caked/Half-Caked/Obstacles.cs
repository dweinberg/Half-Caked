using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Half_Caked
{
    [Serializable]
    public class KeyValuePair<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public KeyValuePair() {}
        public KeyValuePair(K one, V two)
        {
            Key = one; Value = two;
        }

    }

    [XmlInclude(typeof(Platform)),
     XmlInclude(typeof(Switch)),
     XmlInclude(typeof(Door))]
    public abstract class Obstacle : Sprite
    {
        #region Fields
        [XmlIgnore]
        public Surface Type;
        [XmlIgnore]
        public float Friction;

        public List<KeyValuePair<Guid, int>> Actions;
        public Guid Guid;

        protected int mState, mInitialState;
        public int InitialState
        {
            get
            {
                return mInitialState;
            }
            set
            {
                mInitialState = mState = value;
            }
        }
        #endregion

        #region Initialization
        public Obstacle()
            : this (new Guid())
        {
        }

        public Obstacle(Guid toGuid)
        {
            Guid = toGuid;
            Actions = new List<KeyValuePair<Guid, int>>();
        }
        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            mState = InitialState;
        }

        public virtual Surface Contact(Rectangle ContactArea)
        {
            return Type;
        }

        public virtual void React(Guid caller, Level level)
        {
            KeyValuePair<Guid, int> temp;
            if((temp = Actions.First(x => x.Key == caller)) != null)
                mState = temp.Value;
        }
        #endregion
    }

    public class Platform : Obstacle
    {        
        public enum PlatformState
        {
            Startionary = 0,
            Forward,
            Reverse,
            Circuit,
            CircuitReverse
        }

        #region Fields
        public List<Vector2> Path;
        public float Speed;
        private int mCurrentPath;
        #endregion

        #region Initialization
        public Platform()
        {
            Scale = .1f;
            AssetName = "Sprites\\Platform";

            Type = Surface.Normal;
            Friction = .60f;
        }

        public Platform(Guid toGuid, List<Vector2> inPath, float speed, PlatformState state)
            : base(toGuid)
        {
            Path = inPath;
            mCurrentPath = 0;
            Speed = speed;
            InitialPosition = Path[0];
            InitialState = (int)state;

            Scale = .1f;
            AssetName = "Sprites\\Platform";

            Type = Surface.Normal;
            Friction = .60f;
        }
        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            mCurrentPath = 0;
            Position = Path[mCurrentPath];
        }

        public override void Update(GameTime theGameTime)
        {
            if ((PlatformState)mState == PlatformState.Startionary)
                return;

            base.Update(theGameTime);
            if (Vector2.Dot(Path[mCurrentPath] - Position, Velocity) <= 0)
            {
                Position = Path[mCurrentPath];
                Size.X = (int)Position.X;
                Size.Y = (int)Position.Y;

                switch ((PlatformState)mState)
                {
                    case PlatformState.Circuit:
                        mCurrentPath = (mCurrentPath + 1) % Path.Count;
                        break;
                    case PlatformState.CircuitReverse:
                        mCurrentPath = (mCurrentPath - 1) % Path.Count;
                        break;
                    case PlatformState.Reverse:
                        if (--mCurrentPath < 0)
                        {
                            mCurrentPath = 1;
                            mState = (int)PlatformState.Forward;
                        }
                        break;
                    case PlatformState.Forward:
                        if (++mCurrentPath >= Path.Count)
                        {
                            mCurrentPath -= 2;
                            mState = (int)PlatformState.Reverse;
                        }
                        break;
                    default:
                        break;
                }
                Velocity = Vector2.Normalize(Path[Math.Abs(mCurrentPath)] - Position)*Speed;
            }
        }

        public override Surface Contact(Rectangle ContactArea)
        {
            return ContactArea.Width > ContactArea.Height ? Type : Surface.Absorbs;
        }
        #endregion
    }

    public class Switch : Obstacle
    {
        public enum SwitchState
        {
            InActive,
            Pressed,
            Active
        }

        #region Fields
        private SoundEffect mTriggerSound;
        #endregion

        #region Initialization
        public Switch()
        {
            AssetName = "Sprites\\Switch";
            Type = Surface.Absorbs;
            Friction = .60f;
        }

        public Switch(Guid toGuid, Vector2 pos, SwitchState state)
            : base(toGuid)
        {
            InitialPosition = pos;
            InitialState = (int)state;

            UpdateSource(state);

            AssetName = "Sprites\\Switch";
            Type = Surface.Absorbs;
            Friction = .60f;
        }

        public override void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            base.LoadContent(theContentManager, theAssetName);
            UpdateSource((SwitchState)mState);
            mTriggerSound = theContentManager.Load<SoundEffect>("Sounds\\Switch");
        }
        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            UpdateSource((SwitchState)InitialState);
        }
        
        public override void React(Guid caller, Level level)
        {
            base.React(caller, level);
            UpdateSource((SwitchState)mState);

            if ((SwitchState)mState == SwitchState.Pressed)
            {
                level.PlaySoundEffect(mTriggerSound);
                foreach (Obstacle obs in level.Obstacles)
                    if (obs.Guid != Guid)
                        obs.React(this.Guid, level);
            }
        }
        #endregion

        #region Private Methods
        private void UpdateSource(SwitchState state)
        {
            mState = (int)state;

            switch (state)
            {
                case SwitchState.InActive:
                    Source = new Rectangle(40, 0, 20, 100);
                    break;
                case SwitchState.Pressed:
                    Source = new Rectangle(20, 0, 20, 100);
                    break;
                default:
                    Source = new Rectangle(0, 0, 20, 100);
                    break;
            }
        }
        #endregion
    }

    public class Door : Obstacle
    {
        public enum DoorState
        {
            Closing = -1,
            Stationary = 0,
            Opening = 1
        }

        #region Constants
        const float SPEED_Y = 50;
        #endregion

        #region Initialization
        public Door() 
        { 
            AssetName = "Sprites\\Door";

            Type = Surface.Absorbs;
            Friction = .60f;
        }

        public Door(Guid toGuid, Vector2 pos, DoorState state)
            : base(toGuid) 
        {
            InitialPosition = pos;
            InitialState = (int)state;

            AssetName = "Sprites\\Door";
            Type = Surface.Absorbs;
            Friction = .60f;
        }
        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            Source = new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height);
        }
        
        public override void Update(GameTime theGameTime)
        {
            if ((Source.Height <= 0   && mState == (int)DoorState.Opening) ||
                (Source.Height >= 200 && mState == (int)DoorState.Closing))
            {
                mState = (int)DoorState.Stationary;
            }

            Velocity = ((int)(DoorState)mState) * Vector2.UnitY * SPEED_Y;
            Source = new Rectangle(Source.X, Source.Y, Source.Width, (int)MathHelper.Clamp(Source.Height - Velocity.Y * (float) theGameTime.ElapsedGameTime.TotalSeconds, 0, 200));

            base.Update(theGameTime);
        }
        #endregion
    }
}
