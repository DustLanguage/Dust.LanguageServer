using System;
using System.Collections.Generic;
using System.Linq;
using Dust.LanguageServer.Extensions;

namespace Dust.LanguageServer.Completion
{
  public class TreeNode<T> : IEquatable<TreeNode<T>>
  {
    public T Value { get; set; }

    private readonly LinkedList<TreeNode<T>> children = new LinkedList<TreeNode<T>>();
    private TreeNode<T> lastAdded;

    public TreeNode(T value)
    {
      Value = value;
    }

    public void AddChild(T value)
    {
      children.AddLast(lastAdded = new TreeNode<T>(value));
    }

    public int IndexOf(TreeNode<T> node)
    {
      return children.IndexOf(node);
    }

    public LinkedListNode<TreeNode<T>> Find(TreeNode<T> node)
    {
      LinkedListNode<TreeNode<T>> found = children.Find(node);

      if (found == null)
      {
        foreach (TreeNode<T> child in children)
        {
          found = child.Find(node);

          if (found != null) return found;
        }
      }

      return found;
    }

    public LinkedListNode<TreeNode<T>> Find(Predicate<TreeNode<T>> predicate)
    {
      foreach (TreeNode<T> node in children)
      {
        if (predicate(node)) return Find(node);
      }

      foreach (TreeNode<T> child in children)
      {
        LinkedListNode<TreeNode<T>> found = child.Find(predicate);

        if (found != null) return found;
      }

      return null;
    }

    public void Modify(TreeNode<T> node, T value)
    {
      LinkedListNode<TreeNode<T>> oldLinkedNode = Find(node);

      if (oldLinkedNode == null)
        return;

      if (!children.Contains(oldLinkedNode.Value))
      {
        foreach (TreeNode<T> child in children)
        {
          child.Modify(node, value);
        }
      }
      else
      {
        children.AddBefore(oldLinkedNode, new TreeNode<T>(value));
        children.Remove(oldLinkedNode);
      }
    }

    public TreeNode<T> Last()
    {
      return children.Count > 0 ? children.Last() : null;
    }

    public TreeNode<T> LastAdded()
    {
      return Last()?.LastAdded() ?? Last();
    }

    public bool Equals(TreeNode<T> other)
    {
      if (ReferenceEquals(null, other)) return false;
      return ReferenceEquals(this, other) || EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      return obj.GetType() == GetType() && Equals((TreeNode<T>) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = (children != null ? children.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (lastAdded != null ? lastAdded.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
        return hashCode;
      }
    }
  }
}