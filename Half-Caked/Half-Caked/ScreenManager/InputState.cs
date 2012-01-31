#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;
        public MouseState CurrentMouseState;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;
        public MouseState LastMouseState;

        public readonly bool[] GamePadWasConnected;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];
            CurrentMouseState = new MouseState();

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];
            LastMouseState = new MouseState();

            GamePadWasConnected = new bool[MaxInputs];
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadWasConnected[i] = true;
                }
            }

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                            out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
            }
        }

        ///<summary> 
        //Checks for a left mouse button click input from the user and returns true  
        //if a left click was performed. 
        /// </summary> 
        /// <returns>True: If left mouse button was clicked.</returns> 
        public bool IsNewLeftMouseClick()
        {
            return (CurrentMouseState.LeftButton == ButtonState.Released &&
                LastMouseState.LeftButton == ButtonState.Pressed);
        }

        /// <summary> 
        /// Checks for a right mouse button click input form the user and returns 
        /// true if a right mouse click was performed. 
        /// </summary> 
        /// <returns>True: If right mouse button was clicked</returns> 
        public bool IsNewRightMouseClick()
        {
            return (CurrentMouseState.RightButton == ButtonState.Released &&
                LastMouseState.RightButton == ButtonState.Pressed);
        }

        /// <summary> 
        /// Checks for a third (middle) mouse button click input form the user and returns 
        /// true if a third mouse click was performed. 
        /// </summary> 
        /// <returns>True: If third mouse button was clicked</returns> 
        public bool IsNewThirdMouseClick()
        {
            return (CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                LastMouseState.MiddleButton == ButtonState.Released);
        }

        ///<summary> 
        ///Checks if the mouse has been scrolled up 
        ///</summary> 
        ///<returns>True: If the mouse wheel scrolled up</returns> 
        public bool IsNewMouseScrollUp()
        {
            return (CurrentMouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue);
        }

        ///<summary> 
        ///Checks if the mouse has been scrolled down 
        ///</summary> 
        ///<returns>True: If the mouse wheel scrolled down</returns> 
        public bool IsNewMouseScrollDown()
        {
            return (CurrentMouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue);
        }

        ///<summary> 
        ///Checks if the mouse has been moved or has been left clicked
        ///</summary> 
        ///<returns>True: If the mouse has been moved or left mouse button was clicked</returns> 
        public bool IsNewMouseState()
        {
            return (CurrentMouseState.X != LastMouseState.X && CurrentMouseState.Y != LastMouseState.Y)
                   || (CurrentMouseState.LeftButton != LastMouseState.LeftButton);
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.W, controllingPlayer, out playerIndex) ||
                   IsNewMouseScrollUp() ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.S, controllingPlayer, out playerIndex) ||
                   IsNewMouseScrollDown() ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Checks for a "horizontal button previous" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPreviousButton(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.A, controllingPlayer, out playerIndex) ||
                   IsNewMouseScrollDown() ||
                   IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Checks for a "horizontal button next" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsNextButton(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.D, controllingPlayer, out playerIndex) ||
                   IsNewMouseScrollUp() ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        #endregion
    }
}
