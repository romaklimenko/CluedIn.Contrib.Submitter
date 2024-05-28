using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace CluedIn.Contrib.Submitter.Helpers;

public static class GuidHelper
{
    private static readonly Guid s_namespaceId = new("400d1d0c-f4e3-498f-bc18-cb3377a51104");

    private static readonly IMemoryCache s_memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });

    /// <summary>
    ///     Generates a deterministic GUID using a namespace GUID and the input string.
    /// </summary>
    /// <param name="input">The input string to generate the GUID from.</param>
    /// <returns>A deterministic GUID.</returns>
    public static Guid ToGuid(this string input)
    {
        return input.ToGuid(s_namespaceId);
    }

    private static Guid ToGuid(this string input, Guid namespaceId)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(null, nameof(input));
        }

        var key = $"{namespaceId}|{input}";

        if (s_memoryCache.TryGetValue(key, out Guid result))
        {
            return result;
        }

        // Convert the namespace GUID to a byte array and ensure it's in big-endian format
        Span<byte> namespaceBytes = stackalloc byte[16];
        namespaceId.TryWriteBytes(namespaceBytes);
        SwapByteOrder(namespaceBytes);

        ReadOnlySpan<byte> nameBytes = Encoding.UTF8.GetBytes(input);

        Span<byte> hashInput = stackalloc byte[namespaceBytes.Length + nameBytes.Length];
        namespaceBytes.CopyTo(hashInput);
        nameBytes.CopyTo(hashInput[namespaceBytes.Length..]);

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(hashInput, hash);

        Span<byte> guidBytes = stackalloc byte[16];

        hash[..16].CopyTo(guidBytes);

        // Set the version to 5 (name-based GUID using SHA-1)
        // https://en.wikipedia.org/wiki/Universally_unique_identifier#Versions_3_and_5_(namespace_name-based)
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (5 << 4));

        // Set the variant to DCE 1.1
        // https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        SwapByteOrder(guidBytes);

        return s_memoryCache.Set(
            key,
            new Guid(guidBytes),
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSize(1));

        // Swaps the byte order of the GUID to ensure big-endian format.
        // https://en.wikipedia.org/wiki/Endianness
        void SwapByteOrder(Span<byte> guidBytes)
        {
            // ReSharper disable once ReplaceSliceWithRangeIndexer
            guidBytes.Slice(0, 4).Reverse();
            guidBytes.Slice(4, 2).Reverse();
            guidBytes.Slice(6, 2).Reverse();
        }
    }
}
