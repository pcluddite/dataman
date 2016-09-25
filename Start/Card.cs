using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Virtual_Flash_Cards.Start
{
    public partial class Card : Form
    {
        private Quiz quiz;
        private Question current;
        private Score score = new Score();
        private Form1 form;
        private int correct = 0;
        private int incorrect = 0;

        public Card(Quiz q, Form1 Form)
        {
            InitializeComponent();
            form = Form;
            quiz = q;
            score.label4.Text = "You are currently on question: " + (q.Current + 1) + " of " + q.Count;
        }

        void Card_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            score.Close();
            form.SetDesktopLocation(this.Location.X, this.Location.Y);
            form.FormBorderStyle = this.FormBorderStyle;
            form.Show();
        }

        private void Card_Load(object sender, EventArgs e)
        {
            this.SetDesktopLocation(form.Location.X, form.Location.Y);
            score.Show(this);
            score.SetDesktopLocation(this.Location.X - 320, this.Location.Y + 60);
            this.FormBorderStyle = form.FormBorderStyle;
            if (form.FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                button3.Visible = true;
            }
            if (quiz.Count == 0)
            {
                MessageBox.Show(this, "There appear to be no questions in this quiz.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
            else
            {
                quiz.Shuffle();
                current = quiz.Next();
                textBox1.Text = current.Prompt;
                textBox2.Focus();
                textBox2.Select();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (current.IsCorrect(textBox2.Text))
            {
                correct++;
                score.label6.Text = "Correct!";
                score.label6.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                incorrect++;
                quiz.AddWrong(current);
                score.label6.Text = "Incorrect!";
                score.label6.ForeColor = System.Drawing.Color.Red;
            }
            current = quiz.Next();
            if (current == null)
            {
                Close();
                Finished fin = new Finished(quiz.Wrong(), correct, incorrect, form);
                fin.Show();
            }
            else {
                textBox1.Text = current.Prompt;
                textBox2.Text = "";
                textBox2.Focus();
                textBox2.Select();
                score.updateScore(correct, incorrect, quiz.Current);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to stop the quiz?", "Virtual Flash Cards", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            score.BringToFront();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This feature is not yet avalible in this version.", "Virtual Flash Cards", MessageBoxButtons.OK, MessageBoxIcon.Information); 
            /*
            if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                EasyXml state = new EasyXml(saveFileDialog1.FileName);
                System.IO.File.Copy(Program.flash.path, saveFileDialog1.FileName, true);
                string order = "";
                foreach (int i in questions)
                {
                    order += "|" + i;
                }
                state.CreateChild("Quiz", "Order", order);
            } */
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point endPoint = PointToScreen(e.Location);
                Location = new Point(endPoint.X - startPoint.X,
                                     endPoint.Y - startPoint.Y);
            }
        }

        private bool drag = false;
        private Point startPoint;

        private void Card_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            startPoint = e.Location;
        }

        private void Card_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }
    }
}
