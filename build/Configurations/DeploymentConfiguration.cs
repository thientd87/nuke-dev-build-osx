using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Configurations
{
    public class SiteDeploymentConfiguration
    {
        public AbsolutePath WebRoot { get; set; }

        public string Site { get; set; }
        public string StorefrontSiteName { get; set; }

        public string CommerceSiteName { get; set; }

        public string ThemeBuildProfile { get; set; }
        public string HotSettingsFolder { get; set; }
        public AbsolutePath StorefrontDeployPath => WebRoot / StorefrontSiteName;

        public AbsolutePath CommerceDeployPath => WebRoot / CommerceSiteName;

        public string StorefrontUrl => $"http://{StorefrontSiteName}";

        public string CommerceUrl => $"http://{CommerceSiteName}";

        public string CommerceAssetsPhysicalPath => $"{CommerceDeployPath}/assets";

        public AbsolutePath CommerceModulesPhysicalPath => CommerceDeployPath / "Modules";

        public List<string> RequireFoldersExisting => new List<string> {
            $"{CommerceDeployPath}/App_Data",
            CommerceAssetsPhysicalPath,
            $"{CommerceDeployPath}/App_Data/Modules",
            CommerceModulesPhysicalPath
        };
    }
    public class DeploymentConfiguration
    {
        public AbsolutePath WebRoot { get; set; }

        public string CurrentSite { get; set; }

        public IList<SiteDeploymentConfiguration> Sites { get; set; }

        public SiteDeploymentConfiguration CurrentSiteConfiguration { get; set; }

        public void ConfigureSites()
        {
            foreach (var site in Sites)
            {
                site.WebRoot = WebRoot;
            }

            CurrentSiteConfiguration = Sites.Single(s => s.Site == CurrentSite);
        }
    }
}
