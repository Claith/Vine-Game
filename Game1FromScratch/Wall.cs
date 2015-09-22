using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Game1FromScratch
{
  class Wall : CollisionSprite
  {
    protected bool flipped = false;

    public const int WALL_NOT_SHOWN = 0;
    public const int WALL_SHOWN = 1;
    public const int WALL_TOP = 2;

    /*public Wall() { }
    public Wall(bool FLAG) {
      flipped = FLAG;
    }*/

    public void Flip()
    {
      flipped = true;
    }

    public float leftWall()
    {
      return (float)personalSpace.Left;
    }

    public float rightWall()
    {
      return (float)personalSpace.Right;
    }

    public override void Setup()
    {
      base.Setup();

      Image = Game1.imageArray[3];
      texture = Game1.colorArray[3];

      oldPosition = Vector2.Zero;
      position = Vector2.Zero;

      if (flipped) { rotation = MathHelper.Pi; }

      state = WALL_NOT_SHOWN;
      zOrder = 0.2f;
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      //update flags depending if they are on screen or not
    }

    public override void Draw(SpriteBatch sb)
    {
      if (state != WALL_NOT_SHOWN)
        sb.Draw(Image, position, null, color, rotation, rotationCenter, scale, SpriteEffects.FlipHorizontally, 1f);
    }
  }
}
