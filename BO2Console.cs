using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
public class BO2Console
{
    private static IntPtr _cBuf_addTextFuncAddress = IntPtr.Zero;
    private static IntPtr _SV_GameSendServercmdAddress = IntPtr.Zero;
    private static byte[] callBytes;
    private static IntPtr cmdAddress = IntPtr.Zero;
    private static byte[] cmdBytes;
    private static IntPtr pHandel;
    private static IntPtr ProcessHandle = IntPtr.Zero;
    private static int ProcessID = -1;

    [DllImport("kernel32")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, [Out] int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    private static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, [Out] int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);

    public static bool SendCommand(string command)
    {
        if (command == "")
        {
            return false;
        }
        Process[] zmprocess = Process.GetProcessesByName("t6zm");
        Process[] mpprocess = Process.GetProcessesByName("t6mp");
        if (mpprocess.Length != 0)
        {
            if (ProcessID != mpprocess[0].Id)
            {
                ProcessID = mpprocess[0].Id;
                ProcessHandle = OpenProcess(0x1f0fff, false, ProcessID);
                if (HandleProcess("t6mp"))
                {
                    WriteNOP(0x8c923a);
                }
            }
            Send(command);
        }
        else if (zmprocess.Length != 0)
        {
            if (ProcessID != zmprocess[0].Id)
            {
                ProcessID = zmprocess[0].Id;
                ProcessHandle = OpenProcess(0x1f0fff, false, ProcessID);
                if (HandleProcess("t6mp"))
                {
                    WriteNOP(0x8c923a);
                }
            }
            Send(command);
        }
        return true;
    }
    private static bool HandleProcess(string ProcessName)
    {
        try
        {
            Process[] process = Process.GetProcessesByName(ProcessName);
            if (process.Length == 0)
            {
                return false;
            }
            pHandel = process[0].Handle;
            return true;
        }
        catch
        {
            return false;
        }
    }
    private static bool Send(string cmd)
    {
        try
        {
            Process[] zmprocess = Process.GetProcessesByName("t6zm");
            if (Process.GetProcessesByName("t6mp").Length != 0)
            {
                callBytes = BitConverter.GetBytes(0x5c6f10);
            }
            /*else if (zmprocess.Length != 0)
            {
                callBytes = BitConverter.GetBytes(0xc0af3e); // THIS ADDRESS IS NOT UPDATED AND IT MAY CAUSE A VAC BAN SO PLEASE GET THE CORRECT ADDRESS BEFORE USING THE CONSOLE FOR ZOMBIES
            }*/
            if (cmd == null)
            {
                return false;
            }
            if (_cBuf_addTextFuncAddress == IntPtr.Zero)
            {
                IntPtr ptr;
                _cBuf_addTextFuncAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)Stubs.WrapperTocBuf_AddText.Length, 0x3000, 0x40);
                cmdBytes = Encoding.ASCII.GetBytes(cmd + '\0');
                cmdAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)cmdBytes.Length, 0x3000, 0x40);
                int lpNumberOfBytesWritten = 0;
                WriteProcessMemory(ProcessHandle, cmdAddress, cmdBytes, (uint)cmdBytes.Length, lpNumberOfBytesWritten);
                Array.Copy(BitConverter.GetBytes(cmdAddress.ToInt32()), 0, Stubs.WrapperTocBuf_AddText, 9, 4);
                Array.Copy(callBytes, 0, Stubs.WrapperTocBuf_AddText, 0x10, 4);
                WriteProcessMemory(ProcessHandle, _cBuf_addTextFuncAddress, Stubs.WrapperTocBuf_AddText, (uint)Stubs.WrapperTocBuf_AddText.Length, lpNumberOfBytesWritten);
                CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, _cBuf_addTextFuncAddress, IntPtr.Zero, 0, out ptr);
                if ((_cBuf_addTextFuncAddress != IntPtr.Zero) && (cmdAddress != IntPtr.Zero))
                {
                    VirtualFreeEx(ProcessHandle, _cBuf_addTextFuncAddress, (UIntPtr)Stubs.WrapperTocBuf_AddText.Length, 0x8000);
                    VirtualFreeEx(ProcessHandle, cmdAddress, (UIntPtr)cmdBytes.Length, 0x8000);
                }
                _cBuf_addTextFuncAddress = IntPtr.Zero;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    private static bool WriteNOP(int Address)
    {
        try
        {
            byte[] buffer = new byte[] { 0x90, 0x90 };
            IntPtr zero = IntPtr.Zero;
            WriteProcessMemory(pHandel, (IntPtr)Address, buffer, (uint)buffer.Length, out zero);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
