

namespace hVault {
	public class Program {

		public static void Main(string[] args) {
			Setup.Run();

			EncryptionWrapper.DecryptFile("teste", ".vault");

			using (var context = new DataContext()) {
				context.Database.EnsureCreated();
				//context.VaultEntries.Add(new VaultEntry{ CreatedAt = DateTime.Now, ModifiedAt = DateTime.Now, EncryptedData = "Data", Name = "Teste" });
				context.SaveChanges();
				Console.WriteLine(context.VaultEntries.Count());
				Console.WriteLine(context.VaultEntries.FirstOrDefault()?.Name ?? "Empty");
			}

			EncryptionWrapper.EncryptFile("teste", ".vault");

		}

	}
}
