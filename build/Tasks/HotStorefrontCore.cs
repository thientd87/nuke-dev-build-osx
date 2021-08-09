using Configurations;
using Helpers;
using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.XmlTasks;
using static Nuke.Common.IO.SerializationTasks;
using Nuke.Common.Tools.DotNet;

namespace Tasks
{
    public class HotStorefrontCore
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private ProjectConfiguration _projectConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static HotStorefrontCore Init() => new HotStorefrontCore();

        public HotStorefrontCore WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public HotStorefrontCore WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public HotStorefrontCore WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        private AbsolutePath HotStorefrontProjectFile => _workingSpaceConfiguration.HotStorefrontSourcePath / "VirtoCommerce.Storefront/VirtoCommerce.Storefront.csproj";

        private string HotThemeReleasePath => $"{_workingSpaceConfiguration.HotThemeWorkingFolder}/{_projectConfiguration.HotThemeNx}/dist/apps/hot-b2b-{_deploymentConfiguration.Site}";


        public void Install()
        {
            BuildAndDeploy();
        }

        public void BuildAndDeploy()
        {
            BuildStorefront();
            Deploy();
        }

        private void BuildStorefront()
        {
            EnsureExistingDirectory(_deploymentConfiguration.StorefrontDeployPath);
            EnsureCleanDirectory(_deploymentConfiguration.StorefrontDeployPath);

            DotNetClean(new DotNetCleanSettings()
                            .SetProject(HotStorefrontProjectFile)
                        );

            DotNetPublish(
                         new DotNetPublishSettings()
                                     .SetProject(HotStorefrontProjectFile)
                                     .SetFramework("netcoreapp3.1")
                                     .SetConfiguration("Debug")
                                     .SetVerbosity(DotNetVerbosity.Minimal)
                                     .SetOutput(_deploymentConfiguration.StorefrontDeployPath)
                       );

            var filesToDelete = new[] { "appsettings.Development.json", "appsettings.Production.json" };

            foreach (var file in filesToDelete)
            {
                DeleteFile($@"{_deploymentConfiguration.StorefrontDeployPath}\{file}");
            }
        }

        private void Deploy()
        {
            var webConfigPath = _deploymentConfiguration.StorefrontDeployPath / "web.config";

            XmlPoke(webConfigPath, "configuration/system.webServer/aspNetCore/@stdoutLogEnabled", $"{true}");

            var appSettingsPath = _deploymentConfiguration.StorefrontDeployPath / "appsettings.json";

            var appSettingsJson = JsonDeserializeFromFile<dynamic>(appSettingsPath);

            var themePath = HotThemeReleasePath.Replace("/", "//");

            appSettingsJson.ConnectionStrings.ContentConnectionString = $"provider=LocalStorage;rootPath={themePath}";
            appSettingsJson.ConnectionStrings.OpenIdConnectionString = _workingSpaceConfiguration.StorefrontOpenIdConnectionString;

            appSettingsJson.VirtoCommerce.ThemePath = themePath;
            appSettingsJson.VirtoCommerce.Endpoint.Url = _deploymentConfiguration.CommerceUrl;
            appSettingsJson.VirtoCommerce.Endpoint.AppId = null;
            appSettingsJson.VirtoCommerce.Endpoint.SecretKey = null;
            appSettingsJson.VirtoCommerce.Endpoint.UserName = "admin";
            appSettingsJson.VirtoCommerce.Endpoint.Password = "Abcd1234";

            appSettingsJson.VirtoCommerce.LiquidThemeEngine.BaseThemeName = _projectConfiguration.BaseThemeName;

            JsonSerializeToFile(appSettingsJson, appSettingsPath);
        }
    }
}
