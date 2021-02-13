using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace CheckSafeMode
{
    class Program
    {

        internal const int SM_CLEANBOOT = 67;

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int smIndex);
        static void Main(string[] args)
        {
            var IsSafeMode = GetSystemMetrics(SM_CLEANBOOT);

            bool safeModeActive = Convert.ToBoolean(IsSafeMode);
            if (safeModeActive)
            { 

            ManagementObjectSearcher wmiData = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
            ManagementObjectCollection data = wmiData.Get();


                foreach (ManagementObject virusChecker in data)
                {
                    var AvName = virusChecker["displayName"];

                    var xstate = virusChecker["productState"];
                    var f = Convert.ToInt32(xstate);
                    var zz = f.ToString("X").PadLeft(6, '0');

                    var StatusOfAV = "";
                    var y = zz.Substring(2, 2);
                    switch (y)
                    {
                        case "00":
                            StatusOfAV = "OFF";
                            break;
                        case "01":
                            StatusOfAV = "Exipired";
                            break;
                        case "10":
                            StatusOfAV = "ON";
                            break;
                        case "11":
                            StatusOfAV = "Snoozed";
                            break;
                        default:
                            StatusOfAV = "Unknown";
                            break;
                    }

                    string text = String.Format("In SafeBoot Mode = {0}   AVInstalled =  {1}  Status = {2} ", Convert.ToBoolean(IsSafeMode).ToString(), AvName.ToString(), StatusOfAV);
                    System.IO.File.WriteAllText(@"C:\Users\Public\SafeBoot.txt", text);
                    BcdStoreAccessor b = new BcdStoreAccessor();
                    b.RemoveSafeboot();
                    Shutdown.Restart();
                }
            }

	    //some stuff came from here
            //https://gallery.technet.microsoft.com/scriptcenter/Get-the-status-of-4b748f25
        }

       
        public static bool AntivirusInstalled()
        {
            string wmipathstr = @"\\" + Environment.MachineName + @"\root\SecurityCenter2";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmipathstr, "SELECT * FROM AntivirusProduct");
                ManagementObjectCollection instances = searcher.Get();           
                return instances.Count > 0;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }
		
		// https://stackoverflow.com/questions/25295117/use-c-sharp-bcd-wmi-provider-to-safeboot-windows?noredirect=1
		public class BcdStoreAccessor
		{
			public const int BcdOSLoaderInteger_SafeBoot = 0x25000080;

			public enum BcdLibrary_SafeBoot
			{
				SafemodeMinimal = 0,
				SafemodeNetwork = 1,
				SafemodeDsRepair = 2
			}

			private ConnectionOptions connectionOptions;
			private ManagementScope managementScope;
			private ManagementPath managementPath;

			public BcdStoreAccessor()
			{
				connectionOptions = new ConnectionOptions();
				connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
				connectionOptions.EnablePrivileges = true;

				managementScope = new ManagementScope("root\\WMI", connectionOptions);

				managementPath = new ManagementPath("root\\WMI:BcdObject.Id=\"{fa926493-6f1c-4193-a414-58f0b2456d1e}\",StoreFilePath=\"\"");
			}

			public  void SetSafeboot()
			{
				ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
				currentBootloader.InvokeMethod("SetIntegerElement", new object[] { BcdOSLoaderInteger_SafeBoot, BcdLibrary_SafeBoot.SafemodeMinimal });
			}

			public  void RemoveSafeboot()
			{
				ManagementObject currentBootloader = new ManagementObject(managementScope, managementPath, null);
				currentBootloader.InvokeMethod("DeleteElement", new object[] { BcdOSLoaderInteger_SafeBoot });
			}
		}
		
		public class Shutdown
		{
			public static void Restart()
			{
				StartShutDown("-f -r -t 5");
			}

			/// <summary>
			/// Log off.
			/// </summary>
			public static void LogOff()
			{
				StartShutDown("-l");
			}

			/// <summary>
			///  Shutting Down Windows 
			/// </summary>
			public static void Shut()
			{
				StartShutDown("-f -s -t 5");
			}

			private static void StartShutDown(string param)
			{
				ProcessStartInfo proc = new ProcessStartInfo();
				proc.FileName = "cmd";
				proc.WindowStyle = ProcessWindowStyle.Hidden;
				proc.Arguments = "/C shutdown " + param;
				Process.Start(proc);
			}
		}			
	}
}
