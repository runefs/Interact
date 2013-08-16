using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Services;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.IO;
using System.Threading;



namespace Interact.Transformation
{
    public static class WorkspaceExtensions
    {
        private static readonly ContextRewriter contextRewriter = new ContextRewriter();

        public static ISolution Rewrite(this ISolution solution)
        {
            Parallel.ForEach(solution.Projects.SelectMany(p => p.Documents), doc =>
            {
                var newDoc = doc.UpdateSyntaxRoot(contextRewriter.Visit((SyntaxNode)doc.GetSyntaxTree().GetRoot()));
                solution = solution.UpdateDocument(newDoc);
            });
            return solution;
        }

        public static void WriteToFile(this IProject project, string outputDir="output")
        {
            var projectPath = project.FilePath != null ? project.FilePath : "temp.csproj";
            var projectDir = Path.Combine(Path.GetDirectoryName(projectPath), outputDir);
            var projectFileName = Path.GetFileName(projectPath);
            if (!Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }

            projectPath = Path.Combine(projectDir, projectFileName);
            if (project.FilePath != null)
            {
                File.Copy(project.FilePath, projectPath, true);
            }
            Parallel.ForEach(project.Documents, doc =>
            {
                var directory = Path.Combine(Path.GetDirectoryName(doc.FilePath), outputDir);
                var fileName = Path.GetFileName(doc.FilePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var outputPath = Path.Combine(directory, fileName);
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                using (var writer = new System.IO.StreamWriter(File.OpenWrite(outputPath)))
                {
                    doc.GetText().Write(writer);
                }
            });
        }
        public static ISolution GetSolution(string _solutionPath)
        {
            if (!File.Exists(_solutionPath))
            {
                throw new InvalidOperationException("File not found (" + _solutionPath + ")");
            }

            IWorkspace workspace = null;
            switch (Path.GetExtension(_solutionPath).ToUpper())
            {
                case ".SLN":
                    workspace = Workspace.LoadSolution(_solutionPath);
                    break;
                case ".CSPROJ":
                    workspace = Workspace.LoadStandAloneProject(_solutionPath);
                    break;
                case ".CS":
                    var solutionId = SolutionId.CreateNewId();
                    workspace = Workspace.GetWorkspace(solutionId);
                    var projectId = ProjectId.CreateNewId(solutionId);
                    var documentId = DocumentId.CreateNewId(projectId, Path.GetFileNameWithoutExtension(_solutionPath));
                    workspace.AddExistingDocument(documentId, _solutionPath);
                    break;
                default:
                    throw new InvalidOperationException("file type not supported");
            }

            var solution = workspace.CurrentSolution;
            return solution;
        }
    }
}
