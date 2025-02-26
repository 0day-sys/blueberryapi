using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

 #nullable disable
namespace BlueberryApi
{
    public class Blueberry
    {
        public static string BlueberryVersion = "1.1.5";
        private bool isInjected;
        private System.Timers.Timer time;
        private bool autoinject;

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize();

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetClients();

        [DllImport("bin\\Xeno.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Execute(byte[] scriptSource, string[] clientUsers, int numUsers);

        [DllImport("bin\\Xeno.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Compilable(byte[] scriptSource);

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Attach();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public Blueberry()
        {
            Blueberry.Initialize();
            this.time = new System.Timers.Timer();
            this.time.Elapsed += new ElapsedEventHandler(this.timertick);
            this.time.AutoReset = true;
            Task.Run((Func<Task>)(async () =>
            {
                while (true)
                {
                    if (this.IsRobloxOpen() && this.autoinject && !this.isInjected)
                        this.InjectBlueberry();
                    await Task.Delay(1000);
                }
            }));
        }

        public void KillRoblox()
        {
            if (!this.IsRobloxOpen())
                return;
            foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
                process.Kill();
        }

        public void AutoInject(bool value) => this.autoinject = value;

        public bool IsInjected() => this.isInjected;

        public bool IsRobloxOpen() => Process.GetProcessesByName("RobloxPlayerBeta").Length != 0;

        public string[] GetActiveClientNames()
        {
            return this.GetClientsFromDll().Select<Blueberry.ClientInfo, string>((Func<Blueberry.ClientInfo, string>)(c => c.name)).ToArray<string>();
        }

        public void InjectBlueberry()
        {
            if (!this.IsRobloxOpen())
                return;
            try
            {
                Blueberry.Attach();
                this.isInjected = true;
                if (!this.time.Enabled)
                    this.time.Start();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Failed to attach BlueberryApi: " + ex.Message, "Attaching Error");
                this.isInjected = false;
            }
        }

        public void Deject()
        {
            this.isInjected = false;
            IntPtr moduleHandle = Blueberry.GetModuleHandle("bin\\Xeno.dll");
            if (moduleHandle != IntPtr.Zero)
                Blueberry.FreeLibrary(moduleHandle);
            this.Reload();
        }

        public void Reload()
        {
            if (this.isInjected)
                return;
            Blueberry.LoadLibrary("bin\\Xeno.dll");
            this.isInjected = true;
        }

        private void timertick(object sender, EventArgs e)
        {
            if (this.IsRobloxOpen())
                return;
            this.isInjected = false;
            if (this.time.Enabled)
                this.time.Stop();
        }

        public void ExecuteScript(string script)
        {
            try
            {
                if (!this.IsInjected() || !this.IsRobloxOpen())
                    return;
                List<Blueberry.ClientInfo> clientsFromDll = this.GetClientsFromDll();
                if (clientsFromDll == null || clientsFromDll.Count == 0)
                    return;
                string[] array = clientsFromDll.GroupBy<Blueberry.ClientInfo, int>((Func<Blueberry.ClientInfo, int>)(c => c.id)).Select<IGrouping<int, Blueberry.ClientInfo>, string>((Func<IGrouping<int, Blueberry.ClientInfo>, string>)(g => g.First<Blueberry.ClientInfo>().name)).ToArray<string>();
                if (array.Length == 0)
                    return;
                Blueberry.Execute(Encoding.UTF8.GetBytes(script), array, array.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing script: " + ex.Message);
            }
        }

        public string GetCompilableStatus(string script)
        {
            IntPtr ptr = Blueberry.Compilable(Encoding.ASCII.GetBytes(script));
            string stringAnsi = Marshal.PtrToStringAnsi(ptr);
            Marshal.FreeCoTaskMem(ptr);
            return stringAnsi;
        }

        private List<Blueberry.ClientInfo> GetClientsFromDll()
        {
            List<Blueberry.ClientInfo> clientsFromDll = new List<Blueberry.ClientInfo>();
            IntPtr clients = Blueberry.GetClients();
            while (true)
            {
                Blueberry.ClientInfo structure = Marshal.PtrToStructure<Blueberry.ClientInfo>(clients);
                if (structure.name != null)
                {
                    clientsFromDll.Add(structure);
                    clients += Marshal.SizeOf<Blueberry.ClientInfo>();
                }
                else
                    break;
            }
            return clientsFromDll;
        }

        private struct ClientInfo
        {
            public string version;
            public string name;
            public int id;
        }
    }
}
