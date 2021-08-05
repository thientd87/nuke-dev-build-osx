Build tool for Dev team who use OSX
This tool is based on http://www.nuke.build, which built on .net core framework. In case you need more information, please take a look on http://www.nuke.build/docs/getting-started/setup.html

1. Install nuke tool from cmd: dotnet tool install Nuke.GlobalTool --global
   
   WARNING
   When using ZSH as a shell, the dotnet tools path $HOME/.dotnet/tools must be added manually (see dotnet/cli#9321). 
   This can be achieved by adding export PATH=$HOME/.dotnet/tools:$PATH to the .zshrc file.
   
2. Checkout source code: https://github.com/thientd87/nuke-dev-build-osx.git
3. Update your PAT (Personal access token) in ConfileFiles\config-azureDevOpsInfo.json, line 2 (PersonalAccessToken field)
   Please refere to below document for getting you PAT: https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page
4. Update Theme build option in configFiels\config-project.json, line 42 (ThemeBuildProfile field)
    - ID: build:prod:ID1
    - MY: build:prod:MY1
4. Open project and compile
5. Ctrl + F5 or F5 to build and run as default target
6. Incase to specific action and performance, you can run custom target from "Package Manager Console" window in Visual Studio
    - Only checkout code: nuke --target CodeInitialization
    - Only build platform: nuke --target VirtoCommercePlatform --skip=Infrastructure
    - Only build modules (include virto, hot and apac modules): nuke --target HeinekenApacModules --skip=Infrastructure,VirtoCommercePlatform