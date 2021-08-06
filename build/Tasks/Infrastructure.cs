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

        private const string ApacheVirtualHostFile = "/private/etc/apache2/extra/httpd-vhosts.conf";
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
            // Add site to reserve proxy Apache

            if (!VirtualHostsExists(CommerceSiteName, "http://localhost:10645/"))
            {
                AddVirtualHost(CommerceSiteName,"http://localhost:10645/",_deploymentConfiguration.CommerceLogPhysicalPath);
            }
            
        }

        private void CreateStorefrontSite()
        {
            EnsureExistingDirectory(_deploymentConfiguration.StorefrontDeployPath);
            
            if (!VirtualHostsExists(StorefrontSiteName, "http://localhost:2082/"))
            {
                AddVirtualHost(StorefrontSiteName,"http://localhost:2082/",_deploymentConfiguration.StoreFrontLogPhysicalPath);
            }
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
        private bool VirtualHostsExists(string siteName, string localhostAddressLink)
        {
            string hostfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),ApacheVirtualHostFile);
            string[] fileContent = File.ReadAllLines(hostfile);
            return fileContent.Any(x => x.Contains(siteName)) &&
                   fileContent.Any(x => x.Contains(localhostAddressLink));
        }
        private void AddVirtualHost(string siteName, string localhostAddressLink, string logPath)
        {
            string vHostfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),ApacheVirtualHostFile);
            File.AppendAllLines(vHostfile, new string[] { "", VirtualHostStringBuilder(siteName,localhostAddressLink,logPath) });
        }

        private string VirtualHostStringBuilder(string siteName,string localhostAddressLink,string logPath)
        {
            StringBuilder myvar = new StringBuilder(); 
            myvar.Append("<VirtualHost *:80> \n")
                .Append("\t    ProxyPreserveHost On \n")
                .Append($"\t    ProxyPass / {localhostAddressLink} \n")
                .Append($"\t    ProxyPassReverse / {localhostAddressLink} \n")
                .Append($"\t    ServerName {siteName} \n")
                .Append($"\t    ServerAlias *.{siteName} \n")
                .Append($"\t    ErrorLog \"{logPath}/{siteName}-error.log\" \n")
                .Append($"\t    CustomLog \"{logPath}/{siteName}-access.log\" common\n")
                .Append("</VirtualHost>");
            return myvar.ToString();
        }
    }
}
