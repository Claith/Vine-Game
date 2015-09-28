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
  class Bomb : Sprite
  {
    private static int inPlay = 0;

    public const int STATE_NOT_READY = 0;
    public const int STATE_READY = 1;
    public const int STATE_FALLING = 2;
    public const int STATE_HIT = 3;

    public override void Setup()
    {
      base.Setup();

			SetType(COLLISION_SPRITE);

      int temp = Live.rand.Next(Live.BombImageIndex, Live.BombImageIndex + Live.BombImageIndexDepth);

      Image = Live.imageArray[temp];
      texture = Live.colorArray[temp];

      switch (temp)
      {
        case 0:
					SetStatus(2, 4);
          break;
        case 1:
					SetStatus(5, 10);
          break;
        case 2:
					SetStatus(15, 2);
          break;
        case 3:
					SetStatus(7, 7);
          break;
        case 4:
					SetStatus(3, 1);
          break;
      }

      //position.X = Live.randBoundedFloat(((Wall)Live.leftWallList.getNext()).rightWall() + 15f, ((Wall)Live.rightWallList.getNext()).leftWall() - 15f); //Live.screenWidth * Live.randFloat();
      position.X = Live.randBoundedFloat(Live.leftBorder, Live.rightBorder);
        //Cheap way //Live.randBoundedFloat(((Wall)Live.leftWallList.getNext(Wall.WALL_TOP, out found)).rightWall(), ((Wall)Live.rightWallList.getNext(Wall.WALL_TOP, out found)).leftWall());
      position.Y = -Image.Height;//Live.screenHeight + Image.Height;

      oldPosition = position;

      speed.X = 120.0f * (Live.randFloat() - 0.5f);
      speed.Y = 60.0f * Live.randFloat();

      scaledGrowth = Vector2.One;

      rotation = MathHelper.TwoPi * (float)Live.rand.NextDouble();
      rotationSpeed = ((float)Live.rand.NextDouble() - 0.5f) * MathHelper.TwoPi;

      rotationCenter = new Vector2(Image.Width / 2, Image.Height / 2);

      zOrder = 0.4f;
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
          break;
        case STATE_HIT:
          Update_STATE_FALLING(gameTime);
          if (stateTime > 200) changeState(STATE_FALLING);
          break;
      }
      
      base.Update(gameTime);
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
      if (inPlay < Live.Difficulty * 2)
      {
        if (Live.randFloat() < (Live.BombRate * Live.Difficulty))
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
      if (state != STATE_HIT)
      {
        //check against walls better (angled walls) later though
        {
          Wall w;
          for (int i = 0; i < Live.WallCount; i++)//Sprite w in Live.leftWallList)
          {
            if (state == STATE_HIT) break;
            w = (Wall)Live.leftWallList.getNext();
            if (w.Collision(personalSpace, transformation, Image, texture))
            {
              //position.X = ((Wall)Live.leftWallList.getNext()).rightWall() + (position.X - oldPosition.X);
              speed.X = Math.Abs(speed.X + Live.randFloat());
              //changeState(STATE_HIT);
            }
          }
          for (int i = 0; i < Live.WallCount; i++)//Sprite w in Live.rightWallList)
          {
            if (state == STATE_HIT) break;
            w = (Wall)Live.rightWallList.getNext();
            if (w.Collision(personalSpace, transformation, Image, texture))
            {
              //position.X = ((Wall)Live.rightWallList.getNext()).leftWall() + (position.X - oldPosition.X);//- personalSpace.Width; 
              speed.X = -1 * Math.Abs(speed.X + Live.randFloat());
              //changeState(STATE_HIT);
            }
          }
        }

        //check against each vine
        {
          Vine v;

          for (int i = 0; i < Live.VineCount; i++)
          {
            if (state == STATE_HIT) break;
            //need to add state checking to the class's collision detection
            v = (Vine)Live.vineList.getNext();
            if ((v.state == Vine.VINE_GROWN) || (v.state == Vine.VINE_GROWING))
              if (v.Collision(personalSpace, transformation, Image, texture))
              {
                Hit(v);
              }
          }
        }

        //check against each tendril
      }

      //Check Walls -- remove position changes.
      if (personalSpace.Left < 0)
      {
        position.X = Live.leftBorder; //((Wall)Live.leftWallList.getNext()).rightWall();//0f;//Live.screenWidth;
        //speed.X = -Speed.X;
        speed.X = Math.Abs(speed.X);// + Live.randFloat());
        //changeState(STATE_HIT);
      }
      if (personalSpace.Right > Live.screenWidth)
      {
        position.X = Live.rightBorder - personalSpace.Width; //((Wall)Live.rightWallList.getNext()).leftWall() - personalSpace.Width;//Live.screenWidth - personalSpace.Width;
        //speed.X = -Speed.X;
        speed.X = -1 * Math.Abs(speed.X);// + Live.randFloat());
        //changeState(STATE_HIT);
      }

      //If bomb reaches bottom/base
      if (personalSpace.Top > Live.screenHeight)//((Wall)Live.leftWallList.getNext(Wall.WALL_SHOWN, out found)).personalSpace.Bottom)
      {
        Live.baseHit(stamina, damage);
        changeState(STATE_NOT_READY);
      }
    }

    public override void Hit(Sprite v)
    {
      //base.Hit(v);

      v.Hit(this);

      Live.score += (uint)(10 * Live.Difficulty);

      if (stamina <= 0)
      {
        changeState(STATE_NOT_READY);
        inPlay--;
      }
      else
      {
        speed.X += Live.randBoundedFloat(-60f, 60f);
        speed.Y = Live.randBoundedFloat(-120f, -360f);

        changeState(STATE_HIT);
      }
    }

    public override void Draw(SpriteBatch sb)
    {
      switch (state)
      {
        case STATE_NOT_READY:
        case STATE_READY:
          break;
        case STATE_FALLING:
        case STATE_HIT:
          //sb.Draw(image, position, null, color, rotation, rotationCenter, scale, SpriteEffects.None, zOrder);
          base.Draw(sb);
          break;
      }
    }
  }
}
