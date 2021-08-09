using Configurations;
using Helpers;
using Nuke.Common.IO;
using Nuke.Common.Tools.Npm;
using System;
using System.Collections.Generic;
using System.Text;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

namespace Tasks
{
    public class HotThemeNx
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;
        private ProjectConfiguration _projectConfiguration;
        private WorkingSpaceConfiguration _workingSpaceConfiguration;

        public static HotThemeNx Init() => new HotThemeNx();

        public HotThemeNx WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        public HotThemeNx WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public HotThemeNx WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        private AbsolutePath WorkingFolder => _projectConfiguration.UseGithub
              ? _workingSpaceConfiguration.WorkingFolderPath / "git"
              : _workingSpaceConfiguration.WorkingFolderPath / "azure-devops";

        private AbsolutePath HotThemeNxSourcePath => WorkingFolder /_projectConfiguration.HotThemeNx;

        public void Install()
        {
            InstallPackages();
            RunBuild();
        }

        public void BuildLatest()
        {
            RunBuild();
        }

        private void InstallPackages()
        {
            Npm("install --production=false", HotThemeNxSourcePath);
        }

        private void RunBuild()
        {
            Npm($"run {_deploymentConfiguration.ThemeBuildProfile}", HotThemeNxSourcePath);
        }
    }
}
