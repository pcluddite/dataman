using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class QuizForm : CardForm
    {
        private ScoreForm scoreForm;
        private AnswerForm answerForm;

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

        public bool SuppressCloseQuestion { get; private set; }

        public QuizForm(AppContext context)
        {
            this.Context = context;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Quiz.Count == 0)
            {
                Context.ShowError("There appear to be no questions in this quiz.");
                SuppressCloseQuestion = true;
                Close();
            }
            else
            {
                Quiz.Shuffle();
                scoreForm = new ScoreForm(this);
                answerForm = new AnswerForm(this);
                answerForm.AnswerSubmitted += btnNext_Click;
                ShowCurrentQuestion();
                scoreForm.Show(this);
                answerForm.Show(this);
            }
        }

        private void btnNext_Click(object sender, AnswerForm.AnswerSubmittedEventArgs e)
        {
            Answer answer = CurrentQuestion.Answer;
            if (answer.IsCorrect(e.AnswerControl))
            {
                scoreForm.AddCorrect();
            }
            else
            {
                Quiz.AddWrongAnswer(scoreForm.Current, answer.CloneWithNewInput(e.AnswerControl));
                scoreForm.AddIncorrect();
            }
            if (scoreForm.NextQuestion())
            {
                ShowCurrentQuestion();
            }
            else
            {
                SuppressCloseQuestion = true;
                Close();
                FinishedForm fin = new FinishedForm(Quiz.WrongQuiz(), scoreForm.Correct, scoreForm.Incorrect, Context);
                fin.Show();
            }
        }

        private void ShowCurrentQuestion()
        {
            lblPrompt.Text = CurrentQuestion.Prompt;
            answerForm.SetAnswerControl(CurrentQuestion.Answer);
            answerForm.Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing 
                && !SuppressCloseQuestion 
                && Context.AskYesNo(this, "Are you sure you want to stop the quiz?") == DialogResult.No)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
            scoreForm.Close();
            answerForm.Close();
        }

        private void lblPrompt_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void lblPrompt_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void lblPrompt_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void lblPrompt_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }
    }
}
