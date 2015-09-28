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
  //This is just the vine type and not the Tendril
  class Vine : Sprite
  {
    protected int vineType = VINE_BASE;
    public int VineType
    {
      get { return vineType; }
      set { vineType = value; }
    }

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

    //constants
    public const int VINE_DEAD = 0;
    public const int VINE_SETUP = 1;
    public const int VINE_GROWING = 2;
    public const int VINE_GROWN = 3;

    public const int VINE_BASE = 0;
    public const int VINE_TOP = 1;
    public const int VINE_DETAIL = 2;

    public override void Setup()
    {
      base.Setup();

			SetType(COLLISION_SPRITE);

      int temp = Live.rand.Next(Live.VineImageIndex, Live.VineImageIndex + Live.VineImageIndexDepth);

      Image = Live.imageArray[temp];
      texture = Live.colorArray[temp];

			SetStatus(15, 2);

      currentGrowth = Vector2.Zero;

      zOrder = 0.8f; //close to back, behind the walls

      scaledGrowth = Vector2.One;
      growthPercent = 0.0f;
      growthRate = 5.0f;

      state = VINE_DEAD;
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      if (stamina <= 0) { this.Setup(); } //If vine is dead, reset it; Add death sound and animation
      else
      {
        makeVine(); //returns a bool, in game error message about why it couldn't be made?

        checkGrowth(gameTime); //update Growth here

        //update scale here -- Messy, find a better cleaner way later, replace growthRate with scale? (different uses)
        scaledGrowth.X = scale * growthPercent;
      }
    }

    public bool makeVine()
    {
      if (state == VINE_SETUP)
      {
        //check collision with nearby walls
        //check from then to, then check nearby for interceptions.

        scale = Vector2.Distance(oldPosition, position) / Image.Width;

        //find rotation
        rotation = MathHelper.Pi + (float)Math.Atan2((double)(position.Y - oldPosition.Y), (double)(position.X - oldPosition.X));

        growthPercent = 0.0f;
        state = VINE_GROWING;

        return true;
      }

      return false;
    }

    private void checkGrowth(GameTime gameTime)
    {
      switch (state)
      {
        case VINE_DEAD:
        case VINE_SETUP:
          break;
        case VINE_GROWING:
          //If the vine is growing
          //setback to unit vector
          currentGrowth.Normalize();

          growthPercent += growthRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

          if (growthPercent >= 1.0f) //when fully grown
          {
            growthPercent = 1.0f;
            state = VINE_GROWN;
            Live.vineMake.Play();
          }

          //set growth length
          currentGrowth.X = (oldPosition.X - position.X) * growthPercent;
          currentGrowth.Y = (oldPosition.Y - position.Y) * growthPercent;
          break;
        case VINE_GROWN:

          //set growth length
          currentGrowth.X = (oldPosition.X - position.X) * growthPercent;
          currentGrowth.Y = (oldPosition.Y - position.Y) * growthPercent;
          break;
      }
    }

    public override void Draw(SpriteBatch sb)
    {
      if( state != VINE_DEAD )
        sb.Draw(Image, position, null, color, rotation, rotationCenter, scaledGrowth, SpriteEffects.None, zOrder);
      //base.Draw(sb);
    }

    public override void Hit(Sprite s)
    {
      base.Hit(s);

      if (stamina <= 0)
      {
        stamina = 0;
        state = VINE_DEAD;
        Live.vineDestroy.Play();
      }
      else
      {
        Live.vineHit.Play();
      }
    }
  }
}
