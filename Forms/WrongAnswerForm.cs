using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualFlashCards.Forms
{
    public partial class WrongAnswerForm : Form
    {
        private Finished fin;
        private MainForm form1;

        private int current = 0;

        public WrongAnswerForm(Finished form, MainForm frm1)
        {
            InitializeComponent();
            fin = form;
            form1 = frm1;
            showCurrent();
        }

        public void showCurrent()
        {
            textBox1.Text = fin.wrongAns[current].Prompt;
            textBox2.Text = fin.wrongAns[current].AnswerHistory.Last();
            textBox3.Text = fin.wrongAns[current].CorrectAnswer;
            numericUpDown1.Value = current + 1;
        }

        public void saveCurrent()
        {
            switch (MessageBox.Show(this, "You changed the correct answer. Would you like to save your changes?", "Virtual Flash Cards", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    fin.wrongAns[current].CorrectAnswer = textBox3.Text;
                    break;
                case DialogResult.Cancel:
                    return;
            }
            if (textBox2.Text.Equals(fin.wrongAns[current].CorrectAnswer))
            {
                correctCurrent();
            }
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
            if (textBox3.Text.Equals(fin.wrongAns[current].CorrectAnswer))
            {
                showNext();
            }
            else
            {
                saveCurrent();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Card c = new Card(fin.wrongAns, fin.form1);
            fin.Close();
            Close();
            form1.Hide();
            c.Show();
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
