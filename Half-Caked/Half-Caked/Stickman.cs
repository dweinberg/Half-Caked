using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public class Stickman : Actor
    {
        public enum State
        {
            Immobile = -1,
            Ground = 0,
            Air = 1,
            Portal = 2,
            Platform = 3,
            GravityPortal = 4
        }

        #region Constants
        public static Guid mGuid =
            new Guid("05159D8A-B739-4AA4-9F6D-3FF5CB29572D");

        const string ASSETNAME= "Stickman";
        const int DEFAULT_SPEED = 160;
        const int MOVE_UP = -1;
        const int MOVE_DOWN = 1;
        const int MOVE_LEFT = -1;
        const int MOVE_RIGHT = 1;
        const float MASS = 80.0f;
        const float STATIC_ACCEL_GND = MASS  * .05f;
        const float DYNAMIC_ACCEL_AIR = MASS * .20f;
        const float STATIC_ACCEL_AIR = MASS * .0025f;
        const float ARM_LENGTH = 37;
        #endregion

        #region Fields
        State mCurrentState = State.Ground;
                
        KeyboardState mPreviousKeyboardState;
        MouseState mPreviousMouseState;

        PortalGunBullet[] mOrbs = new PortalGunBullet[2];
        Gunarm mGunhand;
        ContentManager mContentManager;

        bool mIsDucking = false, mForcedDucking;
        float mCurrentFriction;
        Rectangle[] mCollisions = new Rectangle[5];
        #endregion

        #region Initialization
        public void LoadContent(ContentManager theContentManager)
        {
            mContentManager = theContentManager;
            base.LoadContent(theContentManager, ASSETNAME);
            Source = new Rectangle(66, 65, 73, 135);

            mGunhand = new Gunarm();
            mGunhand.LoadContent(this.mContentManager);

            mOrbs[0] = new PortalGunBullet();
            mOrbs[0].LoadContent(this.mContentManager, 0);
            mOrbs[1] = new PortalGunBullet();
            mOrbs[1].LoadContent(this.mContentManager, 1);

            Center = new Vector2(Size.Width / 2, Size.Height / 2);
        }
        #endregion

        #region Update and Draw

        public void Update(GameTime theGameTime, Level level)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            MouseState aCurrentMouseState = Mouse.GetState();

            Acceleration = Vector2.Zero;
            CheckCollisions(level, aCurrentKeyboardState.IsKeyDown(Keys.E));
            UpdateMovement(aCurrentKeyboardState);
            UpdateJump(aCurrentKeyboardState);
            UpdateDuck(aCurrentKeyboardState);

            mPreviousKeyboardState = aCurrentKeyboardState;
            Acceleration.Y = (mCurrentState == State.Air || mCurrentState == State.GravityPortal  || mCurrentState == State.Portal ? 1 : 0) * level.Gravity * Level.METERS_TO_UNITS;
            if (Angle != 0)
            {
                if(Angle > 0)
                    Angle = (float)Math.Max(0, Angle - Math.PI * theGameTime.ElapsedGameTime.TotalSeconds / 2);
                else
                    Angle = (float)Math.Min(0, Angle + Math.PI * theGameTime.ElapsedGameTime.TotalSeconds / 2);

            }

            base.Update(theGameTime);

            mFlip = mGunhand.Update(aCurrentMouseState, mIsDucking, this, level.Position);
            UpdateProjectile(theGameTime, aCurrentMouseState, level);

            mPreviousMouseState = aCurrentMouseState;
        }

        public void CheckCollisions(Level level, bool ePressed)
        {
            FrameVelocity = Vector2.Zero;

            if (level.Portals.IsOpen())
            {
                if (HandlePortalCollision(0, level) || HandlePortalCollision(1, level))
                    return;
            }

            if(mForcedDucking)
                StopDucking();
            mCurrentState = State.Air;

            foreach (Obstacle obs in level.Obstacles)
            {
                Rectangle result = Rectangle.Intersect(obs.CollisionSurface, CollisionSurface);
                if (!result.IsEmpty)
                {
                    if (HandleStandardCollision(result, obs.CollisionSurface, obs.Contact(result), obs.Friction * level.Gravity))
                    {
                        level.PlayerDeath();
                        return;
                    }
                    FrameVelocity = obs.Velocity;
                    if(!mPreviousKeyboardState.IsKeyDown(Keys.E) && ePressed)
                        obs.React(mGuid, level);
                }
            }

            foreach (Tile tile in level.Tiles)
            {
                Rectangle result = Rectangle.Intersect(tile.Dimensions, CollisionSurface);
                if (!result.IsEmpty)
                {
                    if (HandleStandardCollision(result, tile.Dimensions, tile.Type, tile.Friction * level.Gravity))
                    {
                        level.PlayerDeath();
                        return;
                    }
                }
            }

            if (mCollisions[(int)Orientation.Down] != Rectangle.Empty && mCollisions[(int)Orientation.Up] != Rectangle.Empty)
            {
                mForcedDucking = true;
                Duck();
                if (mCollisions[(int)Orientation.Down].Intersects(this.CollisionSurface) &&
                    mCollisions[(int)Orientation.Up].Intersects(this.CollisionSurface))
                    level.PlayerDeath();
            }
            else
                mForcedDucking = false;

            for(int i = 0; i < 5; i++)
                mCollisions[i] = Rectangle.Empty;
        }

        public override void PortalUpdateDependents()
        {
            mGunhand.Update(mIsDucking, this);
        }

        public override void Draw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            if (!Visible)
                return;
            base.Draw(theSpriteBatch, Relative);
            mOrbs[0].Draw(theSpriteBatch, Relative);
            mOrbs[1].Draw(theSpriteBatch, Relative);
            mGunhand.Draw(theSpriteBatch, Relative);
        }

        public override void PortalDraw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            base.PortalDraw(theSpriteBatch, Relative);
            mGunhand.PortalDraw(theSpriteBatch, Relative);
        }

        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            mOrbs[0].Reset();
            mOrbs[1].Reset();
            mGunhand.Reset();

            mCurrentState = State.Ground;
            StopDucking();
        }

        public void DeathReset()
        {
            base.Reset();
            mForcedDucking = false;
            StopDucking();
        }

        public bool IsGrounded()
        {
            return State.Platform == mCurrentState || State.Ground == mCurrentState;
        }

        #endregion

        #region Private Methods
        private bool HandlePortalCollision(int portalNumber, Level level)
        {
            Sprite Portal = (portalNumber == 0 ? level.Portals.Portal1 : level.Portals.Portal2);

            Rectangle result = Rectangle.Intersect(CollisionSurface, Portal.CollisionSurface);
            if (!result.IsEmpty)
            {
                // Horizontal
                if ((int)Portal.Oriented % 2 == 1)
                {
                    if (result.Height >= this.CollisionSurface.Height - 3 ||
                        mCurrentState == State.Portal || mCurrentState == State.GravityPortal)
                    {
                        mCurrentState = State.Portal;
                        level.Portals.AddSprite(portalNumber, this);
                        FrameVelocity = Portal.FrameVelocity;

                        bool first;
                        if ((first = (CollisionSurface.Y >= Portal.CollisionSurface.Bottom - CollisionSurface.Height)) || Portal.CollisionSurface.Y >= CollisionSurface.Y)
                        {
                            if (first)
                                Velocity.Y = Math.Min(Velocity.Y, 0);
                            else
                                Velocity.Y = Math.Max(Velocity.Y, 0);

                            Position = new Vector2(Position.X, MathHelper.Clamp(Position.Y, Portal.CollisionSurface.Y + Center.Y, Portal.CollisionSurface.Bottom - Center.Y));
                        }
                        return true;
                    }
                }
                //Vertical
                else
                {
                    if (result.Width >= this.CollisionSurface.Width - 1 ||
                        mCurrentState == State.Portal || mCurrentState == State.GravityPortal)
                    {
                        mCurrentState = State.GravityPortal;
                        level.Portals.AddSprite(portalNumber, this);

                        if (CollisionSurface.X > Portal.CollisionSurface.Right - CollisionSurface.Width || Portal.CollisionSurface.X > CollisionSurface.X)
                        {
                            Position = new Vector2(MathHelper.Clamp(Position.X, Portal.CollisionSurface.X + Center.X, Portal.CollisionSurface.Right - Center.X), Position.Y);
                            Velocity.X = 0;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HandleStandardCollision(Rectangle result, Rectangle obj, Surface type, float friction)
        {
            if (3 >= result.Height || result.Width >= this.CollisionSurface.Width - 1
                || result.Height / (float)CollisionSurface.Height < result.Width / (float)CollisionSurface.Width)
            {
                if (CollisionSurface.Center.Y < obj.Center.Y)
                {
                    mCurrentFriction = friction * MASS;
                    mCurrentState = State.Ground;
                    Velocity.Y = 0;
                    Position = new Vector2(Position.X, obj.Top - Center.Y + 1);
                    mCollisions[(int)Orientation.Down] = obj;
                }
                else
                {
                    Velocity.Y = Math.Min(Velocity.Y, 0);
                    Position = new Vector2(Position.X, obj.Bottom + Center.Y + 1);
                    mCollisions[(int)Orientation.Up] = obj;
                }
            }
            else
            {
                if (Position.X > result.X)
                {
                    mCollisions[(int)Orientation.Left] = obj;
                }
                else
                {
                    mCollisions[(int)Orientation.Right] = obj;
                }
                Position = new Vector2(result.X - (Position.X - result.X < 0 ? CollisionSurface.Width - Center.X + 1 : -result.Width - 1 - Center.X), Position.Y);
            }

            return type == Surface.Death;
        }
                                                    
        private void UpdateMovement(KeyboardState aCurrentKeyboardState)
        {
            if (mCurrentState == State.Ground || mCurrentState == State.Platform || mCurrentState == State.Portal)
            {
                if (Math.Abs(Velocity.X) <= STATIC_ACCEL_GND)
                {
                    Acceleration.X = 0;
                    Velocity.X = 0;
                }
                else
                    Acceleration.X = mCurrentFriction * (-Math.Sign(Velocity.X));

                if (aCurrentKeyboardState.IsKeyDown(Keys.A) == true || aCurrentKeyboardState.IsKeyDown(Keys.Left) == true)
                {
                    Velocity.X = DEFAULT_SPEED * MOVE_LEFT * (mIsDucking ? .5f : 1);
                }
                else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true || aCurrentKeyboardState.IsKeyDown(Keys.D) == true)
                {
                    Velocity.X = DEFAULT_SPEED * MOVE_RIGHT * (mIsDucking ? .5f : 1);
                }
            }
            else if(mCurrentState == State.Air || mCurrentState == State.GravityPortal)
            {
                if (Math.Abs(Velocity.X) <= STATIC_ACCEL_AIR)
                {
                    Acceleration.X = 0;
                    Velocity.X = 0;
                }
                else
                {
                    Acceleration.X = DYNAMIC_ACCEL_AIR * (Math.Abs(Velocity.X) <= STATIC_ACCEL_AIR ? 0 : 1) * (-Math.Sign(Velocity.X));
                }
                    if (aCurrentKeyboardState.IsKeyDown(Keys.A) == true || aCurrentKeyboardState.IsKeyDown(Keys.Left) == true)
                    {
                        //Acceleration.X += DYNAMIC_ACCEL_AIR * MOVE_LEFT / 4;
                        Velocity.X = Math.Min(Velocity.X, DEFAULT_SPEED / 2f * MOVE_LEFT * (mIsDucking ? .5f : 1));
                    }
                    else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true || aCurrentKeyboardState.IsKeyDown(Keys.D) == true)
                    {
                        //Acceleration.X += DYNAMIC_ACCEL_AIR * MOVE_RIGHT / 4;
                        Velocity.X = Math.Max(Velocity.X, DEFAULT_SPEED / 2f * MOVE_RIGHT * (mIsDucking ? .5f : 1));
                    }

                    //if (mIsDucking)
                    //    Acceleration.X *= .75f;
                //}
            }
        }

        private void UpdateJump(KeyboardState aCurrentKeyboardState)
        {
            if ((mCurrentState == State.Ground || mCurrentState == State.Platform || mCurrentState == State.Portal) && Velocity.Y == 0)
            {
                if ((aCurrentKeyboardState.IsKeyDown(Keys.W) == true || aCurrentKeyboardState.IsKeyDown(Keys.Up) == true) &&
                    (mPreviousKeyboardState.IsKeyDown(Keys.W) == false && mPreviousKeyboardState.IsKeyDown(Keys.Up) == false))
                {
                    mCurrentState = State.Air;
                    Velocity.Y = -DEFAULT_SPEED;
                }
            }
        }

        private void UpdateDuck(KeyboardState aCurrentKeyboardState)
        {
            if (aCurrentKeyboardState.IsKeyDown(Keys.S) == true || aCurrentKeyboardState.IsKeyDown(Keys.Down) == true || mForcedDucking)
            {
                Duck();
            }
            else
            {
                StopDucking();
            }
        }

        private void Duck()
        {
            if (!mIsDucking)
            {
                Position += new Vector2(0 ,16);
                Source = new Rectangle(272, 97, 67, 103);
                Center = new Vector2(Size.Width / 2, Size.Height / 2);
                mIsDucking = true;
            }
        }

        private void StopDucking()
        {
            if (mIsDucking)
            {
                Position -= new Vector2(0, 16);
                Source = new Rectangle(66, 65, 72, 135);
                Center = new Vector2(Size.Width / 2, Size.Height / 2);
                mIsDucking = false;
            }
        }

        private void UpdateProjectile(GameTime theGameTime, MouseState aCurrentMouseState, Level level)
        {
            if (mOrbs[0].Visible)
            {
                mOrbs[0].Update(theGameTime);
                mOrbs[0].CheckCollisions(level);
            }
            else if (aCurrentMouseState.LeftButton == ButtonState.Pressed && mPreviousMouseState.LeftButton == ButtonState.Released && level.Portals.CanClose())
            {
                ShootProjectile(0, aCurrentMouseState);
                mOrbs[0].CheckCollisions(level);
                level.Portals.Close(0);
            }

            if (mOrbs[1].Visible)
            {
                mOrbs[1].Update(theGameTime);
                mOrbs[1].CheckCollisions(level);
            }
            else if (aCurrentMouseState.RightButton == ButtonState.Pressed && mPreviousMouseState.RightButton == ButtonState.Released && level.Portals.CanClose())
            {
                ShootProjectile(1, aCurrentMouseState);
                mOrbs[1].CheckCollisions(level);
                level.Portals.Close(1);
            }
        }

        private void ShootProjectile(int type, MouseState aCurrentMouseState)
        {
            Vector2 dir = new Vector2((float)Math.Cos(mGunhand.Angle), (float)Math.Sin(mGunhand.Angle));

            mOrbs[type].Fire(mGunhand.Position + ARM_LENGTH * dir, 
                            dir,
                            Vector2.Zero);
        }
        
        private void Die(Level level)
        {
            level.PlayerDeath();
        }
        #endregion
    }

    class Gunarm : Actor
    {

        #region Constants
        Vector2 ARM_ANCHOR = new Vector2(-4, -8);
        Vector2 ARM_ANCHOR_DUCKED = new Vector2(-6, 12);

        Vector2 ARM_ANCHOR_LEFT = new Vector2(6, 0);
        Vector2 ARM_ANCHOR_DUCKED_LEFT = new Vector2(13, 0);
        #endregion

        #region Initialization

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, "Gunarm");
            Source = new Rectangle(0, 0, 37, 5);
            Scale = 1.0f;
            Center = new Vector2(0, 3);
        }

        #endregion

        #region Update and Draw

        public SpriteEffects Update(MouseState aCurrentMouseState, bool ducking, Stickman theMan, Vector2 rel)
        {
            Position = theMan.Position + Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED : ARM_ANCHOR), Matrix.CreateRotationZ(theMan.Angle));

            Angle = (float)Math.Atan2(aCurrentMouseState.Y - (double)(Position.Y + rel.Y), aCurrentMouseState.X - (double)(Position.X + rel.X));

            if (aCurrentMouseState.X < Position.X + rel.X)
            {
                Position += Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.Angle));
                return SpriteEffects.FlipHorizontally;
            }
            else
                return SpriteEffects.None;
        }

        public void Update(bool ducking, Stickman theMan)
        {
            PortalPosition = theMan.PortalPosition + Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED : ARM_ANCHOR), Matrix.CreateRotationZ(theMan.PortalAngle));

            if (Math.Abs(MathHelper.WrapAngle(Angle)) > MathHelper.PiOver2)
                PortalPosition += Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.PortalAngle));

            PortalAngle = Angle + theMan.PortalAngle;
        }

        #endregion
    }
}
