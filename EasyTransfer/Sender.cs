using Sender.Services;
using Share_App.Error_Handling;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Sender : Form
    {
        private string[] filepath;
        private FileTransferSenderService service;
        private ExceptionHelper ExceptionHelper;
        private bool isPaused;
        Form main;

        // Define Colors
        private readonly Color enabledPrimaryColor = Color.FromArgb(50, 50, 50);
        private readonly Color enabledCancelColor = Color.Red;
        private readonly Color disabledColor = Color.FromArgb(35, 35, 35);
        private readonly Color disabledTextColor = Color.FromArgb(150, 150, 150);

        public Sender()
        {
            InitializeComponent();
            InitializeUI();
        }
        public Sender( Form form)
        {
            InitializeComponent();
            InitializeUI();
            main = form;
        }

        private void InitializeUI()
        {
            info.AutoSize = true;
            progressBar.Visible = false;
            Speed.Visible = false;

            EnablePauseButton();
            DisablePauseButton();
            DisableCancelButton();
            if (filepath == null)
            {
                DisableSendButton();
                info.Text = "Choose the file you want to send";
            }
            else
            {
                EnableSendButton();
            }
            EnableBackButton();
            EnableSelectFilesButton();
            UpdateStatus("Not Connected!");
        }
        #region style
        private void EnablePauseButton()
        {
            PauseResume.Enabled = true;
            PauseResume.BackColor = enabledPrimaryColor;
            PauseResume.ForeColor = Color.White;
        }
        private void EnableBackButton()
        {
            BackButton.Enabled = true;
            BackButton.BackColor = Color.FromArgb(0, 120, 215);
            BackButton.ForeColor = Color.White;
        }

        private void DisablePauseButton()
        {
            PauseResume.Enabled = false;
            PauseResume.BackColor = disabledColor;
            PauseResume.ForeColor = disabledTextColor;
        }

        private void EnableCancelButton()
        {
            Cancel.Enabled = true;
            Cancel.BackColor = enabledCancelColor;
            Cancel.ForeColor = Color.White;
        }

        private void DisableCancelButton()
        {
            Cancel.Enabled = false;
            Cancel.BackColor = disabledColor;
            Cancel.ForeColor = disabledTextColor;
        }

        private void EnableSendButton()
        {
            SendButton.Enabled = true;
            SendButton.BackColor = enabledPrimaryColor;
            SendButton.ForeColor = Color.White;
        }

        private void DisableSendButton()
        {
            SendButton.Enabled = false;
            SendButton.BackColor = disabledColor;
            SendButton.ForeColor = disabledTextColor;
        }

        private void EnableSelectFilesButton()
        {
            SelectFilesButton.Enabled = true;
            SelectFilesButton.BackColor = enabledPrimaryColor;
            SelectFilesButton.ForeColor = Color.White;
        }

        private void DisableSelectFilesButton()
        {
            SelectFilesButton.Enabled = false;
            SelectFilesButton.BackColor = disabledColor;
            SelectFilesButton.ForeColor = disabledTextColor;
        }
        #endregion 
        private void UpdateSpeed(string message)
        {
            Speed.Text = $"Speed : {message}";
        }
        private FileTransferSenderService InitializeServiceObject(string[] _filepath)
        {

                var progress = new Progress<int>(value => progressBar.Value = value);
                var status = new Progress<string>(text => UpdateStatus(text));
                var visible = new Progress<bool>(boolean => progressBar.Visible = boolean);
                var speed = new Progress<string>(speed => UpdateSpeed(speed));
                var fileProgress = new Progress<string>(prog => fileProgressLabel.Text = prog);

                ExceptionHelper = new ExceptionHelper();

               return new FileTransferSenderService(_filepath, IpAddressBox.Text, 8080, 8081, progress, visible, status, speed, fileProgress);
            

        }
        private async void SendButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                EnableCancelButton();
                EnablePauseButton();
                DisableSelectFilesButton();
                DisableSendButton();

                Speed.Visible = true;
                progressBar.Visible = true;
                progressBar.Maximum = 100;
                progressBar.Value = 0;
                using (service = InitializeServiceObject(filepath))
                {
                    await service.Start();
                }
               
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "main Sender ");
            }
            finally
            {
                InitializeUI();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(IpAddressBox.Text) ||
                !IPAddress.TryParse(IpAddressBox.Text, out _))
            {
                MessageBox.Show("Please enter a valid IP address.",
                                "Invalid Input",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return false;
            }

            if (filepath == null || filepath.Length == 0)
            {
                MessageBox.Show("No file selected.",
                                "Files Selection",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return false;
            }

            return true;
        }


        private void SelectFilesButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filepath = openFileDialog.FileNames;
                    info.Text = $"Selected files: {filepath[0]}";
                    if (filepath.Length > 1)
                    {
                        info.Text = $"Selected files: Multiple Files";
                    }
                }
                if (filepath == null)
                {
                    MessageBox.Show("No file selected.", "Files Selection",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
                else
                {
                    EnableSendButton();
                }
            }
        }
        private void BackButton_Click(object sender, EventArgs e)
        {
            main.Show();
            this.Hide();
        }
        private async void PauseResume_Click(object sender, EventArgs e)
        {
            if (isPaused == false)
            {
                SetPause();
                await service.Pause();
            }
            else
            {
                SetResume();
                await service.Resume();
            }
        }

        private void SetPause()
        {
            isPaused = true;
            PauseResume.Text = "Resume";
            UpdateStatus("Paused!");
            
        }
        private void SetResume()
        {
            isPaused = false;
            PauseResume.Text = "Pause";
            UpdateStatus("Sending...");
        }

        private async void Cancel_Click(object sender, EventArgs e)
        {
            isPaused = false;
            PauseResume.Text = "Pause";
            await service.Cancel();
            ResetFileSelection();
        }

        private void ResetFileSelection()
        {
            filepath = null;
            info.Text = "No path selected!";
        }

        private void UpdateStatus(string message)
        {
            Status.Text = $"Status: {message}";
        }
    }
}
