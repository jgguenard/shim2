using System;
using System.Collections.Generic;

namespace Shim.Library
{
  public sealed class Logger
  {
    public static List<string> Lines;

    public static void Init()
    {
      Lines = new List<string>();
    }

    public static void Log(string message, bool separator = false)
    {
      if (separator)
      {
        message = $"====== {message} ======";
      }
      Lines.Add(message);
      Console.WriteLine(message);
    }
  }
}
