using System;
using System.Collections.Generic;

namespace Dust.LanguageServer.Completion
{
  public class Tree<T>
  {
    private readonly TreeNode<T> root;

    public Tree()
    {
      root = new TreeNode<T>(default(T));
    }

    public void Add(T value)
    {
      root.AddChild(value);
    }

    public int IndexOf(TreeNode<T> node)
    {
      return root.IndexOf(node);
    }
    
    public TreeNode<T> LastAdded()
    {
      return root.LastAdded();
    }

    public LinkedListNode<TreeNode<T>> Find(TreeNode<T> node)
    {
      return root.Find(node);
    }

    public LinkedListNode<TreeNode<T>> Find(Predicate<TreeNode<T>> predicate)
    {
      return root.Find(predicate);
    }

    public void Modify(TreeNode<T> node, T value)
    {
      root.Modify(node, value);
    }
  }
}