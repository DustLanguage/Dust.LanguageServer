using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dust.Language;
using Dust.Language.Nodes.Expressions;
using Dust.LanguageServer.Extensions;
using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer.Completion
{
  public class CompletionProvider : Provider
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

    public CompletionProvider(Project project)
      : base(project)
    {
    }

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
        // Remove the current line because it might contain errors.
        lines[position.Line] = "";

        DustContext globalContext = project.Compile(string.Join(Environment.NewLine, lines));
        DustContext currentContext = document.GetContextAtPosition(position, globalContext);

        currentContext.Functions.Union(globalContext.Functions).DistinctBy(function => function.Name).ToList().ForEach(function => completions.Add(new CompletionItem
        {
          Label = function.Name,
          Kind = CompletionItemKind.Function,
          Detail = GetFunctionDetail(function)
        }));

        currentContext.Properties.Union(globalContext.Properties).ToList().ForEach(property => completions.Add(new CompletionItem
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

    private string GetFunctionDetail(Function function)
    {
      StringBuilder builder = new StringBuilder($"let fn {function.Name}");

      builder.Append("(");

      if (function.Parameters.Length > 0)
      {
        for (int i = 0; i < function.Parameters.Length; i++)
        {
          builder.Append($"{function.Parameters[i].Identifier.Name}: any");

          if (i != function.Parameters.Length - 1)
          {
            builder.Append(", ");
          }
        }
      }

      builder.Append("): any");

      return builder.ToString();
    }

    private string GetPropertyDetail(IdentifierExpression property)
    {
      return $"let {(property.IsMutable ? "mut" : "")} {property.Name}: any";
    }
  }
}