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
    public partial class Main : CardForm
    {
        public int current = 0;
        public Quiz quiz;
        
        public bool saved = false;

        MainForm form;
        Control control;
        public Main(Quiz q, MainForm Form)
        {
            InitializeComponent();
            form = Form;
            control = new Control(this);
            control.Show(this);
            if (q == null)
            {
                quiz = new Quiz();
            }
            else
            {
                quiz = q;
                showCurrent();
                saved = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveCurrent())
            {
                current++;
                showCurrent();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveCurrent();
            if (current > 0)
            {
                current--;
                showCurrent();
            }
        }

        public void showCurrent()
        {
            if (current >= quiz.Count)
            {
                textBox3.Text = "";
                textBox2.Text = "";
            }
            else
            {
                textBox3.Text = quiz[current].Prompt;
                textBox2.Text = quiz[current].CorrectAnswer;
            }
            control.numericUpDown1.Value = (current + 1);
            if (current == 0)
            {
                button1.Enabled = false;
            }
            else if (!button1.Enabled)
            {
                button1.Enabled = true;
            }
        }

        public bool saveCurrent()
        {
            if (current < quiz.Count)
            {
                quiz[current].Prompt = textBox3.Text;
                quiz[current].CorrectAnswer = textBox2.Text;
                return true;
            }
            else if (textBox3.Text.Length > 0 || textBox2.Text.Length > 0)
            {
                quiz.AddQuestion(new Question(textBox3.Text, textBox2.Text));
                return true;
            }
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveCurrent())
            {
                if (MessageBox.Show(this, "Are you sure you want to delete this card? (This cannot be undone)", "Virtual Flash Cards", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    quiz.RemoveAt(current);
                    current--;
                    if (current < 0)
                    {
                        current = 0;
                    }
                    showCurrent();
                }
            }
            else
            {
                MessageBox.Show(this, "This card hasn't been added to the quiz yet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            saved = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            saved = false;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.SetDesktopLocation(form.Location.X, form.Location.Y);
            this.FormBorderStyle = form.FormBorderStyle;
            control.SetDesktopLocation(this.Location.X + 500, this.Location.Y + 50);
        }
    }
}
