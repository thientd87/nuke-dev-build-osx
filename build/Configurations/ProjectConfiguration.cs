using System;
using System.Collections.Generic;
using System.Text;

namespace Configurations
{
    public class ProjectConfiguration
    {
        public List<string> HotModules { get; set; } = new List<string>();

        public string HotStorefrontCore { get; set; }

        public string HotThemeNx { get; set; }

        public string HotSettings { get; set; }

        public string HotStorefrontCoreSecretKey { get; set; }

        public bool UseGithub { get; set; }

        public AzureDevOpsInfo AzureDevOpsInfo { get; set; }

        public GithubInfo GithubInfo { get; set; }

        public string CloneUrl => UseGithub ? GithubInfo.CloneUrlTemplate : AzureDevOpsInfo.CloneUrlTemplate;

        public string DevStorefrontBranch => "ci";

        public string DevThemeBranch => "ci";

        public string DevDotModuleBranch => "ci";

        public int PasswordLengthPolicy => 8;

        public List<string> HeinekenApacModules { get; set; } = new List<string>();

        public string BaseThemeName { get; set; }
    }

    public class GithubInfo
    {
        public string ApiToken { get; set; }

        public string Organization { get; set; }

        public string CloneUrlTemplate => $@"https://{ApiToken}@github.com/{Organization}/{{0}}.git";
    }

    public class HotModuleWithVersion
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
