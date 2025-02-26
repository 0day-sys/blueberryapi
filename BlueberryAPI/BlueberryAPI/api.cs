using System;
using System.Diagnostics;
using System.Windows.Forms;

#nullable disable
namespace BlueberryApi
{
    public static class Api
    {
        private static Timer time1 = new Timer();
        private static Blueberry blueberry;

        static Api()
        {
            Api.CreateBlueberry();
            Api.time1.Tick += new EventHandler(Api.ticktimer32433);
            Api.time1.Start();
        }

        private static void CreateBlueberry() => Api.blueberry = new Blueberry();

        public static void Inject() => Api.blueberry?.InjectBlueberry();

        public static void KillRoblox() => Api.blueberry?.KillRoblox();

        public static bool IsInjected()
        {
            Blueberry blueberry = Api.blueberry;
            return blueberry != null && blueberry.IsInjected();
        }

        public static bool IsRobloxOpen() => Process.GetProcessesByName("RobloxPlayerBeta").Length != 0;

        public static string[] GetActiveClientNames() => Api.blueberry?.GetActiveClientNames();

        public static void ExecuteScript(string script) => Api.blueberry?.ExecuteScript(script);

        private static void ticktimer32433(object sender, EventArgs e)
        {
            if (!Api.IsRobloxOpen())
            {
                if (Api.blueberry == null)
                    return;
                Api.blueberry.Deject();
                Api.blueberry = (Blueberry)null;
            }
            else
            {
                if (Api.blueberry != null)
                    return;
                Api.CreateBlueberry();
            }
        }

        public static void SetAutoInject(bool value) => Api.blueberry?.AutoInject(value);
    }
}
