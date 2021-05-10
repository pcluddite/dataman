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
    public partial class Finished : Form
    {
        public Quiz wrongAns;
        public int correct;
        public int incorrect;
        public MainForm form1;

        public Finished(Quiz WrongAns, int cor, int incor, MainForm frm1)
        {
            form1 = frm1;
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
                WrongAns wAnswers = new WrongAns(this, form1);
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
