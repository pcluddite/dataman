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
    public partial class EditForm : Form
    {
        private AppContext context;

        public EditForm(AppContext context)
        {
            this.context = context;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            comboAnswerType.SelectedIndex = 1;
        }

        private void comboAnswerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAnswerType.SelectedItem == "Text")
            {
                txtAnswer.Visible = true;
                btnAnswerList.Visible = false;
            }
            else
            {
                txtAnswer.Visible = false;
                btnAnswerList.Visible = true;
            }
        }
    }
}
