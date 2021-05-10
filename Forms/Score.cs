using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Virtual_Flash_Cards.Forms
{
    public partial class Score : Form
    {
        public Score()
        {
            InitializeComponent();
        }

        private void Score_Load(object sender, EventArgs e)
        {
            
        }

        public void updateScore(int correct, int incorrect, int current)
        {
            label1.Text = "Correct: " + correct;
            label2.Text = "Incorrect: " + incorrect;
            if (correct + incorrect > 0)
            {
                decimal p = Math.Round(((decimal)correct / (decimal)(incorrect + correct)) * 100, 2);
                label3.Text = "Percentage: " + p + "%";
            }
            label4.Text = "You are currently on question: " + current;
        }
    }
}
