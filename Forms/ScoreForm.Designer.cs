namespace VirtualFlashCards.Forms
{
    partial class ScoreForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScoreForm));
            this.lblRight = new System.Windows.Forms.Label();
            this.lblWrong = new System.Windows.Forms.Label();
            this.lblPct = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblGrade = new System.Windows.Forms.Label();
            this.lblLatest = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblRight
            // 
            this.lblRight.AutoSize = true;
            this.lblRight.Location = new System.Drawing.Point(32, 10);
            this.lblRight.Name = "lblRight";
            this.lblRight.Size = new System.Drawing.Size(44, 13);
            this.lblRight.TabIndex = 0;
            this.lblRight.Text = "Correct:";
            // 
            // lblWrong
            // 
            this.lblWrong.AutoSize = true;
            this.lblWrong.Location = new System.Drawing.Point(24, 23);
            this.lblWrong.Name = "lblWrong";
            this.lblWrong.Size = new System.Drawing.Size(52, 13);
            this.lblWrong.TabIndex = 1;
            this.lblWrong.Text = "Incorrect:";
            // 
            // lblPct
            // 
            this.lblPct.AutoSize = true;
            this.lblPct.Location = new System.Drawing.Point(11, 36);
            this.lblPct.Name = "lblPct";
            this.lblPct.Size = new System.Drawing.Size(65, 13);
            this.lblPct.TabIndex = 2;
            this.lblPct.Text = "Percentage:";
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(37, 58);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(148, 13);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "You are currently on question:";
            // 
            // lblGrade
            // 
            this.lblGrade.AutoSize = true;
            this.lblGrade.Location = new System.Drawing.Point(152, 23);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(76, 13);
            this.lblGrade.TabIndex = 4;
            this.lblGrade.Text = "Current Grade:";
            // 
            // lblLatest
            // 
            this.lblLatest.AutoSize = true;
            this.lblLatest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLatest.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblLatest.Location = new System.Drawing.Point(161, 10);
            this.lblLatest.Name = "lblLatest";
            this.lblLatest.Size = new System.Drawing.Size(0, 13);
            this.lblLatest.TabIndex = 5;
            // 
            // ScoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 85);
            this.ControlBox = false;
            this.Controls.Add(this.lblLatest);
            this.Controls.Add(this.lblGrade);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblPct);
            this.Controls.Add(this.lblWrong);
            this.Controls.Add(this.lblRight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScoreForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Current Statistics";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLatest;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblRight;
        private System.Windows.Forms.Label lblWrong;
        private System.Windows.Forms.Label lblPct;
        private System.Windows.Forms.Label lblGrade;
    }
}