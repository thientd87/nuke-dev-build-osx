using Configurations;
using Nuke.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.XmlTasks;
using static Nuke.Common.IO.HttpTasks;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.IO;
using Nuke.Common.Tools.Npm;
using Helpers;

namespace Tasks
{
    public class VirtoCommerceModules
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private PlatformConfiguration _platformConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static VirtoCommerceModules Init() => new VirtoCommerceModules();

        public VirtoCommerceModules WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public VirtoCommerceModules WithPlatformConfiguration(PlatformConfiguration platformConfiguration)
        {
            _platformConfiguration = platformConfiguration;
            return this;
        }

        public VirtoCommerceModules WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        public void Install()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.VirtoCommerceModulesDownloadPath);
            EnsureExistingDirectory(_workingSpaceConfiguration.VirtoCommerceModulesSourceCode);
            RemoveObsoleteModulePackages();
            RemoveObsoleteModules();
            DownloadModules();
            ExtractModules();
            CompileModules();
            DeployModule();
        }

        private void RemoveObsoleteModulePackages()
        {
            var currentModulesWithVersion = _platformConfiguration.Modules.Select(x => new {
                Name = x.Name,
                Version = x.Version
            }).ToList();

            var previousModules = Directory.GetFiles(_workingSpaceConfiguration.VirtoCommerceModulesDownloadPath, "*.zip").Select(x => Path.GetFileNameWithoutExtension(x)).ToList();

            foreach (var module in currentModulesWithVersion)
            {
                var prevModule = previousModules.FirstOrDefault(x => x.StartsWith(module.Name));

                if (!string.IsNullOrEmpty(prevModule) && prevModule != $"{module.Name}-{module.Version}")
                {
                    Logger.Info($"Removing previous version: {prevModule}");
                    DeleteFile($@"{_workingSpaceConfiguration.VirtoCommerceModulesDownloadPath}/{prevModule}.zip");
                }
            }
        }

        private void RemoveObsoleteModules()
        {
            var currentModulesWithVersion = _platformConfiguration.Modules.Select(x => new {
                Name = x.Name,
                Version = x.Version
            }).ToList();

            var previousModules = Directory.GetDirectories(_workingSpaceConfiguration.VirtoCommerceModulesSourceCode)
                .Select(x => Path.GetRelativePath(_workingSpaceConfiguration.VirtoCommerceModulesSourceCode, x))
                .ToList();

            foreach (var module in currentModulesWithVersion)
            {
                var prevModule = previousModules.FirstOrDefault(x => x.StartsWith(module.Name));
                if (!string.IsNullOrEmpty(prevModule) && prevModule != $"{module.Name}-{module.Version}")
                {
                    var path = _workingSpaceConfiguration.VirtoCommerceModulesSourceCode / prevModule;

                    var manifestFile = Directory.GetFiles(path, "*.manifest", SearchOption.AllDirectories);

                    foreach (var item in manifestFile)
                    {
                        var moduleId = XmlPeek(item, "module/id").FirstOrDefault();

                        var linkFolderPath = _deploymentConfiguration.CommerceModulesPhysicalPath / moduleId;

                        if (DirectoryExists(linkFolderPath))
                        {
                            DeleteDirectory(linkFolderPath);
                        }
                    }

                    DeleteDirectory(path);
                }
            }
        }

        private void DownloadModules()
        {
            foreach (var module in _platformConfiguration.Modules)
            {
                var modulePackagePath = _workingSpaceConfiguration.VirtoCommerceModulesDownloadPath / $"{module.Name}-{module.Version}.zip";

                if (!FileExists(modulePackagePath))
                {
                    Logger.Info($"Downloading Module {module.Name} - {module.Version}");

                    HttpDownloadFile(module.DownloadUrl, modulePackagePath);
                }
            }

        }

        private void ExtractModules()
        {

            foreach (var module in _platformConfiguration.Modules)
            {
                var modulePackagePath = $"{_workingSpaceConfiguration.VirtoCommerceModulesDownloadPath}/{module.Name}-{module.Version}.zip";
                var moduleSourcePath = _workingSpaceConfiguration.VirtoCommerceModulesSourceCode/ $"{module.Name}-{module.Version}";

                if (!DirectoryExists(moduleSourcePath))
                {
                    UncompressZip(modulePackagePath, _workingSpaceConfiguration.VirtoCommerceModulesSourceCode);
                }
            }
        }



        private void CompileModules()
        {
            var modules = Directory.GetDirectories(_workingSpaceConfiguration.VirtoCommerceModulesSourceCode);

            foreach (var module in modules)
            {
                var manifestFile = Directory.GetFiles(module, "*.manifest", SearchOption.AllDirectories);

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

        public void DeployModule()
        {
            var modules = Directory.GetDirectories(_workingSpaceConfiguration.VirtoCommerceModulesSourceCode);

            foreach (var module in modules)
            {
                var manifestFile = new DirectoryInfo(module)
                                        .GetDirectories("*.Web", SearchOption.AllDirectories)
                                        .SelectMany(d => d.GetFiles("*.manifest"))
                                        .Select(f => f.FullName);

                foreach (var item in manifestFile)
                {
                    var source = Path.GetDirectoryName(item);

                    var moduleId = XmlPeek(item, "module/id").FirstOrDefault();

                    var linkFolderPath = _deploymentConfiguration.CommerceModulesPhysicalPath / moduleId;

                    if (DirectoryExists(linkFolderPath))
                    {
                        Directory.Delete(linkFolderPath);
                    }

                    Logger.Info($"Deploying Module - {moduleId}");

                    LinkHelper.CreateSymbolicLink(linkFolderPath, source);
                }
            }
        }
    }
}
