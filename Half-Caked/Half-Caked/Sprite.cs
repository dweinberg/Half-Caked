using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public enum Orientation
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        NA = 4
    }

    public class Sprite
    {
        #region Fields

        private Vector2 mPosition = new Vector2(0, 0);
        private Vector2 mInitialPosition = Vector2.Zero;

        [XmlIgnore]
        public Vector2 Position
        {
            get { return mPosition; }
            set
            {
                mPosition = value;
            }
        }
        public Vector2 InitialPosition
        {
            get { return mInitialPosition; }
            set { mInitialPosition = mPosition = value; }
        }

        [XmlIgnore]
        public Orientation Oriented = Orientation.NA;

        [XmlIgnore]
        public Vector2 Velocity = Vector2.Zero;

        [XmlIgnore]
        public Vector2 FrameVelocity = Vector2.Zero;

        [XmlIgnore]
        public Vector2 Acceleration = Vector2.Zero;

        [XmlIgnore]
        public Rectangle CollisionSurface
        {
            get
            {
                Rectangle rectReturn = Rectangle.Empty;

                float absAngle = Math.Abs(Angle);

                rectReturn.Height = Math.Abs((int)(Math.Sin(absAngle) * Size.Width + Math.Cos(absAngle) * Size.Height));
                rectReturn.Width = Math.Abs((int)(Math.Sin(absAngle) * Size.Height + Math.Cos(absAngle) * Size.Width));

                var rot = Matrix.CreateRotationZ(Angle);
                var toCenter = Vector2.Transform(new Vector2(Size.Width / 2f, Size.Height / 2f) - Center, rot);

                rectReturn.X = (int)(Position.X - rectReturn.Width/2 + toCenter.X);
                rectReturn.Y = (int)(Position.Y - rectReturn.Height / 2 + toCenter.Y);
                return rectReturn;
            }
        }

        [XmlIgnore]
        public float LastSwapTime = 0;
        protected Texture2D mSpriteTexture;

        protected SpriteEffects mFlip = SpriteEffects.None;

        private float mAngle;
        [XmlIgnore]
        public float Angle
        {
            get { return mAngle; }
            set
            {
                mAngle = value;
            }
        }

        private Vector2 mCenter;
        [XmlIgnore]
        public Vector2 Center
        {
            get { return mCenter; }
            set
            {
                mCenter = value;
            }
        }

        [XmlIgnore]
        public bool Visible = true;

        //The asset name for the Sprite's Texture
        public string AssetName;

        //The Size of the Sprite (with scale applied)
        [XmlIgnore]
        public Rectangle Size;

        //The Rectangular area from the original image that defines the Sprite. 
        Rectangle mSource;
        [XmlIgnore]
        public Rectangle Source
        {
            get { return mSource; }
            set
            {
                mSource = value;
                Size = new Rectangle(0, 0, (int)(mSource.Width * Scale), (int)(mSource.Height * Scale));
            }
        }

        //The amount to increase/decrease the size of the original sprite. 
        private float mScale = 1.0f;

        //When the scale is modified throught he property, the Size of the 
        //sprite is recalculated with the new scale applied.
        [XmlIgnore]
        public float Scale
        {
            get { return mScale; }
            set
            {
                mScale = value;
                Size = new Rectangle(0, 0, (int)(Source.Width * Scale), (int)(Source.Height * Scale));
            }
        }
        #endregion

        #region Initialization
        //Load the texture for the sprite using the Content Pipeline
        public virtual void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            mSpriteTexture = theContentManager.Load<Texture2D>(theAssetName);
            AssetName = theAssetName;
            Source = new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height);
            Size = new Rectangle(0, 0, (int)(mSpriteTexture.Width * Scale), (int)(mSpriteTexture.Height * Scale));
        }
        #endregion

        #region Draw and Update
        //Update the Sprite and change it's position based on the passed in speed, direction and elapsed time.
        public virtual void Update(GameTime theGameTime)
        {
            if (!Visible)
                return;
            Velocity += Acceleration * (float)theGameTime.ElapsedGameTime.TotalSeconds;

            Position += (Velocity + FrameVelocity) * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }
        
        //Draw the sprite to the screen
        public virtual void Draw(SpriteBatch theSpriteBatch)
        {
            Draw(theSpriteBatch, Vector2.Zero);
        }

        //Draw the sprite to the screen
        public virtual void Draw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            if (Visible)
                theSpriteBatch.Draw(mSpriteTexture, Position+Relative, Source,
                    Color.White, Angle, Center, Scale, mFlip, .9f);
        }
        #endregion

        #region Public Methods
        public virtual void Reset()
        {
            Position = InitialPosition;
            Acceleration = Vector2.Zero;
            Velocity = Vector2.Zero;
            FrameVelocity = Vector2.Zero;
            Angle = 0;
            Visible = true;
            LastSwapTime = 0;
        }
        #endregion
    }
}
