using System;
using System.Windows.Forms;

namespace FreeDiskSpace_Monitoring_Tool
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DiskMonitoring monitor = new DiskMonitoring("CriticalValues.json");
            MainForm mainForm = new MainForm();
            MainPresenter presenter = new MainPresenter(mainForm, monitor);

            Application.Run(mainForm);

        }
    }
}
