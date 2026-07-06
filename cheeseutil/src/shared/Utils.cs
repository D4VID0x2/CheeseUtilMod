using System.IO;
using System.IO.Compression;

namespace CheeseUtilMod.Shared.CustomData
{
	public static class Utils
	{
		public static byte[] Compress(byte[] data)
		{
			using MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
			{
				dstream.Write(data, 0, data.Length);
			}
			return output.ToArray();
		}

		public static byte[] Decompress(byte[] compressed)
		{
			using MemoryStream input = new MemoryStream(compressed);
			using DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress);
			using MemoryStream output = new MemoryStream();
			deflateStream.CopyTo(output);
			return output.ToArray();
		}
	}
}
