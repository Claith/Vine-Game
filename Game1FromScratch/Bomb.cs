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
  class Bomb : CollisionSprite
  {
    public const int STATE_NOT_READY = 0;
    public const int STATE_READY = 1;
    public const int STATE_FALLING = 2;
    public const int STATE_HIT = 3;

    public override void Setup()
    {
      Image = Game1.imageArray[0];
      texture = Game1.colorArray[0];

      //bool found;

      Stamina = 5;
      Damage = 4;

      position.X = Game1.screenWidth * Game1.randFloat();
        //Cheap way //Game1.randBoundedFloat(((Wall)Game1.leftWallList.getNext(Wall.WALL_TOP, out found)).rightWall(), ((Wall)Game1.rightWallList.getNext(Wall.WALL_TOP, out found)).leftWall());
      position.Y = -Image.Height;//Game1.screenHeight + Image.Height;

      oldPosition = position;

      speed.X = 120.0f * (Game1.randFloat() - 0.5f);
      speed.Y = 60.0f * Game1.randFloat();

      scaledGrowth = Vector2.One;

      rotation = MathHelper.TwoPi * (float)Game1.rand.NextDouble();
      rotationSpeed = ((float)Game1.rand.NextDouble() - 0.5f) * MathHelper.TwoPi;

      rotationCenter = new Vector2(Image.Width / 2, Image.Height / 2);

      zOrder = 0.4f;
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

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
      //chance to spawn
      if (Game1.randFloat() < (Game1.BombRate * Game1.Difficulty))
      {
        changeState(STATE_FALLING);
      }
    }

    private void Update_STATE_FALLING(GameTime gameTime)
    {
      float gravity = 160.0f;

      speed.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
      rotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

      oldPosition = position;
      position += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

      //Collision Detection here

      if ((position.X + personalSpace.Width) > Game1.screenWidth)
      {
        position.X = Game1.screenWidth - personalSpace.Width;
        speed.X = -Speed.X;
      }
      if ((position.X) < 0)
      {
        position.X = 0f;//Game1.screenWidth;
        speed.X = -Speed.X;
      }

      if (state != STATE_HIT)
      {
        //check against walls better (angled walls) later though
        /*{
          Wall w;
          for (int i = 0; i < Game1.WallCount; i++)//Sprite w in Game1.leftWallList)
          {
            w = (Wall)Game1.leftWallList.getNext();
            if (w.Collision(personalSpace, transformation, Image, texture)) { Hit(w); }
          }
          for (int i = 0; i < Game1.WallCount; i++)//Sprite w in Game1.leftWallList)
          {
            w = (Wall)Game1.rightWallList.getNext();
            if (w.Collision(personalSpace, transformation, Image, texture)) { Hit(w); }
          }
        }*/

        //check against each vine
        {
          Vine v;

          for (int i = 0; i < Game1.VineCount; i++)
          {
            //need to add state checking to the class's collision detection
            v = (Vine)Game1.vineList.getNext();
            if ((v.state == Vine.VINE_GROWN) || (v.state == Vine.VINE_GROWING))
              if (v.Collision(personalSpace, transformation, Image, texture))
              {
                Hit(v);
              }
          }
        }

        //check against each tendril
      }

      //If bomb reaches bottom/base
      if (position.Y > Game1.screenHeight)//((Wall)Game1.leftWallList.getNext(Wall.WALL_SHOWN, out found)).personalSpace.Bottom)
      {
        Game1.baseHit(stamina, damage);
        changeState(STATE_NOT_READY);
      }
    }

    public override void Hit(CollisionSprite v)
    {
      //base.Hit(v);

      v.Hit(this);

      Game1.score += (uint)(10 * Game1.Difficulty);

      Vector2 normal = Vector2.One;
      //Vector2 temp = Vector2.Zero;
      //float tempRotation = 0.0f;

      if (stamina <= 0) { changeState(STATE_NOT_READY); }

      //calculate reflection here
      //A lame attempt to decide which side to bounce off of. Will need to look into this.
      //if (v.personalSpace.Top < v.personalSpace.Bottom)//((v.personalSpace.Center.Y - personalSpace.Center.Y) >= (v.personalSpace.Center.X - personalSpace.Center.X))
      //{
      //  normal = new Vector2((v.personalSpace.Height), -(v.personalSpace.Width));
      //  //tempRotation = MathHelper.Pi + (float)Math.Atan2((double)(personalSpace.Height), (double)(personalSpace.Width));
      //}
      //else
      //{
      //  normal = new Vector2(-(v.personalSpace.Height), (v.personalSpace.Width));
      //}
      //normal = Vector2.Reflect(position - oldPosition, normal);
      //normal.Normalize();
      //temp.X = (float)(normal.Length() / normal.X);
      //temp.Y = (float)(normal.Length() / normal.Y);

      //normal.X = Speed.Length() * temp.X * 0.25f;
      //normal.Y = Speed.Length() * temp.Y * 0.25f;
      normal = Speed;
      speed.X = MathHelper.Clamp((normal.Y + (Game1.randFloat() - 0.5f)) * -2.25f, -80f, 80f);
      speed.Y = MathHelper.Clamp((normal.Y * Game1.randFloat()) * -1.25f, -400.0f, 0f);
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
          base.Draw(sb);
          break;
      }
    }
  }
}
