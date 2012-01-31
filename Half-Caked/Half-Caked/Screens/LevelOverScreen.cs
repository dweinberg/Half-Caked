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
        public LevelOverScreen(Level level)
            : base("Level Complete!", new string[] {"Next Level", "Play Again", "Exit"}, 0)
        {
            IsPopup = true;
            mLevel = level;
            
            // Hook up menu event handlers.
            Buttons[0].Pressed += NextLevelMenuEntrySelected;
            Buttons[1].Pressed += ResetMenuEntrySelected;
            Buttons[2].Pressed += QuitGameMenuEntrySelected;
        }

        public override void LoadContent()
        {
            CreateContentDimensions();
            base.LoadContent();
            if (mLevel.LevelIdentifier + 1 > Level.MAX_LEVELS)
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

        private void CreateContentDimensions()
        {
            Statistics[] scores = Statistics.LoadHighScores(mLevel.LevelIdentifier);

            int insert = -1;
            for (int i = 0; i < scores.Length; i++)
                if (insert >= 0)
                    scores[i - 1] = scores[i];
                else if (scores[i] == null)
                {
                    insert = i;
                    break;
                }
                else if (scores[i].TimeElapsed < mLevel.LevelStatistics.TimeElapsed)
                    insert = i;

            if (insert >= 0)
            {
                scores[insert] = mLevel.LevelStatistics;
                Statistics.SaveHighScores(mLevel.LevelIdentifier, scores);
            }
            
            this.mContentHeightPadding = 200;

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
