using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//sample code reference  https://stackoverflow.com/questions/23734792/c-sharp-export-private-public-rsa-key-from-rsacryptoserviceprovider-to-pem-strin
//https://social.msdn.microsoft.com/Forums/vstudio/en-US/d7e2ccea-4bea-4f22-890b-7e48c267657f/creating-a-x509-certificate-from-a-rsa-private-key-in-pem-file?forum=csharpgeneral

namespace RSAWindowsFormsApplication
{
    public partial class Form1 : Form
    {
        string strPublicKey = "";
        string strPrivateKey = "";
        RSACryptoServiceProvider rsaCryptoServiceProvider;
        RSAParameters rsaParameters;
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            rsaCryptoServiceProvider = new RSACryptoServiceProvider();
            rsaParameters = rsaCryptoServiceProvider.ExportParameters(true);
            strPublicKey = rsaCryptoServiceProvider.ToXmlString(false);
            strPrivateKey = rsaCryptoServiceProvider.ToXmlString(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string encryptString = encrypt(strPublicKey, textBoxOrigional.Text.Trim());

            textBoxEncrypt.Text = encryptString;
            textBoxKeyInfo.Text = strPublicKey;
            textBoxKeyInfo.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(strPublicKey));
            textBoxKeyInfo.Text = ExportPublicKeyToPEMFormat(rsaCryptoServiceProvider);
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
            textBoxKeyInfo.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(strPrivateKey));
            
            textBoxKeyInfo.Text = ExportPrivateKey(rsaCryptoServiceProvider);
        }


        //public void ConvertPKCS8(string privXmlString, string privPkcs8Filename)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string line;

        //    var xmlKey = privXmlString;
        //    var rsa = RSA.Create();
        //    rsa.FromXmlString(xmlKey);
        //    var bcKeyPair = rsa.GetRsaKeyPair(rsa);
        //    var pkcs8Gen = new Pkcs8Generator(bcKeyPair.Private);
        //    var pemObj = pkcs8Gen.Generate();
        //    var pkcs8Out = new StreamWriter(privPkcs8Filename, false);
        //    var pemWriter = new PemWriter(pkcs8Out);
        //    pemWriter.WriteObject(pemObj);
        //    pkcs8Out.Close();
        //}
        #region pkcs8 format converter
        //public class DotNetToPkcs8Pem
        //{

        //    public static void Convert(string privXmlFilename, string privPkcs8Filename)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        string line;
        //        var xmlIn = new StreamReader(privXmlFilename);
        //        while ((line = xmlIn.ReadLine()) != null)
        //        {
        //            sb.Append(line);
        //        }
        //        var xmlKey = sb.ToString();
        //        var rsa = RSA.Create();
        //        rsa.FromXmlString(xmlKey);
        //        var bcKeyPair = DotNetUtilities.GetRsaKeyPair(rsa);
        //        var pkcs8Gen = new Pkcs8Generator(bcKeyPair.Private);
        //        var pemObj = pkcs8Gen.Generate();
        //        var pkcs8Out = new StreamWriter(privPkcs8Filename, false);
        //        var pemWriter = new PemWriter(pkcs8Out);
        //        pemWriter.WriteObject(pemObj);
        //        pkcs8Out.Close();
        //    }

        //    public static void Main(string[] args)
        //    {
        //        var xmlFile = "exportedDotNetPrivKey.xml";
        //        var pkcs8File = "privkey.pk8";
        //        Convert(xmlFile, pkcs8File);
        //    }
        //}
        #endregion


        private static string ExportPrivateKey(RSACryptoServiceProvider csp)//, TextWriter outputStream)
        {
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                    
                }
                

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                //outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                
                //// Output as Base64 with lines chopped at 64 characters
                //for (var i = 0; i < base64.Length; i += 64)
                //{
                //    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                //}
                //outputStream.WriteLine("-----END RSA PRIVATE KEY-----");

                return new string(base64);
                
            }
        }

        public static String ExportPublicKeyToPEMFormat(RSACryptoServiceProvider csp)
        {
            TextWriter outputStream = new StringWriter();

            var parameters = csp.ExportParameters(false);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);

                    //All Parameter Must Have Value so Set Other Parameter Value Whit Invalid Data  (for keeping Key Structure  use "parameters.Exponent" value for invalid data)
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.D
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.P
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.Q
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DP
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DQ
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.InverseQ

                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine("-----BEGIN PUBLIC KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END PUBLIC KEY-----");

                return outputStream.ToString();

            }
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }
}
