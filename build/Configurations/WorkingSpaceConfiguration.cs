using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurations
{
    public class WorkingSpaceConfiguration
    {
        public ProjectConfiguration ProjectConfiguration { get; set; }

        public AbsolutePath WorkingSpace { get; set; }

        public string SqlServerAddress { get; set; }

        public string SqlServerSAPassword { get; set; }

        public string DatabaseName { get; set; }

        public string BakPath { get; set; }

        public string ElasticSearchAddress { get; set; }

        public AbsolutePath VirtoCommerceSourcePath => WorkingSpace / "virto-commerce";

        public AbsolutePath VirtoCommerceDownloadPath => VirtoCommerceSourcePath / "downloads";

        public AbsolutePath VirtoCommerceModulesDownloadPath => VirtoCommerceDownloadPath / "modules";

        public AbsolutePath VirtoCommerceModulesSourceCode => WorkingSpace / "vc-modules-source";


        public AbsolutePath WorkingFolderPath => WorkingSpace / "working";

        public AbsolutePath ModulesSourcePath => ProjectConfiguration.UseGithub
           ? WorkingFolderPath / "git"
           : WorkingFolderPath / "azure-devops";

        public AbsolutePath HotStorefrontWorkingFolder => ModulesSourcePath;

        public AbsolutePath HeinekenApacModulesPath => WorkingSpace / "heineken-apac-modules";

        public AbsolutePath HotStorefrontSourcePath => HotStorefrontWorkingFolder / ProjectConfiguration.HotStorefrontCore;

        public AbsolutePath HotThemeWorkingFolder => ModulesSourcePath;

        public AbsolutePath HotThemeSourcePath => HotThemeWorkingFolder / ProjectConfiguration.HotThemeNx;

        public string ConnectionString
        => $"Data Source={SqlServerAddress};Initial Catalog={DatabaseName};Persist Security Info=True;User ID=sa;Password={SqlServerSAPassword};MultipleActiveResultSets=True;Connect Timeout=420";

        public string StorefrontOpenIdConnectionString
        => $"Data Source={SqlServerAddress};Initial Catalog={DatabaseName}OpenIdDict;Persist Security Info=True;User ID=sa;Password={SqlServerSAPassword};MultipleActiveResultSets=True;Connect Timeout=420";
    }
}
