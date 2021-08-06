using Configurations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using System;
using System.Collections.Generic;
using System.Text;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.HttpTasks;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Tasks
{
    public class VirtoCommercePlatform
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private PlatformConfiguration _platformConfiguration;
        private ProjectConfiguration _projectConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static VirtoCommercePlatform Init() => new VirtoCommercePlatform();

        public VirtoCommercePlatform WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public VirtoCommercePlatform WithPlatformConfiguration(PlatformConfiguration platformConfiguration)
        {
            _platformConfiguration = platformConfiguration;
            return this;
        }

        public VirtoCommercePlatform WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public VirtoCommercePlatform WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        private string PlatformPackageName => $"vc-platform-{_platformConfiguration.PlatformVersion}";

        private string PlatformZipPackage => $"{PlatformPackageName}.zip";

        private AbsolutePath PlatformPackagePath => _workingSpaceConfiguration.VirtoCommerceDownloadPath / PlatformZipPackage;

        private AbsolutePath PlatformSourceCode => _workingSpaceConfiguration.VirtoCommerceSourcePath / PlatformPackageName;

        private AbsolutePath PlatformArtifact => _workingSpaceConfiguration.VirtoCommerceSourcePath / "platform-artifact";


        public void Install()
        {
            DownloadPlatform();
            ExtractPlatform();
            CompilePlatform();
            UpdateWebConfig();

            Logger.Info($"Deploy platform to - {_deploymentConfiguration.CommerceDeployPath}");
            CopyDirectoryRecursively(PlatformArtifact, _deploymentConfiguration.CommerceDeployPath, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
        }

        private void DownloadPlatform()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.VirtoCommerceDownloadPath);

            if (!FileExists(PlatformPackagePath))
            {
                Logger.Info($"Download Package - {PlatformZipPackage}");
                HttpDownloadFile(_platformConfiguration.PlatformDownloadUrl, PlatformPackagePath);
            }
        }

        private void ExtractPlatform()
        {
            if (!DirectoryExists(PlatformSourceCode))
            {
                Logger.Info($"Extracting Package to - {_workingSpaceConfiguration.VirtoCommerceSourcePath}");
                EnsureExistingDirectory(PlatformSourceCode);
                UncompressZip(PlatformPackagePath, _workingSpaceConfiguration.VirtoCommerceSourcePath);
            }
        }

        private void CompilePlatform()
        {
            EnsureExistingDirectory(PlatformArtifact);
            EnsureCleanDirectory(PlatformArtifact);

            var platformProject = PlatformSourceCode / "src/VirtoCommerce.Platform.Web/VirtoCommerce.Platform.Web.csproj";

            var projectDirectory = Path.GetDirectoryName(platformProject);

            if (FileExists((AbsolutePath)$"{projectDirectory}/package-lock.json") || FileExists((AbsolutePath)$"{projectDirectory}/package.json"))
            {
                Npm("ci", projectDirectory);
                Npm("run webpack:build", projectDirectory);
            }

            DotNetPublish(
                new DotNetPublishSettings()
                            .SetProject(platformProject)
                            .SetFramework("netcoreapp3.1")
                            .SetConfiguration("Debug")
                            .SetOutput(PlatformArtifact)
                            .SetVerbosity(DotNetVerbosity.Minimal)
            );

            var filesToDelete = new[] { "appsettings.Development.json", "appsettings.Production.json" };

            foreach (var file in filesToDelete)
            {
                DeleteFile(PlatformArtifact / file);
            }
        }

        private void UpdateWebConfig()
        {
            var appSettingsPath = PlatformArtifact / "appsettings.json";
            var appSettingsJson = JsonDeserializeFromFile<dynamic>(appSettingsPath);
            appSettingsJson.ConnectionStrings.VirtoCommerce = _workingSpaceConfiguration.ConnectionString;
            appSettingsJson.Search.Provider = "ElasticSearch";
            appSettingsJson.Search.ElasticSearch.Server = _workingSpaceConfiguration.ElasticSearchAddress;
            appSettingsJson.Assets.FileSystem.RootPath = "~/assets";
            appSettingsJson.Assets.FileSystem.PublicUrl = $"{this._deploymentConfiguration.CommerceUrl}/assets";
            appSettingsJson.ExternalModules.AutoInstallModuleBundles = new JArray();
            appSettingsJson.IdentityOptions.RequiredLength = _projectConfiguration.PasswordLengthPolicy;
            appSettingsJson.Hot = new JObject();
            appSettingsJson.Hot.StorefrontUrl = _deploymentConfiguration.StorefrontUrl;
            appSettingsJson.Hot.AllowFeaturesEditing = true;

            JsonSerializeToFile(appSettingsJson, appSettingsPath);
        }
    }
}
