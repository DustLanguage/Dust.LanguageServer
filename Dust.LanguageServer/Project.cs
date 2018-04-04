using System;
using Antlr4.Runtime;
using Dust.Compiler;
using Dust.Language;
using Dust.Language.Nodes;

namespace Dust.LanguageServer
{
  public class Project
  {
    public Uri Root { get; }
    public TextDocumentManager Documents { get; }

    public Project(Uri root)
    {
      Root = root;
      Documents = new TextDocumentManager();
    }

    public static CompileResult<object> CompileFile(string text)
    {
      AntlrInputStream inputStream = new AntlrInputStream(text);
      DustLexer lexer = new DustLexer(inputStream);
      CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
      DustParser parser = new DustParser(commonTokenStream);
      DustContext context = new DustContext();
      DustVisitor visitor = new DustVisitor(context);
      DustRuntimeCompiler compiler = new DustRuntimeCompiler(context);

      Module module = (Module) visitor.VisitModule(parser.module());

      return compiler.Compile(module);
    }
  }
}