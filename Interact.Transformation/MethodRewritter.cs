using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Interact.Transformation
{
    public class RoleMethodDeclarationRewriter : SyntaxRewriter
    {
        private readonly string roleName;
        private readonly Dictionary<string,HashSet<string>> roles;

        public RoleMethodDeclarationRewriter(string roleName, Dictionary<string, HashSet<string>> roles)
        {
            this.roleName = roleName;
            this.roles = roles;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodName = node.Identifier.ValueText;
            var methodIdentifier = Syntax.Identifier(" self__" + roleName + "__" + methodName);

            node = (MethodDeclarationSyntax)node.WithIdentifier(methodIdentifier);
            return (MethodDeclarationSyntax)new MethodBodyRewriter(roles, roleName).Visit(node);
            
        }
    }
}