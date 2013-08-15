using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Services;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.IO;

namespace Interact.Transformation
{
   public class Transformer
    {
       private readonly string _solutionPath;
       public Transformer(string solutionPath)
       {
           _solutionPath = Path.GetFullPath(solutionPath);
       }

        public ISolution RewriteSolution(ISolution solution)
        {
            var contextRewriter = new ContextRewriter();

            foreach (var project in solution.Projects)
            {
                foreach (var doc in project.Documents)
                {
                    var newDoc = doc.UpdateSyntaxRoot(contextRewriter.Visit((SyntaxNode)doc.GetSyntaxTree().GetRoot()));
                    solution = solution.UpdateDocument(newDoc);
                }
            }
            return solution;
        }

        public ISolution GetSolution()
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

        public void WriteProjectToFile(IProject project, string outputDir="output")
        {
            var projectPath = project.FilePath != null ? project.FilePath : Path.GetFileNameWithoutExtension(_solutionPath) + ".csproj";
            var projectDir = Path.Combine(Path.GetDirectoryName(projectPath), outputDir);
            var projectFileName = Path.GetFileName(projectPath);
            if (!Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }
            projectPath = Path.Combine(projectDir, projectFileName);
            if (project.FilePath != null)
            {
                File.Copy(project.FilePath, projectPath,true);
            }
            foreach (var doc in project.Documents)
            {
                var directory = Path.Combine(Path.GetDirectoryName(doc.FilePath), outputDir);
                var fileName = Path.GetFileName(doc.FilePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var outputPath = Path.Combine(directory, fileName);

                using (var writer = new System.IO.StreamWriter(File.OpenWrite(outputPath)))
                {
                    doc.GetText().Write(writer);
                }
            }
        }
    }
}
