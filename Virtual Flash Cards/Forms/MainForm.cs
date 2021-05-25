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
    public partial class MainForm : CardForm
    {
        private AppContext context;

        public MainForm(AppContext context)
        {
            this.context = context;
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (openQuizDialog.ShowDialog() == DialogResult.Cancel)
                return;
            context.StartQuiz(openQuizDialog.FileName);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            context.NewQuiz();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (openQuizDialog.ShowDialog() == DialogResult.Cancel)
                return;
            context.EditQuiz(openQuizDialog.FileName);
        }
    }
}
