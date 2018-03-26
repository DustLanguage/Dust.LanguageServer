using System.Collections.Generic;

namespace Dust.LanguageServer.Extensions
{
  public static class LinkedListExtenions
  {
    public static int IndexOf<T>(this LinkedList<T> list, T item)
    {
      int count = 0;
      
      for (LinkedListNode<T> node = list.First; node != null; node = node.Next, count++)
      {
        if (item.Equals(node.Value))
          return count;
      }
      
      return -1;
    }
  }
}