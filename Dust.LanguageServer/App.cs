using System;
using System.IO;
using LanguageServer;
using LanguageServer.Json;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;

namespace Dust.LanguageServer
{
  public class App : ServiceConnection
  {
    private Uri workspaceRoot;
    private readonly TextDocumentManager documents = new TextDocumentManager();
    public CompletionProvider completionProvider = new CompletionProvider();
    
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
            TriggerCharacters = new[]
            {
              ".",
              " "
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
      return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new CompletionList
      {
        IsIncomplete = true,
        Items = completionProvider.GetCompletions(documents.Get(@params.TextDocument.Uri), @params.Position).ToArray()
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