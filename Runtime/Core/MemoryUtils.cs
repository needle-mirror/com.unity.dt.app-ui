using System;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Utility class for memory operations.
    /// </summary>
    public static class MemoryUtils
    {
        const int k_BufferSize = 8 * 1024;
        
        static readonly char[] k_Buffer = new char[k_BufferSize];
        
        static int s_BufferOffset = 0;
        
        /// <summary>
        /// Concatenates the strings into a single string.
        /// </summary>
        /// <remarks>
        /// This method is optimized for performance by using a shared buffer.
        /// The buffer size is 8KB, and if the total length of the strings exceeds the buffer size, an exception is thrown.
        /// </remarks>
        /// <param name="args"> The strings to concatenate. </param>
        /// <returns> The concatenated string. </returns>
        /// <exception cref="ArgumentException"> Thrown when the total length of the strings exceeds the buffer size. </exception>
        internal static string Concatenate(params string[] args)
        {
            s_BufferOffset = 0;
            foreach (var str in args)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                
                if (s_BufferOffset + str.Length > k_BufferSize)
                    throw new ArgumentException("The total length of the strings exceeds the buffer size.");
                
                str.CopyTo(0, k_Buffer, s_BufferOffset, str.Length);
                s_BufferOffset += str.Length;
            }

            return new string(k_Buffer, 0, s_BufferOffset);
        }
    }
}
