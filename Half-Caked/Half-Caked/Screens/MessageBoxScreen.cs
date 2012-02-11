#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class MessageBoxScreen : GameScreen
    {
        protected enum ButtonDisplayMode
        {
            VariableSpacing,
            VariableWidth,
            VariableOffset
        }

        #region Fields

        protected string mMessage;
        Texture2D mGradientTexture;
        Rectangle mBackgroundRectangle;
        Vector2 mTextPosition;

        public List<Button> Buttons = new List<Button>();

        protected int mSelectedButton;
        protected int mContentHeightPadding = 0, mContentWidth = 0;
        protected ButtonDisplayMode mButtonDisplayMode = ButtonDisplayMode.VariableWidth;

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #endregion
        
        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard Yes/No
        /// buttons.
        /// </summary>
        public MessageBoxScreen(string message)
            : this(message, new string[]{"Yes", "No"}, 0)
        { }


        /// <summary>
        /// Constructor lets the caller specify what prompts they
        /// want and which is selected by default.
        /// </summary>
        public MessageBoxScreen(string message, string[] prompts, int defaultSelection)
        {
            this.mMessage = message;
            IsPopup = true;
            mSelectedButton = defaultSelection;

            foreach( string str in prompts)
                Buttons.Add(new Button(str));

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            mGradientTexture = content.Load<Texture2D>("UI\\gradient");

            foreach(Button btn in Buttons)
                btn.LoadContent(ScreenManager.Font, content);

            Buttons[mSelectedButton].State = Button.ButtonState.Selected;

            CreateDimensions();
        }
        
        #endregion

        #region Handle Input
        
        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            ScreenManager.Game.IsMouseVisible = true;

            PlayerIndex playerIndex;
            int state = -2;
            var prevButton = mSelectedButton;

            if (input.IsNextButton(ControllingPlayer))
            {
                do
                    mSelectedButton = (mSelectedButton + 1) % Buttons.Count;
                while(Buttons[mSelectedButton].State == Button.ButtonState.Inactive);
            }
            if (input.IsPreviousButton(ControllingPlayer))
            {
                do
                    mSelectedButton = (mSelectedButton - 1 + Buttons.Count) % Buttons.Count;
                while(Buttons[mSelectedButton].State == Button.ButtonState.Inactive);
            }

            for(int i = 0; i < Buttons.Count; i++)
                if (Buttons[i].Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y) && input.IsNewMouseState()
                    && Buttons[i].State != Button.ButtonState.Inactive)
                {
                    mSelectedButton = i;
                    if (input.IsNewLeftMouseClick())
                    {
                        state = i;
                    }
                }

            if (prevButton != mSelectedButton)
            {
                Buttons[prevButton].State = Button.ButtonState.Active;
                Buttons[mSelectedButton].State = Button.ButtonState.Selected;
            }

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex) && !input.IsNewLeftMouseClick())
            {
                state = mSelectedButton;
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                state = -1;
            }

            switch (state)
            {
                case -2:
                    break;
                case -1:
                    if (Cancelled != null)
                        Cancelled(this, new PlayerIndexEventArgs(PlayerIndex.One));

                    ExitScreen();
                    break;
                default:
                    ExitScreen();
                    Buttons[state].RaiseEvent();
                    break;
            }
        }

        #endregion

        #region Draw
        
        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);
            Vector2 spaceSize = font.MeasureString(" ");

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(mGradientTexture, mBackgroundRectangle, color);

            foreach (Button btn in Buttons)
                btn.Draw(spriteBatch, TransitionAlpha);

            // Draw the message box text.
            spriteBatch.DrawString(font, mMessage, mTextPosition, color);

            spriteBatch.End();
        }

        protected void CreateDimensions()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 textSize = font.MeasureString(mMessage);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            float mButtonsWidth = 0;
            float mButtonsHeight = 0;

            foreach (Button btn in Buttons)
            {
                mButtonsWidth += btn.Size.X;
                mButtonsHeight = Math.Max(mButtonsHeight, btn.Size.Y);
            }

            mButtonsWidth += 10 * (Buttons.Count - 1);

            int width = (int)Math.Max(Math.Max(mButtonsWidth, mContentWidth), textSize.X);
            int height = (int)(textSize.Y + Buttons[0].Size.Y + mContentHeightPadding);

            mTextPosition.Y = (viewport.Height - height) / 2;
            mTextPosition.X = (viewport.Width - width)/2;

            mBackgroundRectangle = new Rectangle((int)mTextPosition.X - hPad,
                                                          (int)mTextPosition.Y - vPad,
                                                          width + hPad * 2,
                                                          height + vPad * 2);
            
            mTextPosition.X = mBackgroundRectangle.Center.X - textSize.X / 2;

            int offset_y = (int)(mBackgroundRectangle.Bottom - vPad - mButtonsHeight);
            float cur_offset_x = mBackgroundRectangle.Left + hPad;

            switch (mButtonDisplayMode)
            {
                case ButtonDisplayMode.VariableWidth:
                    float additionalButtonWidth = (mBackgroundRectangle.Width - mButtonsWidth - 2 * hPad) / Buttons.Count;

                    for (int i = 0; i < Buttons.Count; cur_offset_x += Buttons[i].Size.X + 10, i++)
                    {
                        Buttons[i].Widen(additionalButtonWidth);
                        Buttons[i].Position = new Vector2(cur_offset_x, offset_y);
                    }
                    break;

                case ButtonDisplayMode.VariableSpacing:
                    float additionalSpacing = (mBackgroundRectangle.Width - mButtonsWidth - 2 * hPad) / Buttons.Count;
                    cur_offset_x += additionalSpacing / 2;

                    for (int i = 0; i < Buttons.Count; cur_offset_x += Buttons[i].Size.X + 10 + additionalSpacing, i++)
                        Buttons[i].Position = new Vector2(cur_offset_x, offset_y);

                    break;
                case ButtonDisplayMode.VariableOffset:
                    cur_offset_x += (mBackgroundRectangle.Width - mButtonsWidth - 2 * hPad) / 2;

                    for (int i = 0; i < Buttons.Count; cur_offset_x += Buttons[i].Size.X + 10, i++)
                        Buttons[i].Position = new Vector2(cur_offset_x, offset_y);
                    break;

                default:
                    break;
            }
        }

        #endregion
    }

    class AudioOptionsDialog : MessageBoxScreen
    {
        public AudioOptionsDialog(Profile curProfile) 
            : base("mAudio", new string[] { "Save", "Cancel" }, 0) 
        {
            Buttons[0].Pressed += SaveButton;
            mProfile = curProfile;
        }

        private Profile mProfile;

        void SaveButton(object sender, PlayerIndexEventArgs e) { }
    }

    class GraphicsDialog : MessageBoxScreen
    {
        public GraphicsDialog(Profile curProfile) 
            : base("Graphics", new string[] { "Save", "Test", "Cancel" }, 0)
        {
            Buttons[0].Pressed += SaveButton;
            Buttons[1].Pressed += TestButton;
            mProfile = curProfile;
        }

        private Profile mProfile;

        void SaveButton(object sender, PlayerIndexEventArgs e) { }
        void TestButton(object sender, PlayerIndexEventArgs e) { }
    }

    class KeybindingsDialog : MessageBoxScreen
    {
        public KeybindingsDialog(Profile curProfile) 
            : base("Keybindings", new string[] { "Save", "Cancel" }, 0)
        {
            Buttons[0].Pressed += SaveButton;
            mProfile = curProfile;
        }
        
        private Profile mProfile;

        void SaveButton(object sender, PlayerIndexEventArgs e) { }
        void KeybindingPressed(object sender, PlayerIndexEventArgs e) { }
    }
}
