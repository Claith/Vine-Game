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
  class Wall : Sprite
  {
    protected bool flipped = false;

    private int lastMoveTimer = 0;

    //how far it has grown so far
    protected Vector2 currentGrowth = Vector2.Zero;

    protected float growthPercent = 0.0f;
    protected float growthRate = 0.0f;
    public float GrowthRate
    {
      set { growthRate = value; }
    }

    protected TimeSpan growDelay = TimeSpan.FromMilliseconds(250f);
    public TimeSpan GrowDelay
    {
      set { growDelay = value; }
    }

    public const int WALL_SHOWN = 0;
    public const int WALL_NOT_SHOWN = 1;

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

			SetType(COLLISION_SPRITE);

      //int temp = Live.rand.Next(Live.WallImageIndex, Live.WallImageIndex + Live.WallImageIndexDepth);
      int temp = 15;
      
      Image = Live.imageArray[temp];
      texture = Live.colorArray[temp];

      currentGrowth = Vector2.One; //full grown from the start

      scaledGrowth.X = Live.halfScreen.X / Image.Width; //grows upto half screen width
      scaledGrowth.Y = Live.screenHeight / Image.Height * 1.5f; //covers some above and below

      speed.X = 0.13f * (Live.randFloat() - 0.5f);
      speed.Y = 0f;

      if (flipped) //Right Side -- rightWallList
      {
        position.X += Live.screenWidth - (Live.screenWidth * 0.15f);
        //personalSpace.Width = (int)Live.screenWidth;
        //personalSpace.Height = (int)Live.screenHeight;
//        position.Y = base.position.Y;// +Live.screenHeight;
      }
      else //Left Side -- leftWallList
      {
        //position.X = 0f;
        //personalSpace.Width = (int)(Live.screenWidth * 0.15f);
        //personalSpace.Height = (int)Live.screenHeight;
        position.X -= (Live.screenWidth * 0.35f);
//        position.Y = base.position.Y;
      }

      position.Y = 0f;

      //if (flipped) { rotation = MathHelper.Pi; }
      personalSpace.X = (int)position.X;
      personalSpace.Y = (int)position.Y;
      personalSpace.Width = (int)(Image.Width * scaledGrowth.X);
      personalSpace.Height = (int)(Image.Height * scaledGrowth.Y);

      state = WALL_SHOWN;
      zOrder = 0.2f;
    }

    public override void Update(GameTime gameTime)
    {
      lastMoveTimer += gameTime.ElapsedGameTime.Milliseconds;

      //if (lastMoveTimer > 300)
        RandomMove(gameTime);

      //Gotta figure out a better way to manage positions of the walls.
      //update personalspace -- using position like speed.. don't like that
      //personalSpace.X += (int)position.X;
      //personalSpace.Y += (int)position.Y;

      //position = Vector2.Zero;
      base.Update(gameTime);
    }

    private void RandomMove(GameTime gameTime)
    {
      if (flipped) //Right Wall
      {
        //works perfectly - I think. need more examples
        position.X += Live.randBoundedFloat(-speed.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds, speed.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        position.X = MathHelper.Clamp(position.X, Live.screenWidth * 0.65f, Live.screenWidth * 0.90f);
        personalSpace.Width = (int)(position.X);

        Live.FindRightBorder();
      }
      else //Left Wall
      {
        //Find new right side for the wall
        position.X += Live.randBoundedFloat(-speed.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds, speed.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        position.X = MathHelper.Clamp(position.X, (Live.screenWidth * 0.65f) - Live.screenWidth, (Live.screenWidth * 0.90f) - Live.screenWidth); //-1 * Live.halfScreen.X * 0.35f, -1 * Live.halfScreen.X * 0.15f);
        personalSpace.Width = (int)(Image.Width * scaledGrowth.X);

        //make the new right side the new found position
        //personalSpace.Width = (int)position.X;
        //reset the position of the left side
        //position.X = 0f;
        Live.FindLeftBorder();
      }
      
      lastMoveTimer = 0;

      //Live.FindBorders();
    }

    public override void Draw(SpriteBatch sb)
    {
      if (state != WALL_NOT_SHOWN)
      {
        if (flipped)
        {
          sb.Draw(Image, position, null, color, rotation, rotationCenter, scaledGrowth, SpriteEffects.FlipHorizontally, zOrder);
        }
        else
        {
          sb.Draw(Image, position, null, color, rotation, rotationCenter, scaledGrowth, SpriteEffects.None, zOrder);
        }
        //sb.Draw(Image, position, null, color, rotation, rotationCenter, scale, SpriteEffects.None, 1f);
      }
    }
  }
}
