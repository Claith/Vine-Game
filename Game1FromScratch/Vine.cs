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
  //This is just the vine type and not the Tendril
  class Vine : CollisionSprite
  {
    //how far it has grown so far
    protected Vector2 currentGrowth = Vector2.Zero;

    //protected Vector2 scaledGrowth = Vector2.Zero;

    protected float growthPercent = 0.0f;
    protected float growthRate = 0.0f;
    public float GrowthRate
    {
      set { growthRate = value; }
    }

    //constants
    public const int VINE_DEAD = 0;
    public const int VINE_SETUP = 1;
    public const int VINE_GROWING = 2;
    public const int VINE_GROWN = 3;

    public override void Setup()
    {
      base.Setup();

      Image = Game1.imageArray[1];
      texture = Game1.colorArray[1];

      stamina = 10;
      Damage = 4;

      currentGrowth = Vector2.Zero;

      zOrder = 0.8f; //close to back, behind the walls

      scaledGrowth = Vector2.One;
      growthPercent = 0.0f;
      growthRate = 15.0f;

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

        scale = Vector2.Distance(oldPosition, position) / Image.Width; //Math.Abs(oldPosition.X - position.X) / Image.Width);

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
      //If the vine is growing
      if(state == VINE_GROWING)
      {
        //setback to unit vector
        currentGrowth.Normalize();

        growthPercent += growthRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (growthPercent >= 1.0f) //when fully grown
        {
          growthPercent = 1.0f;
          state = VINE_GROWN;
          Game1.vineMake.Play();
        }

        //set growth length
        currentGrowth.X = ( oldPosition.X - position.X ) * growthPercent;
        currentGrowth.Y = ( oldPosition.Y - position.Y ) * growthPercent;
      }
    }

    public override void Draw(SpriteBatch sb)
    {
      //base.Draw(sb);
      if( state != VINE_DEAD )
        sb.Draw(Image, position, null, color, rotation, rotationCenter, scaledGrowth, SpriteEffects.None, zOrder);
    }

    public override void Hit(CollisionSprite s)
    {
      base.Hit(s);

      if (stamina <= 0)
      {
        stamina = 0;
        state = VINE_DEAD;
        Game1.vineDestroy.Play();
      }
    }

    /*public override void Collision(Vector2 oldPosition, Vector2 newPosition, Vector2 imgBound)
    {
      //idea #1
      //check the distance between old and new positions to height and width of this.bounds
      //if distance <= img bounds, then check for overlap?
      //detect from which side the collion came from, by comparing xs and ys
      //reflect off the normal?


      //check for hit with passed information
      //check for if it is nearby
      float distance = Vector2.Distance(newPosition, position);

      if (distance <= imgBound.Length)
      {
        //close to impact
      }
      //check for an intersection

      //bounce back
    }*/
  }
}
