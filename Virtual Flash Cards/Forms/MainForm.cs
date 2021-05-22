using System;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace VirtualFlashCards.Forms
{
    public partial class MainForm : CardForm
    {
        private AppContext context;

        public MainForm(AppContext context)
        {
            this.context = context;
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (openQuizDialog.ShowDialog() == DialogResult.Cancel)
                return;
            context.StartQuiz(openQuizDialog.FileName);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            context.NewQuiz();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (openQuizDialog.ShowDialog() == DialogResult.Cancel)
                return;
            context.EditQuiz(openQuizDialog.FileName);
        }
    }
}
