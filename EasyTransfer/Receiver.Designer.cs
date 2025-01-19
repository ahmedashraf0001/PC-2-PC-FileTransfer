namespace WinFormsApp2
{
    partial class Receiver
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Receiver));
            Status = new Label();
            fileProgressLabel = new Label();
            BackButton = new Button();
            btnStartListening = new Button();
            SavePath = new Button();
            progressBar = new ProgressBar();
            pathlabel = new Label();
            ip_address = new Label();
            speedlabel = new Label();
            Cancel = new Button();
            SuspendLayout();
            // 
            // Status
            // 
            Status.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Status.AutoSize = true;
            Status.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            Status.ForeColor = Color.White;
            Status.Location = new Point(35, 15);
            Status.Name = "Status";
            Status.Size = new Size(269, 32);
            Status.TabIndex = 0;
            Status.Text = "Status: Not Connected";
            Status.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // fileProgressLabel
            // 
            fileProgressLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            fileProgressLabel.ForeColor = Color.FromArgb(160, 160, 160);
            fileProgressLabel.Location = new Point(300, 56);
            fileProgressLabel.Name = "fileProgressLabel";
            fileProgressLabel.Size = new Size(100, 25);
            fileProgressLabel.TabIndex = 0;
            fileProgressLabel.Text = "0 out of 0 files";
            // 
            // BackButton
            // 
            BackButton.BackColor = Color.FromArgb(0, 120, 215);
            BackButton.FlatAppearance.BorderSize = 0;
            BackButton.FlatStyle = FlatStyle.Flat;
            BackButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            BackButton.ForeColor = Color.White;
            BackButton.Location = new Point(40, 317);
            BackButton.Name = "BackButton";
            BackButton.Size = new Size(360, 45);
            BackButton.TabIndex = 4;
            BackButton.Text = "Back ->";
            BackButton.UseVisualStyleBackColor = false;
            BackButton.Click += BackButton_Click;
            // 
            // btnStartListening
            // 
            btnStartListening.BackColor = Color.FromArgb(50, 50, 50);
            btnStartListening.FlatAppearance.BorderSize = 0;
            btnStartListening.FlatStyle = FlatStyle.Flat;
            btnStartListening.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnStartListening.ForeColor = Color.White;
            btnStartListening.Location = new Point(40, 207);
            btnStartListening.Name = "btnStartListening";
            btnStartListening.Size = new Size(360, 45);
            btnStartListening.TabIndex = 4;
            btnStartListening.Text = "Start Listening";
            btnStartListening.UseVisualStyleBackColor = false;
            btnStartListening.Click += btnStartListening_Click;
            // 
            // SavePath
            // 
            SavePath.BackColor = Color.FromArgb(50, 50, 50);
            SavePath.FlatAppearance.BorderSize = 0;
            SavePath.FlatStyle = FlatStyle.Flat;
            SavePath.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            SavePath.ForeColor = Color.White;
            SavePath.Location = new Point(40, 152);
            SavePath.Name = "SavePath";
            SavePath.Size = new Size(360, 45);
            SavePath.TabIndex = 3;
            SavePath.Text = "Select Path";
            SavePath.UseVisualStyleBackColor = false;
            SavePath.Click += SavePath_Click;
            // 
            // progressBar
            // 
            progressBar.ForeColor = Color.FromArgb(85, 170, 85);
            progressBar.Location = new Point(45, 84);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(355, 25);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 1;
            // 
            // pathlabel
            // 
            pathlabel.AutoSize = true;
            pathlabel.Font = new Font("Segoe UI", 12F);
            pathlabel.ForeColor = Color.LightGray;
            pathlabel.Location = new Point(40, 372);
            pathlabel.Name = "pathlabel";
            pathlabel.Size = new Size(172, 21);
            pathlabel.TabIndex = 5;
            pathlabel.Text = "Path is not selected yet!";
            pathlabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ip_address
            // 
            ip_address.AutoSize = true;
            ip_address.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            ip_address.ForeColor = Color.White;
            ip_address.Location = new Point(40, 115);
            ip_address.Name = "ip_address";
            ip_address.Size = new Size(152, 25);
            ip_address.TabIndex = 2;
            ip_address.Text = "IP Address: N/A";
            // 
            // speedlabel
            // 
            speedlabel.AutoSize = true;
            speedlabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            speedlabel.ForeColor = Color.FromArgb(160, 160, 160);
            speedlabel.Location = new Point(40, 53);
            speedlabel.Name = "speedlabel";
            speedlabel.Size = new Size(53, 19);
            speedlabel.TabIndex = 6;
            speedlabel.Text = "Speed :";
            // 
            // Cancel
            // 
            Cancel.BackColor = Color.FromArgb(50, 50, 50);
            Cancel.FlatAppearance.BorderSize = 0;
            Cancel.FlatStyle = FlatStyle.Flat;
            Cancel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            Cancel.ForeColor = Color.White;
            Cancel.Location = new Point(40, 262);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(360, 45);
            Cancel.TabIndex = 7;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = false;
            Cancel.Click += Cancel_Click;
            // 
            // Receiver
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 28, 28);
            ClientSize = new Size(450, 423);
            Controls.Add(Cancel);
            Controls.Add(fileProgressLabel);
            Controls.Add(speedlabel);
            Controls.Add(ip_address);
            Controls.Add(pathlabel);
            Controls.Add(btnStartListening);
            Controls.Add(SavePath);
            Controls.Add(progressBar);
            Controls.Add(Status);
            Controls.Add(BackButton);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Receiver";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Transfer Receiver";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }

        private Label Status;
        private Button btnStartListening;
        private Button SavePath;
        private Button BackButton;
        private Label fileProgressLabel;
        private ProgressBar progressBar;
        private Label pathlabel;
        private Label ip_address;
        private Label label1;
        private Label speedlabel;
        private Button Cancel;
    }
}
