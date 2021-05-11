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
            InitializeComponent();
            this.context = context;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            Quiz q = null;

            try
            {
                q = Quiz.FromFile(openFileDialog1.FileName);
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

        private void button2_Click(object sender, EventArgs e)
        {
            context.EditQuiz(null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (context.CurrentQuiz == null && openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            Quiz q = null;

            try
            {
                q = Quiz.FromFile(openFileDialog1.FileName);
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
