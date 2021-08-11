Build tool for Dev team who use OSX
This tool is based on http://www.nuke.build, which built on .net core framework. In case you need more information, please take a look on http://www.nuke.build/docs/getting-started/setup.html

### PREREQUISITES
 - .NET Core 3.1 SDK
 - Nodejs v12.x
 - Docker - install SQL Server 2019
 - Apache host (enable for MacOs): serve as virtual host for using custom domain

### STEP BY STEP TO LOCAL DEPLOYMENT


1. Install nuke tool from cmd: dotnet tool install Nuke.GlobalTool --global
   
   WARNING
   When using ZSH as a shell, the dotnet tools path $HOME/.dotnet/tools must be added manually (see dotnet/cli#9321). 
   This can be achieved by adding export PATH=$HOME/.dotnet/tools:$PATH to the .zshrc file.
  
2. Checkout source code: https://github.com/thientd87/nuke-dev-build-osx.git
   
3. Update your PAT (Personal access token) in ConfileFiles\config-azureDevOpsInfo.json, line 2 (PersonalAccessToken field)
   Please refere to below document for getting you PAT: https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page
   
4. Add HOT nuget source with PAT to nuget config

   Run in this command in terminal   
   dotnet nuget add source https://pkgs.dev.azure.com/heineken/B2B-DOT-APAC-Development/_packaging/hot-nuget-service/nuget/v3/index.json -n hot-nugget-service-2 -u <YOUR-NITECO-EMAIL@niteco.se> -p <YOUR-PAT> --store-password-in-clear-text
   
   Reference : https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-add-source
   
Double check nuget.config(usually it is in ~/users/<your-user-name>/.nuget/Nuget/NuGet.Config) file to make sure it has this block
<packageSourceCredentials>
<hot-nugget-service>
<add key="Username" value="YOUR-NITECO-EMAIL@niteco.se" />
<add key="ClearTextPassword" value="YOUR-PAT" />
</hot-nugget-service>
</packageSourceCredentials>

4. Update Theme build option in configFiels\config-project.json, line 42 (ThemeBuildProfile field)
    - ID: build:prod:ID1
    - MY: build:prod:MY1
5. Open project and compile 
6. Ctrl + F5 or F5 to build and run as default target
7. Incase to specific action and performance, you can run custom target from "Package Manager Console" window in Visual Studio
    - Only checkout code: nuke --target CodeInitialization
    - Only build platform: nuke --target VirtoCommercePlatform --skip=Infrastructure
    - Only build modules (include virto, hot and apac modules): nuke --target HeinekenApacModules --skip=Infrastructure,VirtoCommercePlatform
   
8. Restore Database to Docker
   - Follow this guide
     https://www.c-sharpcorner.com/article/restoring-a-sql-server-database-in-docker/
   
9. Start web app process
   - Goto wwwroot/<SITE-NAME> deployment folder --> Open terminal at this folder
   - Run command : dotnet VirtoCommerce.Platform.Web.dll --urls "http://localhost:10645;https://localhost:10646/" (for admin site)
   or
     dotnet VirtoCommerce.Storefront.dll --urls "http://localhost:2082;https://localhost:2083/" (for public site)
     
10. Start Apache Virtual host
  Run command
    sudo apachectl start
    
GOTO BROWSER and RUN WEBSITE

### KNOW ISSUE

1. Javascrip heapout of memory when build FE HOT-Theme-NX
   
   - Run these commands
    
    npm install -g increase-memory-limit
    
    - Run from the root location of your project:
    
    increase-memory-limit
    
    - Look Here For more details https://www.npmjs.com/package/increase-memory-limit
    
    
2. Understanding about Kestrel , Virtual Host and Enable APACHE virtual host on MACOS

3. Restore DB
   - If you get error "No such folder/file or Can not find folder/file BAK" --> Copy file HeinekenNitecoGlobalV3.bak to MacOS user root folder
   - After copy file bak to docker, can use Azure data studio to restore back file.
   
4. MACOS Environment
   - Global language support.
   Change code "VirtoCommerce.Storefront.Model.Language" line 32
     From 
         var regionInfo = new RegionInfo(culture.LCID);
     To
         var regionInfo = new RegionInfo(culture.Name);

https://andrewlock.net/dotnet-core-docker-and-cultures-solving-culture-issues-porting-a-net-core-app-from-windows-to-linux/

   - File Path of Content Blob
   Appsetting.config
     Change ConnectionStrings.ContentConnectionString
     rootPath: should repleace "/" by "\\"( make sure it end with "\\")
  Change code "VirtoCommerce.Storefront.Domain.FileSystemContentBlobProvider"
     Remove ".TrimEnd('\') + '\';" line 34
     Code shoule be : var rootPath = _options.Path;//TrimEnd('\') + '\';
     

