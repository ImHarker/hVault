
using System.IO;

namespace hVault {
	public static class Setup {

		public static void Run() {
			Utils.CreateAppDataFolder();
			var localAppdata = Utils.GetLocalAppDataPath();
			var setupCompleteFile = Path.Combine(localAppdata, ".vault_setup_complete");
			if (File.Exists(setupCompleteFile)) return;
			if (!Utils.IsAdministrator()) {
				Utils.RestartAsAdmin();
				
			} else {
				try {
					Utils.CreateProtectedFolder(localAppdata);

					var vaultFile = Path.Combine(localAppdata, ".vault");
					Utils.CreateProtectedFile(vaultFile);
					EncryptionWrapper.EncryptFile(vaultFile, ".vault");


					var backupFolder = Path.Combine(localAppdata, "backup");
					Utils.CreateProtectedFolder(backupFolder);

					Utils.CreateProtectedFile(setupCompleteFile);

					Console.WriteLine("Setup complete");
					Console.ReadKey();
					Environment.Exit(0);
				} catch (Exception e) {
					Console.WriteLine($"Setup failed: {e.Message}");
					Console.ReadKey();
					Environment.Exit(1);
				}
			}
		}


		

	}
}
