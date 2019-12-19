using System;

namespace VerticaSanta
{
  public class PositionUtil
  {
    public static double DistanceInMeters(string unit, double value)
    {
      switch (unit)
      {
        case "foot":
          return value * 0.304800610;
        case "kilometer":
          return value * 1000;
        case "meter":
          return value;
        default:
          throw new Exception("Unknown unit: " + unit);
      }
    }
  }
}