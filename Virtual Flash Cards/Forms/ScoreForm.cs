using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class ScoreForm : Form
    {
        private QuizForm quizForm;

        public Quiz Quiz
        {
            get
            {
                return quizForm.Quiz;
            }
        }

        public int Current { get; set; }
        public int Correct { get; set; }
        public int Incorrect { get; set; }
        
        public int Completed
        {
            get
            {
                return Correct + Incorrect;
            }
        }

        public double Percentage
        {
            get
            {
                return (double)Correct / Completed;
            }
        }

        public ScoreForm(QuizForm quizForm)
        {
            this.quizForm = quizForm;
            InitializeComponent();
            MoveToLeftOfQuiz();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateScore();
        }

        protected override void OnShown(EventArgs e)
        {
            MoveToLeftOfQuiz();
            base.OnShown(e);
        }

        public void UpdateScore()
        {
            lblRight.Text = "Correct: " + Correct;
            lblWrong.Text = "Incorrect: " + Incorrect;
            if (Completed > 0)
            {
                lblPct.Text = string.Format("Percentage: {0:P2}", Percentage);
            }
            else
            {
                lblPct.Text = "Percentage: -- %";
            }
            lblMessage.Text = "You are currently on question " + (Current + 1) + " of " + Quiz.Count;
        }

        public void AddIncorrect()
        {
            lblLatest.ForeColor = Color.Red;
            lblLatest.Text = "Incorrect!";
            ++Incorrect;
            UpdateScore();
        }

        public void AddCorrect()
        {
            lblLatest.ForeColor = Color.Green;
            lblLatest.Text = "Correct!";
            ++Correct;
            UpdateScore();
        }

        public bool NextQuestion()
        {
            if (++Current < Quiz.Count)
            {
                UpdateScore();
                return true;
            }
            return false;
        }

        public void MoveToLeftOfQuiz()
        {
            Point leftOfQuiz = quizForm.DesktopLocation;
            leftOfQuiz.Offset(-Width - 25, (quizForm.Height - Height) / 2);
            SetDesktopLocation(leftOfQuiz.X, leftOfQuiz.Y);
        }
    }
}
