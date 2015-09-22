//************************************************************
//
// (c) Copyright 2009 Christopher Smith
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
  public class Sprite
  {
    private Texture2D image;
    public Texture2D Image
    {
      get { return image; }
      set { image = value; }
    }

    protected Vector2 position = Vector2.Zero;
    //protected Vector2 positionBound = Vector2.Zero;
    protected Vector2 speed = Vector2.Zero;
    public Vector2 Speed {
      get { return speed; }
      set { speed = value; }
    }

    public Vector2 rotationCenter = Vector2.Zero;
    public float rotation = 0.0f;
    public float rotationSpeed = 0.0f;

    public float scale = 1f;
    protected float zOrder = 0.0f;
    public Color color = Color.White;

    public int state = 0;
    protected int stateTime;
    protected int stateFrameCount;

    public virtual void changeState(int newState)
    {
      stateTime = 0;
      stateFrameCount = 0;
      state = newState;
    }

    public virtual void Setup()
    {
      stateTime = 0;
      stateFrameCount = 0;
      state = 0;
    }

    public virtual void Update(GameTime gameTime)
    {
      stateTime += gameTime.ElapsedGameTime.Milliseconds;
      stateFrameCount++;
    }

    public virtual void  Draw(SpriteBatch sb)
    {
      sb.Draw(image, position, null, color, rotation, rotationCenter, scale, SpriteEffects.None, zOrder);
    }
  }
}
