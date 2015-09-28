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

namespace Infection
{
  public class Live : Microsoft.Xna.Framework.Game
  {
    //Utilities - General
    public static Random rand = new Random();
    public static TimeSpan playTime = new TimeSpan(); //Total time playing
    public const double WARM_UP_SECONDS = 5.0f;
    public static string timerString = "";

		//Utilities - Storage
		IAsyncResult result;
		private bool GameSaveRequested = false;

    //Utilities - Video
    public static GraphicsDeviceManager graphics;

    //consider changing these to vectors to represent the borders of the play area.
		public static float screenWidth { get { return (float)graphics.GraphicsDevice.Viewport.Width; } }
    public static float screenHeight { get { return (float)graphics.GraphicsDevice.Viewport.Height; } }
    public static Vector2 halfScreen;

    public static float leftBorder = 0f;
    public static float rightBorder = 0f;

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
    public static SoundEffect vineHit;
    public static SoundEffect coreHit;
    public static SoundEffectInstance backgroundHum;

    //Utilities - Input
    public static KeyboardState keyboardState;
		private static KeyboardState oldKeyboardState;
    public static MouseState mouseState;
    public static bool pauseDown = false; // work to remove this with oldKeyboardState

    //Utilities - Display
    SpriteBatch spriteBatch;
    public static Sprite cursor;
    public static SpriteFont sFont;
    public static Vector2 textSize;

    public Color textColor = Color.Yellow;
    Color backColor = Color.Blue;

    //Globals
    public static int lives = 15;
    public static float gravity = 240f; //percent speed adjustment

    //Global - Game State
    protected static int state = GAME_MENU;
    public static int State
    {
      set { state = value; }
      get { return state; }
    }

    public const int GAME_MENU = 0;
    public const int GAME_START = 1; //runs a timer and counts down before the game runs
    public const int GAME_RUN = 2;
    public const int GAME_PAUSE = 3;
    public const int GAME_OVER = 4;

    //Global - Score
    protected static uint score = 0;
    protected static uint highScore = 0;
    protected static uint bonus = 10;

    private static Vector2 scoreLocation;
    private static Vector2 healthLocation;
    private static Vector2 bonusLocation;

    //Global - Data Structures
    public static Texture2D[] imageArray;
    public static Color[][] colorArray;

    public static SpriteList vineList = new SpriteList();
    public static SpriteList bombList = new SpriteList();
    public static SpriteList leftWallList = new SpriteList();
    public static SpriteList rightWallList = new SpriteList();
    public static SpriteList bubbleList = new SpriteList();
    public static SpriteList objectList = new SpriteList();
    public static SpriteList uI = new SpriteList();

    //Private Variables
    private static int vineCount = 3;
    public static int VineCount
    {
      get { return vineCount; }
    }

    private static Point vineImageIndex = Point.Zero;
    public static int VineImageIndex
    {
      get { return vineImageIndex.X; }
    }
    public static int VineImageIndexDepth
    {
      get { return vineImageIndex.Y; }
    }
  
    private static int wallCount = 1;
    public static int WallCount
    {
      get { return wallCount; }
    }
    private static Point wallImageIndex = Point.Zero;
    public static int WallImageIndex
    {
      get { return wallImageIndex.X; }
    }
    public static int WallImageIndexDepth
    {
      get { return wallImageIndex.Y; }
    }

    private static int bubbleCount = 50;
    public static int BubbleCount
    {
      get { return bubbleCount; }
    }
    private static Point bubbleImageIndex = Point.Zero;
    public static int BubbleImageIndex
    {
      get { return bubbleImageIndex.X; }
    }
    public static int BubbleImageIndexDepth
    {
      get { return bubbleImageIndex.Y; }
    }

    private static int bombCount = 10;
    public static int BombCount
    {
      get { return bombCount; }
    }
    private static Point bombImageIndex = Point.Zero;
    public static int BombImageIndex
    {
      get { return bombImageIndex.X; }
    }
    public static int BombImageIndexDepth
    {
      get { return bombImageIndex.Y; }
    }

    private static float bombRate = 0.0025f;
    public static float BombRate
    {
      get { return bombRate; }
    }

