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

    public class ContextAttribute : Attribute { }

    public static class SyntaxNodeExtensions
    {
        public static bool IsRole(this ClassDeclarationSyntax node)
        {
            return node.HasAttribute("Role");
        }

        public static bool IsContext(this ClassDeclarationSyntax node)
        {
            return node.HasAttribute("Context");
        }

        public static IEnumerable<AttributeSyntax> GetByName(this SyntaxList<AttributeListSyntax> self, string attributeName)
        {
            return from attribute in self.SelectMany(x => x.Attributes)
                   where ((SimpleNameSyntax)attribute.Name).Identifier.ValueText == attributeName
                   select attribute;
        }

        public static bool HasAttribute(this ClassDeclarationSyntax node, string attributeName)
        {
            if (node == null) return false;

            return node.AttributeLists.GetByName(attributeName).Any();
        }

        public static T RemoveAttribute<T>(this T self, AttributeSyntax attribute) where T : SyntaxNode
        {
            var list = (AttributeListSyntax)attribute.Parent;
            var newList = list.RemoveNode(attribute, SyntaxRemoveOptions.KeepNoTrivia);
            if (newList.Attributes.Count == 0)
            {
                return (T)self.RemoveNode(list, SyntaxRemoveOptions.KeepNoTrivia);
            }
            else
            {
                return (T)self.ReplaceNode(list, newList);
            }
        }
    }

    public class ContextRewriter : SyntaxRewriter
    {
        public ContextRewriter()
        {
        }

        private MethodDeclarationSyntax ThrowStaticMethodError()
        {
            throw new InvalidOperationException("Can't use static methods in a role");
        }

        private SyntaxList<MemberDeclarationSyntax> AddMemberWithTrivia(SyntaxList<MemberDeclarationSyntax> members, MemberDeclarationSyntax member, SyntaxTriviaList leadingTrivia)
        {
            return members.Add(member.WithLeadingTrivia(leadingTrivia));
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.IsContext())
            {
                node = node.RemoveAttribute(node.AttributeLists.GetByName("Context").First());
                var firstMember = node.Members.FirstOrDefault(n => n.HasLeadingTrivia);
                var memberTrivia = firstMember == null
                                   ? Syntax.TriviaList(Syntax.Whitespace("\t\t"))
                                   : firstMember.GetLeadingTrivia();
                Func<SyntaxList<MemberDeclarationSyntax>, MemberDeclarationSyntax, SyntaxList<MemberDeclarationSyntax>> AddMember =
                    (members, member) => AddMemberWithTrivia(members, member, memberTrivia);
                var roles = node.Members.OfType<ClassDeclarationSyntax>().Where(cls => cls.IsRole());
                if (roles.Any())
                {
                    var rolesAndMethods = roles.ToDictionary(
                                           ro => ro.Identifier.ValueText,
                                           ro => new HashSet<string>(ro.Members.OfType<MethodDeclarationSyntax>()
                                                                    .Select(m => m.Identifier.ValueText)));
                    var generalRewriter = new ExpressionRewriter(rolesAndMethods);
                    var members = (from m in node.Members
                                   let cls = m as ClassDeclarationSyntax
                                   where !cls.IsRole()
                                   select (MemberDeclarationSyntax)generalRewriter.Visit(m)).Aggregate(new SyntaxList<MemberDeclarationSyntax>(), AddMember);


                    members = (from r in roles
                               let roleName = r.Identifier.ValueText
                               let fieldType = r.BaseList != null
                                               ? r.BaseList.Types.Single().WithTrailingTrivia(Syntax.Whitespace(" ")).WithLeadingTrivia(Syntax.Whitespace(" "))
                                               : Syntax.IdentifierName(" dynamic ")
                               let field = Syntax.FieldDeclaration(Syntax.VariableDeclaration(fieldType))
                                               .WithModifiers(Syntax.Token(SyntaxKind.PrivateKeyword))
                                               .AddDeclarationVariables(Syntax.VariableDeclarator("role____" + roleName)).WithTrailingTrivia(Syntax.Whitespace("\r\n"))
                               select field).Aggregate(members, AddMember);

                    members = (
                        from r in roles
                        let roleName = r.Identifier.ValueText
                        from m in r.Members.OfType<MethodDeclarationSyntax>()
                        let roleMethodRewriter = generalRewriter.WithRoleName(roleName)

                        let mth = m.Modifiers.Contains(Syntax.Token(SyntaxKind.StaticKeyword))
                                    ? ThrowStaticMethodError()
                                    : ((MethodDeclarationSyntax)roleMethodRewriter.Visit(m))
                        let openBrace = mth.Body.OpenBraceToken.WithLeadingTrivia(memberTrivia)
                        let closeBrace = mth.Body.CloseBraceToken.WithLeadingTrivia(memberTrivia)
                        let body = mth.Body.WithOpenBraceToken(openBrace).WithCloseBraceToken(closeBrace)
                        let method = mth.WithBody(body)
                        select method
                        ).Aggregate(members, AddMember);
                    node = node.WithMembers(members);
                }
            }
            return base.VisitClassDeclaration(node);
        }

        
    }
}
