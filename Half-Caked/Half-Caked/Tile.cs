using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    public enum Surface
    {
        Reflects = 0,
        Absorbs,
        Amplifies,
        Normal,
        Portal,
        Death
    }

    public class Tile
    {
        #region Fields
        public Rectangle Dimensions;
        public float Friction;
        public Surface Type;
        #endregion

        #region Initialization
        public Tile() 
            : this(Rectangle.Empty, 0, Surface.Death)
        {
        }

        public Tile(Rectangle shape, float friction, Surface type)
        {
            Dimensions = shape;
            Friction = friction;
            Type = type;
        }
        #endregion
    }
}
