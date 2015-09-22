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
  class CollisionSprite : Sprite
  {
    protected int stamina = 0;
    public int Stamina
    {
      get { return stamina; }
      set { stamina = value; }
    }

    protected int damage = 0;
    public int Damage
    {
      get { return damage; }
      set { damage = value; }
    }

    protected Vector2 oldPosition = Vector2.Zero;

    protected Vector2 scaledGrowth = Vector2.Zero;

    public Color[] texture;
    public Rectangle personalSpace = Rectangle.Empty;
    public Matrix transformation = new Matrix();

    public override void Setup()
    {
      base.Setup();

      scaledGrowth = Vector2.One;
      oldPosition = Vector2.Zero;
    }

    public virtual void Setup(Vector2 from, Vector2 to)
    {
      oldPosition = from;
      position = to;

      stateTime = 0;
      stateFrameCount = 0;
      state = 0;
    }

    public virtual void Setup(Vector2 from, Vector2 to, int newState)
    {
      oldPosition = from;
      position = to;

      stateTime = 0;
      stateFrameCount = 0;
      state = newState;
    }

    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
                    Matrix.CreateScale(scale) * Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateTranslation(new Vector3(position, 0.0f));

      personalSpace = Game1.CalculateBoundingRectangle(new Rectangle(0, 0, Image.Width, Image.Height), transformation);
    }

    public virtual void Hit(CollisionSprite s)
    {
      Stamina -= s.Damage;
      s.Stamina -= Damage;
    }

    public virtual bool Collision(Rectangle incomingSprite, Matrix incomingMatrix, Texture2D incomingImage, Color[] incomingTexture)
    {
      transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
                    Matrix.CreateScale(scaledGrowth.X, scaledGrowth.Y, 1.0f) * Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateTranslation(new Vector3(position, 0.0f));

      personalSpace = Game1.CalculateBoundingRectangle( new Rectangle(0, 0, Image.Width, Image.Height),transformation);

      if (personalSpace.Intersects(incomingSprite))
      {
        // Check collision with person
        if (Game1.IntersectPixels(transformation, Image.Width, Image.Height, texture,
                            incomingMatrix, incomingImage.Width, incomingImage.Height, incomingTexture))
        {
          return true;
        }
      }

      return false;
    }
  }
}
