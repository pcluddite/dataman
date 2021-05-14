using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class QuizForm : CardForm
    {
        private ScoreForm scoreForm;

        public AppContext Context { get; private set; }

        public Quiz Quiz
        {
            get
            {
                return Context.CurrentQuiz;
            }
        }

        public Question CurrentQuestion
        {
            get
            {
                return Quiz[scoreForm.Current];
            }
        }

        public bool CloseError { get; private set; }

        public QuizForm(AppContext context)
        {
            this.Context = context;
            InitializeComponent();
            scoreForm = new ScoreForm(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            scoreForm.Show(this);
            if (Quiz.Count == 0)
            {
                Context.ShowError("There appear to be no questions in this quiz.");
                CloseError = true;
                Close();
            }
            else
            {
                Quiz.Shuffle();
                txtPrompt.Text = CurrentQuestion.Prompt;
                txtAnswer.Focus();
                txtAnswer.Select();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Answer answer = CurrentQuestion.Answer;
            if (answer.IsCorrect(txtAnswer.Text))
            {
                scoreForm.AddCorrect();
            }
            else
            {
                Quiz.AddWrongAnswer(scoreForm.Current, answer.CloneWithNewInput(txtAnswer.Text));
                scoreForm.AddIncorrect();
            }
            if (scoreForm.NextQuestion())
            {
                txtPrompt.Text = CurrentQuestion.Prompt;
                txtAnswer.Text = "";
                txtAnswer.Focus();
                txtAnswer.Select();
            }
            else
            {
                Close();
                FinishedForm fin = new FinishedForm(Quiz.WrongQuiz(), scoreForm.Correct, scoreForm.Incorrect, Context);
                fin.Show();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing 
                && !CloseError 
                && Context.AskYesNo(this, "Are you sure you want to stop the quiz?") == DialogResult.No)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
