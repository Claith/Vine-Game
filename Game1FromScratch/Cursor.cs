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
  class Cursor : CollisionSprite
  {
    ButtonState leftDown = new ButtonState(); //Left Mouse Button down
    ButtonState rightDown = new ButtonState(); //Right Mouse Button down

    bool making = false;
    bool positionSet;

    public override void Setup()
    {
      base.Setup();

      Image = Game1.imageArray[4];

      oldPosition = Vector2.Zero;
      position = new Vector2(Game1.screenWidth / 2, Game1.screenHeight / 2);

      positionSet = false;

      rotation = 0.0f;
      rotationCenter = new Vector2(Image.Width / 2, Image.Height / 2);
      rotationSpeed = 0.0f;

      zOrder = 0.0f; //top of all
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      //update location due to mouse movement
      position.X = (float)Game1.mouseState.X;
      position.Y = (float)Game1.mouseState.Y;

      if (leftDown == ButtonState.Pressed)
      {
        //if left button is pressed, update status
        leftDown = Game1.mouseState.LeftButton;

        if (!positionSet)
        {
          oldPosition = position;
          positionSet = true;
        }

        //if left is not pressed, grow vine
        if (leftDown == ButtonState.Released)
        {
          //grow vine code
          ((CollisionSprite)Game1.vineList.getNext(Vine.VINE_DEAD, out making)).Setup(position, oldPosition, Vine.VINE_SETUP);
          if (!making)
          {
            //just a debug message for testing
            //Game1.ssb.DrawString(Game1.sFont, "Too many vines", new Vector2((Game1.screenWidth * 0.45f), (Game1.screenHeight * 0.45f)), Game1.textColor);
          }
          else { }

          positionSet = false;
        }
      }
      else
      {
        if (rightDown == ButtonState.Pressed)
        {
          rightDown = Game1.mouseState.RightButton;

          if (!positionSet)
          {
            oldPosition = position;
            positionSet = true;
          }

          if (rightDown == ButtonState.Released)
          {
            //grow tendril

            positionSet = false;
          }
        }
        else
        {
          leftDown = Game1.mouseState.LeftButton;
          rightDown = Game1.mouseState.RightButton;
        }
      }

      //use oldPosition to determine the rotation. Eye Candy only so later on.
      //rotation = Math.Atan2((double)(position.Y - oldPosition.Y), (double)(position.X - oldPosition.X));

      //is boundry checking needed when the OS handles the mouse position?
      //check borders to prevent cursor from going off screen.
    }
  }
}
