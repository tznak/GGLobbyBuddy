using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace GGLobbyBuddy
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Kernel32
    {
        [Flags]
        public enum AccessRights
        {
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ      = 0x0010,
            PROCESS_VM_WRITE     = 0x0020
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess
        (
            AccessRights dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory
        (
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            ref int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory
        (
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            ref int lpNumberOfBytesWritten
        );

        public static T ReadProcessMemory<T>(IntPtr process_handle, IntPtr memory_address)
        {
            var _ = 0;
            var buffer = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(process_handle, memory_address, buffer, buffer.Length, ref _);

            var gc_handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(gc_handle.AddrOfPinnedObject());
            }
            finally
            {
                gc_handle.Free();
            }
        }
        
        public static T ReadProcessMemory<T>(IntPtr process_handle, int[] multi_level_ptr)
        {
            var current_address = (IntPtr) multi_level_ptr[0];
            
            for (var i = 1; i < multi_level_ptr.Length - 1; i++)
            {
                current_address = ReadProcessMemory<IntPtr>
                (
                    process_handle,
                    current_address + multi_level_ptr[i]
                );
            }
            
            return ReadProcessMemory<T>
            (
                process_handle,
                current_address + multi_level_ptr[multi_level_ptr.Length - 1]
            );
        }
    }
}