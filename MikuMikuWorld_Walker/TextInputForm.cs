using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    public partial class TextInputForm : Form
    {
        public bool EnterToClose { get; set; }

        public TextInputForm(string title, int maxlength, bool enterToClose = true)
        {
            InitializeComponent();
            Text = title;
            EnterToClose = enterToClose;
            textBox.MaxLength = maxlength;
        }

        public event EventHandler<string> TextInputed = delegate { };

        private string inputText;
        public string InputText
        {
            get { return inputText; }
            set { textBox.Text = value; inputText = value; }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (string.IsNullOrWhiteSpace(textBox.Text)) return;
                TextInputed(this, textBox.Text);
                inputText = textBox.Text;
                textBox.Text = null;
                DialogResult = DialogResult.OK;
                if (EnterToClose) Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                textBox.Text = null;
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
