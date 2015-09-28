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

namespace Infection
{
  class Cursor : Sprite
  {
    ButtonState leftDown = new ButtonState(); //Left Mouse Button down
    ButtonState rightDown = new ButtonState(); //Right Mouse Button down

    bool making = false;
		//Marks when button is pressed and ready to define end point of the dragging
    bool positionSet;

    public override void Setup()
    {
      base.Setup();
			
			SetType(MOVING_SPRITE);			

      Image = Live.imageArray[20];

      position = new Vector2(Live.screenWidth / 2, Live.screenHeight / 2);

      positionSet = false;

      rotation = 0.0f;
      rotationCenter = ImageCenter();
      rotationSpeed = 0.0f;

      zOrder = 0.0f; //top of all
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      //update location due to mouse movement
      position.X = (float)Live.mouseState.X;
      position.Y = (float)Live.mouseState.Y;

      if ((Live.State == Live.GAME_RUN) || (Live.State == Live.GAME_START))
      {
        if (leftDown == ButtonState.Pressed)
        {
          //if left button is pressed, update status
          leftDown = Live.mouseState.LeftButton;

          if (!positionSet)
          {
            oldPosition = position;
            positionSet = true;
          }

          //if left is not pressed, grow vine
          if (leftDown == ButtonState.Released)
          {
            //grow vine code
            ((Sprite)Live.vineList.getNext(Vine.VINE_DEAD, out making)).Setup(position, oldPosition, Vine.VINE_SETUP);
            if (!making)
            {
              //just a debug message for testing
              //Live.ssb.DrawString(Live.sFont, "Too many vines", new Vector2((Live.screenWidth * 0.45f), (Live.screenHeight * 0.45f)), Live.textColor);
            }
            else { }

            positionSet = false;
          }
        }
        else
        {
          if (rightDown == ButtonState.Pressed)
          {
            rightDown = Live.mouseState.RightButton;

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
            leftDown = Live.mouseState.LeftButton;
            rightDown = Live.mouseState.RightButton;
          }
        }
      }

      //use oldPosition to determine the rotation. Eye Candy only so later on.
      rotation = (float)Math.Atan2((double)(position.Y - oldPosition.Y), (double)(position.X - oldPosition.X)) * MathHelper.TwoPi;

      //is boundry checking needed when the OS handles the mouse position?
      //check borders to prevent cursor from going off screen.

			//use collision detection to determine if cursor is in growable zones.
    }
  }
}
