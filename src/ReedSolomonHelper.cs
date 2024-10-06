using STH1123.ReedSolomon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hVault {

	public static class ReedSolomonHelper {
		private const double ParityFraction = 0.20; // Fraction of total symbols that should be parity

		private static (int dataSymbols, int paritySymbols, int totalSymbols) CalculateSymbols(int dataSize) {
			// Calculate total symbols (n) dynamically
			int totalSymbols = (int)Math.Ceiling(dataSize / (1 - ParityFraction));
			int paritySymbols = totalSymbols - dataSize; // Calculate parity symbols
			return (dataSize, paritySymbols, totalSymbols);
		}

		public static byte[] Encode(string file) {
			byte[] buffer = File.ReadAllBytes(file);
			int dataSize = buffer.Length;

			// Calculate symbols for Reed-Solomon
			var (dataSymbols, paritySymbols, totalSymbols) = CalculateSymbols(dataSize);

			// Prepare the data for Reed-Solomon encoding
			int[] dataSymbolsArray = new int[totalSymbols];

			// Copy the original data to the data symbols array
			for (int i = 0; i < dataSize; i++) {
				dataSymbolsArray[i] = buffer[i]; 
			}

			// Fill the remaining space with zeroes (null symbols) to make it totalSymbols
			for (int i = dataSize; i < totalSymbols; i++) {
				dataSymbolsArray[i] = 0;
			}

			// Create a Galois field for Reed-Solomon
			GenericGF field = new GenericGF(285, 256, 0);
			ReedSolomonEncoder encoder = new ReedSolomonEncoder(field);

			// Encode the data with Reed-Solomon
			encoder.Encode(dataSymbolsArray, paritySymbols);

			// Prepare the output
			byte[] output = new byte[totalSymbols];
			for (int i = 0; i < totalSymbols; i++) {
				output[i] = (byte)dataSymbolsArray[i];
			}

			// Append the amount of parity symbols to the output
			byte[] parityCountBytes = BitConverter.GetBytes(paritySymbols);
			byte[] result = new byte[parityCountBytes.Length + output.Length];
			parityCountBytes.CopyTo(result, 0);
			output.CopyTo(result, parityCountBytes.Length);

			return result;
		}


		public static byte[] Decode(string file) {
			// Read the encoded data from the file
			byte[] data = File.ReadAllBytes(file);

			// Extract the parity count
			byte[] parityCountBytes = data.Take(4).ToArray();
			int paritySymbols = BitConverter.ToInt32(parityCountBytes, 0);

			// Prepare the data for Reed-Solomon decoding
			int dataSize = data.Length - 4; // Total data minus the parity count bytes
			int[] dataSymbolsArray = new int[dataSize];

			// Fill the dataSymbolsArray with the encoded data, excluding the parity count
			for (int i = 0; i < dataSize; i++) {
				dataSymbolsArray[i] = data[i + 4]; 
			}

			// Create a Galois field for Reed-Solomon
			GenericGF field = new GenericGF(285, 256, 0);
			ReedSolomonDecoder decoder = new ReedSolomonDecoder(field);

			// Decode the data with Reed-Solomon
			if (!decoder.Decode(dataSymbolsArray, paritySymbols)) {
				Console.WriteLine($"Decoding failed");
				return null;
			}

				// Calculate the size of the original data
			int originalDataSize = dataSize - paritySymbols;

			// Prepare the output without parity symbols
			byte[] output = new byte[originalDataSize];
			for (int i = 0; i < originalDataSize; i++) {
				output[i] = (byte)dataSymbolsArray[i]; 
			}

			return output;
		}




	}
}
