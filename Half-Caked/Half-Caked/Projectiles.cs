using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    abstract class Projectiles : Sprite
    {
        #region Fields
        protected float mSpeed = 1000f;
        #endregion

        #region Public Methods
        public void Fire(Vector2 startPosition, Vector2 direction, Vector2 acceleration)
        {
            Position = startPosition;
            Velocity = direction * mSpeed;
            Acceleration = acceleration;
            Visible = true;
        }

        public void CheckCollisions(Level level)
        {
            if (Position.X != MathHelper.Clamp(Position.X, 0, level.Size.Width - Size.Width) ||
                Position.Y != MathHelper.Clamp(Position.Y, 0, level.Size.Height - Size.Height))
            {
                Visible = false;
                return;
            }
            
            foreach (Tile tile in level.Tiles)
            {
                Rectangle result = Rectangle.Intersect(tile.Dimensions, CollisionSurface);
                if (!result.IsEmpty)
                {
                    HandleTileCollision(tile, result, level);
                    if (!Visible)
                        return;
                }
            }

            foreach (Obstacle obs in level.Obstacles)
            {
                Rectangle result = Rectangle.Intersect(obs.CollisionSurface, CollisionSurface);
                if (!result.IsEmpty)
                {
                    HandleObstacleCollision(obs, result, level);
                    if (!Visible)
                        return;
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            Visible = false;
            Scale = .1f;
        }
        #endregion

        #region Private Methods
        abstract protected void HandleTileCollision(Tile tile, Rectangle result, Level level);
        abstract protected void HandleObstacleCollision(Obstacle obs, Rectangle result, Level level);
        #endregion
    }

    class PortalGunBullet : Projectiles
    {
        #region Fields
        private int mPortalNumber;
        #endregion

        #region Initialization
        public void LoadContent(ContentManager theContentManager, int classification)
        {
            base.LoadContent(theContentManager, "PortalBullets");
            Source = new Rectangle(200 * classification, 0, 200, Source.Height);
            Center = new Vector2(10, 10);
            Scale = 0.1f;
            Visible = false;
            mPortalNumber = classification;
        }
        #endregion

        #region Private Methods
        protected override void HandleTileCollision(Tile tile, Rectangle result, Level level)
        {
            switch (tile.Type)
            {
                case Surface.Amplifies:
                    Amplify(tile.Dimensions, result, level, Vector2.Zero);
                    break;
                case Surface.Normal:
                    Act(tile.Dimensions, result, level, Vector2.Zero);
                    break;
                case Surface.Reflects:
                    Reflect(result);
                    break;
                default:
                    Absorb();
                    break;
            }
        }

        protected override void HandleObstacleCollision(Obstacle obs, Rectangle result, Level level)
        {
            switch (obs.Contact(result))
            {
                case Surface.Amplifies:
                    Amplify(obs.CollisionSurface, result, level, obs.Velocity);
                    break;
                case Surface.Normal:
                    Act(obs.CollisionSurface, result, level, obs.Velocity);
                    break;
                case Surface.Reflects:
                    Reflect(result);
                    break;
                default:
                    Absorb();
                    break;
            }
        }

        protected void Amplify(Rectangle target, Rectangle result, Level level, Vector2 targetVelocity)
        {
            Scale *= 2; //Special effect
            Act(target, result, level, targetVelocity);
        }

        protected void Absorb()
        {
            Velocity = Vector2.Zero;
            Visible = false;
        }

        protected void Act(Rectangle target, Rectangle result, Level level, Vector2 targetVelocity)
        {
            Visible = false;
            Orientation orientation;
            Vector2 openingPoint;

            bool validXCollision = (Velocity.X > targetVelocity.X == result.Center.X < target.Center.X) || (target.Width - Math.Abs(Velocity.X) / 30 <= 0);
            bool validYCollision = (Velocity.Y > targetVelocity.Y == result.Center.Y < target.Center.Y) || (target.Height - Math.Abs(Velocity.Y) / 30 <= 0);

            if (result.Width < result.Height && validXCollision)
            {
                if (PortalGroup.PORTAL_HEIGHT > target.Height)
                {
                    Absorb();
                    return;
                }
                if (Velocity.X > targetVelocity.X)
                {
                    orientation = Orientation.Left;
                    openingPoint.X = target.Left - 1;
                }
                else
                {
                    orientation = Orientation.Right;
                    openingPoint.X = target.Right - PortalGroup.PORTAL_WIDTH + 1;
                }

                openingPoint.Y = MathHelper.Clamp(result.Center.Y, target.Top + PortalGroup.PORTAL_HEIGHT / 2, target.Bottom - PortalGroup.PORTAL_HEIGHT / 2);
            }
            else if (validYCollision)
            {
                if (PortalGroup.PORTAL_HEIGHT > target.Width)
                {
                    Absorb();
                    return;
                }
                if (Velocity.Y > targetVelocity.Y)
                {
                    orientation = Orientation.Up;
                    openingPoint.Y = target.Top - 1;
                }
                else
                {
                    orientation = Orientation.Down;
                    openingPoint.Y = target.Bottom - PortalGroup.PORTAL_WIDTH + 1;
                }

                openingPoint.X = MathHelper.Clamp(result.Center.X, target.Left + PortalGroup.PORTAL_HEIGHT / 2, target.Right - PortalGroup.PORTAL_HEIGHT / 2);
            }
            else
            {
                Visible = true;
                return;
            }
            Velocity = Vector2.Zero;
            level.Portals.Open(openingPoint, orientation, mPortalNumber, FrameVelocity);
        }

        protected void Reflect(Rectangle result)
        {
            if (result.Width < result.Height)
            {
                Position = new Vector2(result.X - (Position.X < result.X ? Size.Width : -result.Width), Position.Y);
                Velocity *= new Vector2(-1, 1);
            }
            else
            {
                Position = new Vector2(Position.X, result.Y - (Position.Y < result.Y ? Size.Height : -result.Height));
                Velocity *= new Vector2(1, -1);
            }
        }
        #endregion
    }
}