    private static float bubbleRate = 0.025f;
    public static float BubbleRate
    {
      get { return bubbleRate; }
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

    public Live()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";

			//for setting up data storage
			this.Components.Add(new GamerServicesComponent(this));

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
			base.Initialize();

      //Make the game full screen like this
      //graphics.PreferredBackBufferWidth = 1280;
      //graphics.PreferredBackBufferHeight = 1024;
      //graphics.ToggleFullScreen();
      //graphics.ApplyChanges();

      //Middle Point of the screen
      halfScreen.X = screenWidth / 2.0f;
      halfScreen.Y = screenHeight / 2.0f;

      //UI locations -- set to within 80% of the screen
      scoreLocation = new Vector2(screenWidth * 0.03f, screenHeight * 0.01f);
      healthLocation = new Vector2(screenWidth * 0.03f, screenHeight * 0.95f);
      bonusLocation = new Vector2(screenWidth * 0.70f, screenHeight * 0.01f);

      //Sound
      audioEngine = new AudioEngine("Content\\Audio\\Infection.xgs");//@"..\..\..\Content\Win\Sounds.xgs");
			soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");//@"..\..\..\Content\Win\Sound Bank.xsb");
			waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");//@"..\..\..\Content\Win\Wave Bank.xwb");
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);

      imageArray = new Texture2D[21];
      colorArray = new Color[21][];

      bombImageIndex.X = 0;
      bombImageIndex.Y = 5;
      imageArray[0] = Content.Load<Texture2D>("Weapon/weapon01");
      imageArray[1] = Content.Load<Texture2D>("Weapon/weapon02");
      imageArray[2] = Content.Load<Texture2D>("Weapon/weapon03");
      imageArray[3] = Content.Load<Texture2D>("Weapon/weapon02");
      imageArray[4] = Content.Load<Texture2D>("Weapon/weapon01");

      vineImageIndex.X = 5;
      vineImageIndex.Y = 4;
      imageArray[5] = Content.Load<Texture2D>("Vine/vinebase");
      imageArray[6] = Content.Load<Texture2D>("Vine/vinebase02");
      imageArray[7] = Content.Load<Texture2D>("Vine/vinebase03");
      imageArray[8] = Content.Load<Texture2D>("Vine/vinebase04");

      bubbleImageIndex.X = 10;
      bubbleImageIndex.Y = 5;
      imageArray[10] = Content.Load<Texture2D>("Bubble/bubble00");
      imageArray[11] = Content.Load<Texture2D>("Bubble/bubble01");
      imageArray[12] = Content.Load<Texture2D>("Bubble/bubble02");
      imageArray[13] = Content.Load<Texture2D>("Bubble/bubble03");
			imageArray[14] = Content.Load<Texture2D>("Bubble/Gas 001");

      wallImageIndex.X = 15;
      wallImageIndex.Y = 2;
      imageArray[15] = Content.Load<Texture2D>("walledge00");
      imageArray[16] = Content.Load<Texture2D>("walledge01");
			
      imageArray[20] = Content.Load<Texture2D>("Cursor/cursor02");

      colorArray[0] = new Color[imageArray[0].Width * imageArray[0].Height];
      colorArray[1] = new Color[imageArray[1].Width * imageArray[1].Height];
      colorArray[2] = new Color[imageArray[2].Width * imageArray[2].Height];
      colorArray[3] = new Color[imageArray[3].Width * imageArray[3].Height];
      colorArray[4] = new Color[imageArray[4].Width * imageArray[4].Height];
      
      colorArray[5] = new Color[imageArray[5].Width * imageArray[5].Height];
      colorArray[6] = new Color[imageArray[6].Width * imageArray[6].Height];
      colorArray[7] = new Color[imageArray[7].Width * imageArray[7].Height];
      colorArray[8] = new Color[imageArray[8].Width * imageArray[8].Height];
      
      colorArray[10] = new Color[imageArray[10].Width * imageArray[10].Height];
      colorArray[11] = new Color[imageArray[11].Width * imageArray[11].Height];
      colorArray[12] = new Color[imageArray[12].Width * imageArray[12].Height];
      colorArray[13] = new Color[imageArray[13].Width * imageArray[13].Height];

      colorArray[15] = new Color[imageArray[15].Width * imageArray[15].Height];
      colorArray[16] = new Color[imageArray[16].Width * imageArray[16].Height];

      imageArray[0].GetData(colorArray[0]);
      imageArray[1].GetData(colorArray[1]);
      imageArray[2].GetData(colorArray[2]);
      imageArray[3].GetData(colorArray[3]);
      imageArray[4].GetData(colorArray[4]);
      
      imageArray[5].GetData(colorArray[5]);
      imageArray[6].GetData(colorArray[6]);
      imageArray[7].GetData(colorArray[7]);
      imageArray[8].GetData(colorArray[8]);
      
      imageArray[10].GetData(colorArray[10]);

      imageArray[15].GetData(colorArray[15]);
      imageArray[16].GetData(colorArray[16]);

      vineMake = Content.Load<SoundEffect>("Vine/vineMake");
      vineDestroy = Content.Load<SoundEffect>("Vine/vineDestroyHigh");
      vineHit = Content.Load<SoundEffect>("Vine/vineHit02");
      coreHit = Content.Load<SoundEffect>("coreHitSudden");
      backgroundHum = Content.Load<SoundEffect>("lowThump").CreateInstance();
      backgroundHum.Volume = 0.3f; //reduces headaches

