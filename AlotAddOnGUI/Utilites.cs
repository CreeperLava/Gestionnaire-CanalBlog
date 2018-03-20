using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;   //This namespace is used to work with WMI classes. For using this namespace add reference of System.Management.dll .
using Microsoft.Win32;     //This namespace is used to work with Registry editor.
using System.IO;
using AlotAddOnGUI.classes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using System.Windows;
using System.Xml.Linq;
using System.Security.Cryptography;
using SlavaGu.ConsoleAppLauncher;
using System.ComponentModel;

namespace AlotAddOnGUI
{
    public class Utilities
    {
        public const uint MEMI_TAG = 0x494D454D;

        public const int WIN32_EXCEPTION_ELEVATED_CODE = -98763;
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        // Pinvoke for API function
        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static int runProcessAsAdmin(string exe, string args, bool standAlone = false)
        {
            using (Process p = new Process())
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = exe;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.Arguments = args;
                p.StartInfo.Verb = "runas";
                try
                {
                    p.Start();
                    if (!standAlone)
                    {
                        p.WaitForExit(60000);
                        try
                        {
                            return p.ExitCode;
                        }
                        catch (Exception e)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    return WIN32_EXCEPTION_ELEVATED_CODE;
                }
            }
        }

        public static Task DeleteAsync(string path)
        {
            if (!File.Exists(path))
            {
                return Task.FromResult(0);
            }
            return Task.Run(() => { File.Delete(path); });
        }

        public static Task<FileStream> CreateAsync(string path)
        {
            if (path == null || path == "")
            {
                return null;
            }
            return Task.Run(() => File.Create(path));
        }

        public static Task MoveAsync(string sourceFileName, string destFileName)
        {
            if (sourceFileName == null || sourceFileName == "")
            {
                return null;
            }
            if (!File.Exists(sourceFileName))
            {
                return null;
            }
            return Task.Run(() => { File.Move(sourceFileName, destFileName); });
        }

        public static bool TestXMLIsValid(string inputXML)
        {
            try
            {
                XDocument.Parse(inputXML);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        public static string sha256(string randomString)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString), 0, Encoding.UTF8.GetByteCount(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}