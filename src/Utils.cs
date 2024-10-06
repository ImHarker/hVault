using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace hVault {
	public static class Utils {
		public static string GetLocalAppDataPath() {
			try {
				var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
				var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (assemblyName == null) throw new Exception("Unexpected Error - Assembly Name is null");
				return Path.Combine(localAppData, assemblyName);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				throw;
			}
		}

		public static void CreateAppDataFolder() {
			try {
				Directory.CreateDirectory(GetLocalAppDataPath());
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				throw;
			}
		}


		public static bool IsAdministrator() {
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public static void RestartAsAdmin() {
			var processInfo = new ProcessStartInfo {
				FileName = Process.GetCurrentProcess().MainModule?.FileName ?? "hVault.exe",
				UseShellExecute = true,
				Verb = "runas"
			};

			try {
				Process.Start(processInfo);
				Environment.Exit(0);
			} catch (Exception ex) {
				Console.WriteLine("UAC elevation failed: " + ex.Message);
				Environment.Exit(1);
			}
		}

		private static void DenyAdminDelete(string vaultPath) {
			FileInfo fileInfo = new FileInfo(vaultPath);
			FileSecurity fileSecurity = fileInfo.GetAccessControl();

			fileSecurity.AddAccessRule(new FileSystemAccessRule(
				new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
				FileSystemRights.Delete,
				AccessControlType.Deny));

			fileInfo.SetAccessControl(fileSecurity);
		}

		public static void CreateProtectedFile(string filePath) {
			File.Create(filePath).Close();
			File.SetAttributes(filePath, FileAttributes.System | FileAttributes.Hidden);
			DenyAdminDelete(filePath);
		}

		public static void CreateProtectedFolder(string folderPath) {
			Directory.CreateDirectory(folderPath);
			File.SetAttributes(folderPath, FileAttributes.System | FileAttributes.Hidden);
			DenyAdminDelete(folderPath);
		}

	}
}

