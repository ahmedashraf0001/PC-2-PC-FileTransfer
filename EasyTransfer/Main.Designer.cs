namespace WinFormsApp1
{
    partial class EasyTransfer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EasyTransfer));
            Sender = new Button();
            //both = new Button();
            reciever = new Button();
            Title = new Label();
            SuspendLayout();
            // 
            // Sender
            // 
            Sender.BackColor = Color.FromArgb(50, 50, 50);
            Sender.FlatAppearance.BorderSize = 0;
            Sender.FlatStyle = FlatStyle.Flat;
            Sender.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            Sender.ForeColor = Color.White;
            Sender.Location = new Point(160, 140);
            Sender.Name = "Sender";
            Sender.Size = new Size(130, 45);
            Sender.TabIndex = 0;
            Sender.Text = "Sender";
            Sender.UseVisualStyleBackColor = true;
            Sender.Click += Sender_Click;
            // 
            // reciever
            // 
            reciever.BackColor = Color.FromArgb(50, 50, 50);
            reciever.FlatAppearance.BorderSize = 0;
            reciever.FlatStyle = FlatStyle.Flat;
            reciever.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            reciever.ForeColor = Color.White;
            reciever.Location = new Point(160, 200);
            reciever.Name = "reciever";
            reciever.Size = new Size(130, 45);
            reciever.TabIndex = 4;
            reciever.Text = "Receiver";
            reciever.UseVisualStyleBackColor = true;
            reciever.Click += Reciever_Click;
            // 
            // both
            // 
            //both.BackColor = Color.FromArgb(50, 50, 50);
            //both.FlatAppearance.BorderSize = 0;
            //both.FlatStyle = FlatStyle.Flat;
            //both.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            //both.ForeColor = Color.White;
            //both.Location = new Point(160, 260);
            //both.Name = "both";
            //both.Size = new Size(130, 45);
            //both.TabIndex = 4;
            //both.Text = "both";
            //both.UseVisualStyleBackColor = true;
            //both.Click += both_Click;
            // 
            // Title
            // 
            Title.AutoSize = true;
            Title.FlatStyle = FlatStyle.Flat;
            Title.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            Title.ForeColor = Color.White;
            Title.Location = new Point(137, 60);
            Title.Name = "Title";
            Title.Size = new Size(176, 37);
            Title.TabIndex = 5;
            Title.Text = "Select Mode";
            // 
            // EasyTransfer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 28, 28);
            ClientSize = new Size(450, 340);
            Controls.Add(Title);
            Controls.Add(Sender);
            Controls.Add(reciever);
            //Controls.Add(both);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "EasyTransfer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "EasyTransfer";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Sender;
        private Button reciever;
        //private Button both;
        private Label Title;
    }
}