using System.Net;
using System;
using System.IO;
using Newtonsoft.Json;

namespace VerticaSanta
{
  class Program
  {
    static void Main(string[] args)
    {
      switch (args[0])
      {
        case "tracking":
        default:
          string data = File.ReadAllText("santatracking.json");

          var positionCalculator = new PositionCalculator(data);

          positionCalculator.Parse();
          Console.WriteLine(positionCalculator.GetPosition().ToJson());
          break;
        case "reindeer":
          string reindeer = File.ReadAllText("reindeer.json");

          var reindeerCalculator = new ReindeerCalculator(reindeer);
          var locations = reindeerCalculator.findReindeers();

          System.Console.WriteLine(JsonConvert.SerializeObject(locations));
          break;

        case "toys2children":
          string toysXml = File.ReadAllText("toyDistribution.xml");

          var toyCalculator = new ToyCalculator(toysXml);
          var toys = toyCalculator.CalculateToy2Children();

          System.Console.WriteLine(JsonConvert.SerializeObject(toys));
          break;
      }
    }
  }
}
