using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    [XmlInclude(typeof(Enemy)),
    XmlInclude(typeof(Character))]
    public class Actor : Sprite
    {
        #region Fields
        [XmlIgnore]
        public float PortalAngle;
        [XmlIgnore]
        public Vector2 PortalPosition = Vector2.Zero;
        #endregion

        #region Public Methods
        //Draw the sprite to the screen
        public virtual void PortalDraw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            if (Visible)
                theSpriteBatch.Draw(mSpriteTexture, PortalPosition + Relative, Source,
                    Color.White, PortalAngle, Center, Scale, mFlip, .9f);
        }

        public virtual void PortalUpdateDependents()
        {
        }

        public virtual void Update(GameTime theGameTime, Level level)
        {
            base.Update(theGameTime);
            CheckCollisions(level);
        }

        public virtual void CheckCollisions(Level level)
        {
            Position = new Vector2(MathHelper.Clamp(Position.X, 0, level.Size.Width - Size.Width), MathHelper.Clamp(Position.Y, 0, level.Size.Height - Size.Height));
        }

        public override void Reset()
        {
            base.Reset();

            PortalPosition = Vector2.Zero;
            PortalAngle = 0;
        }
        #endregion
    }

    public class Enemy : Actor
    {
        [XmlIgnore]
        List<EnemyBullet> mBullets = new List<EnemyBullet>();

        public int Range = 100;
        private int mReloadingTime = 1000;
        private float mTimeSinceFired = 0;

        private static Vector2 SPRITE_BODY_TARGET = new Vector2(0, -30);

        public Enemy()
        {
            AssetName = "Sprites\\Enemy";
            Center = new Vector2(24, 60);
        }

        public Enemy(Vector2 pos, int range)
        {
            AssetName = "Sprites\\Enemy";
            Center = new Vector2(24, 60);
            InitialPosition = pos;
            Range = range;
            mTimeSinceFired = mReloadingTime;
        }

        public override void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            base.LoadContent(theContentManager, theAssetName);

            for (int i = 0; i < 30; i++)
            {
                mBullets.Add(new EnemyBullet());
                mBullets[i].LoadContent(theContentManager);
            }
        }

        public override void Update(GameTime theGameTime, Level lvl)
        {
            CheckCollisions(lvl);

            if (!Visible)
                return;

            base.Update(theGameTime);

            foreach (EnemyBullet bullet in mBullets)
            {
                if (bullet.Visible)
                {
                    bullet.Update(theGameTime);
                }
            }

            Vector2 offset = lvl.Player.Position + SPRITE_BODY_TARGET - Position;
            if (offset.Length() < Range)
            {
                Fire(lvl);
                mTimeSinceFired += theGameTime.ElapsedGameTime.Milliseconds;
            }
            else if (mTimeSinceFired > mReloadingTime)
                mTimeSinceFired = mReloadingTime;
        }

        public override void CheckCollisions(Level level)
        {
            if (CollisionSurface.Intersects(level.Portals.Portal1.CollisionSurface)
                || CollisionSurface.Intersects(level.Portals.Portal2.CollisionSurface))
            {
                Visible = false; //Replace with fancy effect
                foreach (EnemyBullet bullet in mBullets)
                    bullet.CheckCollisions(level);
            }
            else
                Visible = true;

            foreach (EnemyBullet bullet in mBullets)
                bullet.CheckCollisions(level);
        }

        public override void Draw(SpriteBatch theSpriteBatch, Vector2 relative)
        {
            base.Draw(theSpriteBatch, relative);
            foreach (EnemyBullet bullet in mBullets)
                bullet.Draw(theSpriteBatch, relative);
        }

        public void Fire(Level lvl)
        {
            Vector2 dir = lvl.Player.Position + SPRITE_BODY_TARGET - Position;//MathHelper.Clamp((float)Math.Atan(dir.Y / dir.X), (int)(Oriented - 1) * MathHelper.PiOver2, (int)(Oriented + 1) * MathHelper.PiOver2);

            dir.Normalize();

            Vector2 firingPosition = 60 * dir + Position;

            foreach (Tile tile in lvl.Tiles)
            {
                if (Intersects(firingPosition, lvl.Player.Position + SPRITE_BODY_TARGET, tile.Dimensions))
                    return;
            }

            foreach (Obstacle obs in lvl.Obstacles)
            {
                if (Intersects(firingPosition, lvl.Player.Position + SPRITE_BODY_TARGET, obs.CollisionSurface))
                    return;
            }

            //dir.Y *= -1;

            Angle = MathHelper.WrapAngle((float)Math.Atan(dir.Y / dir.X) + MathHelper.PiOver2);

            if (mTimeSinceFired < mReloadingTime)
                return;

            foreach (EnemyBullet bullet in mBullets)
            {
                if (!bullet.Visible)
                {
                    bullet.Fire(firingPosition, dir, Vector2.Zero, lvl);
                    bullet.Angle = MathHelper.WrapAngle(Angle - MathHelper.PiOver2);
                    mTimeSinceFired -= mReloadingTime;
                    return;
                }
            }
        }

        private bool Intersects(Vector2 p1, Vector2 p2, Rectangle rect)
        {
            if (((p1.Y < rect.Top && p2.Y < rect.Top) || (p1.Y > rect.Bottom && p2.Y > rect.Bottom)) ||
                ((p1.X < rect.Left && p2.X < rect.Left) || (p1.X > rect.Right && p2.X > rect.Right)))
            {
                return false;
            }

            float slope = (p1.Y - p2.Y) / (p1.X - p2.X);

            float y1 = (rect.Left - p1.X) * slope + p1.Y;
            float y2 = (rect.Right - p1.X) * slope + p1.Y;
            
            return !((y1 < rect.Top && y2 < rect.Top) || (y1 > rect.Bottom && y2 > rect.Bottom));
        }

        public override void Reset()
        {
            base.Reset();
            foreach (EnemyBullet bullet in mBullets)
            {
                bullet.Reset();
            }
        }
    }
}
