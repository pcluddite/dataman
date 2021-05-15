using System;
using System.Drawing;
using System.Windows.Forms;

namespace VirtualFlashCards.Forms
{
    public partial class AnswerForm : Form
    {
        private QuizForm quizForm;
        private Control answerControl;

        public event EventHandler<AnswerSubmittedEventArgs> AnswerSubmitted;

        public AnswerForm(QuizForm quizForm)
        {
            this.quizForm = quizForm;
            InitializeComponent();
            MoveUnderQuiz();
        }

        private void MoveUnderQuiz()
        {
            Point underQuiz = quizForm.DesktopLocation;
            underQuiz.Offset(0, quizForm.Height + 25);
            SetDesktopLocation(underQuiz.X, underQuiz.Y);
        }

        public void SetAnswerControl(Control control)
        {
            SuspendLayout();
            control.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            control.Location = new Point(10, 10);
            control.Width = ClientRectangle.Width - 20;
            btnSubmit.Location = new Point(btnSubmit.Left, control.Location.Y + control.Height + 5);
            ClientSize = new Size(ClientSize.Width, btnSubmit.Location.Y + btnSubmit.Height + 5);
            Controls.Add(control);
            ResumeLayout();
            answerControl = control;
            answerControl.Focus();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (AnswerSubmitted != null)
            {
                Control answerControl = this.answerControl;
                AnswerSubmittedEventArgs args = new AnswerSubmittedEventArgs(answerControl);
                AnswerSubmitted(this, args);
                if (!args.Cancel)
                {
                    SuspendLayout();
                    Controls.Remove(answerControl);
                    answerControl.Dispose();
                    ResumeLayout();
                    answerControl = null;
                }
            }
        }

        public class AnswerSubmittedEventArgs : EventArgs
        {
            public Control AnswerControl { get; private set; }
            public bool Cancel { get; set; }

            public AnswerSubmittedEventArgs(Control answerControl)
            {
                AnswerControl = answerControl;
            }
        }
    }
}
