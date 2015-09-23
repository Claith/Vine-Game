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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace Game1FromScratch
{
  public class Game1 : Microsoft.Xna.Framework.Game
  {
    //Utilities - General
    public static Random rand = new Random();

    //Utilities - Video
    public static GraphicsDeviceManager graphics;

    public static float screenHeight { get { return (float)graphics.GraphicsDevice.Viewport.Height; } }
    public static float screenWidth { get { return (float)graphics.GraphicsDevice.Viewport.Width; } }
    
    int frameRateCounter = 0;
    public static double frameRate;
    public static double lowestFrameRate = 9999.0;
    const bool useFixedFrameRate = false;

    //Utilities - Audio
    AudioEngine audioEngine;
    WaveBank waveBank;
    public static SoundBank soundBank;
    public static SoundEffect vineDestroy;
    public static SoundEffect vineMake;

    //Utilities - Input
    public static KeyboardState keyboardState;
    public static MouseState mouseState;

    //Utilities - Display
    SpriteBatch spriteBatch;
    public static Sprite cursor;
    public static SpriteFont sFont;

    public Color textColor = Color.Yellow;
    Color backColor = Color.Blue;

    //Globals
    public static int lives = 15;
    public static float gravity = 0.15f; //percent speed adjustment

    protected static int state = 0;
    public static int State
    {
      set { state = value; }
      get { return state; }
    }

    public const int GAME_MENU = 0;
    public const int GAME_START = 1;
    public const int GAME_OVER = 2;

    public static uint score = 0;
    public static uint highScore = 0;
    public static uint bonus = 10;

    //Global - Data Structures
    public static Texture2D[] imageArray;
    public static Color[][] colorArray;

    public static SpriteList vineList = new SpriteList();
    public static SpriteList bombList = new SpriteList();
    public static SpriteList leftWallList = new SpriteList();
    public static SpriteList rightWallList = new SpriteList();
    public static SpriteList objectList = new SpriteList();

    //Private Variables
    private static int vineCount = 10;
    public static int VineCount
    {
      get { return vineCount; }
    }

    private static int wallCount = 80; //need to find a way to minimize this count
    public static int WallCount
    {
      get { return wallCount; }
    }

    private static int bombCount = 10;
    public static int BombCount
    {
      get { return bombCount; }
    }

    private static float bombRate = 0.0025f;
    public static float BombRate
    {
      get { return bombRate; }
    }

    private static float difficulty = 1.0f;
    public static float Difficulty
    {
        get { return difficulty; }
        set
        { 
          difficulty = value;
          bonus = (uint)Math.Pow(10.0, Math.Min(4.0,Math.Floor(difficulty)));
        }
    }

    public Game1()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";

      // set fixed frame rate
      this.IsFixedTimeStep = true;
      this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / 65.0f);
      //graphics.SynchronizeWithVerticalRetrace = true;

      /*if (!useFixedFrameRate)
      {
          // set floating frame rate
          this.IsFixedTimeStep = false;
          graphics.SynchronizeWithVerticalRetrace = false;
      }*/
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        //Make the game full screen like this
        //graphics.PreferredBackBufferWidth = 1280;
        //graphics.PreferredBackBufferHeight = 800;
        //graphics.ToggleFullScreen();
        //graphics.ApplyChanges();

        //Sound
        audioEngine = new AudioEngine(@"..\..\..\Content\Win\Sounds.xgs");
        //waveBank = new WaveBank(audioEngine, @"..\..\..\Content\Win\Wave Bank.xwb"); //Need to update to newer version of WaveBank -- Addendum. No idea why this is here. Not used any where.
        soundBank = new SoundBank(audioEngine, @"..\..\..\Content\Win\Sound Bank.xsb");


        base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice); 

      imageArray = new Texture2D[10];
      colorArray = new Color[10][];

      imageArray[0] = Content.Load<Texture2D>("weapon01");
      imageArray[1] = Content.Load<Texture2D>("vinebase");
      imageArray[2] = Content.Load<Texture2D>("greenbubble");
      imageArray[3] = Content.Load<Texture2D>("walledge01");
			imageArray[4] = Content.Load<Texture2D>("cursor01");

      colorArray[0] = new Color[imageArray[0].Width * imageArray[0].Height];
      colorArray[1] = new Color[imageArray[1].Width * imageArray[1].Height];
      colorArray[2] = new Color[imageArray[2].Width * imageArray[2].Height];
      colorArray[3] = new Color[imageArray[3].Width * imageArray[3].Height];
      colorArray[4] = new Color[imageArray[4].Width * imageArray[4].Height];

      imageArray[0].GetData(colorArray[0]);
      imageArray[1].GetData(colorArray[1]);
      imageArray[2].GetData(colorArray[2]);
      imageArray[3].GetData(colorArray[3]);
      imageArray[4].GetData(colorArray[4]);

      vineMake = Content.Load<SoundEffect>("vineMake");
      vineDestroy = Content.Load<SoundEffect>("vineDestroy");

      sFont = Content.Load<SpriteFont>("SpriteFont1");

      for (int i = 0; i < vineCount; i++)
      {
        Sprite s = new Vine();
        vineList.Add(s);
      }
      objectList.Add(vineList);

      for (int i = 0; i < bombCount; i++)
      {
        Sprite s = new Bomb();
        bombList.Add(s);
      }
      objectList.Add(bombList);

      for (int i = 0; i < wallCount; i++)
      {
        Sprite s = new Wall();
        ((Wall)s).Flip();
        leftWallList.Add(s);
      }
      objectList.Add(leftWallList);

      for (int i = 0; i < wallCount; i++)
      {
        Sprite s = new Wall();
        rightWallList.Add(s);
      }
      objectList.Add(rightWallList);

      cursor = new Cursor();
      objectList.Add(cursor);

      objectList.Setup();

      // Comment out this line for one game
      //   to clear the high score
      //GetHighScore();
    }

    public static bool hitBetweenCircleAndLineSegment(
        Vector2 circleCenter,
        float circleRadius,
        Vector2 end1,
        Vector2 end2,
        ref Vector2 normal,
        ref float distanceFromPivot
        )
    {
      float m1 = (end2.Y-end1.Y)/(end2.X-end1.X);
      float b1 = (end1.Y-m1*end1.X);
      float m2 = -(1/m1);
      float b2 = circleCenter.Y-m2*circleCenter.X;

      Vector2 intersection=new Vector2();
      intersection.X=(b1-b2)/(m2-m1);
      intersection.Y=m1*intersection.X+b1;

      normal = Vector2.Normalize(circleCenter - intersection);

      float d = Vector2.Distance(circleCenter,intersection);

      distanceFromPivot = Vector2.Distance(end1, intersection);  

      if(d>circleRadius) return false;
      if((end1.X>intersection.X)&&(intersection.X>end2.X))return true;
      if((end1.X<intersection.X)&&(intersection.X<end2.X))return true;
      if((end1.Y>intersection.Y)&&(intersection.Y>end2.Y))return true;
      if((end1.Y<intersection.Y)&&(intersection.Y<end2.Y))return true;
      return false;
    }

    //High Score Functions
    public static void GetHighScore()
    {
      TextReader tr = new StreamReader("hiScore.txt");

      // read a line of text
      string str = tr.ReadLine();

      highScore = Convert.ToUInt32(str);

      // close the stream
      tr.Close();
    }

    public static void PutHighScore()
    {
      TextWriter tw = new StreamWriter("hiScore.txt");

      // read a line of text
      tw.WriteLine(highScore.ToString());

      // close the stream
      tw.Close();
    }

    ////////////////////////////////////////////////////
    //Random Functions
    ////////////////////////////////////////////////////
    public static float randFloat() { return (float)rand.NextDouble(); }
    public static float randBoundedFloat(float leftBound, float rightBound)
    {
      return MathHelper.Clamp(randFloat(), leftBound, rightBound);
    }

    public static float randAngle() { return MathHelper.TwoPi*(float)rand.NextDouble(); }

    public static Vector2 randVector2(float range)
    {
      Vector2 result = new Vector2(range * randFloat() - range * randFloat(), range * randFloat() - range * randFloat());
      return result;
    }

    public static void randVector2True(ref Vector2 v, float range)
    {
      float theta = randAngle();
      float r = randFloat() * range;
      v.X = (float)Math.Cos(theta) * r;
      v.Y = (float)Math.Sin(theta) * r;
    }
    
    public static bool probability(double p) { return  ( p > rand.NextDouble()); }
    public static bool adjustedProbability(double p) { return (p * (60.0 / Game1.frameRate) > (rand.NextDouble())); }

    public static void randColor(ref Color c)
    {
      c.R = (byte)rand.Next(256);
      c.G = (byte)rand.Next(256);
      c.B = (byte)rand.Next(256);
    }
    
    //Frame Functions
    public static float frameFactor() { return (float)(60.0 / Game1.frameRate); }

    public static void changeColor(ref Color c, int amount)
    {
      int r,g,b;
      r = c.R;
      r+= rand.Next(-amount, amount+1);
      g = c.G;
      g += rand.Next(-amount, amount + 1);
      b = c.B;
      b += rand.Next(-amount, amount + 1);
      r = (int)MathHelper.Clamp(r, 0, 255);
      g = (int)MathHelper.Clamp(g, 0, 255);
      b = (int)MathHelper.Clamp(b, 0, 255);
      c.R = (byte)r;
      c.G = (byte)g;
      c.B = (byte)b;
    }

    public static void setColorFade(ref Color c, int fade)
    {
      c.A = (byte)(fade);
    }

    static public void Reset()
    {
      objectList.changeState(0);
      score = 0;
      Difficulty = 1.0f;
      lives = 15;
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: Unload any non ContentManager content here
    }
 
    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      // Allows the game to exit
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) { this.Exit(); }

      keyboardState = Keyboard.GetState();
      mouseState = Mouse.GetState();

      if (keyboardState.IsKeyDown(Keys.Q))Exit();

      frameRateCounter++;
      if (frameRateCounter >= 100)
      {
        frameRateCounter = 0;
        lowestFrameRate = 9999f;
      }

      frameRate=Math.Truncate((1.0/gameTime.ElapsedGameTime.TotalSeconds)+0.5);
      if (frameRate < lowestFrameRate) lowestFrameRate = frameRate;

      // TODO: Add your update logic here

      if (State != GAME_OVER)
      {
        objectList.Update(gameTime);

        changeColor(ref backColor, 3);

        Difficulty = gameTime.TotalGameTime.Minutes;

        if (Difficulty == 0) Difficulty = 1;

        base.Update(gameTime);
      }
      else
      {
        Draw(gameTime);
        if (keyboardState.IsKeyDown(Keys.Enter))
        {
          state = GAME_START;
          Reset();
        }
      }
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(backColor);

      // TODO: Add your drawing code here
      //spriteBatch.Begin(SpriteBlendMode.AlphaBlend); //Old XNA
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); //Updated XNA

      objectList.Draw(spriteBatch);

      spriteBatch.DrawString(sFont, "SCORE " + score.ToString(),
                  new Vector2(screenWidth * 0.03f, screenHeight * 0.01f), textColor);

      spriteBatch.DrawString(sFont, "BONUS X " + Difficulty.ToString(),
                  new Vector2(screenWidth * 0.60f, screenHeight * 0.01f), textColor);

      spriteBatch.DrawString(sFont, "Stamina " + lives.ToString(),
                  new Vector2(screenWidth * 0.03f, screenHeight * 0.95f), textColor);

      spriteBatch.DrawString(sFont, "HIGH SCORE " + highScore.ToString(),
                  new Vector2(screenWidth * 0.60f, screenHeight * 0.95f), textColor);

      if (state == GAME_OVER)
      {
        spriteBatch.DrawString(sFont, "GAME OVER", new Vector2(screenWidth * 0.45f, screenHeight * 0.5f - 50f), textColor);
        spriteBatch.DrawString(sFont, "Press Enter to restart", new Vector2(screenWidth * 0.40f, screenHeight * 0.5f + 50f), textColor);
      }

      //spriteBatch.DrawString(sFont, "FR "+lowestFrameRate.ToString(), new Vector2(screenWidth * 0.30f, screenHeight * 0.95f), textColor);

      spriteBatch.End();

      base.Draw(gameTime);
    }

    internal static void baseHit(int stamina, int damage)
    {
      lives -= damage;
      if (lives <= 0) {
        state = GAME_OVER;
        //if (score > highScore) PutHighScore();
      }
    }

    /// <summary>
    /// Determines if there is overlap of the non-transparent pixels
    /// between two sprites.
    /// </summary>
    /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
    /// <param name="dataA">Pixel data of the first sprite</param>
    /// <param name="rectangleB">Bouding rectangle of the second sprite</param>
    /// <param name="dataB">Pixel data of the second sprite</param>
    /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
    public static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                       Rectangle rectangleB, Color[] dataB)
    {
      // Find the bounds of the rectangle intersection
      int top = Math.Max(rectangleA.Top, rectangleB.Top);
      int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
      int left = Math.Max(rectangleA.Left, rectangleB.Left);
      int right = Math.Min(rectangleA.Right, rectangleB.Right);

      // Check every point within the intersection bounds
      for (int y = top; y < bottom; y++)
      {
        for (int x = left; x < right; x++)
        {
          // Get the color of both pixels at this point
          Color colorA = dataA[(x - rectangleA.Left) +
                               (y - rectangleA.Top) * rectangleA.Width];
          Color colorB = dataB[(x - rectangleB.Left) +
                               (y - rectangleB.Top) * rectangleB.Width];

          // If both pixels are not completely transparent,
          if (colorA.A != 0 && colorB.A != 0)
          {
            // then an intersection has been found
            return true;
          }
        }
      }

      // No intersection found
      return false;
    }


    /// <summary>
    /// Determines if there is overlap of the non-transparent pixels between two
    /// sprites.
    /// </summary>
    /// <param name="transformA">World transform of the first sprite.</param>
    /// <param name="widthA">Width of the first sprite's texture.</param>
    /// <param name="heightA">Height of the first sprite's texture.</param>
    /// <param name="dataA">Pixel color data of the first sprite.</param>
    /// <param name="transformB">World transform of the second sprite.</param>
    /// <param name="widthB">Width of the second sprite's texture.</param>
    /// <param name="heightB">Height of the second sprite's texture.</param>
    /// <param name="dataB">Pixel color data of the second sprite.</param>
    /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
    public static bool IntersectPixels( Matrix transformA, int widthA, int heightA, Color[] dataA,
                                        Matrix transformB, int widthB, int heightB, Color[] dataB)
    {
      // Calculate a matrix which transforms from A's local space into
      // world space and then into B's local space
      Matrix transformAToB = transformA * Matrix.Invert(transformB);

      // When a point moves in A's local space, it moves in B's local space with a
      // fixed direction and distance proportional to the movement in A.
      // This algorithm steps through A one pixel at a time along A's X and Y axes
      // Calculate the analogous steps in B:
      Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
      Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

      // Calculate the top left corner of A in B's local space
      // This variable will be reused to keep track of the start of each row
      Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

      // For each row of pixels in A
      for (int yA = 0; yA < heightA; yA++)
      {
        // Start at the beginning of the row
        Vector2 posInB = yPosInB;

        // For each pixel in this row
        for (int xA = 0; xA < widthA; xA++)
        {
          // Round to the nearest pixel
          int xB = (int)Math.Round(posInB.X);
          int yB = (int)Math.Round(posInB.Y);

          // If the pixel lies within the bounds of B
          if (0 <= xB && xB < widthB &&
              0 <= yB && yB < heightB)
          {
            // Get the colors of the overlapping pixels
            Color colorA = dataA[xA + yA * widthA];
            Color colorB = dataB[xB + yB * widthB];

            // If both pixels are not completely transparent,
            if (colorA.A != 0 && colorB.A != 0)
            {
              // then an intersection has been found
              return true;
            }
          }

          // Move to the next pixel in the row
          posInB += stepX;
        }

        // Move to the next row
        yPosInB += stepY;
      }

      // No intersection found
      return false;
    }


    /// <summary>
    /// Calculates an axis aligned rectangle which fully contains an arbitrarily
    /// transformed axis aligned rectangle.
    /// </summary>
    /// <param name="rectangle">Original bounding rectangle.</param>
    /// <param name="transform">World transform of the rectangle.</param>
    /// <returns>A new rectangle which contains the trasnformed rectangle.</returns>
    public static Rectangle CalculateBoundingRectangle(Rectangle rectangle, Matrix transform)
    {
      // Get all four corners in local space
      Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
      Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
      Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
      Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

      // Transform all four corners into work space
      Vector2.Transform(ref leftTop, ref transform, out leftTop);
      Vector2.Transform(ref rightTop, ref transform, out rightTop);
      Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
      Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

      // Find the minimum and maximum extents of the rectangle in world space
      Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                Vector2.Min(leftBottom, rightBottom));
      Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                Vector2.Max(leftBottom, rightBottom));

      // Return that as a rectangle
      return new Rectangle((int)min.X, (int)min.Y,
                           (int)(max.X - min.X), (int)(max.Y - min.Y));
    }
  }
}
