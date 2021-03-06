using System;
using System.Linq;
using Configurations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    
    
    readonly Tool Git;

    AbsolutePath ConfigFilesDirectory => RootDirectory / "build" / "ConfigFiles";

    PlatformConfiguration platformConfiguration { get; set; }

    DeploymentConfiguration deployConfiguration { get; set; }

    WorkingSpaceConfiguration workingSpaceConfiguration { get; set; }

    ProjectConfiguration projectConfiguration { get; set; }

    AzureDevOpsInfo azureDevOpsInfo { get; set; }
    protected override void OnBuildCreated()
    {
        platformConfiguration = SerializationTasks.JsonDeserializeFromFile<PlatformConfiguration>(ConfigFilesDirectory / "config-platform.json");
        deployConfiguration = SerializationTasks.JsonDeserializeFromFile<DeploymentConfiguration>(ConfigFilesDirectory / "config-deployment.json");
        workingSpaceConfiguration = SerializationTasks.JsonDeserializeFromFile<WorkingSpaceConfiguration>(ConfigFilesDirectory / "config-workingspace.json");
        projectConfiguration = SerializationTasks.JsonDeserializeFromFile<ProjectConfiguration>(ConfigFilesDirectory / "config-project.json");
        azureDevOpsInfo = SerializationTasks.JsonDeserializeFromFile<AzureDevOpsInfo>(ConfigFilesDirectory / "config-azureDevOpsInfo.json");
        projectConfiguration.AzureDevOpsInfo = azureDevOpsInfo;

        deployConfiguration.WebRoot = RootDirectory.Parent / "wwwroot";
        workingSpaceConfiguration.WorkingSpace = RootDirectory.Parent / "hot-dev";
        workingSpaceConfiguration.ProjectConfiguration = projectConfiguration;

        deployConfiguration.ConfigureSites();
    }
    Target CodeInitialization => _ => _
        .Executes(() =>
        {
            Tasks.CodeInitialization.Init()
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .Run();
        });
    Target Infrastructure => _ => _
        .Executes(() =>
        {
            Tasks.Infrastructure.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .Install();
        });
    Target VirtoCommercePlatform => _ => _
        .After
        (
            Infrastructure
        )
        .Executes(() =>
        {
            Tasks.VirtoCommercePlatform.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithPlatformConfiguration(platformConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
         });
    
    Target VirtoCommerceModules => _ => _
        .After
        (
            Infrastructure,
            VirtoCommercePlatform
        )
        .Executes(() =>
        {
            Tasks.VirtoCommerceModules.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithPlatformConfiguration(platformConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });
    
    Target HotModules => _ => _
        .After
        (
            Infrastructure,
            CodeInitialization,
            VirtoCommercePlatform,
            VirtoCommerceModules
        )
        .Executes(() =>
        {
            Tasks.HotModules.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });
    
    Target HeinekenApacModules => _ => _
        .After
        (
            Infrastructure,
            CodeInitialization,
            VirtoCommercePlatform,
            HotModules
        )
        .Executes(() =>
        {
            Tasks.HeinekenApacModules.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });
    
    
    Target HotStorefrontCore => _ => _
        .After
        (
            Infrastructure,
            CodeInitialization
        )
        .Executes(() =>
        {
            Tasks.HotStorefrontCore.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });

    Target HotThemeNx => _ => _
        .After
        (
            Infrastructure,
            CodeInitialization
        )
        .Executes(() =>
        {
            Tasks.HotThemeNx.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });
    Target HotSettings => _ => _
        .After
        (
            HotStorefrontCore,
            Infrastructure,
            CodeInitialization
        )
        .Executes(() =>
        {
            Tasks.HotSettings.Init()
                .WithDeploymentConfiguration(deployConfiguration.CurrentSiteConfiguration)
                .WithProjectConfiguration(projectConfiguration)
                .WithWorkingSpaceConfiguration(workingSpaceConfiguration)
                .Install();
        });
    
    Target Default => _ => _
        .DependsOn
        (
            //CodeInitialization,
            Infrastructure,
            VirtoCommercePlatform,
            VirtoCommerceModules,
            HotModules,
            HeinekenApacModules, HotStorefrontCore,
            HotThemeNx,
            HotSettings
        )
        .Executes(() =>
        {

        });

}
