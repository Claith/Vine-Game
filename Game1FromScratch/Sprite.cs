using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Infection
{
  public class Sprite
  {
		//Constants
		protected const int HIDDEN_SPRITE = 0;
		protected const int STATIC_SPRITE = 1;
		protected const int MOVING_SPRITE = 2;
		protected const int COLLISION_SPRITE = 3;

		//Texture Data
    private Texture2D image;
    public Texture2D Image
    {
      get { return image; }
      set { image = value; }
    }
		public Color[] texture;
		public Color color = Color.White;

		//Spacial Data
		protected Vector2 position = Vector2.Zero;
		protected Vector2 oldPosition = Vector2.Zero;

		protected Vector2 speed = Vector2.Zero;
		public Vector2 Speed
		{
			get { return speed; }
			set { speed = value; }
		}

		protected Vector2 rotationCenter = Vector2.Zero;
		protected float rotation = 0.0f;
		protected float rotationSpeed = 0.0f;

		protected float scale = 1f;
		protected float zOrder = 0.0f;

		protected Vector2 scaledGrowth = Vector2.Zero;

		protected Rectangle personalSpace = Rectangle.Empty;
		protected Matrix transformation = new Matrix();

		//State Data
		public int state = 0;
		protected int stateTime;
		protected int stateFrameCount;

		protected int spriteType;

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

		/////////////////////////////////////////////////////
		//Setup & State Functions////////////////////////////
		/////////////////////////////////////////////////////

		public virtual void Setup()
		{
			stateTime = 0;
			stateFrameCount = 0;
			state = 0;

			spriteType = HIDDEN_SPRITE;

			scaledGrowth = Vector2.One;
			oldPosition = Vector2.Zero;
		}

		public virtual void Setup(Vector2 from, Vector2 to)
		{
			Setup();

			oldPosition = from;
			position = to;
		}

		public virtual void Setup(Vector2 from, Vector2 to, int newState)
		{
			Setup();

			state = newState;

			oldPosition = from;
			position = to;
		}

		public virtual void SetType(int newType)
		{
			spriteType = newType;
		}

		public virtual void changeState(int newState)
		{
			stateTime = 0;
			stateFrameCount = 0;
			state = newState;
		}

		protected virtual void SetStatus(int newStamina, int newDamage)
		{
			stamina = newStamina;
			damage = newDamage;

			if (stamina < 0) stamina = 0; //with 0 stamina, the next time it hits or checks for collision (and true) sprite is destroyed.
			//don't want to bound damage. damage could be possibly used as a healing item later on.
		}

		//protected void setImage()

		/////////////////////////////////////////////////////
		//Cycling Functions//////////////////////////////////
		/////////////////////////////////////////////////////

		/// <summary>
		/// Looping Update Function
		/// </summary>
		/// <param name="gameTime"></param>
    public virtual void Update(GameTime gameTime)
    {
      stateTime += gameTime.ElapsedGameTime.Milliseconds;
      stateFrameCount++;

			switch (spriteType)
			{
				case HIDDEN_SPRITE:
					break;
				case STATIC_SPRITE:
					//work on removing this at a later date
					transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
												Matrix.CreateScale(scale) * Matrix.CreateRotationZ(rotation) *
												Matrix.CreateTranslation(new Vector3(position, 0.0f));
					break;
				case MOVING_SPRITE:
					transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
												Matrix.CreateScale(scale) * Matrix.CreateRotationZ(rotation) *
												Matrix.CreateTranslation(new Vector3(position, 0.0f));
					break;
				case COLLISION_SPRITE:
					transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
												Matrix.CreateScale(scale) * Matrix.CreateRotationZ(rotation) *
												Matrix.CreateTranslation(new Vector3(position, 0.0f));

					personalSpace = Live.CalculateBoundingRectangle(new Rectangle(0, 0, Image.Width, Image.Height), transformation);
					break;
			}
    }

		public virtual void CheckCollisions(SpriteList against)
		{
		}

    public virtual void Draw(SpriteBatch sb)
    {
			switch(spriteType)
			{
				case HIDDEN_SPRITE:
					break;
				case STATIC_SPRITE:
				case MOVING_SPRITE:
				case COLLISION_SPRITE:
					sb.Draw(image, position, null, color, rotation, rotationCenter, scale, SpriteEffects.None, zOrder);
					break;
			}
			//base.Draw(sb); //look into how to draw nested images (shouldn't be here, but in sprite list)
    }

		/////////////////////////////////////////////////////
		//Collion Sprite Functions///////////////////////////
		/////////////////////////////////////////////////////

		public virtual bool Collision(Rectangle incomingSprite, Matrix incomingMatrix, Texture2D incomingImage, Color[] incomingTexture)
		{
			transformation = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0.0f)) *
										Matrix.CreateScale(scaledGrowth.X, scaledGrowth.Y, 1.0f) * Matrix.CreateRotationZ(rotation) *
										Matrix.CreateTranslation(new Vector3(position, 0.0f));

			personalSpace = Live.CalculateBoundingRectangle(new Rectangle(0, 0, Image.Width, Image.Height), transformation);

			if (personalSpace.Intersects(incomingSprite))
			{
				// Check collision with person
				if (Live.IntersectPixels(transformation, Image.Width, Image.Height, texture,
														incomingMatrix, incomingImage.Width, incomingImage.Height, incomingTexture))
				{
					return true;
				}
			}

			return false;
		}

		public virtual void Hit(Sprite s)
		{
			Stamina -= s.Damage;
			s.Stamina -= Damage;
		}

		/////////////////////////////////////////////////////
		//Utility Functions//////////////////////////////////
		/////////////////////////////////////////////////////

		protected Vector2 ImageCenter()
		{
			return new Vector2((float)Image.Width / 2, (float)Image.Height / 2);
		}
  }
}
