using System.Linq;
using Configurations;
using Helpers;
using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.IO.FileSystemTasks;

namespace Tasks
{

    public class CodeInitialization
    {
        private WorkingSpaceConfiguration _workingSpaceConfiguration;
        private ProjectConfiguration _projectConfiguration;

        public static CodeInitialization Init() => new CodeInitialization();

        private string StorefrontCloneUrl => string.Format(_projectConfiguration.CloneUrl, _projectConfiguration.HotStorefrontCore);

        private string HotThemeCloneUrl => string.Format(_projectConfiguration.CloneUrl, _projectConfiguration.HotThemeNx);

        private AbsolutePath HotSettingsWorkingFolder => _workingSpaceConfiguration.WorkingFolderPath;

        private AbsolutePath HotSettingsSourcePath => HotSettingsWorkingFolder / _projectConfiguration.HotSettings;

        private string HotSettingsCloneUrl => string.Format(_projectConfiguration.CloneUrl, _projectConfiguration.HotSettings);

        public CodeInitialization WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        public CodeInitialization WithProjectConfiguration(ProjectConfiguration projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
            return this;
        }

        public void Run()
        {
            ApacInstall();
            HotInstall();
            StorefrontInstall();
            HotThemeInstall();
            HotSettingsInstall();
        }

        private void ApacInstall()
        {
            ApacCloneModulesSourceCode();
            ApacSwitchAndPullDevBranch();
        }

        private void HotInstall()
        {
            HotCloneModulesSourceCode();
            HotSwitchAndPullDevBranch();
        }

        private void StorefrontInstall()
        {
            StorefrontCloneSourceCode();
            StorefrontPullLatest();
        }

        private void HotThemeInstall()
        {
            HotThemeCloneSource();
            HotThemPullLastestBranch();
        }

        private void HotSettingsInstall()
        {
            HotSettingsCloneSource();
            HotSettingsPullLastestBranch();
        }

        private void HotSettingsCloneSource()
        {
            EnsureExistingDirectory(HotSettingsWorkingFolder);

            if (!DirectoryExists(HotSettingsSourcePath))
            {
                GitHelpers.Clone(HotSettingsCloneUrl, HotSettingsWorkingFolder);
            }
        }

        private void HotSettingsPullLastestBranch()
        {
            GitHelpers.PullLatest(HotSettingsSourcePath, "master");
        }
        private void HotThemeCloneSource()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.HotThemeWorkingFolder);

            if (!DirectoryExists(_workingSpaceConfiguration.HotThemeSourcePath))
            {
                GitHelpers.Clone(HotThemeCloneUrl, _workingSpaceConfiguration.HotThemeWorkingFolder);
            }
        }

        private void HotThemPullLastestBranch()
        {
            GitHelpers.PullLatest(_workingSpaceConfiguration.HotThemeSourcePath, _projectConfiguration.DevThemeBranch);
        }

        private void StorefrontCloneSourceCode()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.HotStorefrontWorkingFolder);

            if (!DirectoryExists(_workingSpaceConfiguration.HotStorefrontSourcePath))
            {
                GitHelpers.Clone(StorefrontCloneUrl, _workingSpaceConfiguration.HotStorefrontWorkingFolder);
            }
        }

        private void StorefrontPullLatest()
        {
            GitHelpers.PullLatest(_workingSpaceConfiguration.HotStorefrontSourcePath, _projectConfiguration.DevStorefrontBranch);
        }

        private void ApacCloneModulesSourceCode()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.HeinekenApacModulesPath);

            foreach (var module in _projectConfiguration.HeinekenApacModules)
            {
                var moduleSourcePath = _workingSpaceConfiguration.HeinekenApacModulesPath /module;

                var cloneUrl = string.Format(_projectConfiguration.CloneUrl, module);

                if (!DirectoryExists(moduleSourcePath))
                {
                    GitHelpers.Clone(cloneUrl, _workingSpaceConfiguration.HeinekenApacModulesPath);
                }
            }
        }

        private void ApacSwitchAndPullDevBranch()
        {
            foreach (var module in _projectConfiguration.HeinekenApacModules)
            {
                var moduleSourceDirectoryPath = _workingSpaceConfiguration.HeinekenApacModulesPath / module;

                Logger.Info($"Pull for - {module}");

                GitHelpers.PullLatest(moduleSourceDirectoryPath, _projectConfiguration.DevDotModuleBranch);
            }
        }

        private void HotCloneModulesSourceCode()
        {
            EnsureExistingDirectory(_workingSpaceConfiguration.ModulesSourcePath);

            foreach (var moduleWithVersion in _projectConfiguration.HotModules)
            {
                var moduleWithVersionArr = moduleWithVersion.Split(' ').ToList();
                var module = moduleWithVersionArr[0];

                var moduleSourcePath = _workingSpaceConfiguration.ModulesSourcePath / module;

                var cloneUrl = string.Format(_projectConfiguration.CloneUrl, module);

                if (!DirectoryExists(moduleSourcePath))
                {
                    GitHelpers.Clone(cloneUrl, _workingSpaceConfiguration.ModulesSourcePath);
                }
            }
        }

        private void HotSwitchAndPullDevBranch()
        {
            foreach (var moduleWithVersion in _projectConfiguration.HotModules)
            {
                var moduleWithVersionArr = moduleWithVersion.Split(' ').ToList();
                var module = moduleWithVersionArr[0];
                var tag = moduleWithVersionArr[1];
                var moduleSourceDirectoryPath = _workingSpaceConfiguration.ModulesSourcePath / module;

                Logger.Info($"Pull for - {module}");

                GitHelpers.CheckoutWithTag(moduleSourceDirectoryPath, tag);
            }
        }
    }
}
