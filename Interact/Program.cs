using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services.Host;
using Roslyn.Services;
using Roslyn.Compilers.Common;
using Interact.Transformation;
using System.Reflection.Emit;
using System.Reflection;

namespace Interact
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var solutionPath = Path.GetFullPath(args.Any() ? args[0] : @"..\..\..\Examples\MoneyTransfer\Moneytransfer.csproj");
            
            var workspace = string.Compare(Path.GetExtension(solutionPath), ".sln", true) == 0
                            ? Workspace.LoadSolution(solutionPath)
                            : Workspace.LoadStandAloneProject(solutionPath);

            var shouldCompile = args.Any(a => string.Compare("/c", a, true) == 0);

            var solution = workspace.CurrentSolution;
            var contextRewriter = new ContextRewriter();

            foreach (var project in solution.Projects)
            {
                foreach (var doc in project.Documents)
                {
                    var newDoc = doc.UpdateSyntaxRoot(contextRewriter.Visit((SyntaxNode)doc.GetSyntaxTree().GetRoot()));
                    solution = solution.UpdateDocument(newDoc);
                }
            }
            if (shouldCompile)
            {
                Compile(solution);
            }
            else
            {
                foreach (var project in solution.Projects)
                {
                    foreach (var doc in project.Documents)
                    {
                        var fileName = Path.GetFileName(doc.FilePath);
                        var directory = Path.Combine(Path.GetDirectoryName(doc.FilePath),"output");
                        if(!Directory.Exists(directory)){
                            Directory.CreateDirectory(directory);
                        }
                        var outputPath = Path.Combine(directory,fileName);

                        using (var writer = new System.IO.StreamWriter(File.OpenWrite(outputPath)))
                        {
                            doc.GetText().Write(writer);
                        }
                    }
                }
            }

        }

        private static void Compile(ISolution solution)
        {
            var workspaceServices = (IHaveWorkspaceServices)solution;
            var projectDependencyService = workspaceServices.WorkspaceServices.GetService<IProjectDependencyService>();

            foreach (var projectId in projectDependencyService.GetDependencyGraph(solution).GetTopologicallySortedProjects())
            {
                var currentDomain = AppDomain.CurrentDomain;
                var assemblyName = new AssemblyName();
                assemblyName.Name = "Most." + solution.GetProject(projectId).AssemblyName;

                var assemblyBuilder = currentDomain.DefineDynamicAssembly
                               (assemblyName, AssemblyBuilderAccess.RunAndSave);

                var moduleBuilder = assemblyBuilder.
                                                DefineDynamicModule(assemblyName.Name);
                var types = moduleBuilder.GetTypes();
                types.Aggregate((ts, t) =>
                {
                    var tb = t as TypeBuilder;
                    return tb.CreateType();
                });
                solution.GetProject(projectId).GetCompilation().Emit(moduleBuilder);
                assemblyBuilder.Save(assemblyName.Name);
            }
        }
    }
}