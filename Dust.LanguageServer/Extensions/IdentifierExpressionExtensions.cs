using Dust.Language.Nodes.Expressions;
 
 namespace Dust.LanguageServer.Extensions
 {
   public static class IdentifierExpressionExtensions
   {
     public static string GetDetail(this IdentifierExpression property)
     {
       return $"let {(property.IsMutable ? "mut " : "")}{property.Name}: any";
     }
   }
 }