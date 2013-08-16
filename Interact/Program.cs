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

            var shouldCompile = args.Any(a => string.Compare("/c", a, true) == 0);
            var solutionPath = Path.GetFullPath(args.Any() ? args[0] : @"..\..\..\Examples\MoneyTransfer\Moneytransfer.csproj");
            var solution = Interact.Transformation.WorkspaceExtensions.GetSolution(solutionPath);
            solution = solution.Rewrite();
            if (shouldCompile)
            {
                Compile(solution);
            }
            else
            {
                foreach (var project in solution.Projects)
                {
                    project.WriteToFile();
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