namespace Raido.Shim.Library
{
  public static class MathHelper
  {
    public static int NormalizeIncrement(int value, int increment, int maxValue)
    {
      int nextValue = value + increment;
      if (nextValue < 0)
      {
        return 0;
      }
      if (nextValue > maxValue)
      {
        return nextValue - maxValue;
      }
      return increment;
    }
  }
}
