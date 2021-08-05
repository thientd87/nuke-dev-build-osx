using Configurations;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Nuke.Common.IO.FileSystemTasks;

namespace Tasks
{
    public class Infrastructure
    {
        private SiteDeploymentConfiguration _deploymentConfiguration;

        public static Infrastructure Init() => new Infrastructure();

        public Infrastructure WithDeploymentConfiguration(SiteDeploymentConfiguration deploymentConfiguration)
        {
            _deploymentConfiguration = deploymentConfiguration;
            return this;
        }

        private string CommerceSiteName => _deploymentConfiguration.CommerceSiteName;

        private string StorefrontSiteName => _deploymentConfiguration.StorefrontSiteName;


        public void Install()
        {
            CreateCommerceSite();
            CreateStorefrontSite();
            AddHostsRecord();
        }


        private void CreateCommerceSite()
        {
            foreach (var folder in _deploymentConfiguration.RequireFoldersExisting)
            {
                EnsureExistingDirectory(folder);
            }

            
            
        }

        private void CreateStorefrontSite()
        {
            EnsureExistingDirectory(_deploymentConfiguration.StorefrontDeployPath);

        
            
        }

        private void AddHostsRecord()
        {
            if (!HostsRecordExists("127.0.0.1", CommerceSiteName))
            {
                AddHostsRecord("127.0.0.1", CommerceSiteName);
            }

            if (!HostsRecordExists("127.0.0.1", StorefrontSiteName))
            {
                AddHostsRecord("127.0.0.1", StorefrontSiteName);
            }
        }

        private bool HostsRecordExists(string ip, string hostname)
        {
            string hostfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"/private/etc/hosts");
            return File.ReadAllLines(hostfile).Contains($"{ip} {hostname}");
        }

        private void AddHostsRecord(string ip, string hostname)
        {
            string hostfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "/private/etc/hosts");
            File.AppendAllLines(hostfile, new string[] { "", $"{ip} {hostname}" });
        }
    }
}
