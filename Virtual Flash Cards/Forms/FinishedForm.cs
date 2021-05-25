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
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    public partial class FinishedForm : Form
    {
        public Quiz wrongAns;
        public int correct;
        public int incorrect;
        public AppContext context;

        public FinishedForm(Quiz WrongAns, int cor, int incor, AppContext context)
        {
            this.context = context;
            wrongAns = WrongAns;
            correct = cor;
            incorrect = incor;
            InitializeComponent();
            decimal p = (decimal)correct / (decimal)(correct + incorrect);
            label4.Text = correct.ToString();
            label5.Text = incorrect.ToString();
            label7.Text = Math.Round(p * 100, 0) + "%";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (wrongAns.Count != 0)
            {
                WrongAnswerForm wAnswers = new WrongAnswerForm(this);
                wAnswers.ShowDialog();
                decimal p = (decimal)correct / (decimal)(correct + incorrect);
                label4.Text = correct + "";
                label5.Text = incorrect + "";
                label7.Text = Math.Round(p * 100, 0) + "%";
            }
            else
            {
                MessageBox.Show(this, "You got 100%. You have no wrong answers.", "Virtual Flash Cards", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
