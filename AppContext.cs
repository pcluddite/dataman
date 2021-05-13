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
        //private EditorForm editForm;
        private QuizForm quizForm;

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
            if (quizForm == null)
            {
                quizForm = new QuizForm(this);
                quizForm.FormClosing += new FormClosingEventHandler(FormClosing);
            }
            if (MainForm.Visible)
            {
                MainForm.Hide();
            }
            quizForm.SetDesktopLocation(MainForm.Location.X, MainForm.Location.Y);
            quizForm.FormBorderStyle = MainForm.FormBorderStyle;
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
            //CurrentQuiz = q;
            //if (editForm == null)
            //{
            //    editForm = new EditorForm(this);
            //    editForm.FormClosing += new FormClosingEventHandler(FormClosing);
            //}
            //if (MainForm.Visible)
            //{
            //    MainForm.Hide();
            //}
            //editForm.SetDesktopLocation(MainForm.Location.X, MainForm.Location.Y);
            //editForm.FormBorderStyle = MainForm.FormBorderStyle;
            //editForm.Show();
        }

        public void NewQuiz()
        {
            EditQuiz((Quiz)null);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(MainForm, message, MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Form form = (Form)sender;
                e.Cancel = true;
                MainForm.SetDesktopLocation(form.Location.X, form.Location.Y);
                MainForm.FormBorderStyle = form.FormBorderStyle;
                form.Hide();
                MainForm.Show();
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
    }
}
