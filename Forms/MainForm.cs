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

            Quiz q = null;

            try
            {
                q = Quiz.FromFile(openQuizDialog.FileName);
            }
            catch(Exception ex)
            {
                if (!(ex is IOException || ex is XmlException))
                    throw;
                context.ShowError(ex.Message);
            }

            if (q != null)
            {
                context.StartQuiz(q);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            context.EditQuiz(null);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (openQuizDialog.ShowDialog() == DialogResult.Cancel)
                return;

            Quiz q = null;

            try
            {
                q = Quiz.FromFile(openQuizDialog.FileName);
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is XmlException))
                    throw;
                context.ShowError(ex.Message);
            }

            if (q != null)
            {
                context.EditQuiz(q);
            }
        }
    }
}
