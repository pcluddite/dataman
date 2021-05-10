using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace VirtualFlashCards.Forms
{
    public partial class Control : Form
    {
        Main main;
        public Control(Main mainForm)
        {
            main = mainForm;
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int index = (int)numericUpDown1.Value - 1;
            if (index > 0 && index < main.quiz.Count)
            {
                main.current = index;
                main.showCurrent();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!main.saved)
            {
                DialogResult r = MessageBox.Show(this, "You have not yet saved these flash cards. Would you like to save them now?", "Virtual Flash Cards", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    if (main.saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }
                    else
                    {
                        main.saveCurrent();
                        XmlDocument doc = new XmlDocument();
                        doc.AppendChild(main.quiz.ToXml(doc));
                        doc.Save(main.saveFileDialog1.FileName);
                    }
                }
                if (r == DialogResult.Cancel)
                {
                    return;
                }
            }
            main.Close();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (main.saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                main.saveCurrent();
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(main.quiz.ToXml(doc));
                doc.Save(main.saveFileDialog1.FileName);
                main.saved = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (main.saveFileDialog1.FileName.Equals(""))
            {
                button2_Click((object)"me", EventArgs.Empty);
            }
            else
            {

                XmlDocument doc = new XmlDocument();
                doc.AppendChild(main.quiz.ToXml(doc));
                doc.Save(main.saveFileDialog1.FileName);
            }
            main.saved = true;
        }
    }
}
