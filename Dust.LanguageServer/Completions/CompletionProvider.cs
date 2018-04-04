using System.Collections.Generic;
using System.Linq;
using Dust.Language;
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
            Kind = CompletionItemKind.Keyword
          },
          new CompletionItem
          {
            Label = "fn",
            Kind = CompletionItemKind.Keyword
          }
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
          }
        }
      }
    };

    public CompletionProvider(Project project)
      : base(project)
    {
    }

    public List<CompletionItem> GetCompletions(TextDocument document, Position position)
    {
      string[] lines = document.Text.Split('\n');
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

        DustContext globalContext = Project.CompileFile(string.Join('\n', lines)).GlobalContext;
        DustContext currentContext = document.GetContextAtPosition(position, globalContext);

        currentContext.Functions.Union(globalContext.Functions).DistinctBy(function => function.Name).ToList().ForEach(function => completions.Add(new CompletionItem
        {
          Label = function.Name,
          Kind = CompletionItemKind.Function,
          Detail = function.GetDetail()
        }));

        currentContext.Properties.Union(globalContext.Properties).ToList().ForEach(property => completions.Add(new CompletionItem
        {
          Label = property.Name,
          Kind = CompletionItemKind.Variable,
          Detail = property.GetDetail()
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

  }
}