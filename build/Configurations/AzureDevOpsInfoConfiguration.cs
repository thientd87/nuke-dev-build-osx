using System;
using System.Collections.Generic;
using System.Text;

namespace Configurations
{
    public class AzureDevOpsInfo
    {
        public string PersonalAccessToken { get; set; }

        public string Organization { get; set; }

        public string ProjectName { get; set; }

        public string CloneUrlTemplate => $"https://{PersonalAccessToken}@dev.azure.com/{Organization}/{ProjectName}/_git/{{0}}";

        public string NugetFeedUrl { get; set; }

        public string NugetFeedName { get; set; }

        public string HotNugetFeedUrl { get; set; }

        public string HotNugetFeedName { get; set; }
    }
}
