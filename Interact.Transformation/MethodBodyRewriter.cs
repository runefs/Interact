using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;



namespace Interact.Transformation
{
    public class MethodBodyRewriter : SyntaxRewriter
    {

        public MethodBodyRewriter(Dictionary<string, HashSet<string>> roles, string roleName)
        {
            expressionRewriter = new ExpressionRewriter(roles, roleName, !string.IsNullOrWhiteSpace(roleName));
        }

        private readonly ExpressionRewriter expressionRewriter;

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return node.WithExpression((ExpressionSyntax)expressionRewriter.Visit(node.Expression));
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            return node.WithExpression((ExpressionSyntax)expressionRewriter.Visit(node.Expression));
        }

        
    }
}
