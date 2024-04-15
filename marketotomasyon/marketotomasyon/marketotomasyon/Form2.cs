using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace marketotomasyon
{
    public partial class Form2 : Form
    {
        public Form2(DataTable satinAlinanUrunler, string label7Text)
        {
            InitializeComponent();


            richTextBox1.Text += "---------------------------------------------" + "\n";

            richTextBox1.Text += "Satın Alınan Ürünler:\n";
            foreach (DataRow row in satinAlinanUrunler.Rows)
            {
                richTextBox1.Text += row["Ürün Adı"].ToString() + " - " + row["Fiyat"].ToString() + " TL - " + row["Alınan Miktar"].ToString() + " adet\n";
            }
            richTextBox1.Text += label7Text + "\n";
            richTextBox1.Text += "---------------------------------------------" + "\n";
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            richTextBox1.BackColor = this.BackColor;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Dosyaları (*.pdf)|*.pdf";
            saveFileDialog.FileName = "Form.pdf";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                Document doc = new Document();
                try
                {
                    PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    doc.Open();


                    Bitmap bitmap = new Bitmap(this.Width, this.Height);
                    this.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, this.Width, this.Height));


                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Bmp);
                    doc.Add(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF oluşturma hatası: " + ex.Message);
                }
                finally
                {

                    doc.Close();
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
