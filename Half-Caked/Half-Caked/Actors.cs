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

    class Enemy : Actor
    {
        List<EnemyBullet> mBullets;

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public void Fire()
        {

        }
    }
}
