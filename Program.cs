using MMOItemKnowledgeBase;

namespace TERADB
{
    internal static class Program
    {
        [STAThread]
        [System.Runtime.Versioning.SupportedOSPlatform("windows10.0.14393")]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}