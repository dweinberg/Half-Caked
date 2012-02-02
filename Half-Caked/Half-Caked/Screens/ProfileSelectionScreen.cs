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
        private Profile[] profiles;
        private StorageDevice mDevice;

        public ProfileSelectionScreen(StorageDevice device)
            : base("Choose your active profile:")
        {
            IsPopup = true;
            mDevice = device;

            this.Buttons.Clear();
            
            var temp = Profile.LoadAll(mDevice);
            mSelectedButton = temp.Key;
            profiles = temp.Value;

            for(int i = 0; i < profiles.Length; i++)
            {
                Button btn = new Button(profiles[i] != null ? profiles[i].Name : "-Empty-");
                btn.Pressed += ProfileSelectedButton;

                Buttons.Add(btn);
            }
        }
        
        void ProfileSelectedButton(object sender, PlayerIndexEventArgs e)
        {
            int index = Buttons.IndexOf(sender as Button);

            Profile prof = profiles[index];
            Profile currentProf = (ScreenManager.Game as HalfCakedGame).CurrentProfile;
            
            if (prof == null)
            {
                prof = new Profile();
                prof.Name = "Profile " + index;
                prof.ProfileNumber = index;

                if(currentProf != null)
                    Profile.SaveProfile(prof, "profile" + index + ".sav", mDevice);
                else
                    Profile.SaveProfile(prof, "default.sav", mDevice);
            }

            if (currentProf != null)
            {
                if (currentProf.ProfileNumber == prof.ProfileNumber)
                {
                    ExitScreen();
                    return;
                }
                Profile.ChangeDefault(currentProf.ProfileNumber, prof.ProfileNumber, mDevice);
            }

            (ScreenManager.Game as HalfCakedGame).CurrentProfile = prof;       
            ExitScreen();
        }
    }
}
