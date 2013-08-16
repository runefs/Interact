using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Interact.Transformation
{
    class ExpressionRewriter : SyntaxRewriter
    {
        private string prefix { get { 
            return "role____"; } }
        public ExpressionRewriter(Dictionary<string, HashSet<string>> roles) : this(roles,null)
        {
        }

        private ExpressionRewriter(Dictionary<string, HashSet<string>> roles, string roleName = null)
        {
            if (_roleName != null && !roles.ContainsKey(_roleName)) throw new ArgumentException("roleName is not a known role");

            this.roles = roles;
            this._roleName = roleName;
            this.IsRoleMethod = !string.IsNullOrWhiteSpace(roleName);
        }

        public ExpressionRewriter WithRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException("roleName");
            return new ExpressionRewriter(roles, roleName);
        }
        private readonly Dictionary<string, HashSet<string>> roles;
        private readonly string _roleName;
        
        private readonly bool IsRoleMethod;

        private bool IsRole(ExpressionSyntax expression)
        {
            if (expression is ThisExpressionSyntax || expression is SimpleNameSyntax)
            {
                return (expression is ThisExpressionSyntax && IsRoleMethod) ||
                    (!(expression is ThisExpressionSyntax) && IsRoleName(((SimpleNameSyntax)expression).Identifier.ValueText));
            }
            return false;
        }

        private string GetRoleName(SyntaxNode node)
        {
            switch(node.Kind){
                case SyntaxKind.IdentifierName:
                var identifier = ((SimpleNameSyntax)node).Identifier.ValueText;
                return GetRoleName(identifier);
                case SyntaxKind.ThisExpression:
                    return _roleName;
                default:
                    throw new InvalidOperationException("Unknown identifier kind");
            }
        }

        private string GetRoleName(string identifier)
        {
            var roleName = identifier.StartsWith(prefix) ?
                identifier.Substring(prefix.Length)
                : identifier;
            if (string.IsNullOrWhiteSpace(roleName)) throw new InvalidOperationException("Couldn't find a role name");
            return roleName;
        }

        private bool IsRoleName(string roleName)
        {
            return roles.ContainsKey(GetRoleName(roleName));
        }

        public override SyntaxNode VisitThisExpression(ThisExpressionSyntax node)
        {
            if (IsRoleMethod)
            {
               return RewriteRoleExpression(node);
            }
            return base.VisitThisExpression(node);
        }
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            return RewriteRoleExpression(node);
        }

        private ExpressionSyntax RewriteRoleExpression(ExpressionSyntax expression)
        {
            var isRole = IsRole(expression);
            if (isRole)
            {
                var fieldName = prefix + GetRoleName(expression);

                expression = (SimpleNameSyntax)Syntax.IdentifierName(fieldName).WithLeadingTrivia(expression.GetLeadingTrivia());
            }
            return expression;
        }

        private bool IsRoleMethodInvocation(MemberAccessExpressionSyntax node)
        {
            if (IsRole(node.Expression))
            {
                var methodName = ((SimpleNameSyntax)node.Name).Identifier.ValueText;
                var expression = node.Expression;
                var rn = (IsRoleMethod && (expression is ThisExpressionSyntax)) ? _roleName : GetRoleName(expression);
                return roles[rn].Contains(methodName);
            }
            return false;
        }

        private SyntaxNode GetRoleMethodInvocation(MemberAccessExpressionSyntax node)
        {
            var expression = Visit(node.Expression);
            if (node.Expression != expression)
            {
                node = node.ReplaceNode(node.Expression, expression);
            }
            if (IsRoleMethodInvocation(node))
            {
                var methodName = ((SimpleNameSyntax)node.Name).Identifier.ValueText;
                return Syntax.IdentifierName("self__" + GetRoleName(node.Expression) + "__" + methodName).WithLeadingTrivia(node.GetLeadingTrivia());
                
            }
            return node;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (IsRoleMethod)
            {
                var methodName = node.Identifier.ValueText;
                var methodIdentifier = Syntax.Identifier(" self__" + _roleName + "__" + methodName);

                node = (MethodDeclarationSyntax)node.WithIdentifier(methodIdentifier);
            }
            return base.VisitMethodDeclaration(node);

        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var memAccess = node.Expression as MemberAccessExpressionSyntax;
            if (memAccess != null)
            {
                var expression = GetRoleMethodInvocation(memAccess);
                var arguments = from arg in node.ArgumentList.Arguments
                                select (ArgumentSyntax)Visit(arg);
                var args = new SeparatedSyntaxList<ArgumentSyntax>().Add(arguments.ToArray());
                node = node.WithArgumentList(Syntax.ArgumentList(args));
                if (expression != node.Expression)
                {
                    node = node.ReplaceNode(node.Expression, expression);
                }
            }
            return base.VisitInvocationExpression(node);
        }       
    }
}
