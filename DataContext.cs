using Microsoft.EntityFrameworkCore;

namespace hVault {
	public class DataContext : DbContext {
		public DbSet<VaultEntry> VaultEntries { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder options) {
			var appDataPath = Utils.GetLocalAppDataPath();
			var vaultPath = Path.Join(appDataPath, ".vault");
			options.UseSqlite($"Data Source={vaultPath};Pooling=false;");
		}
	}
}