      sFont = Content.Load<SpriteFont>("SpriteFont1");

      for (int i = 0; i < wallCount; i++)
      {
        Sprite s = new Wall();
        leftWallList.Add(s);
      }
      objectList.Add(leftWallList);

      for (int i = 0; i < wallCount; i++)
      {
        Sprite s = new Wall();
        ((Wall)s).Flip();
        rightWallList.Add(s);
      }
      objectList.Add(rightWallList);

      for (int i = 0; i < vineCount; i++)
      {
				SpriteList sl = new SpriteList();//Vine();
        for (int j = 0; j < vineCount; j++)
        {
          Sprite s = new Vine();
          sl.Add(s);
        }
        vineList.Add(sl);
      }
      objectList.Add(vineList);

      for (int i = 0; i < bombCount; i++)
      {
        Sprite s = new Bomb();
        bombList.Add(s);
      }
      objectList.Add(bombList);

      for (int i = 0; i < bubbleCount; i++)
      {
        Sprite s = new Bubble();
        bubbleList.Add(s);
      }
      uI.Add(bubbleList); //think they should always move. they are cosmetic afterall

      cursor = new Cursor();
      uI.Add(cursor);

      objectList.Setup();
      uI.Setup();

      FindBorders();

      State = GAME_MENU;

      // Comment out this line for one game
      //   to clear the high score
      //GetHighScore();
    }

    //called on start up and when walls move
    public static void FindBorders()
    {
      FindLeftBorder();
      FindRightBorder();
    }

    public static void FindLeftBorder()
    {
      if (leftWallList.array.Length > 0)
      {
        float best = float.MinValue;
        float temp;

        for (int i = 0; i < WallCount; i++)
        {
          temp = ((Wall)leftWallList.getNext()).rightWall();
          if (temp > best) best = temp;
        }

        leftBorder = best;
      }
      else
      {
        leftBorder = 0f;
      }
    }

    public static void FindRightBorder()
    {
      if (rightWallList.array.Length > 0)
      {
        float best = float.MaxValue;
        float temp;

        for (int i = 0; i < WallCount; i++)
        {
          temp = ((Wall)rightWallList.getNext()).leftWall();
          if (temp < best) best = temp;
        }

        rightBorder = best;
      }
      else
      {
        rightBorder = screenWidth;
      }
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
    public static float randBoundedFloat(float min, float max)
    {
      return min + (randFloat() * (max - min));
      //return MathHelper.Clamp(randFloat(), min, max);
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
    public static bool adjustedProbability(double p) { return (p * (60.0 / Live.frameRate) > (rand.NextDouble())); }

    public static void randColor(ref Color c)
    {
      c.R = (byte)rand.Next(256);
      c.G = (byte)rand.Next(256);
      c.B = (byte)rand.Next(256);
    }
    
    //Frame Functions
    public static float frameFactor() { return (float)(60.0 / Live.frameRate); }

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
      playTime -= playTime.Negate();
      lives = 15;

      //objectList.Setup();
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

			oldKeyboardState = keyboardState;
      keyboardState = Keyboard.GetState();
      mouseState = Mouse.GetState();

      if (keyboardState.IsKeyDown(Keys.Escape))Exit();

      frameRateCounter++;
      if (frameRateCounter >= 100)
      {
        frameRateCounter = 0;
        lowestFrameRate = 9999f;
      }

      frameRate=Math.Truncate((1.0/gameTime.ElapsedGameTime.TotalSeconds)+0.5);
      if (frameRate < lowestFrameRate) lowestFrameRate = frameRate;

      uI.Update(gameTime);

      if (backgroundHum.State == SoundState.Stopped) backgroundHum.Play();

      switch (State)
      {
        case GAME_MENU:
          Draw(gameTime);

          changeColor(ref backColor, 2);
          
          playTime = TimeSpan.Zero;

					if (oldKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space))
					{
						state = GAME_START;
					}
					//if (keyboardState.IsKeyDown(Keys.Space))
					//  pauseDown = true;
					//if (pauseDown && keyboardState.IsKeyUp(Keys.Space))
					//{
					//  pauseDown = false;
					//  State = GAME_START;
					//}

          break;
        case GAME_START:
          playTime += gameTime.ElapsedGameTime;

          Draw(gameTime);
          
          if (playTime >= TimeSpan.FromSeconds(WARM_UP_SECONDS))
          {
            State = GAME_RUN;
            playTime = TimeSpan.Zero;
          }
          break;
        case GAME_RUN:
          objectList.Update(gameTime); //possible setup where 0 time has passed and only draws objects

          changeColor(ref backColor, 3);

          playTime += gameTime.ElapsedGameTime;
          Difficulty = playTime.Minutes + 1;//gameTime.TotalGameTime.Minutes;

          if (Difficulty == 0) Difficulty = 1;

          base.Update(gameTime);  //use of this?

					if (oldKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space))
					{
						state = GAME_PAUSE;
					}
					//if (keyboardState.IsKeyDown(Keys.Space))
					//  pauseDown = true;
					//if (pauseDown && keyboardState.IsKeyUp(Keys.Space))
					//{
					//  pauseDown = false;
					//  State = GAME_PAUSE;
					//}

          break;
        case GAME_PAUSE:
          Draw(gameTime);
          changeColor(ref backColor, 1);

					if (oldKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space))
					{
						state = GAME_RUN;
					}
					//if (keyboardState.IsKeyDown(Keys.Space))
					//  pauseDown = true;
					//if (pauseDown && keyboardState.IsKeyUp(Keys.Space))
					//{
					//  pauseDown = false;
					//  State = GAME_RUN;
					//}

