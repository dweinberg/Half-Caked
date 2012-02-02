#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
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
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry profileMenuEntry;
        MenuEntry resolutionMenuEntry;
        MenuEntry soundMenuEntry;

        static string[] resolution = { "1280 x 1024", "1024 x 768", "1280 x 720" };
        static int currentResolution = 0;

        static bool soundEnabled = true;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen(string currentProfileName)
            : base("Options")
        {
            // Create our menu entries.
            profileMenuEntry = new MenuEntry(string.Empty);
            resolutionMenuEntry = new MenuEntry(string.Empty);
            soundMenuEntry = new MenuEntry(string.Empty);

            profileMenuEntry.Text = "Current Profile: " + currentProfileName;
            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            profileMenuEntry.Selected += ProfileMenuEntrySelected;
            resolutionMenuEntry.Selected += ResolutionMenuEntrySelected;
            soundMenuEntry.Selected += SoundMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(profileMenuEntry);
            MenuEntries.Add(resolutionMenuEntry);
            MenuEntries.Add(soundMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            resolutionMenuEntry.Text = "Resolution: " + resolution[currentResolution];
            soundMenuEntry.Text = "Sound: " + (soundEnabled ? "on" : "off");
        }

        void SetProfileNameText(object sender, PlayerIndexEventArgs e)
        {
            profileMenuEntry.Text = "Current Profile: " + (ScreenManager.Game as HalfCakedGame).CurrentProfile.Name;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        void ProfileMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            var scrn = new ProfileSelectionScreen((ScreenManager.Game as HalfCakedGame).Device);
            for(int i = 0; i < scrn.Buttons.Count; i++)
                scrn.Buttons[i].Pressed += SetProfileNameText;

            ScreenManager.AddScreen(scrn, null);
        }


        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void ResolutionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentResolution = (currentResolution + 1) % resolution.Length;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void SoundMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            soundEnabled = !soundEnabled;

            SetMenuEntryText();
        }
        
        #endregion
    }
}
