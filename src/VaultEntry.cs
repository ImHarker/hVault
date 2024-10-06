using System.ComponentModel.DataAnnotations;

namespace hVault {

	public class VaultEntry {
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime ModifiedAt { get; set; }
		
		[Required]
		public string EncryptedData { get; set; }

	}
}