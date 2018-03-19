using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageServer;
using LanguageServer.Json;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using SampleServer;

namespace Dust.LanguageServer
{
  public class App : ServiceConnection
  {
    private Uri workspaceRoot;
    private readonly TextDocumentManager documents = new TextDocumentManager();

    public App(Stream input, Stream output)
      : base(input, output)
    {
      documents.OnChanged += DocumentChanged;
    }

    protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
    {
      workspaceRoot = @params.RootUri;

      return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(new InitializeResult
      {
        Capabilities = new ServerCapabilities
        {
          TextDocumentSync = TextDocumentSyncKind.Full,
          CompletionProvider = new CompletionOptions
          {
            TriggerCharacters = new []
            {
              "."
            },
            ResolveProvider = false
          }
        }
      });
    }

    protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
    {
      documents.Add(@params.TextDocument);
    }

    protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
    {
      documents.Change(@params.TextDocument.Uri, @params.TextDocument.Version, @params.ContentChanges);
    }

    protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
    {
      documents.Remove(@params.TextDocument.Uri);
    }

    protected override Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError> Completion(TextDocumentPositionParams @params)
    {
      Dictionary<string[], CompletionList> possibleCompletions = new Dictionary<string[], CompletionList>
      {
        {
          new[] {"let"},
          new CompletionList
          {
            Items = new[]
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
              },
            }
          }
        },
        {
          new[] {"fn", "mut"},
          new CompletionList
          {
            Items = null
          }
        },
        {
          new[] {"public", "internal", "private"},
          new CompletionList
          {
            Items = new[]
            {
              new CompletionItem
              {
                Label = "let",
                Kind = CompletionItemKind.Keyword
              },
            }
          }
        }
      };

      TextDocumentItem document = documents.Get(@params.TextDocument.Uri);

      string text = document.Text.Split(Environment.NewLine)[@params.Position.Line].Substring(0, (int) @params.Position.Character).Trim();
      
      string word = text.Split(" ").Last();

      foreach (KeyValuePair<string[], CompletionList> completion in possibleCompletions)
      {
        foreach (string entry in completion.Key)
        {
          if (word == entry)
          {
            return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(completion.Value);
          }
        }
      }

      return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new CompletionList
      {
        Items = new[]
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
        }
      });
    }

    protected override Result<CompletionItem, ResponseError> ResolveCompletionItem(CompletionItem @params)
    {
      return Result<CompletionItem, ResponseError>.Success(@params);
    }

    private void DocumentChanged(TextDocumentChangedEventArgs args)
    {
    }
  }
}