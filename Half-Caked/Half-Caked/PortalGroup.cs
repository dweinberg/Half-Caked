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
    public class PortalGroup
    {
        private enum PortalState
        {
            Closed,
            Open,
            InUse
        }

        #region Constants
        public static float PORTAL_HEIGHT = 150;
        public static float PORTAL_WIDTH = 5;
        #endregion

        #region Fields
        [XmlIgnore]
        public Sprite Portal1, Portal2;
        
        PortalState mState = PortalState.Closed;
        List<Actor> mInPortal1 = new List<Actor>();
        List<Actor> mInPortal2 = new List<Actor>();
        SoundEffect mOpenPortalEffect;
        #endregion

        #region Initialization
        public PortalGroup()
        {
            Portal1 = new Sprite();
            Portal2 = new Sprite();
            
            Portal1.Center  = new Vector2(0, PORTAL_HEIGHT / 2);
            Portal2.Center  = new Vector2(0, PORTAL_HEIGHT / 2);
            Portal1.Visible = Portal2.Visible = false;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            Portal1.LoadContent(theContentManager, "Sprites\\Portal1");
            Portal2.LoadContent(theContentManager, "Sprites\\Portal2");
            mOpenPortalEffect  = theContentManager.Load<SoundEffect>("Sounds\\PortalOpen");
        }
        #endregion

        #region Public Methods
        public bool IsOpen()
        {
            return mState != PortalState.Closed;
        }

        public bool CanClose()
        {
            return mState != PortalState.InUse;
        }

        public void Open(Vector2 position, Orientation orientation, int portalNumber, Vector2 movement, Level lvl)
        {
            Sprite chosenOne = portalNumber == 1 ? Portal2 : Portal1;

            chosenOne.Visible = true;
            chosenOne.Angle = (int)orientation % 2== 0 ? MathHelper.PiOver2 : 0;
            chosenOne.Position = position;
            chosenOne.Oriented = orientation;
            chosenOne.FrameVelocity = movement;

            if (Portal1.Visible == Portal2.Visible)
            {
                if (!Rectangle.Intersect(Portal1.CollisionSurface, Portal2.CollisionSurface).IsEmpty)
                {
                    Close(portalNumber);
                    return;
                }
                mState = PortalState.Open;
                lvl.PlaySoundEffect(mOpenPortalEffect);
            }            

        }

        public void Close(int portalNumber)
        {
            Sprite chosenOne = portalNumber == 1 ? Portal2 : Portal1;
            chosenOne.Angle = 0;

            chosenOne.Visible = false;
            mState = PortalState.Closed;
        }

        public void Reset()
        {
            Portal1.Reset();
            Portal2.Reset();
            mState = PortalState.Closed;
            mInPortal1.Clear();
            mInPortal2.Clear();
            Portal1.Visible = Portal2.Visible = false;
        }

        public void AddSprite(int portalNumber, Actor spr)
        {
            (portalNumber == 0 ? mInPortal1 : mInPortal2).Add(spr);
            mState = PortalState.InUse;
        }

        public void ClearSprites()
        {
            mInPortal1.Clear();
            mInPortal2.Clear();
        }
        #endregion

        #region Draw and Update
        public void Update(GameTime theGameTime)
        {            
            if (mInPortal1.Count + mInPortal2.Count > 0)
                mState = PortalState.InUse;
            else if(mState == PortalState.InUse)
                mState = PortalState.Open;

            Portal1.Update(theGameTime);
            Portal2.Update(theGameTime);

            foreach (Actor spr in mInPortal1)
            {
                HandleSpriteInPortal(spr, Portal1, Portal2, (float)theGameTime.TotalGameTime.TotalSeconds);
            }

            foreach (Actor spr in mInPortal2)
            {
                HandleSpriteInPortal(spr, Portal2, Portal1, (float)theGameTime.TotalGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            foreach (Actor spr in mInPortal1)
            {
                spr.PortalDraw(theSpriteBatch, Relative);
            }

            foreach (Actor spr in mInPortal2)
            {
                spr.PortalDraw(theSpriteBatch, Relative);
            }
        }

        public void DrawPortals(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            Portal1.Draw(theSpriteBatch, Relative);
            Portal2.Draw(theSpriteBatch, Relative);
        }
        #endregion

        #region Private Methods
        private void HandleSpriteInPortal(Actor spr, Sprite portalIn,  Sprite portalOut, float currentTime)
        {
            spr.PortalAngle = MathHelper.WrapAngle(MathHelper.PiOver2 * (2 - (portalIn.Oriented - portalOut.Oriented)));
            Matrix rotation = Matrix.CreateRotationZ(spr.PortalAngle);

            Vector2 dXY = new Vector2(spr.Position.X - portalIn.CollisionSurface.Center.X,
                                      spr.Position.Y - portalIn.CollisionSurface.Center.Y);


            spr.PortalPosition = new Vector2(portalOut.CollisionSurface.Center.X, portalOut.CollisionSurface.Center.Y) + Vector2.Transform(dXY, rotation);
            
            bool swap = false;
            switch (portalIn.Oriented)
            {
                case Orientation.Up:
                    swap = portalIn.CollisionSurface.Center.Y < spr.CollisionSurface.Center.Y;
                    break;
                case Orientation.Right:
                    swap = portalIn.CollisionSurface.Center.X > spr.CollisionSurface.Center.X;
                    break;
                case Orientation.Down:
                    swap = portalIn.CollisionSurface.Center.Y > spr.CollisionSurface.Center.Y;
                    break;
                case Orientation.Left:
                    swap = portalIn.CollisionSurface.Center.X < spr.CollisionSurface.Center.X;
                    break;
                default:
                    break;
            }

            if (swap && (currentTime - spr.LastSwapTime > .2))
            {
                spr.LastSwapTime = currentTime;

                var temp = spr.PortalPosition;
                spr.PortalPosition = spr.Position;
                spr.Position = temp;

                var temp2 = spr.PortalAngle;
                spr.PortalAngle = spr.Angle;
                spr.Angle = temp2;

                spr.Velocity = Vector2.Transform(spr.Velocity, rotation);
                spr.Acceleration = Vector2.Transform(spr.Acceleration, rotation);

                if (portalOut.Oriented == Orientation.Up)
                    spr.Velocity += 25 * new Vector2(0, -1);
            }

            spr.PortalUpdateDependents();
        }
        #endregion
    }
}
