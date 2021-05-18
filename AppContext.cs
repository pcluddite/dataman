using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using VirtualFlashCards.Forms;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards
{
    public class AppContext : ApplicationContext
    {
        public Quiz CurrentQuiz { get; private set; }

        public AppContext(string[] args)
        {
            MainForm = new MainForm(this);
            if (args.Length > 0)
            {
                Quiz quiz = null;
                try
                {
                    quiz = Quiz.FromFile(args.Length == 1 ? args[0] : args[1]);
                }
                catch (Exception ex)
                {
                    if (!(ex is IOException || ex is XmlException))
                        throw;
                    ShowError(ex.Message);
                    MainForm.Show();
                }
                if (quiz != null)
                {
                    if (args.Length == 1)
                    {
                        StartQuiz(quiz);
                    }
                    else if (args[0].Equals("/run"))
                    {
                        StartQuiz(quiz);
                    }
                    else if (args[0].Equals("/edit"))
                    {
                        EditQuiz(quiz);
                    }
                    else
                    {
                        ShowError("Invalid argument '" + args[0] + "'");
                    }
                }
            }
            else
            {
                MainForm.Show();
            }
        }

        public void StartQuiz(string path)
        {
            Quiz q = OpenQuiz(path);
            if (q != null)
            {
                StartQuiz(q);
            }
        }

        public void StartQuiz(Quiz q)
        {
            CurrentQuiz = q;
            QuizForm quizForm = new QuizForm(this);
            quizForm.FormClosing += new FormClosingEventHandler(FormClosing);
            quizForm.SetDesktopLocation(MainForm.Location.X, MainForm.Location.Y);
            quizForm.FormBorderStyle = MainForm.FormBorderStyle;
            MainForm.Hide();
            quizForm.Show();
        }

        public void EditQuiz(string path)
        {
            Quiz q = OpenQuiz(path);
            if (q != null)
            {
                EditQuiz(q);
            }
        }

        public void EditQuiz(Quiz q)
        {
            CurrentQuiz = q;
            EditForm editForm = new EditForm(this);
            editForm.FormClosing += new FormClosingEventHandler(FormClosing);
            MainForm.Hide();
            editForm.Show();
        }

        public void NewQuiz()
        {
            EditQuiz((Quiz)null);
        }

        private void FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                CardForm cardForm = sender as CardForm;
                if (cardForm != null)
                {
                    MainForm.SetDesktopLocation(cardForm.Location.X, cardForm.Location.Y);
                    MainForm.FormBorderStyle = cardForm.FormBorderStyle;
                }
                MainForm.Show();
                MainForm.BringToFront();
                MainForm.Activate();
            }
            else if (MainForm != null && !MainForm.IsDisposed)
            {
                MainForm.Close();
            }
        }

        public Quiz OpenQuiz(string path)
        {
            try
            {
                return Quiz.FromFile(path);
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is XmlException))
                    throw;
                ShowError(ex.Message);
                return null;
            }
        }

        public void ShowError(string text)
        {
            ShowError(MainForm, text);
        }

        public void ShowError(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public DialogResult AskYesNo(string text)
        {
            return AskYesNo(MainForm, text);
        }

        public DialogResult AskYesNo(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, "Are you sure you want to stop the quiz?", MainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
