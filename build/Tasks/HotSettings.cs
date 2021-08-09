using Configurations;
using Nuke.Common.IO;
using static Nuke.Common.IO.FileSystemTasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Tasks
{
    public class HotSettings
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private ProjectConfiguration _projectConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static HotSettings Init() => new HotSettings();

        public HotSettings WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public HotSettings WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public HotSettings WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        private AbsolutePath WorkingFolder => _projectConfiguration.UseGithub
          ? _workingSpaceConfiguration.WorkingFolderPath / "git"
          : _workingSpaceConfiguration.WorkingFolderPath / "azure-devops";

        private AbsolutePath HotSettingsSourcePath => _workingSpaceConfiguration.WorkingFolderPath / _projectConfiguration.HotSettings / _deploymentConfiguration.HotSettingsFolder;

        public void Install()
        {
            DeployCommerce();
            DeployStorefront();
        }

        private void DeployStorefront()
        {
            var configFiles = Directory.GetFiles(HotSettingsSourcePath / "frontend", "*.*");
            
            foreach (var configFile in configFiles)
            {
                CopyFileToDirectory(configFile, _deploymentConfiguration.StorefrontDeployPath, FileExistsPolicy.Overwrite);
            }
        }

        private void DeployCommerce()
        {
            var configFiles = Directory.GetFiles(HotSettingsSourcePath / "backend", "*.*");

            foreach (var configFile in configFiles)
            {
                CopyFileToDirectory(configFile, _deploymentConfiguration.CommerceDeployPath / "wwwroot", FileExistsPolicy.Overwrite);
            }
        }
    }
}
