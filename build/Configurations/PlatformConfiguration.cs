using System;
using System.Collections.Generic;
using System.Text;

namespace Configurations
{
    public class PlatformConfiguration
    {
        public string PlatformVersion { get; set; }

        public List<VirtomCommerceModule> Modules { get; set; } = new List<VirtomCommerceModule>();

        public string PlatformDownloadUrl => $"https://github.com/VirtoCommerce/vc-platform/archive/{PlatformVersion}.zip";
    }

    public class VirtomCommerceModule
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string DownloadUrl => $"https://github.com/VirtoCommerce/{Name}/archive/{Version}.zip";
    }
}
