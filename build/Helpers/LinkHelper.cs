using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Helpers
{
    public class LinkHelper
    {
         private static Chilkat.FileAccess obj = new Chilkat.FileAccess();
        public enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }


        public static bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName)
        {
            return obj.SymlinkCreate(lpTargetFileName, lpSymlinkFileName);
        }
        
    }
}
