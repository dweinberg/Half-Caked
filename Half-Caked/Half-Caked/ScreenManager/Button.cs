using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

//CUSTOM CODE

namespace Half_Caked
{
    class Button
    {
        public enum TextAlignment
        {
            Left = 0,
            Centered = 1,
            Right = 2
        }

        public enum ButtonState
        {
            Selected,
            Active,
            Inactive
        }

        #region Fields

        public string Text;
        private ButtonState mState;
        private Rectangle mSource;
        private Color mTextColor, mDimColor;

        public static Color SELECTED_COLOR = Color.FromNonPremultiplied(79, 129, 184, 255);
        
        public ButtonState State
        {
            get { return mState; }
            set
            {
                mState = value;
                switch (mState)
                {
                    case ButtonState.Selected:
                        mTextColor = SELECTED_COLOR;
                        mDimColor = Color.White;
                        mSource = new Rectangle(250, 0, 250, 100);
                        break;
                    case ButtonState.Inactive:
                        mTextColor = Color.DimGray;
                        mDimColor = Color.DimGray;
                        mSource = new Rectangle(0, 0, 250, 100);
                        break;
                    default:
                        mTextColor = Color.White;
                        mDimColor = Color.White;
                        mSource = new Rectangle(0, 0, 250, 100);
                        break;

                }
            }
        }

        public TextAlignment Alignment;

        Rectangle mRectangle;
        public Vector2 Position
        {
            get { return new Vector2(mRectangle.X, mRectangle.Y); }
            set { mRectangle.X = (int)value.X; mRectangle.Y = (int)value.Y; }
        }

        public Vector2 Size
        {
            get { return new Vector2(mRectangle.Width, mRectangle.Height); }
        }

        private Texture2D mBackground;
        private SpriteFont mFont;
        Vector2 mOrigin;
        int mPaddingX, mPaddingY;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the button is pressed.
        /// </summary>
        public event EventHandler<PlayerIndexEventArgs> Pressed;
        
        /// <summary>
        /// Method for raising the Pressed event.
        /// </summary>
        protected internal virtual void OnPressedButton(PlayerIndex playerIndex)
        {
            if (Pressed != null)
                Pressed(this, new PlayerIndexEventArgs(playerIndex));
        }

        #endregion

        public Button(string txt)
            : this(txt, 10, 5, TextAlignment.Centered)
        {
        }

        public Button(string txt, int xPad, int yPad, TextAlignment algn)
        {
            Text = txt;
            mPaddingX = xPad;
            mPaddingY = yPad;
            mRectangle = new Rectangle(0, 0, xPad * 2, yPad * 2);
            Alignment = algn;
            State = ButtonState.Active;
        }

        public void LoadContent(SpriteFont font, ContentManager contentManager)
        {
            mFont = font;
            mBackground = contentManager.Load<Texture2D>("Buttons");

            Vector2 mTextSize = mFont.MeasureString(Text);
            mRectangle.Width  += (int)mTextSize.X;
            mRectangle.Height += (int)mTextSize.Y;

            mOrigin = new Vector2(mTextSize.X / 2 * (int)Alignment, mTextSize.Y / 2);
        }

        public void Draw(SpriteBatch spriteBatch, byte alpha)
        {
            mDimColor.A = alpha;
            mTextColor.A = alpha;

            Vector2 textPosition = new Vector2(mRectangle.X + mPaddingX + (mRectangle.Width/2 - mPaddingX) * (int)Alignment, mRectangle.Center.Y);

            spriteBatch.Draw(mBackground, mRectangle, mSource, mDimColor);
            spriteBatch.DrawString(mFont, Text, textPosition, mTextColor, 0, mOrigin, 1, SpriteEffects.None, 1.0f);       
        }

        public void Widen(float width)
        {
            mRectangle.Width += (int)width;
        }

        public bool Contains(int x, int y)
        {
            return mRectangle.Contains(x, y);
        }

        public void RaiseEvent()
        {
            if(Pressed != null)
                Pressed(this, new PlayerIndexEventArgs(PlayerIndex.One));
        }
    }
}
