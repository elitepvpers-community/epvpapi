using System;
using System.Runtime.InteropServices;
using System.Text;


namespace epvpapi.Generation
{
    public class HardwareID
    {
        [Flags()]
        private enum DockInfo
        {
            DOCKINFO_DOCKED = 0x2,
            DOCKINFO_UNDOCKED = 0x1,
            DOCKINFO_USER_SUPPLIED = 0x4,
            DOCKINFO_USER_DOCKED = 0x5,
            DOCKINFO_USER_UNDOCKED = 0x6
        }

        [StructLayout(LayoutKind.Sequential)]
        private class HW_PROFILE_INFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwDockInfo;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 39)]
            public string szHwProfileGuid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szHwProfileName;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetCurrentHwProfile(IntPtr lpHwProfileInfo);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern long GetVolumeInformationA(string PathName, StringBuilder VolumeNameBuffer, int VolumeNameSize, ref int VolumeSerialNumber, ref int MaximumComponentLength, ref int FileSystemFlags, StringBuilder FileSystemNameBuffer, int FileSystemNameSize);

        private static HW_PROFILE_INFO ProfileInfo()
        {
            HW_PROFILE_INFO profile = null;
            IntPtr profilePtr = IntPtr.Zero;
            try
            {
                profile = new HW_PROFILE_INFO();
                profilePtr = Marshal.AllocHGlobal(Marshal.SizeOf(profile));
                Marshal.StructureToPtr(profile, profilePtr, false);

                if (!GetCurrentHwProfile(profilePtr))
                {
                    throw new Exception("Error cant get current hw profile!");
                }
                else
                {
                    Marshal.PtrToStructure(profilePtr, profile);
                    return profile;
                }
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
            StringBuilder VolLabel = new StringBuilder(256), FSName = new StringBuilder(256);
            GetVolumeInformationA(strDriveLetter + ":\\", VolLabel, VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, FSName.Capacity);
            return Convert.ToString(serNum);
        }

        /// <summary>
        /// Generates the computer's hardware id (HWID) based on elitepvpers algorithm
        /// </summary>
        /// <param name="salt"> Salt that will be added to the md5 generation </param>
        /// <returns> Returns the computer's hardware id (HWID) </returns>
        public static string Generate(string salt = "")
        {
            var profileGuid = ProfileInfo().szHwProfileGuid.ToString();
            var volumeSerial = GetVolumeSerial(Environment.SystemDirectory.Substring(0, 1));
            return Cryptography.GetMD5(profileGuid + volumeSerial + salt);
        }
    }
}