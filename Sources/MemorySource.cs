using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources
{
    /// <summary>
    /// Base class for system resources that support memory reading.
    /// </summary>
    public abstract class MemorySource {
        /// <summary>
        /// When overriden, in a derived class, reads bytes from the source into an already initialized span.
        /// </summary>
        /// <param name="address">Memory address to start reading from.</param>
        /// <param name="count">Amount of bytes to read.</param>
        /// <param name="buffer">An existing span to copy memory to.</param>
        /// <returns>Whether the read operation was successful.</returns>
        public abstract bool ReadBytes(ulong address, nuint count, Span<byte> buffer);

        /// <summary>
        /// Reads a value of <typeparamref name="T"/> from the source. Returns <see cref="null"/> if unsuccessful.
        /// </summary>
        /// <typeparam name="T">Value type to read.</typeparam>
        /// <param name="address">Memory address to start reading from.</param>
        public T? Read<T>(ulong address) where T : unmanaged {
            return TryRead<T>(address, out T result) ? result : null;
        }

        /// <summary>
        /// Reads a value of <typeparamref name="T"/> from the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns>Resulting value.</returns>
        /// <exception cref="MemoryReadingException"></exception>
        public unsafe T ReadValue<T>(ulong address) where T : unmanaged {
            if (TryRead<T>(address, out T result))
                return result;
            else
                throw new MemoryReadingException(this, address, typeof(T), (nuint)sizeof(T));
        }

        /// <summary>
        /// Attempts to read a value of <typeparamref name="T"/> from the source.
        /// </summary>
        /// <typeparam name="T">Value tyoe to read.</typeparam>
        /// <param name="address">Memory address to start reading from.</param>
        /// <param name="result">A variable to store the result in.</param>
        /// <returns>Whether the read operation was successful.</returns>
        public unsafe bool TryRead<T>(ulong address, out T result) where T : unmanaged {
            Span<byte> buffer = stackalloc byte[sizeof(T)];
            if (ReadBytes(address, (nuint)sizeof(T), buffer)) {
                result = Unsafe.As<byte, T>(ref buffer.GetPinnableReference());
                return true;
            } else {
                result = default(T);
                return false;
            }
        }
    }

    public class MemoryReadingException : ApplicationException {
        public MemorySource MemorySource { get; }
        public ulong Address { get; }
        public nuint Count { get; }
        public MemoryReadingException(MemorySource source, ulong address, Type type, nuint sizeOfType) :
            base($"Failed to read {type.Name}[{sizeOfType}] at 0x{address:X}") {
            MemorySource = source;
            Address = address;
            Count = sizeOfType;
        }
    }
}
