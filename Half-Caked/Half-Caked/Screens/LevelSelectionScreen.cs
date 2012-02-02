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
    class LevelSelectionScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public LevelSelectionScreen(Profile p)
            : base("Level Selection")
        {
            for (int i = 1; i <= p.CurrentLevel + 1; i++)
            {
                MenuEntry entry = new MenuEntry("Level " + i);
                entry.Selected += EntrySelected;
                MenuEntries.Add(entry);
            }
        }


        #endregion

        #region Handle Input
        
        /// <summary>
        /// Event handler for when a level is selected
        /// </summary>
        void EntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen(MenuEntries.IndexOf(sender as MenuEntry)));
        }
        
        #endregion
    }
}
