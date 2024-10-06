using System.Security.Cryptography;

namespace hVault; 

public static class EncryptionWrapper {
	public static Aes InitializeAes(string pwd, byte[] salt) {
		var key = Rfc2898DeriveBytes.Pbkdf2(pwd, salt, 150_000, HashAlgorithmName.SHA256, 256 / 8);
		Aes aes = Aes.Create();
		aes.Key = key;
		aes.GenerateIV();
		aes.Mode = CipherMode.CFB;
		aes.Padding = PaddingMode.None;
		return aes;
	}

	public static byte[] EncryptData(string pwd, byte[] data) {
		var salt = new byte[32];
		using (var rng = RandomNumberGenerator.Create()) {
			rng.GetBytes(salt);
		}

		using (var aes = InitializeAes(pwd, salt)) {
			byte[] iv = aes.IV;
			using (ICryptoTransform encryptor = aes.CreateEncryptor()) {
				byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

				using (var hmac = new HMACSHA256(salt)) {
					byte[] hmacValue = hmac.ComputeHash(data);

					return salt.Concat(iv).Concat(hmacValue).Concat(encryptedData).ToArray();
				}
			}
		}
	}

	public static byte[] DecryptData(string pwd, byte[] data) {
		byte[] salt = data.Take(32).ToArray();
		byte[] iv = data.Skip(32).Take(16).ToArray();
		byte[] hmacValue = data.Skip(32 + 16).Take(32).ToArray();
		byte[] encryptedData = data.Skip(32 + 16 + 32).Take(data.Length - (32 + 16 + 32)).ToArray();

		using (var aes = InitializeAes(pwd, salt)) {
			aes.IV = iv; 

			using (ICryptoTransform decryptor = aes.CreateDecryptor()) {
				byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

				
				using (var hmac = new HMACSHA256(salt)) {
					byte[] hmacCheck = hmac.ComputeHash(decryptedData);
					if (!hmacCheck.SequenceEqual(hmacValue)) {
						throw new CryptographicException("Integrity check failed. Data may be tampered with or password is incorrect.");
					}
				}

				return decryptedData;
			}
		}
	}

	public static void DecryptFile(string pwd, string file) {
		var path = Path.Join(Utils.GetLocalAppDataPath(), file);
		var data = File.ReadAllBytes(path);
		var bytes = EncryptionWrapper.DecryptData(pwd, data);
		using (FileStream fs = new FileStream(path, FileMode.Truncate, FileAccess.Write)) {
			fs.Write(bytes, 0, bytes.Length);
		}
	}

	public static void EncryptFile(string pwd, string file) {
		var path = Path.Join(Utils.GetLocalAppDataPath(), file);
		var data = File.ReadAllBytes(path);
		var bytes = EncryptionWrapper.EncryptData(pwd, data);
		using (FileStream fs = new FileStream(path, FileMode.Truncate, FileAccess.Write)) {
			fs.Write(bytes, 0, bytes.Length);
		}
	}

}