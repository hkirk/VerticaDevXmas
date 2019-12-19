using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace VerticaSanta
{
  public class PositionCalculator
  {
    private string data;
    private RootObject rootObject;

    private const double EARTH = 6378.137;

    public PositionCalculator(string data)
    {
      this.data = data;
    }

    public void Parse()
    {
      rootObject = JsonConvert.DeserializeObject<RootObject>(data);
    }

    public CanePosition GetPosition()
    {
      Debug.Assert(rootObject != null);

      var canePosition = rootObject._source.canePosition;
      var i = 1;
      //System.Console.WriteLine(canePosition.DebugString(i));

      double x = 0;
      double y = 0;

      foreach (var movement in rootObject._source.santaMovements)
      {
        i++;
        double movementInMeters = CalculateMovementInMeters(movement);

        if (movement.direction.ToLower() == "up") y += movementInMeters;
        else if (movement.direction.ToLower() == "down") y -= movementInMeters;
        else if (movement.direction.ToLower() == "left") x -= movementInMeters;
        else if (movement.direction.ToLower() == "right") x += movementInMeters;
        else throw new Exception("Unknown direction " + movement.direction);

        //System.Console.WriteLine(canePosition.DebugString(i));
      }

      var lat = CalculateLatitude(canePosition.lat, y);
      var lon = CalculateLongitude(canePosition.lon, canePosition.lat, x);

      return new CanePosition()
      {
        lat = lat,
        lon = lon
      };
    }

    private double CalculateLatitude(double latitude, double meters)
    {
      return latitude + (meters * OneMeterInDegrees());
    }

    private double CalculateLongitude(double longitude, double latitude, double meters)
    {
      return longitude + (meters * OneMeterInDegrees()) / Math.Cos(latitude * (Math.PI / 180));
    }

    private double OneMeterInDegrees()
    {
      return 1 / (2 * Math.PI / 360 * EARTH) / 1000;
    }

    private double CalculateMovementInMeters(SantaMovement santaMovement)
    {
      return PositionUtil.DistanceInMeters(santaMovement.unit, santaMovement.value);
    }
  }

  public class CanePosition
  {
    public double lat { get; set; }
    public double lon { get; set; }

    public override string ToString() => string.Format("CanePosition[{0}, {1}]", lat, lon);

    public string ToJson() => string.Format("{{\"latitude\":{0},\"longitude\":{1}}}", lat, lon);

    public string DebugString(int i) => string.Format("{0}\t{1}\tcircle4\tred\t{2}\tdebug", lat, lon, i);
  }

  public class SantaMovement
  {
    public string direction { get; set; }
    public double value { get; set; }
    public string unit { get; set; }
  }

  public class Source
  {
    public string id { get; set; }
    public CanePosition canePosition { get; set; }
    public List<SantaMovement> santaMovements { get; set; }
  }

  public class RootObject
  {
    public string _index { get; set; }
    public string _type { get; set; }
    public string _id { get; set; }
    public int _version { get; set; }
    public int _seq_no { get; set; }
    public int _primary_term { get; set; }
    public bool found { get; set; }
    public Source _source { get; set; }
  }
}