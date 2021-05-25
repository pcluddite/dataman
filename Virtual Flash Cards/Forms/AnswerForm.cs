//
//    Virtual Flash Cards
//    Copyright (C) 2009-2021 Timothy Baxendale
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualFlashCards.QuizData;

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

        public void SetAnswerControl(Answer answer)
        {
            SuspendLayout();
            Control control = answer.CreateFormControl(new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0));
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
