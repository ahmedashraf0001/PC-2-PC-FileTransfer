using Reciever.Services;
using Share_App.Error_Handling;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Receiver : Form
    {
        private string savepath;
        private ReceiverFileTransferService service;
        private ExceptionHelper ExceptionHelper;
        private bool isRunning = true;
        
        Form main;

        private readonly Color enabledPrimaryColor = Color.FromArgb(50, 50, 50);
        private readonly Color disabledColor = Color.FromArgb(35, 35, 35);
        private readonly Color disabledTextColor = Color.FromArgb(150, 150, 150);

        public void EnableStartListeningButton()
        {
            btnStartListening.Enabled = true;
            btnStartListening.BackColor = enabledPrimaryColor;
            btnStartListening.ForeColor = Color.White;
        }
        public void DisableStartListeningButton()
        {
            btnStartListening.Enabled = false;
            btnStartListening.BackColor = disabledColor;
            btnStartListening.ForeColor = disabledTextColor;
        }

        public void EnableStopListeningButton()
        {
            btnStartListening.Enabled = true;
            btnStartListening.Text = "Stop Listening";
            btnStartListening.BackColor = Color.Red;
            btnStartListening.ForeColor = Color.White;
        }
        public void DisableStopListeningButton()
        {
            btnStartListening.Enabled = false;
            btnStartListening.BackColor = disabledColor;
            btnStartListening.ForeColor = disabledTextColor;
        }

        public void EnableCancelButton()
        {
            Cancel.Enabled = true;
            Cancel.BackColor = Color.Red;
            Cancel.ForeColor = Color.White;
        }
        public void DisableCancelButton()
        {
            Cancel.Enabled = false;
            Cancel.BackColor = disabledColor;
            Cancel.ForeColor = disabledTextColor;
        }

        public Receiver()
        {
            InitializeComponent();
            InitializeUI();
        }
        public Receiver(Form form)
        {
            main = form;
            InitializeComponent();
            InitializeUI();
        }
        private void InitializeUI()
        {
            progressBar.Visible = false;
            speedlabel.Visible = false;
            fileProgressLabel.Visible = false;
            DisableStartListeningButton();
            DisableCancelButton();
            UpdateStatus("Not Connected!");
            ShowIpAddress();
        }

        private ReceiverFileTransferService InitializeServiceObject(string SaveFile)
        {
  
                var progress = new Progress<int>(value => progressBar.Value = value);
                var status = new Progress<string>(text => UpdateStatus(text));
                var visible = new Progress<bool>(boolean => progressBar.Visible = boolean);
                var speed = new Progress<string>(speed => UpdateSpeed(speed));
                var filetransfared = new Progress<string>(file => fileProgressLabel.Text = file);

                ExceptionHelper = new ExceptionHelper();

                return new ReceiverFileTransferService(this,SaveFile, 8080, 8081, status, progress, visible, speed, filetransfared);

        }
        private async void btnStartListening_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(savepath))
            {
                MessageBox.Show("No path selected.");
                return;
            }
            try
            {

                if (isRunning == true)
                {
                    isRunning = false;
                    
                    btnStartListening.Text = "Stop Listening";
                    btnStartListening.BackColor = Color.Red;
                    SavePath.Enabled = false;
                    speedlabel.Visible = true;
                    fileProgressLabel.Visible = true;

                    progressBar.Maximum = 100;
                    progressBar.Value = 0;
                    service = InitializeServiceObject(savepath);
                    
                        await service.Start();
                    
                }
                else
                {
                    
                    isRunning = true;
                    btnStartListening.Text = "Start Listening";
                    btnStartListening.BackColor = Color.FromArgb(50, 50, 50);
                    service.Dispose();
                    progressBar.Value = 0;
                    progressBar.Visible = false;
                    SavePath.Enabled = true;

                    speedlabel.Visible = false;
                    fileProgressLabel.Visible = false;
                    
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "main receiver ");
            }
            finally
            {
                if (service.Configurations.ReadTransferState(isCancelled: true))
                {
                    Status.Text = "Status: Not Connected!";
                    progressBar.Visible = false;
                }
            }
        }
        private async void Cancel_Click(object sender, EventArgs e)
        {
            await service.Cancel();
            DisableCancelButton();
        }
        private void SavePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    savepath = folderBrowserDialog.SelectedPath;
                    pathlabel.Text = $"Path: {savepath}";
                }
                if (savepath != null)
                {
                    EnableStartListeningButton();
                }
                else
                {
                    MessageBox.Show("No file selected.", "Files Selection",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
                }
            }
        }
        private void BackButton_Click(object sender, EventArgs e)
        {
            main.Show();
            this.Hide();
        }
        private void UpdateStatus(string message)
        {
            Status.Text = $"Status: {message}";
        }
        private void UpdateSpeed(string message)
        {
            speedlabel.Text = $"Speed : {message}";
        }
        private void ShowIpAddress()
        {
            try
            {
                var ip = ReceiverFileTransferService.GetLocalIPAddress();
                ip_address.Text = "IP Address: " + ip.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ip_address.Text = "IP Address: N/A";
            }
        }


    }
}
