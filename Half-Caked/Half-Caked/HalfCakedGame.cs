using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Half_Caked
{
    public class HalfCakedGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        public Profile CurrentProfile;
        public StorageDevice Device;
        #endregion

        #region Initalization
        public HalfCakedGame()
        {
            graphics = new GraphicsDeviceManager(this);      
            graphics.PreferredBackBufferWidth = Level.GAME_WIDTH;
            graphics.PreferredBackBufferHeight = Level.GAME_HEIGHT;

            Content.RootDirectory = "Content";

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);
           
            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            IAsyncResult  result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
            result.AsyncWaitHandle.WaitOne();
            Device = StorageDevice.EndShowSelector(result);
            CurrentProfile = Profile.Load(-1, Device);

            if (CurrentProfile == null)
                screenManager.AddScreen(new ProfileSelectionScreen(Device), null);
            //LevelCreator.CreateAndSaveLevel(0);
            //LevelCreator.CreateAndSaveLevel(1);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            base.Draw(gameTime);
        }
        #endregion
    }

    #region Static Run Code
    #if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            LevelCreator.CreateAndSaveLevel(1);
            using (HalfCakedGame game = new HalfCakedGame())
            {
                game.Run();
            }
        }
    }
    #endif
    #endregion
}
