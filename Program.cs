using System;
using System.Windows.Forms;
using VirtualFlashCards.Forms;

namespace VirtualFlashCards
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext(args));
        }
    }
}
