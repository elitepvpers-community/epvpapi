using System;
using System.Runtime.InteropServices;
using System.Text;

namespace epvpapi.Generation
{
    public class HardwareId
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetCurrentHwProfile(IntPtr lpHwProfileInfo);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern long GetVolumeInformationA(string pathName, StringBuilder volumeNameBuffer,
            int volumeNameSize, ref int volumeSerialNumber, ref int maximumComponentLength, ref int fileSystemFlags,
            StringBuilder fileSystemNameBuffer, int fileSystemNameSize);

        private static HwProfileInfo ProfileInfo()
        {
            IntPtr profilePtr = IntPtr.Zero;
            try
            {
                var profile = new HwProfileInfo();
                profilePtr = Marshal.AllocHGlobal(Marshal.SizeOf(profile));
                Marshal.StructureToPtr(profile, profilePtr, false);

                if (!GetCurrentHwProfile(profilePtr))
                {
                    throw new Exception("Error cant get current hw profile!");
                }
                Marshal.PtrToStructure(profilePtr, profile);
                return profile;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                if (profilePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(profilePtr);
                }
            }
        }

        private static string GetVolumeSerial(string strDriveLetter)
        {
            int serNum = 0, maxCompLen = 0, VolFlags = 0;
            StringBuilder volLabel = new StringBuilder(256), fsName = new StringBuilder(256);
            GetVolumeInformationA(strDriveLetter + ":\\", volLabel, volLabel.Capacity, ref serNum, ref maxCompLen,
                ref VolFlags, fsName, fsName.Capacity);
            return Convert.ToString(serNum);
        }

        /// <summary>
        ///     Generates the computer's hardware id (HWID) based on elitepvpers algorithm
        /// </summary>
        /// <param name="salt"> Salt that will be added to the md5 generation </param>
        /// <returns> Returns the computer's hardware id (HWID) </returns>
        public static string Generate(string salt = "")
        {
            string profileGuid = ProfileInfo().szHwProfileGuid;
            string volumeSerial = GetVolumeSerial(Environment.SystemDirectory.Substring(0, 1));
            return Cryptography.GetMd5(profileGuid + volumeSerial + salt);
        }

        [Flags]
        private enum DockInfo
        {
            DockinfoDocked = 0x2,
            DockinfoUndocked = 0x1,
            DockinfoUserSupplied = 0x4,
            DockinfoUserDocked = 0x5,
            DockinfoUserUndocked = 0x6
        }

        [StructLayout(LayoutKind.Sequential)]
        private class HwProfileInfo
        {
            [MarshalAs(UnmanagedType.U4)] public int dwDockInfo;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 39)] public string szHwProfileGuid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szHwProfileName;
        }
    }
}