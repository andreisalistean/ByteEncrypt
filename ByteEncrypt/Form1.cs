using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace ByteEncrypt
{
    public partial class Form1 : Form
    {
        private string text,terminator;
        public Form1()
        {
            InitializeComponent();
            text = "";
            terminator = "$finish$";
        }

        bool check_terminator()
        {
            if (text.Contains(terminator))
                return true;
            else return false;
        }

        

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Multiselect = false;
            if (od.ShowDialog() == DialogResult.OK)
            {
                txtPathMask.Text = od.FileName;
            }
        }

        

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if(!File.Exists(txtPathMask.Text))
            {
                MessageBox.Show("Fisierul imagine nu exista");
                return;
            }
            if (!File.Exists(txtText.Text))
            {
                MessageBox.Show("Fisierul imagine nu exista");
                return;
            }

            try
            {
                Bitmap img = new Bitmap(txtPathMask.Text.ToString());
                
                byte[] a = File.ReadAllBytes(txtText.Text.ToString());
                byte[] b = Encoding.UTF8.GetBytes(terminator);
                byte[] message = new byte[a.Length+b.Length];
                Array.Copy(a, 0, message, 0, a.Length);
                Array.Copy(b, 0, message, a.Length, b.Length);

                byte masca = 0x03,rez,m2;

                Color pix;
                byte p_maskR,p_maskG,p_maskB;

                int indice_message = 0;
                short bit=0;
            
                for(int x=0;x<img.Width;++x)
                {
                    if (indice_message >= message.Length)
                        break;
                    for (int y=0;y<img.Height;++y)
                    {
                        if (indice_message >= message.Length)
                            break;
                        pix = img.GetPixel(x,y);

                        p_maskR = pix.R;
                        p_maskG = pix.G;
                        p_maskB = pix.B;

                        for(short c=0;c<3;++c)
                        {
                            if (indice_message >= message.Length)
                                break;
                            m2 = masca;
                            m2=(byte)(m2 << bit);
                            rez = (byte)(message[indice_message] & m2);
                            rez = (byte)(rez >> bit);
                            rez = (byte)(rez | 0xFC);
                            if (c==0)//R
                            {
                                p_maskR = (byte)(p_maskR | 0x03);
                                p_maskR = (byte)(p_maskR & rez);
                            }
                            else if(c==1)//G
                            {
                                p_maskG = (byte)(p_maskG | 0x03);
                                p_maskG = (byte)(p_maskG & rez);
                            }
                            else if(c==2)//B
                            {
                                p_maskB = (byte)(p_maskB | 0x03);
                                p_maskB = (byte)(p_maskB & rez);
                            }
                            bit += 2;
                            if(bit==8)
                            {
                                bit = 0;
                                ++indice_message;
                            }
                        }
                        img.SetPixel(x, y, Color.FromArgb(p_maskR, p_maskG, p_maskB));
                    }
                }
                String fileExt = Path.GetExtension(txtPathMask.Text);
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "Files (" + fileExt + ") | " + fileExt;
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    img.Save(sd.FileName);

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Fisierul nu s-a putut deschide"+ex.ToString()+ex.Source);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            text = "";
            if (!File.Exists(txtPathMask.Text))
            {
                MessageBox.Show("Fisierul nu exista");
                return;
            }
            try
            {
                Bitmap img = new Bitmap(txtPathMask.Text);

                byte masca = 0x03, carry=0x00,rez=0x00;

                Color pix;

                short bit = 0;
                //char litera;

                for(int x=0;x<img.Width;++x)
                {
                    if (check_terminator())
                        break;
                    for (int y=0;y<img.Height;++y)
                    {
                        if (check_terminator())
                            break;
                        pix = img.GetPixel(x, y);
                        for(short c=0;c<3;++c)
                        {
                            if (check_terminator())
                                break;
                            if(c==0)//R
                            {
                                rez = (byte)(pix.R & masca);
                            }
                            else if(c==1)//G
                            {
                                rez = (byte)(pix.G & masca);
                            }
                            else if(c==2)//B
                            {
                                rez = (byte)(pix.B & masca);
                            }
                            rez = (byte)(rez << bit);
                            carry = (byte)(carry | rez);
                            bit += 2;
                            if(bit==8)
                            {
                                bit = 0;
                                text += (char)carry;
                                carry = 0x00;
                            }
                        }
                    }
                }
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "Files ( txt ) | txt";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sd.FileName, text);

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Fisierul nu s-a putut deschide" + ex.ToString() + ex.Source);
            }
        }

        private void buttonTxtFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Multiselect = false;
            if (od.ShowDialog() == DialogResult.OK)
            {
                txtText.Text = od.FileName;
            }
        }
    }
}
