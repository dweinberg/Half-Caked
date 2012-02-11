#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Half-Cake'd")
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Play Game");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }


        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new LevelSelectionScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile), e.PlayerIndex);
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            if ((ScreenManager.Game as HalfCakedGame).CurrentProfile.Name.Length < 1)
            {
                const string message = "You have currently unsaved game progress.\nWould you like to save before you exit?";
                MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message, new string[]{"Yes", "No", "Cancel"}, 0);

                confirmExitMessageBox.Buttons[0].Pressed += ConfirmSaveMessageBoxAccepted;
                confirmExitMessageBox.Buttons[1].Pressed += ConfirmExitMessageBoxAccepted;

                ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
            }
            else
            {
                const string message = "Are you sure you want to exit the game?";
                MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

                confirmExitMessageBox.Buttons[0].Pressed += ConfirmExitMessageBoxAccepted;

                ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
            }
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        void ConfirmSaveMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            var pss = new ProfileSelectionScreen((ScreenManager.Game as HalfCakedGame).Device);
            pss.ProfileSaved += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(pss, e.PlayerIndex);
        }

        #endregion
    }
}
