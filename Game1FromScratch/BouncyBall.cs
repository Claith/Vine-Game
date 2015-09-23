//************************************************************
//
// (c) Copyright 2009 Dr. Thomas Fernandez
//
//  All rights reserved.
//
//************************************************************


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
    class Ball : Sprite
    {
        public override void setup()
        {
            Position = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width * (float)Game1.rand.NextDouble(),
                                    Game1.graphics.GraphicsDevice.Viewport.Height * (float)Game1.rand.NextDouble());
            Speed = new Vector2(300f * (float)Game1.rand.NextDouble() - 150f, 300f * (float)Game1.rand.NextDouble() - 150f);
            Rotation = MathHelper.TwoPi * (float)Game1.rand.NextDouble();
            RotationSpeed = MathHelper.TwoPi * (((float)Game1.rand.NextDouble() * 4f) - 2f);
            //Center = new Vector2(Image.Width / 2, Image.Height / 2);
            color = new Color((byte)Game1.rand.Next(256), (byte)Game1.rand.Next(256), (byte)Game1.rand.Next(256));
 
            //color = Color.White;
            scale = (float)(Game1.rand.NextDouble() + 0.1);
            //scale = 1.0f;

            Image = Game1.imageArray[7];
            Center = new Vector2(Image.Width / 2, Image.Height / 2);

        }

        public override void Update(GameTime gameTime)
        {
            switch (state)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 99:
                    // Move the sprite by speed, scaled by elapsed time.
                    Position +=
                        Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    int MaxX =
                        Game1.graphics.GraphicsDevice.Viewport.Width - Image.Width / 2;
                    int MinX = 0 + Image.Width / 2;
                    int MaxY =
                        Game1.graphics.GraphicsDevice.Viewport.Height - Image.Height / 2;
                    int MinY = 0 + Image.Height / 2;

                    Rotation +=
                            RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Check for bounce.
                    if (Position.X > MaxX)
                    {
                        Speed.X *= -1;
                        Position.X = MaxX;
                    }

                    else if (Position.X < MinX)
                    {
                        Speed.X *= -1;
                        Position.X = MinX;
                    }

                    if (Position.Y > MaxY)
                    {
                        Speed.Y *= -1;
                        Position.Y = MaxY;
                        Game1.soundBank.PlayCue("pipebang");
                    }

                    else if (Position.Y < MinY)
                    {
                        Speed.Y *= -1;
                        Position.Y = MinY;
                        Game1.soundBank.PlayCue("implosion2");
                    }
                    break;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            switch (state)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 99:
                    sb.Draw(Image, Position, null, color, Rotation, Center, scale, SpriteEffects.None, 1f);
                    break;
            }
        }

    }
}
