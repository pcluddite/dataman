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
using System.Windows.Forms;

namespace VirtualFlashCards.Forms
{
    public partial class WrongAnswerForm : Form
    {
        private FinishedForm fin;
        private AppContext context;

        private int current = 0;

        public WrongAnswerForm(FinishedForm form)
        {
            fin = form;
            context = fin.context;
            InitializeComponent();
            showCurrent();
        }

        public void showCurrent()
        {
            //textBox1.Text = fin.wrongAns[current].Prompt;
            //textBox2.Text = fin.wrongAns[current].AnswerHistory.Last();
            //textBox3.Text = fin.wrongAns[current].CorrectAnswer;
            //numericUpDown1.Value = current + 1;
        }

        public void saveCurrent()
        {
            //switch (MessageBox.Show(this, "You changed the correct answer. Would you like to save your changes?", "Virtual Flash Cards", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            //{
            //    case DialogResult.Yes:
            //        fin.wrongAns[current].CorrectAnswer = textBox3.Text;
            //        break;
            //    case DialogResult.Cancel:
            //        return;
            //}
            //if (textBox2.Text.Equals(fin.wrongAns[current].CorrectAnswer))
            //{
            //    correctCurrent();
            //}
        }

        private void perfect()
        {
            Hide();
            MessageBox.Show(this, "You got 100%. You have no wrong answers.", "Virtual Flash Cards", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void showNext()
        {
            current = (current + 1) % fin.wrongAns.Count;
            showCurrent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (textBox3.Text.Equals(fin.wrongAns[current].CorrectAnswer))
            //{
            //    showNext();
            //}
            //else
            //{
            //    saveCurrent();
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            context.StartQuiz(fin.wrongAns);
            fin.Close();
            Close();
        }

        private void WrongAns_Load(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = fin.wrongAns.Count;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            current = (int)numericUpDown1.Value - 1;
            showCurrent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Mark this answer as correct?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                correctCurrent();
            }
        }

        private void correctCurrent()
        {
            fin.wrongAns.RemoveAt(current);
            fin.incorrect--;
            fin.correct++;
            if (fin.wrongAns.Count == 0)
            {
                perfect();
            }
            else
            {
                showNext();
                numericUpDown1.Maximum = fin.wrongAns.Count;
            }
        }
    }
}
