using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp2;
namespace WinFormsApp1
{
    public partial class EasyTransfer : Form
    {

        public EasyTransfer()
        {
            InitializeComponent();
        
        }

        private void Sender_Click(object sender, EventArgs e)
        {
            Sender senderForm = new Sender(this);
            senderForm.Show();
            this.Hide();
        }
        private void Reciever_Click(object sender, EventArgs e)
        {
            Receiver recieverForm = new Receiver(this);
            recieverForm.Show();
            this.Hide();
        }
        private void both_Click(object sender, EventArgs e)
        {
            Receiver recieverForm = new Receiver(this);
            Sender senderForm = new Sender(this);

            recieverForm.Show();
            senderForm.Show();

            this.Hide();
        }

    }

}
