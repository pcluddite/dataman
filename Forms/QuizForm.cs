using System;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class QuizForm : CardForm
    {
        private Quiz quiz;
        private Question current;
        private ScoreForm score = new ScoreForm();
        private AppContext context;
        private int correct = 0;
        private int incorrect = 0;

        public QuizForm(AppContext context)
        {
            InitializeComponent();
            this.context = context;
            quiz = context.CurrentQuiz;
            score.label4.Text = "You are currently on question: " + (quiz.Current + 1) + " of " + quiz.Count;
        }

        private void Card_Load(object sender, EventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                button3.Visible = true;
            }
            if (quiz.Count == 0)
            {
                context.ShowError("There appear to be no questions in this quiz.");
                Close();
            }
            else
            {
                quiz.Shuffle();
                current = quiz.Next();
                textBox1.Text = current.Prompt;
                textBox2.Focus();
                textBox2.Select();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (current.IsCorrect(textBox2.Text))
            //{
            //    correct++;
            //    score.label6.Text = "Correct!";
            //    score.label6.ForeColor = System.Drawing.Color.Green;
            //}
            //else
            //{
            //    incorrect++;
            //    quiz.AddWrong(current);
            //    score.label6.Text = "Incorrect!";
            //    score.label6.ForeColor = System.Drawing.Color.Red;
            //}
            //current = quiz.Next();
            //if (current == null)
            //{
            //    Close();
            //    FinishedForm fin = new FinishedForm(quiz.Wrong(), correct, incorrect, context);
            //    fin.Show();
            //}
            //else {
            //    textBox1.Text = current.Prompt;
            //    textBox2.Text = "";
            //    textBox2.Focus();
            //    textBox2.Select();
            //    score.updateScore(correct, incorrect, quiz.Current);
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to stop the quiz?", "Virtual Flash Cards", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            score.BringToFront();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This feature is not yet avalible in this version.", "Virtual Flash Cards", MessageBoxButtons.OK, MessageBoxIcon.Information); 
            /*
            if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                EasyXml state = new EasyXml(saveFileDialog1.FileName);
                System.IO.File.Copy(Program.flash.path, saveFileDialog1.FileName, true);
                string order = "";
                foreach (int i in questions)
                {
                    order += "|" + i;
                }
                state.CreateChild("Quiz", "Order", order);
            } */
        }
    }
}
