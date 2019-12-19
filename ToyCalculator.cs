using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VerticaSanta
{
  public class ToyCalculator
  {
    private ToyDistributionProblem toyDistributionProblem;
    public ToyCalculator(string text)
    {
      XmlSerializer serializer = new XmlSerializer(typeof(ToyDistributionProblem));
      using (TextReader reader = new StringReader(text))
      {
        toyDistributionProblem = (ToyDistributionProblem)serializer.Deserialize(reader);
      }
    }

    public IEnumerable<Toy2Child> CalculateToy2Children()
    {
      var toysSet = new HashSet<Toy>(toyDistributionProblem.Toys.Toy);
      Debug.Assert(toysSet.Count == toyDistributionProblem.Toys.Toy.Count);

      var childrenWishList = toyDistributionProblem.Children.Child.ToDictionary(c => c.Name, c => c.WishList.Toys.Toy);

      return CalculateToy2Children(toysSet, childrenWishList, new List<Toy2Child>());
    }

    private IEnumerable<Toy2Child> CalculateToy2Children(HashSet<Toy> toysSet,
            Dictionary<string, List<Toy>> childrenWishList, List<Toy2Child> result)
    {
      if (toysSet.Count == 0) return result;
      var shrukenList = childrenWishList
          .ToList() // To be able to call ToDictionary ??
          .ToDictionary(c => c.Key, c => c.Value.Where(t => toysSet.Contains(t)).ToList())
          .ToList();
      result.AddRange(shrukenList.Where(o => o.Value.Count() == 1).Select(e =>
      {
        childrenWishList.Remove(e.Key);
        toysSet.Remove(e.Value.First());
        return new Toy2Child()
        {
          childName = e.Key,
          toyName = e.Value.First().Name
        };
      }).ToList());

      return CalculateToy2Children(toysSet, childrenWishList, result);
    }
  }


  [XmlRoot(ElementName = "Toy")]
  public class Toy
  {
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }

    public override bool Equals(object obj) => obj is Toy toy && Name == toy.Name;

    public override int GetHashCode() => HashCode.Combine(Name);

  }

  [XmlRoot(ElementName = "Toys")]
  public class Toys
  {
    [XmlElement(ElementName = "Toy")]
    public List<Toy> Toy { get; set; }
  }

  [XmlRoot(ElementName = "WishList")]
  public class WishList
  {
    [XmlElement(ElementName = "Toys")]
    public Toys Toys { get; set; }
  }

  [XmlRoot(ElementName = "Child")]
  public class Child
  {
    [XmlElement(ElementName = "WishList")]
    public WishList WishList { get; set; }
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }
  }

  [XmlRoot(ElementName = "Children")]
  public class Children
  {
    [XmlElement(ElementName = "Child")]
    public List<Child> Child { get; set; }
  }

  [XmlRoot(ElementName = "ToyDistributionProblem")]
  public class ToyDistributionProblem
  {
    [XmlElement(ElementName = "Toys")]
    public Toys Toys { get; set; }
    [XmlElement(ElementName = "Children")]
    public Children Children { get; set; }
    [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
    public string Xsi { get; set; }
    [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
    public string Xsd { get; set; }
  }

  public class Toy2Child
  {
    public string childName { get; set; }
    public string toyName { get; set; }
  }
}