          break;
        case GAME_OVER:
          Draw(gameTime);

					if (oldKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space))
					{
						state = GAME_MENU;
						Reset();
					}
					//if (keyboardState.IsKeyDown(Keys.Space))
					//  pauseDown = true;
					//if (pauseDown && keyboardState.IsKeyUp(Keys.Space))
					//{
					//  state = GAME_MENU;
					//  Reset();
					//}
					// Set the request flag
					if ((!Guide.IsVisible) && (GameSaveRequested == false))
					{
						GameSaveRequested = true;
						result = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One,
								null, null);
					}
          break;
      }

			if ((GameSaveRequested) && (result.IsCompleted))
			{
				StorageDevice device = Guide.EndShowStorageDeviceSelector(result);
				if (device != null && device.IsConnected)
				{
					DoSaveGame(device);
				}
				// Reset the request flag
				GameSaveRequested = false;
			}

			base.Update();
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(backColor);

      // TODO: Add your drawing code here
      spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

			objectList.Draw(spriteBatch);

      spriteBatch.DrawString(sFont, "SCORE " + score.ToString() + " (x " + Difficulty.ToString() + ")", scoreLocation, textColor);

      //spriteBatch.DrawString(sFont, "BONUS X " + Difficulty.ToString(), bonusLocation, textColor);
      //spriteBatch.DrawString(sFont, "Countdown: " + (60 - playTime.Seconds).ToString(), bonusLocation, textColor);

      spriteBatch.DrawString(sFont, "Core Health " + lives.ToString(), healthLocation, textColor);

      //spriteBatch.DrawString(sFont, "HIGH SCORE " + highScore.ToString(), new Vector2(screenWidth * 0.60f, screenHeight * 0.95f), textColor);

      switch (State)
      {
        case GAME_MENU:
          textSize = sFont.MeasureString("Prototype01");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "Prototype01", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y - 50f), textColor);

          textSize = sFont.MeasureString("By: Christopher Smith");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "By: Christopher Smith", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y), textColor);

          textSize = sFont.MeasureString("Press Space to Start");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "Press Space to Start", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y + 50f), textColor);
          break;
        case GAME_START:
          timerString = (WARM_UP_SECONDS * 1000 - playTime.TotalMilliseconds).ToString();
          if (timerString.Length >= 4)
            timerString.Substring(0, 3);

          textSize = sFont.MeasureString(timerString);
          textSize /= 2f;
          spriteBatch.DrawString(sFont, timerString, new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y), textColor);
          break;
        case GAME_RUN:
          spriteBatch.DrawString(sFont, "Countdown: " + (60 - playTime.Seconds).ToString(), bonusLocation, textColor);
          break;
        case GAME_PAUSE:
          spriteBatch.DrawString(sFont, "Countdown: " + (60 - playTime.Seconds).ToString(), bonusLocation, textColor);
          break;
        case GAME_OVER:
          textSize = sFont.MeasureString("GAME OVER");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "GAME OVER", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y - 50f), textColor);

          textSize = sFont.MeasureString("Your Core is Destroyed");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "Your Core is Destroyed", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y), textColor);

          textSize = sFont.MeasureString("Press Space to Restart");
          textSize /= 2f;
          spriteBatch.DrawString(sFont, "Press Space to Restart", new Vector2(halfScreen.X - textSize.X, halfScreen.Y - textSize.Y + 50f), textColor);
          break;
      }

      //spriteBatch.DrawString(sFont, "FR "+lowestFrameRate.ToString(), new Vector2(screenWidth * 0.30f, screenHeight * 0.95f), textColor);
			
			uI.Draw(spriteBatch);

      spriteBatch.End();

      base.Draw(gameTime);
    }

    internal static void baseHit(int stamina, int damage)
    {
      lives -= damage;
      coreHit.Play();
      if (lives <= 0) {
        state = GAME_OVER;
        lives = 0;
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
