using System;
using System.Diagnostics;

namespace GGLobbyBuddy
{
    public class MemoryEditor
    {
        private const Kernel32.AccessRights ACCESS_FLAGS 
            = Kernel32.AccessRights.PROCESS_VM_READ
            | Kernel32.AccessRights.PROCESS_VM_WRITE 
            | Kernel32.AccessRights.PROCESS_VM_OPERATION;

        private Process process;
        private IntPtr process_handle;
        private int base_address;
        public bool IsConnected => !(process?.HasExited ?? true);

        public bool Connect()
        {
            if (IsConnected) return true;
            
            var processes = Process.GetProcessesByName("GuiltyGearXrd");
            if (processes.Length == 0 || processes[0]?.MainModule == null) return false;

            process = processes[0];
            process_handle = Kernel32.OpenProcess(ACCESS_FLAGS, false, process.Id);
            base_address = (int) (process.MainModule?.BaseAddress ?? default);
            
            return true;
        }

        public long GetLobbyId()
        {
            if (process == null) return 0;
            var lobby_id_ptr = new[] {base_address, 0x016FE410, 0x1BC, 0x1F8, 0x0};
            return Kernel32.ReadProcessMemory<long>(process_handle, lobby_id_ptr);
        }
        
        public int GetPlayerCount()
        {
            if (process == null) return 0;
            var lobby_id_ptr = new[] {base_address, 0x016FE410, 0x1BC, 0x1F8, 0x18};
            return Kernel32.ReadProcessMemory<int>(process_handle, lobby_id_ptr);
        }
    }
}