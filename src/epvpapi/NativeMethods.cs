using System;
using System.Runtime.InteropServices;
using System.Text;

namespace epvpapi
{
    static class NativeMethods
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetCurrentHwProfile(IntPtr lpHwProfileInfo);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern long GetVolumeInformationA(string pathName, StringBuilder volumeNameBuffer, int volumeNameSize, ref int volumeSerialNumber, ref int maximumComponentLength, ref int fileSystemFlags, StringBuilder fileSystemNameBuffer, int fileSystemNameSize);
    }
}
