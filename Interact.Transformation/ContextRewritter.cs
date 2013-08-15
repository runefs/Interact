using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;

namespace Interact.Transformation
{
    public class RoleAttribute : Attribute
    {
    }

    public class ContextRewriter : SyntaxRewriter
    {
        public ContextRewriter()
        {
        }

        private BaseMethodDeclarationSyntax ThrowStaticMethodError()
        {
            throw new InvalidOperationException("Can't use static methods in a role");
        }

        private SyntaxList<MemberDeclarationSyntax> AddMember(SyntaxList<MemberDeclarationSyntax> members, MemberDeclarationSyntax member)
        {
            return members.Add(member);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var roles = node.Members.OfType<ClassDeclarationSyntax>().Where(cls => IsRole(cls));
            if (roles.Any())
            {
                var rolesAndMethods = roles.ToDictionary(
                                       ro => ro.Identifier.ValueText,
                                       ro => new HashSet<string>(ro.Members.OfType<MethodDeclarationSyntax>()
                                                                .Select(m => m.Identifier.ValueText)));
                var methodBodyRewriter = new MethodBodyRewriter(rolesAndMethods, null);
                var members = (from m in node.Members
                               let cls = m as ClassDeclarationSyntax
                               where !IsRole(cls)
                               select (MemberDeclarationSyntax)methodBodyRewriter.Visit(m)).Aggregate(new SyntaxList<MemberDeclarationSyntax>(), AddMember);

                var generalRewriter = new MethodBodyRewriter(rolesAndMethods, null);
                members = (from r in roles
                           let roleName = r.Identifier.ValueText
                           let field = Syntax.FieldDeclaration(
                                               Syntax.VariableDeclaration(
                                                       Syntax.IdentifierName(" dynamic ")))
                                           .WithModifiers(Syntax.Token(SyntaxKind.PrivateKeyword))
                                           .AddDeclarationVariables(Syntax.VariableDeclarator("role____" + roleName))
                           select field).Aggregate(members, AddMember);

                members = (
                    from r in roles
                    let roleName = r.Identifier.ValueText
                    from m in r.Members.OfType<MethodDeclarationSyntax>()
                    let roleMethodRewriter = new MethodBodyRewriter(rolesAndMethods, roleName)
                    let declRewriter = new RoleMethodDeclarationRewriter(roleName, rolesAndMethods)
                    let mth = m.Modifiers.Contains(Syntax.Token(SyntaxKind.StaticKeyword))
                                ? ThrowStaticMethodError()
                                : (MethodDeclarationSyntax)declRewriter.Visit(m)
                    select (MethodDeclarationSyntax)roleMethodRewriter.Visit(mth)
                    ).Aggregate(members, AddMember);
                return node.WithMembers(members);
            }
            return base.VisitClassDeclaration(node);
        }

        private static bool IsRole(ClassDeclarationSyntax node)
        {
            if (node == null) return false;

            var attributes = node.AttributeLists.Select(x => x.Attributes);

            var isRole = attributes.Any(a => a.Any(x => ((SimpleNameSyntax)x.Name).Identifier.ValueText == "Role"));
            return isRole;
        }
    }
}
