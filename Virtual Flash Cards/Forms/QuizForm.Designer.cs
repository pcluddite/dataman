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
namespace VirtualFlashCards.Forms
{
    partial class QuizForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPrompt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.BackColor = System.Drawing.Color.Transparent;
            this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.Location = new System.Drawing.Point(12, 28);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(401, 213);
            this.lblPrompt.TabIndex = 1;
            this.lblPrompt.Text = "Lorem Ipsum Dolor Sit Amet";
            this.lblPrompt.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblPrompt.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblPrompt_MouseMove);
            this.lblPrompt.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lblPrompt_MouseDoubleClick);
            this.lblPrompt.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblPrompt_MouseDown);
            this.lblPrompt.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblPrompt_MouseUp);
            // 
            // QuizForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(425, 250);
            this.Controls.Add(this.lblPrompt);
            this.Name = "QuizForm";
            this.Text = "Quiz";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;

    }
}