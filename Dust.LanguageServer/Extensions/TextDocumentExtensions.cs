using System.Collections.Generic;
using Dust.Language;
using Dust.LanguageServer.Completion;
using LanguageServer.Extensions;
using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer.Extensions
{
  public static class TextDocumentExtensions
  {
    public static DustContext GetContextAtPosition(this TextDocument document, Position position, DustContext globalContext)
    {
      Tree<BraceMatch> tree = new Tree<BraceMatch>();
      Stack<BraceMatch> stack = new Stack<BraceMatch>();
      string text = document.Text;
      int index = text.GetPosition(position);
      
      for (int i = 0; i < text.Length; i++)
      {
        char character = text[i];

        if (character == '{')
        {
          if (tree.LastAdded()?.Value.End == -1)
          {
            tree.LastAdded().AddChild(new BraceMatch(i, -1));
          }
          else
          {
            tree.Add(new BraceMatch(i, -1));
          }

          stack.Push(new BraceMatch(i, -1));
        }

        if (character == '}')
        {
          LinkedListNode<TreeNode<BraceMatch>> node = tree.Find(new TreeNode<BraceMatch>(stack.Pop()));
          tree.Modify(node.Value, new BraceMatch(node.Value.Value.Start, i));
        }
      }

      BraceMatch current = tree.Find(node => node.Value.Start <= index && node.Value.End >= index)?.Value.Value;

      return current == null ? globalContext : globalContext.Children[tree.IndexOf(new TreeNode<BraceMatch>(current))];
    }
  }
}