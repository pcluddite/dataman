using System;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class FinishedForm : Form
    {
        public Quiz wrongAns;
        public int correct;
        public int incorrect;
        public AppContext context;

        public FinishedForm(Quiz WrongAns, int cor, int incor, AppContext context)
        {
            this.context = context;
            wrongAns = WrongAns;
            correct = cor;
            incorrect = incor;
            InitializeComponent();
            decimal p = (decimal)correct / (decimal)(correct + incorrect);
            label4.Text = correct.ToString();
            label5.Text = incorrect.ToString();
            label7.Text = Math.Round(p * 100, 0) + "%";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (wrongAns.Count != 0)
            {
                WrongAnswerForm wAnswers = new WrongAnswerForm(this);
                wAnswers.ShowDialog();
                decimal p = (decimal)correct / (decimal)(correct + incorrect);
                label4.Text = correct + "";
                label5.Text = incorrect + "";
                label7.Text = Math.Round(p * 100, 0) + "%";
            }
            else
            {
                MessageBox.Show(this, "You got 100%. You have no wrong answers.", "Virtual Flash Cards", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
