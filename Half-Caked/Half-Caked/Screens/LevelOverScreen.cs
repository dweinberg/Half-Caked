using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    class LevelOverScreen : MessageBoxScreen
    {
        Level mLevel;

        public LevelOverScreen(Level level, HalfCakedGame game)
            : base("Level Complete!", new string[] {"Next Level", "Play Again", "Exit"}, 0)
        {
            IsPopup = true;
            mLevel = level;
            
            // Hook up menu event handlers.
            Buttons[0].Pressed += NextLevelMenuEntrySelected;
            Buttons[1].Pressed += ResetMenuEntrySelected;
            Buttons[2].Pressed += QuitGameMenuEntrySelected;

            Profile prof = game.CurrentProfile;
            String scoreString = "\n";

            if (prof.CurrentLevel < Level.MAX_LEVELS - 1 && prof.CurrentLevel == level.LevelIdentifier)
                prof.CurrentLevel++;

            if (prof.LevelStatistics[mLevel.LevelIdentifier] == null ||
                prof.LevelStatistics[mLevel.LevelIdentifier].Score > level.LevelStatistics.Score)
            {
                scoreString += "Congratulations! You set a new high score.\n";
                prof.LevelStatistics[mLevel.LevelIdentifier] = level.LevelStatistics;
                Profile.SaveProfile(prof, "default.sav", game.Device);
            }
            else
            {
                scoreString += "High Score: " + prof.LevelStatistics[mLevel.LevelIdentifier].Score + "\n";
            }
            scoreString += "Your Score: " + level.LevelStatistics.Score + "\n";

            mMessage += scoreString;

        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (mLevel.LevelIdentifier + 1 >= Level.MAX_LEVELS)
            {
                Buttons[0].State = Button.ButtonState.Inactive;
                Buttons[1].State = Button.ButtonState.Selected;
                mSelectedButton = 1;
            }
        }

        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        void NextLevelMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen(mLevel.LevelIdentifier+1));
        }

        void ResetMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            mLevel.Reset();
            ExitScreen();
        }
        
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
