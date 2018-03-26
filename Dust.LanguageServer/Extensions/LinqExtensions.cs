using System;
using System.Collections.Generic;
using System.Linq;

namespace Dust.LanguageServer.Extensions
{
  public static class LinqExtensions
  {
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> predicate)
    {
      GeneralComparer<T, TKey> comparer = new GeneralComparer<T, TKey>(predicate);

      return items.Distinct(comparer);
    }
  }

  public class GeneralComparer<T, TKey> : IEqualityComparer<T>
  {
    private readonly Func<T, TKey> predicate;

    public GeneralComparer(Func<T, TKey> predicate)
    {
      this.predicate = predicate;
    }

    public bool Equals(T left, T right)
    {
      TKey leftProp = predicate.Invoke(left);
      TKey rightProp = predicate.Invoke(right);
      
      if (leftProp == null && rightProp == null)
      {
        return true;
      }

      if (leftProp == null ^ rightProp == null)
      {
        return false;
      }

      return leftProp.Equals(rightProp);
    }

    public int GetHashCode(T obj)
    {
      var prop = predicate.Invoke(obj);
      return prop == null ? 0 : prop.GetHashCode();
    }
  }
}