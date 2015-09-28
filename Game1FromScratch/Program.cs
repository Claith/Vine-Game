using System;

namespace Infection
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
      using (Live game = new Live())
      {
          game.Run();
      }
    }
  }
}

