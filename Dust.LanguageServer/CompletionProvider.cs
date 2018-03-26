using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Dust.Compiler;
using Dust.Language;
using Dust.Language.Nodes.Expressions;
using Dust.LanguageServer.Extensions;
using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer
{
  public class CompletionProvider
  {
    private static readonly Dictionary<string[], CompletionItem[]> keywordCompletions = new Dictionary<string[], CompletionItem[]>
    {
      {
        new[] {"let"},
        new[]
        {
          new CompletionItem
          {
            Label = "mut",
            Kind = CompletionItemKind.Keyword,
          },
          new CompletionItem
          {
            Label = "fn",
            Kind = CompletionItemKind.Keyword,
          },
        }
      },
      {
        new[] {"fn", "mut"},
        new CompletionItem[0]
      },
      {
        new[] {"public", "internal", "private"},
        new[]
        {
          new CompletionItem
          {
            Label = "let",
            Kind = CompletionItemKind.Keyword
          },
        }
      }
    };

    public List<CompletionItem> GetCompletions(TextDocument document, Position position)
    {
      string[] lines = document.Text.Split(Environment.NewLine);
      string word = lines[position.Line].Substring(0, position.Character).Trim().Split(" ").Last();
      List<CompletionItem> completions = new List<CompletionItem>();

      bool found = false;

      foreach (KeyValuePair<string[], CompletionItem[]> completion in keywordCompletions)
      {
        foreach (string entry in completion.Key)
        {
          if (word == entry)
          {
            found = true;

            completions.AddRange(completion.Value);

            break;
          }
        }
      }

      if (found == false)
      {
        Tree<BraceMatch> tree = BuildBraceTree(document.Text);
        int index = document.GetPosition(position);
        BraceMatch current = tree.Find(node => node.Value.Start <= index && node.Value.End >= index)?.Value.Value;

        // Remove the current line because it might contain errors.
        lines[position.Line] = "";

        AntlrInputStream inputStream = new AntlrInputStream(string.Join(Environment.NewLine, lines));
        DustLexer lexer = new DustLexer(inputStream);
        CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
        DustParser parser = new DustParser(commonTokenStream);
        DustContext context = new DustContext();
        DustVisitor visitor = new DustVisitor(context);
        DustRuntimeCompiler compiler = new DustRuntimeCompiler(context);

        visitor.VisitModule(parser.module());

        DustContext currentContext = current == null ? compiler.CompilerContext : compiler.CompilerContext.Children[tree.IndexOf(new TreeNode<BraceMatch>(current))];

        currentContext.Functions.Union(compiler.CompilerContext.Functions).DistinctBy(function => function.Name).ToList().ForEach(function => completions.Add(new CompletionItem
        {
          Label = function.Name,
          Kind = CompletionItemKind.Function,
          Detail = GetFunctionDetail(function)
        }));

        currentContext.Properties.Union(compiler.CompilerContext.Properties).ToList().ForEach(property => completions.Add(new CompletionItem
        {
          Label = property.Name,
          Kind = CompletionItemKind.Variable,
          Detail = GetPropertyDetail(property)
        }));

        completions.AddRange(new[]
        {
          new CompletionItem
          {
            Label = "let",
            Kind = CompletionItemKind.Keyword
          },
          new CompletionItem
          {
            Label = "public",
            Kind = CompletionItemKind.Keyword
          },
          new CompletionItem
          {
            Label = "internal",
            Kind = CompletionItemKind.Keyword
          },
          new CompletionItem
          {
            Label = "private",
            Kind = CompletionItemKind.Keyword
          }
        });
      }

      return completions;
    }

    private Tree<BraceMatch> BuildBraceTree(string text)
    {
      Tree<BraceMatch> tree = new Tree<BraceMatch>();
      Stack<BraceMatch> stack = new Stack<BraceMatch>();

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

      return tree;
    }

    private string GetFunctionDetail(Function function)
    {
      StringBuilder builder = new StringBuilder($"let fn {function.Name}");

      if (function.Parameters.Length > 0)
      {
        builder.Append("(");

        for (int i = 0; i < function.Parameters.Length; i++)
        {
          builder.Append($"{function.Parameters[i].Identifier.Name}: any");

          if (i != function.Parameters.Length - 1)
          {
            builder.Append(", ");
          }
        }

        builder.Append(")");
      }

      builder.Append(": any");

      return builder.ToString();
    }

    private string GetPropertyDetail(IdentifierExpression property)
    {
      return $"let {(property.IsMutable ? "mut" : "")} {property.Name}: any";
    }
  }
}