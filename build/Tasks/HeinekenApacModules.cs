using Configurations;
using Helpers;
using Nuke.Common;
using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.XmlTasks;
using Nuke.Common.Tools.DotNet;
using System.Linq;

namespace Tasks
{
    public class HeinekenApacModules
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private ProjectConfiguration _projectConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static HeinekenApacModules Init() => new HeinekenApacModules();

        public HeinekenApacModules WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public HeinekenApacModules WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public HeinekenApacModules WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        private AbsolutePath HeinekenApacModulesPath => _workingSpaceConfiguration.WorkingSpace / "heineken-apac-modules";

        public void Install()
        {
            CompileModules();
            DeployModules();
        }

        public void UpdateRemoteUrls()
        {
            foreach (var module in _projectConfiguration.HeinekenApacModules)
            {
                var moduleSourcePath = HeinekenApacModulesPath / module;

                var cloneUrl = string.Format(_projectConfiguration.CloneUrl, module);

                GitHelpers.UpdateRemoteUrl(moduleSourcePath, cloneUrl);
            }
        }

        private void CompileModules()
        {
            foreach (var module in _projectConfiguration.HeinekenApacModules)
            {
                var moduleSourceDirectoryPath = HeinekenApacModulesPath / module;

                Logger.Info($"Working with {module}: {moduleSourceDirectoryPath}");

                var manifestFile = Directory.GetFiles(moduleSourceDirectoryPath, "*.manifest", SearchOption.AllDirectories);

                foreach (var item in manifestFile)
                {
                    var currentDirectory = Path.GetDirectoryName(item);
                    var projectFiles = Directory.GetFiles(currentDirectory, "*.csproj");

                    foreach (var projectFile in projectFiles)
                    {
                        DotNetPublish(
                          new DotNetPublishSettings()
                                      .SetProject(projectFile)
                                      .SetFramework("netcoreapp3.1")
                                      .SetConfiguration("Debug")
                                      .SetVerbosity(DotNetVerbosity.Minimal)
                        );

                        var projectDirectory = Path.GetDirectoryName(projectFile);

                        if (FileExists((AbsolutePath)$"{projectDirectory}/package-lock.json"))
                        {
                            Npm("ci", projectDirectory);
                            Npm("run webpack:build", projectDirectory);
                        }
                    }
                }
            }
        }

        private void DeployModules()
        {
            foreach (var module in _projectConfiguration.HeinekenApacModules)
            {
                var moduleSourceDirectoryPath = HeinekenApacModulesPath / module;

                var manifestFile = new DirectoryInfo(moduleSourceDirectoryPath)
                                        .GetDirectories("*.Web", SearchOption.AllDirectories)
                                        .SelectMany(d => d.GetFiles("*.manifest"))
                                        .Select(f => f.FullName);

                foreach (var item in manifestFile)
                {
                    var source = Path.GetDirectoryName(item);

                    Logger.Info($"Deploying - {item}");

                    var moduleId = XmlPeek(item, "module/id").FirstOrDefault();

                    Logger.Info($"Deploying - {moduleId}");

                    var linkFolderPath = _deploymentConfiguration.CommerceModulesPhysicalPath / moduleId;

                    if (DirectoryExists(linkFolderPath))
                    {
                        Directory.Delete(linkFolderPath);
                    }
                    
                    LinkHelper.CreateSymbolicLink(linkFolderPath, source);
                }
            }
        }
    }
}
