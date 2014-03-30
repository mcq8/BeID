namespace SampleEidUsage
{
    partial class CardDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblFirstNames = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblReader = new System.Windows.Forms.Label();
            this.lblStreet = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFirstNames
            // 
            this.lblFirstNames.AutoSize = true;
            this.lblFirstNames.Location = new System.Drawing.Point(12, 30);
            this.lblFirstNames.Name = "lblFirstNames";
            this.lblFirstNames.Size = new System.Drawing.Size(0, 17);
            this.lblFirstNames.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Reader:";
            // 
            // lblReader
            // 
            this.lblReader.AutoSize = true;
            this.lblReader.Location = new System.Drawing.Point(79, 13);
            this.lblReader.Name = "lblReader";
            this.lblReader.Size = new System.Drawing.Size(0, 17);
            this.lblReader.TabIndex = 2;
            // 
            // lblStreet
            // 
            this.lblStreet.AutoSize = true;
            this.lblStreet.Location = new System.Drawing.Point(12, 47);
            this.lblStreet.Name = "lblStreet";
            this.lblStreet.Size = new System.Drawing.Size(0, 17);
            this.lblStreet.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(188, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(127, 162);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // CardDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 205);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblStreet);
            this.Controls.Add(this.lblReader);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFirstNames);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CardDisplay";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "CardDisplay";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFirstNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblReader;
        private System.Windows.Forms.Label lblStreet;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}