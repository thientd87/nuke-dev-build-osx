using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Helpers
{
    public class LinkHelper
    {
        public enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
    }
}
