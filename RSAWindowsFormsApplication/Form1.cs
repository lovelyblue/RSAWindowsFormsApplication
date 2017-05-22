using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSAWindowsFormsApplication
{
    public partial class Form1 : Form
    {
        string strPublicKey = "";
        string strPrivateKey = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider();
            RSAParameters rsaParameters = rsaCryptoServiceProvider.ExportParameters(true);
            strPublicKey = rsaCryptoServiceProvider.ToXmlString(false);
            strPrivateKey = rsaCryptoServiceProvider.ToXmlString(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string encryptString = encrypt(strPublicKey, textBoxOrigional.Text.Trim());

            textBoxEncrypt.Text = encryptString;
            textBoxKeyInfo.Text = strPublicKey;
        }

        private string encrypt(string publicKey, string context)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.FromXmlString(publicKey);
            var encryptString = Convert.ToBase64String(rsaProvider.Encrypt(Encoding.UTF8.GetBytes(context),false));
            return encryptString;
        }

        private string decrypt(string privateKey, string context)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var decryptString = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(context), false));
            return decryptString;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBoxDecrypt.Text = decrypt(strPrivateKey, textBoxEncrypt.Text.Trim());
            textBoxKeyInfo.Text = strPrivateKey;
        }
    }
}
