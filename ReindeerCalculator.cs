using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace VerticaSanta
{
  public class ReindeerCalculator
  {
    private Uri API_URI = new Uri("https://xmas2019.documents.azure.com");
    private string DatabaseId = "World";
    private string CollectionId = "Objects";
    private Reindeer reindeers;

    public ReindeerCalculator(string data)
    {
      reindeers = JsonConvert.DeserializeObject<Reindeer>(data);
    }

    public List<Location> findReindeers()
    {
      using (var documentClient = new DocumentClient(API_URI, reindeers.token))
        return reindeers.zones.Select(zone =>
        {
          var feedOptions = new FeedOptions()
          {
            PartitionKey = new PartitionKey(zone.countryCode),
            MaxItemCount = 1
          };

          var distance = PositionUtil.DistanceInMeters(zone.radius.unit, zone.radius.value);
          var from = new Point(zone.center.lon, zone.center.lat);
          // var sql = "SELECT * FROM root r WHERE root.name = '" + zone.reindeer
          // + "' AND ST_DISTANCE(root.location, {'type': 'Point', 'coordinates':["
          //      + zone.center.lat + ", " + zone.center.lon + "]}) < " + distance;
          // System.Console.WriteLine(sql);
          ReindeerLocation reindeerLocation = documentClient.CreateDocumentQuery<ReindeerLocation>(
            UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
            feedOptions
          ).Where(a => a.name == zone.reindeer && a.location.Distance(from) <= distance)
            .AsEnumerable()
            .FirstOrDefault();

          return new Location()
          {
            name = reindeerLocation.name,
            position = new Center()
            {
              lat = reindeerLocation.location.Position.Latitude,
              lon = reindeerLocation.location.Position.Longitude
            }
          };


        }).ToList();
    }

  }

  public class Location
  {
    public string name { get; set; }
    public Center position { get; set; }
  }

  public class ReindeerLocation
  {
    public string id { get; set; }
    public string countryCode { get; set; }
    public string name { get; set; }
    public Point location { get; set; }

  }

  public class Center
  {
    public double lat { get; set; }
    public double lon { get; set; }
  }

  public class Radius
  {
    public string unit { get; set; }
    public double value { get; set; }
  }

  public class Zone
  {
    public string reindeer { get; set; }
    public string countryCode { get; set; }
    public string cityName { get; set; }
    public Center center { get; set; }
    public Radius radius { get; set; }
  }

  public class Reindeer
  {
    public List<Zone> zones { get; set; }
    public string token { get; set; }
  }
}