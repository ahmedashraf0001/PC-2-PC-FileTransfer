using System.Drawing.Drawing2D;

namespace WinFormsApp1
{
    partial class Sender
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sender));
            Speed = new Label();
            fileProgressLabel = new Label();
            IpAddressLabel = new Label();
            info = new Label();
            IpAddressBox = new TextBox();
            progressBar = new ProgressBar();
            SelectFilesButton = new Button();
            SendButton = new Button();
            PauseResume = new Button();
            Cancel = new Button();
            Status = new Label();
            BackButton = new Button();
            SuspendLayout();
            // 
            // Speed
            // 
            Speed.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            Speed.ForeColor = Color.FromArgb(160, 160, 160);
            Speed.Location = new Point(30, 90);
            Speed.Name = "Speed";
            Speed.Size = new Size(500, 25);
            Speed.TabIndex = 10;
            Speed.Text = "Speed :";
            // 
            // fileProgressLabel
            // 
            fileProgressLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            fileProgressLabel.ForeColor = Color.FromArgb(160, 160, 160);
            fileProgressLabel.Location = new Point(600, 90);
            fileProgressLabel.Name = "fileProgressLabel";
            fileProgressLabel.Size = new Size(101, 25);
            fileProgressLabel.TabIndex = 10;
            fileProgressLabel.Text = "0 out of 0 files";
            // 
            // IpAddressLabel
            // 
            IpAddressLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            IpAddressLabel.ForeColor = Color.White;
            IpAddressLabel.Location = new Point(30, 30);
            IpAddressLabel.Name = "IpAddressLabel";
            IpAddressLabel.Size = new Size(120, 30);
            IpAddressLabel.TabIndex = 0;
            IpAddressLabel.Text = "IP Address:";
            // 
            // info
            // 
            info.Font = new Font("Segoe UI", 10F);
            info.ForeColor = Color.FromArgb(150, 150, 150);
            info.Location = new Point(30, 150);
            info.Name = "info";
            info.Size = new Size(500, 25);
            info.TabIndex = 4;
            info.Text = "Choose the file you want to send";
            info.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // IpAddressBox
            // 
            IpAddressBox.BackColor = Color.FromArgb(240, 240, 240);
            IpAddressBox.BorderStyle = BorderStyle.FixedSingle;
            IpAddressBox.Font = new Font("Segoe UI", 12F);
            IpAddressBox.ForeColor = Color.FromArgb(50, 50, 50);
            IpAddressBox.Location = new Point(150, 28);
            IpAddressBox.Name = "IpAddressBox";
            IpAddressBox.PlaceholderText = "Enter IP Address";
            IpAddressBox.Size = new Size(550, 29);
            IpAddressBox.TabIndex = 1;
            // 
            // progressBar
            // 
            progressBar.BackColor = Color.FromArgb(200, 200, 200);
            progressBar.ForeColor = Color.FromArgb(0, 120, 215);
            progressBar.Location = new Point(35, 120);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(665, 20);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 3;
            // 
            // SelectFilesButton
            // 
            SelectFilesButton.FlatAppearance.BorderSize = 0;
            SelectFilesButton.FlatStyle = FlatStyle.Flat;
            SelectFilesButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            SelectFilesButton.Location = new Point(30, 180);
            SelectFilesButton.Name = "SelectFilesButton";
            SelectFilesButton.Size = new Size(130, 45);
            SelectFilesButton.TabIndex = 5;
            SelectFilesButton.Text = "Select File";
            SelectFilesButton.Click += SelectFilesButton_Click;
            // 
            // SendButton
            // 
            SendButton.FlatAppearance.BorderSize = 0;
            SendButton.FlatStyle = FlatStyle.Flat;
            SendButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            SendButton.Location = new Point(180, 180);
            SendButton.Name = "SendButton";
            SendButton.Size = new Size(130, 45);
            SendButton.TabIndex = 6;
            SendButton.Text = "Send";
            SendButton.Click += SendButton_Click;
            // 
            // PauseResume
            // 
            PauseResume.FlatAppearance.BorderSize = 0;
            PauseResume.FlatStyle = FlatStyle.Flat;
            PauseResume.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            PauseResume.Location = new Point(330, 180);
            PauseResume.Name = "PauseResume";
            PauseResume.Size = new Size(130, 45);
            PauseResume.TabIndex = 7;
            PauseResume.Text = "Pause";
            PauseResume.Click += PauseResume_Click;
            // 
            // Cancel
            // 
            Cancel.FlatAppearance.BorderSize = 0;
            Cancel.FlatStyle = FlatStyle.Flat;
            Cancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            Cancel.Location = new Point(480, 180);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(100, 45);
            Cancel.TabIndex = 8;
            Cancel.Text = "Cancel";
            Cancel.Click += Cancel_Click;
            // 
            // Status
            // 
            Status.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            Status.ForeColor = Color.White;
            Status.Location = new Point(30, 70);
            Status.Name = "Status";
            Status.Size = new Size(500, 25);
            Status.TabIndex = 2;
            Status.Text = "Status: Ready";
            // 
            // BackButton
            // 
            BackButton.FlatAppearance.BorderSize = 0;
            BackButton.FlatStyle = FlatStyle.Flat;
            BackButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            BackButton.Location = new Point(600, 180);
            BackButton.Name = "BackButton";
            BackButton.Size = new Size(100, 45);
            BackButton.TabIndex = 9;
            BackButton.Text = "Back ->";
            BackButton.Click += BackButton_Click;
            // 
            // Sender
            // 
            BackColor = Color.FromArgb(28, 28, 28);
            ClientSize = new Size(730, 250);
            Controls.Add(IpAddressLabel);
            Controls.Add(fileProgressLabel);
            Controls.Add(IpAddressBox);
            Controls.Add(Status);
            Controls.Add(progressBar);
            Controls.Add(info);
            Controls.Add(SelectFilesButton);
            Controls.Add(SendButton);
            Controls.Add(PauseResume);
            Controls.Add(Cancel);
            Controls.Add(BackButton);
            Controls.Add(Speed);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(10);
            MaximizeBox = false;
            Name = "Sender";
            Padding = new Padding(20);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Transfer Sender";
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }

        private Label IpAddressLabel;
        private Label info;
        private TextBox IpAddressBox;
        private ProgressBar progressBar;
        private Button SelectFilesButton;
        private Button SendButton;
        private Button PauseResume;
        private Label fileProgressLabel;
        private Button Cancel;
        private Button BackButton;
        private Label Status;
        private Label Speed;

    }
}
