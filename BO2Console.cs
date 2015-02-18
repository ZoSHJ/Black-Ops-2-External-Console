using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

// This code was downloaded from https://github.com/ZoSHJ/Black-Ops-2-External-Console

public class BO2Console
{
    // Update the addresses here
    private static int mpaddress = 0x5bdf70;
    private static int zmaddress = 0;
    private static int mpnopaddress = 0x8c90da;
    private static int zmnopaddress = 0;
    #region Variable Declarations
    private static byte[] WrapperTocBuf_AddText = new byte[] { 
            0x55, 0x8b, 0xec, 0x83, 0xec, 8, 0xc7, 0x45, 0xf8, 0, 0, 0, 0, 0xc7, 0x45, 0xfc, 
            0, 0, 0, 0, 0xff, 0x75, 0xf8, 0x6a, 0, 0xff, 0x55, 0xfc, 0x83, 0xc4, 8, 0x8b, 
            0xe5, 0x5d, 0xc3
    };
    private static IntPtr _cBuf_addTextFuncAddress = IntPtr.Zero;
    private static byte[] callBytes;
    private static IntPtr cmdAddress = IntPtr.Zero;
    private static byte[] cmdBytes;
    private static IntPtr pHandel;
    private static IntPtr ProcessHandle = IntPtr.Zero;
    private static int ProcessID = -1;
    #endregion
    #region DLLImports
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
    #endregion
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
                if (mpnopaddress != 0 && HandleProcess("t6mp"))
                {
                    WriteNOP(mpnopadress);
                }
            }
            Send(command);
        }
        else if (zmprocess.Length != 0 & zmaddress != 0)
        {
            if (ProcessID != zmprocess[0].Id)
            {
                ProcessID = zmprocess[0].Id;
                ProcessHandle = OpenProcess(0x1f0fff, false, ProcessID);
                if (zmnopaddress != 0 && HandleProcess("t6zp"))
                {
                    WriteNOP(zmnopaddress);
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
            Process[] mpprocess = Process.GetProcessesByName("t6mp");
            Process[] zmprocess = Process.GetProcessesByName("t6zm");
            if (mpprocess.Length != 0 & mpaddress != 0)
            {
                callBytes = BitConverter.GetBytes(mpaddress);
            }
            else if (zmprocess.Length != 0 & zmaddress != 0)
            {
                callBytes = BitConverter.GetBytes(zmaddress);
            }
            if (cmd == null)
            {
                return false;
            }
            if (_cBuf_addTextFuncAddress == IntPtr.Zero)
            {
                IntPtr ptr;
                _cBuf_addTextFuncAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)WrapperTocBuf_AddText.Length, 0x3000, 0x40);
                cmdBytes = Encoding.ASCII.GetBytes(cmd + '\0');
                cmdAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)cmdBytes.Length, 0x3000, 0x40);
                int lpNumberOfBytesWritten = 0;
                WriteProcessMemory(ProcessHandle, cmdAddress, cmdBytes, (uint)cmdBytes.Length, lpNumberOfBytesWritten);
                Array.Copy(BitConverter.GetBytes(cmdAddress.ToInt32()), 0, WrapperTocBuf_AddText, 9, 4);
                Array.Copy(callBytes, 0, WrapperTocBuf_AddText, 0x10, 4);
                WriteProcessMemory(ProcessHandle, _cBuf_addTextFuncAddress, WrapperTocBuf_AddText, (uint)WrapperTocBuf_AddText.Length, lpNumberOfBytesWritten);
                CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, _cBuf_addTextFuncAddress, IntPtr.Zero, 0, out ptr);
                if ((_cBuf_addTextFuncAddress != IntPtr.Zero) && (cmdAddress != IntPtr.Zero))
                {
                    VirtualFreeEx(ProcessHandle, _cBuf_addTextFuncAddress, (UIntPtr)WrapperTocBuf_AddText.Length, 0x8000);
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