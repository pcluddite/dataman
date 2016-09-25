using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Virtual_Flash_Cards
{
    public partial class Form1 : Form
    {
        bool doOnce = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);
                Hide();
                Start.Card card = new Virtual_Flash_Cards.Start.Card(Quiz.FromXml(doc.SelectSingleNode("quiz")), this);
                card.Show();
            }
        }

        void Form1_Activated(object sender, System.EventArgs e)
        {
            if (!doOnce)
            {
                doOnce = true;
                string[] args = Environment.GetCommandLineArgs();
                if (args.GetUpperBound(0) == 2)
                {
                    if (System.IO.File.Exists(args[1]))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(args[1]);
                        Quiz q = Quiz.FromXml(doc.SelectSingleNode("quiz"));
                        Hide();
                        switch(args[2].ToLower()) 
                        {
                            case "/run":
                                Start.Card card = new Virtual_Flash_Cards.Start.Card(q, this); 
                                card.Show();
                                break;
                            case "/edit":
                                Create.Main main = new Virtual_Flash_Cards.Create.Main(q, this);
                                main.Show();
                                break;
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            Create.Main main = new Virtual_Flash_Cards.Create.Main(null, this);
            main.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);
                Hide();
                Create.Main main = new Virtual_Flash_Cards.Create.Main(Quiz.FromXml(doc.SelectSingleNode("quiz")), this);
                main.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
