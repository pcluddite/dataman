using System;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class QuizForm : CardForm
    {
        private Quiz quiz;
        private ScoreForm score = new ScoreForm();
        private AppContext context;

        public Quiz Quiz { get; private set; }
        public int Current { get; set; }
        public int Correct { get; set; }
        public int Incorrect { get; set; }

        public Question CurrentQuestion
        {
            get
            {
                return quiz[Correct];
            }
        }

        public QuizForm(AppContext context)
        {
            InitializeComponent();
            this.context = context;
            quiz = context.CurrentQuiz;
            score.label4.Text = "You are currently on question: " + (Current + 1) + " of " + Quiz.Count;
        }

        private void Card_Load(object sender, EventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                button3.Visible = true;
            }
            if (Quiz.Count == 0)
            {
                context.ShowError("There appear to be no questions in this quiz.");
                Close();
            }
            else
            {
                quiz.Shuffle();
                textBox1.Text = Quiz[++Current].Prompt;
                textBox2.Focus();
                textBox2.Select();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Answer answer = CurrentQuestion.Answer;
            if (answer.IsCorrect(textBox2.Text))
            {
                Correct++;
                score.label6.Text = "Correct!";
                score.label6.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                Incorrect++;
                quiz.AddWrongAnswer(Current, Answer.Create(answer.GetType(), textBox2.Text));
                score.label6.Text = "Incorrect!";
                score.label6.ForeColor = System.Drawing.Color.Red;
            }
            current = quiz.Next();
            if (current == null)
            {
                Close();
                FinishedForm fin = new FinishedForm(quiz.Wrong(), correct, incorrect, context);
                fin.Show();
            }
            else
            {
                textBox1.Text = current.Prompt;
                textBox2.Text = "";
                textBox2.Focus();
                textBox2.Select();
                score.updateScore(correct, incorrect, quiz.Current);
            }
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
