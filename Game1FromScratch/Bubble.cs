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
  class Bubble : Sprite
  {
		//tracks number of bubbles being used
    private static int inPlay = 0;

    public const int STATE_NOT_READY = 0;
    public const int STATE_READY = 1;
    public const int STATE_FALLING = 2;
    public const int STATE_HIT = 3;

    public override void Setup()
    {
      base.Setup();

			SetType(MOVING_SPRITE);

			SetStatus(1, 0);

      int temp = Live.rand.Next(Live.BubbleImageIndex, Live.BubbleImageIndex + Live.BubbleImageIndexDepth);

      Image = Live.imageArray[temp];
      texture = Live.colorArray[temp];

      position.X = Live.randBoundedFloat(Live.leftBorder, Live.rightBorder) - (Image.Width * this.scale);
      position.Y = Live.halfScreen.Y + (Live.randFloat() * Live.halfScreen.Y);

      oldPosition = position;

      scale = 0.5f;

      speed.X = 8f * (Live.randFloat() - 0.5f);
      speed.Y = 60.0f * Live.randFloat();

      rotation = MathHelper.TwoPi * (float)Live.rand.NextDouble();
      rotationSpeed = ((float)Live.rand.NextDouble() - 0.5f) * MathHelper.TwoPi;

      rotationCenter = new Vector2(Image.Width / 2, Image.Height / 2);

      zOrder = 0.9f;
    }

    public override void Update(GameTime gameTime)
    {

      switch (state)
      {
        case STATE_NOT_READY:
          Update_STATE_NOT_READY();
          break;
        case STATE_READY:
          Update_STATE_READY();
          break;
        case STATE_FALLING:
          Update_STATE_FALLING(gameTime);
          //If bomb reaches bottom/base
          if (personalSpace.Top > Live.screenHeight)//((Wall)Live.leftWallList.getNext(Wall.WALL_SHOWN, out found)).personalSpace.Bottom)
          {
            changeState(STATE_NOT_READY);
          }
          break;
        case STATE_HIT:
          Update_STATE_FALLING(gameTime);
          if (stateTime > 200) changeState(STATE_FALLING);
          break;
      }

      base.Update(gameTime);
    }

    public override void Draw(SpriteBatch sb)
    {
      switch (state)
      {
        case STATE_NOT_READY:
        case STATE_READY:
          break;
        case STATE_FALLING:
          base.Draw(sb);
          break;
        case STATE_HIT:
          //base.Draw(sb);
          break;
      }
    }

    private void Update_STATE_NOT_READY()
    {
      if (stateTime > 500)
      {
        //this line fixes the bug from class
        Setup();
        changeState(STATE_READY);
      }
    }

    private void Update_STATE_READY()
    {
      if (inPlay < Live.BubbleCount)
      {
        if (Live.randFloat() < Live.BubbleRate)
        {
          changeState(STATE_FALLING);
          inPlay++;
        }
      }
    }

    private void Update_STATE_FALLING(GameTime gameTime)
    {
      speed.Y += Math.Abs(Live.gravity * (float)gameTime.ElapsedGameTime.TotalSeconds);
      rotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

      oldPosition = position;
      position += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

      //Collision Detection here

      //If bomb reaches bottom/base
      if (personalSpace.Top > Live.screenHeight)//((Wall)Live.leftWallList.getNext(Wall.WALL_SHOWN, out found)).personalSpace.Bottom)
      {
        //Live.baseHit(stamina, damage);
        changeState(STATE_NOT_READY);
        inPlay--;
      }
    }
  }
}
