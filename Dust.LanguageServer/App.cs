using System;
using System.IO;
using Dust.LanguageServer.Completion;
using Dust.LanguageServer.Signature;
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
    private Project project;
    private CompletionProvider completionProvider;
    private SignatureHelpProvider signatureHelpProvider;

    public App(Stream input, Stream output)
      : base(input, output)
    {
    }

    protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
    {
      workspaceRoot = @params.RootUri;
      project = new Project(workspaceRoot);
      completionProvider = new CompletionProvider(project);
      signatureHelpProvider = new SignatureHelpProvider(project);
      
      project.Documents.OnChanged += DocumentChanged;
      
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
          },
          SignatureHelpProvider = new SignatureHelpOptions
          {
            TriggerCharacters = new[]
            {
              "(",
              ","
            }
          }
        }
      });
    }

    protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
    {
      project.Documents.Add(@params.TextDocument);
    }

    protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
    {
      project.Documents.Change(@params.TextDocument.Uri, @params.TextDocument.Version, @params.ContentChanges);
    }

    protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
    {
      project.Documents.Remove(@params.TextDocument.Uri);
    }

    protected override Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError> Completion(TextDocumentPositionParams @params)
    {
      return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new CompletionList
      {
        IsIncomplete = true,
        Items = completionProvider.GetCompletions(project.Documents.Get(@params.TextDocument.Uri), @params.Position).ToArray()
      });
    }

    protected override Result<SignatureHelp, ResponseError> SignatureHelp(TextDocumentPositionParams @params)
    {
      return Result<SignatureHelp, ResponseError>.Success(signatureHelpProvider.GetSignatureHelp(project.Documents.Get(@params.TextDocument.Uri), @params.Position));
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