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

namespace Infection
{
  public class SpriteList : Sprite
  {
    List<Sprite> list = new List<Sprite>();
    public Sprite[] array;

    int current = -1;

    public void Add(Sprite s) { list.Add(s); }

    public Sprite getNext()
    {
      current++;
      current %= array.Length;
      return array[current];
    }

    public Sprite getNext(int stateMatch, out bool found)
    {
      found = false;
      int count = 0;
      
      Sprite s = getNext();
      
      while (count < array.Length)
      {
        if (s.state == stateMatch)
        {
          found = true;
          return s;
        }

        s = getNext();
        
        count++;
      }
      
      return s;
    }

    public override void changeState(int nuState)
    {
      stateTime = 0;
      stateFrameCount = 0;
      state = nuState;

      foreach (Sprite s in array) { s.changeState(nuState); }
    }

    public override void Setup()
    {
      array = list.ToArray<Sprite>();
      list.Clear();
      
      foreach (Sprite s in array) s.Setup();
    }

    public override void Update(GameTime gameTime)
    {
      foreach (Sprite s in array) { s.Update(gameTime); }
      
      base.Update(gameTime);
    }

    public override void Draw(SpriteBatch sb)
    {
      //if(array.Length == 0) base.Draw(sb);
      foreach (Sprite s in array) { s.Draw(sb); }
    }
  }
}
