using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Half_Caked
{
    class ProfileSelectionScreen : MessageBoxScreen
    {
        private Profile[] mProfiles;
        private int mDefault;
        private StorageDevice mDevice;
        private bool mReadingInput = false;

        public event EventHandler<PlayerIndexEventArgs> ProfileSaved;

        public ProfileSelectionScreen(StorageDevice device)
            : base("Choose your active profile:")
        {
            IsPopup = true;
            mDevice = device;

            this.Buttons.Clear();
            
            var temp = Profile.LoadAll(mDevice);
            mDefault = mSelectedButton = temp.Key;

            if (temp.Key < 0)
            {
                mSelectedButton = 0;
            }

            mProfiles = temp.Value;

            for(int i = 0; i < mProfiles.Length; i++)
            {
                Button btn = new Button(mProfiles[i] != null ? mProfiles[i].Name : "-Empty-");
                btn.Pressed += ProfileSelectedButton;

                Buttons.Add(btn);
            }
        }
        
        void ProfileSelectedButton(object sender, PlayerIndexEventArgs e)
        {
            int index = mSelectedButton;

            Profile prof = mProfiles[index];
            Profile currentProf = (ScreenManager.Game as HalfCakedGame).CurrentProfile;
            
            if (prof == null)
            {
                if (mDefault == -1)
                    prof = currentProf;
                else
                {
                    prof = new Profile();
                }
                mReadingInput = true;
                prof.ProfileNumber = index;

                if(currentProf != null && mDefault != -1)
                    Profile.SaveProfile(prof, "profile" + index + ".sav", mDevice);
                else
                    Profile.SaveProfile(prof, "default.sav", mDevice);
                mProfiles[index] = prof;
                Buttons[index].Text = "";
                CreateDimensions();
                IsExiting = false;
                return;
            }

            if (currentProf != null && mDefault != -1)
            {
                if (currentProf.ProfileNumber == prof.ProfileNumber)
                {
                    ExitScreen();
                    return;
                }
                Profile.ChangeDefault(currentProf.ProfileNumber, prof.ProfileNumber, mDevice);
                (ScreenManager.Game as HalfCakedGame).CurrentProfile = prof;      
            }
            if (mDefault == -1)
                Profile.SaveProfile(prof, "default.sav", mDevice);
        }

        public override void HandleInput(InputState input)
        {
            if (mReadingInput)
            {
                string text = input.GetTextSinceUpdate(ControllingPlayer);
                if (text.Length == 0)
                    return;

                if (text[text.Length - 1] == '\n')
                {
                    text = text.Remove(text.Length - 1);
                    mReadingInput = false;
                }

                if (text.Length > 0 && text[0] == '\b')
                {
                    if (mProfiles[mSelectedButton].Name.Length > 0)
                        mProfiles[mSelectedButton].Name = mProfiles[mSelectedButton].Name.Remove(mProfiles[mSelectedButton].Name.Length - 1) + text.Substring(1);
                    else
                        text = text.Substring(1);
                }
                else
                    mProfiles[mSelectedButton].Name += text;

                Buttons[mSelectedButton].Text = mProfiles[mSelectedButton].Name;
                CreateDimensions();

                if (!mReadingInput)
                {
                    if (mProfiles[mSelectedButton].Name.Length < 1)
                        mProfiles[mSelectedButton].Name = " ";
                    ProfileSelectedButton(Buttons[mSelectedButton], null);

                    if (ProfileSaved != null)
                        ProfileSaved(this, new PlayerIndexEventArgs(ControllingPlayer.HasValue ? ControllingPlayer.Value : PlayerIndex.One));

                    ExitScreen();
                }
            }
            else
            {
                base.HandleInput(input);
                PlayerIndex pl;

                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Delete, ControllingPlayer, out pl))
                {
                    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen("Are you sure you want to delete Profile:\n         " + mProfiles[mSelectedButton].Name);

                    confirmExitMessageBox.Buttons[0].Pressed += ConfirmExitMessageBoxAccepted;

                    ScreenManager.AddScreen(confirmExitMessageBox, pl);
                }
            }
        }

        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            mProfiles[mSelectedButton].Delete(mDevice);
            mProfiles[mSelectedButton] = null;
            Buttons[mSelectedButton].Text = "-Empty-";

            if (mSelectedButton == mDefault)
            {
                Profile prof = new Profile();
                prof.ProfileNumber = mDefault;
                (ScreenManager.Game as HalfCakedGame).CurrentProfile = prof;
                mDefault = -1;
            }

            mSelectedButton = -1;
            for (int i = 0; i < mProfiles.Length; i++)
            {
                if (mProfiles[i] != null)
                    mSelectedButton = i;
            }

            CreateDimensions();

            if (mSelectedButton == -1)
                mSelectedButton = 0;
        }
    }
}
