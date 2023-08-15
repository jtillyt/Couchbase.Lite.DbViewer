using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Spectre.Console;
using NuGet.Protocol;
using NuGet.Common;
using System.Reflection;
using Microsoft.Build.Construction;

namespace VersionSelector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var solutionPath = FindAllSolutionPath();

            if (!File.Exists(solutionPath))
            {
                Console.WriteLine($"Unable to find solution: {ALL_SOLUTION_NAME}");
                return;
            }

            ILogger logger = NullLogger.Instance;

            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            "Couchbase.Lite",
                cache,
                logger,
                CancellationToken.None);

            //var couchbaseVersion = "3.1.1";
            var couchbaseVersion = AnsiConsole.Prompt(
                     new SelectionPrompt<string>()
                         .Title("Select the couchbase version below:")
                         .PageSize(10)
                         .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                         .AddChoices(versions.Select(v => v.ToFullString()).OrderByDescending(s => s))
                         );

            // Echo the fruit back to the terminal
            AnsiConsole.WriteLine($"{couchbaseVersion} selected");

            UpdateProjects(solutionPath, couchbaseVersion);

            Console.WriteLine("Projects Updated");
            Console.ReadLine();
        }

        private static void UpdateProjects(string solutionPath, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return;
            }

            Console.WriteLine($"Loading solution: {solutionPath}");
            var solutionFile = SolutionFile.Parse(solutionPath);
            foreach (var project in solutionFile.ProjectsInOrder)
            {
                if (!project.ProjectName.Contains("DbViewer"))
                {
                    continue;
                }

                Console.WriteLine($"Processing project: {project.ProjectName}");

                var projectRootElement = ProjectRootElement.Open(project.AbsolutePath);

                bool updatedProject = false;
                foreach (var child in projectRootElement.Children)
                {
                    if (child is ProjectItemGroupElement itemGroup)
                    {
                        foreach (var itemGroupChild in itemGroup.Children)
                        {
                            if (itemGroupChild is ProjectItemElement item)
                            {
                                if (item.ElementName == "PackageReference")
                                {
                                    if (item.Include.Contains("Couchbase.Lite"))
                                    {
                                        var versionElement = item.FirstChild as ProjectMetadataElement;

                                        if (versionElement != null)
                                        {
                                            versionElement.Value = version;
                                            updatedProject = true;
                                            Console.WriteLine($"Updated '{project.ProjectName}' - '{item.Include}'");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (updatedProject)
                {
                    projectRootElement.Save();
                }
            }
        }

        private static string FindAllSolutionPath()
        {
            var location = Assembly.GetExecutingAssembly().Location;

            while (true)
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    return "";
                }

                var path = Path.Combine(location, ALL_SOLUTION_NAME);

                if (File.Exists(path))
                {
                    return path;
                }

                location = Path.GetDirectoryName(location);
            }
        }

        private const string ALL_SOLUTION_NAME = "DbViewer.All.sln";
    }
}