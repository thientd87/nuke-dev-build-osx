using Nuke.Common.Tooling;
using System;
using System.Collections.Generic;
using System.Text;
using static Nuke.Common.Tools.Git.GitTasks;

namespace Helpers
{
    public static class GitHelpers
    {
        public static void Clone(string cloneUrl, string clonePath)
        {
            Git($"clone {cloneUrl}", clonePath);
        }

        public static void UpdateRemoteUrl(string sourcePath, string remoteUrl)
        {
            Git($"remote set-url origin {remoteUrl}", sourcePath);
        }

        public static void PullLatest(string sourcePath, string branch)
        {
            Git("fetch", sourcePath);
            Git($"checkout {branch}", sourcePath);
            Git($"reset --hard origin/{branch}", sourcePath);
        }

        public static void CheckoutWithTag(string sourcePath, string tag)
        {
            Git("reset --hard", sourcePath);
            Git("fetch", sourcePath);
            Git("fetch -t", sourcePath);
            Git($"checkout tags/{tag}", sourcePath);
        }

        public static void Tag(string sourcePath, string tag)
        {
            Git($"tag {tag}", sourcePath);
        }

        public static void Branch(string sourcePath, string branch)
        {
            Git($"branch {branch}", sourcePath);
        }
    }
}
