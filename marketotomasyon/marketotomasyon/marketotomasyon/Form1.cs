using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using PdfSharp;





namespace marketotomasyon
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter dataAdapter;
        private DataSet dataSet;
        public Form1()
        {
            InitializeComponent();

        }



        private void Form1_Load(object sender, EventArgs e)
        {


            
            dataGridView2.Columns.Add("UrunAdi", "Ürün Adı");
            dataGridView2.Columns.Add("Fiyat", "Fiyat");
            dataGridView2.Columns.Add("AlinanMiktar", "Alınan Miktar");
            
            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
            connection = new SqlConnection(connectionString);

            
            string query = "SELECT * FROM urunler";

            
            dataAdapter = new SqlDataAdapter(query, connection);
            dataSet = new DataSet();

            
            dataAdapter.Fill(dataSet, "urunler");

            
            dataGridView1.DataSource = dataSet.Tables["urunler"];
            


            dataGridView1.Columns["resim"].DefaultCellStyle.NullValue = null;
            dataGridView1.Columns["resim"].DefaultCellStyle.Padding = new Padding(0);
            ((DataGridViewImageColumn)dataGridView1.Columns["resim"]).ImageLayout = DataGridViewImageCellLayout.Zoom;
        }


        private void label5_Click(object sender, EventArgs e)
        {

        }
        private Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            string urunID = textBox1.Text;
            string urunAdi = textBox2.Text;
            int stok;
            decimal fiyat;

            
            if (string.IsNullOrEmpty(urunID))
            {
                MessageBox.Show("Lütfen bir ürün ID'si girin.");
                return;
            }

            if (string.IsNullOrEmpty(urunAdi))
            {
                MessageBox.Show("Lütfen bir ürün adı girin.");
                return;
            }

            if (!int.TryParse(textBox3.Text, out stok) || stok < 0)
            {
                MessageBox.Show("Geçersiz stok miktarı. Lütfen pozitif bir tamsayı girin.");
                return;
            }

            if (!decimal.TryParse(textBox4.Text, out fiyat) || fiyat <= 0)
            {
                MessageBox.Show("Geçersiz fiyat.");
                return;
            }

            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Lütfen bir resim seçin.");
                return;
            }

            
            byte[] resimBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                pictureBox1.Image.Save(ms, ImageFormat.Jpeg); 
                resimBytes = ms.ToArray();
            }

           
            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
            string checkQuery = "SELECT COUNT(*) FROM urunler WHERE urunid = @UrunID";

            int existingCount;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@UrunID", urunID);
                connection.Open();
                existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
            }

            
            if (existingCount > 0)
            {
                MessageBox.Show("Bu ID'ye sahip bir ürün zaten mevcut. Lütfen farklı bir ID girin.");
                return;
            }

            
            string insertQuery = "INSERT INTO urunler (urunid, urunadi, stok, fiyat, resim) VALUES (@UrunID, @UrunAdi, @Stok, @Fiyat, @Resim)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@UrunID", urunID);
                command.Parameters.AddWithValue("@UrunAdi", urunAdi);
                command.Parameters.AddWithValue("@Stok", stok);
                command.Parameters.AddWithValue("@Fiyat", fiyat);
                command.Parameters.AddWithValue("@Resim", resimBytes);

                connection.Open();
                command.ExecuteNonQuery();
            }



            
            SqlDataAdapter adapter;
            DataTable dataTable = new DataTable();
            string query = "SELECT * FROM urunler";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(dataTable);
            }

            dataGridView1.DataSource = dataTable;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            pictureBox1.Image = null;
        }



        private void button2_Click(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedRows.Count > 0)
            {
                
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                string urunAdi = selectedRow.Cells["UrunAdi"].Value.ToString();
                decimal fiyat = Convert.ToDecimal(selectedRow.Cells["Fiyat"].Value);
                int stok = Convert.ToInt32(selectedRow.Cells["Stok"].Value);

                
                if (stok == 0)
                {
                    MessageBox.Show("Stokta bu üründen kalmamış.", "Stok Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                
                string miktarStr = Interaction.InputBox("Kaç tane almak istiyorsunuz?", "Alınacak Adet", "1");
                if (string.IsNullOrWhiteSpace(miktarStr))
                    return;

                int alinanMiktar;
                if (!int.TryParse(miktarStr, out alinanMiktar) || alinanMiktar <= 0)
                {
                    MessageBox.Show("Geçersiz miktar. Lütfen pozitif bir tam sayı girin.");
                    return;
                }

                
                if (alinanMiktar > stok)
                {
                    MessageBox.Show("Stokta yeterli ürün yok.");
                    return;
                }

                
                int yeniStok = stok - alinanMiktar;

                
                string connectionString1 = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
                string updateQuery = $"UPDATE urunler SET stok = {yeniStok} WHERE urunadi = '{urunAdi}'";

                using (SqlConnection connection = new SqlConnection(connectionString1))
                {
                    SqlCommand command = new SqlCommand(updateQuery, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                
                if (yeniStok <= 10 && yeniStok > 0)
                {
                    MessageBox.Show("Stokta sadece " + yeniStok.ToString() + " ürün kaldı.", "Stok Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (yeniStok == 0)
                {
                    MessageBox.Show("Stok bitti.", "Stok Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                
                decimal fiyat1 = fiyat * alinanMiktar;
                decimal toplamFiyatDG2 = 0;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    toplamFiyatDG2 += Convert.ToDecimal(row.Cells["Fiyat"].Value);
                }

                
                label7.Text = $"Toplam Fiyat: {(fiyat1 + toplamFiyatDG2):C}";

                
                dataGridView2.Rows.Add(urunAdi, fiyat1, alinanMiktar);
            }
            else
            {
                MessageBox.Show("Lütfen bir ürün seçin.");
            }


            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
            connection = new SqlConnection(connectionString);

            
            string query = "SELECT * FROM urunler";

            
            dataAdapter = new SqlDataAdapter(query, connection);
            dataSet = new DataSet();

            
            dataAdapter.Fill(dataSet, "urunler");

            
            dataGridView1.DataSource = dataSet.Tables["urunler"];

        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp|Tüm Dosyalar|*.*";
            openFileDialog.Title = "Resim Seç";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                string urunAdi = selectedRow.Cells["UrunAdi"].Value.ToString();
                decimal fiyat = Convert.ToDecimal(selectedRow.Cells["Fiyat"].Value);
                int alinanMiktar = Convert.ToInt32(selectedRow.Cells["AlinanMiktar"].Value);

                if (alinanMiktar > 1)
                {
                    
                    Form inputBox = new Form();
                    inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
                    inputBox.ClientSize = new Size(325, 100);
                    inputBox.Text = "Kaç tane çıkarmak istiyorsunuz?";

                    TextBox textBox = new TextBox();
                    textBox.Location = new Point(20, 20);
                    textBox.Name = "inputText";
                    textBox.Width = 200;
                    inputBox.Controls.Add(textBox);

                    Button okButton = new Button();
                    okButton.Location = new Point(80, 50);
                    okButton.Name = "okButton";
                    okButton.Text = "Tamam";
                    inputBox.Controls.Add(okButton);

                    okButton.Click += (sender, e) =>
                    {
                        if (int.TryParse(textBox.Text, out int cikarilacakMiktar) && cikarilacakMiktar > 0 && cikarilacakMiktar <= alinanMiktar)
                        {
                            
                            decimal yeniFiyat = fiyat / alinanMiktar * (alinanMiktar - cikarilacakMiktar);
                            selectedRow.Cells["Fiyat"].Value = yeniFiyat.ToString("0.00");

                            
                            selectedRow.Cells["AlinanMiktar"].Value = (alinanMiktar - cikarilacakMiktar).ToString();

                            if (alinanMiktar - cikarilacakMiktar == 0)
                            {
                                
                                dataGridView2.Rows.Remove(selectedRow);
                            }

                            
                            string connectionString1 = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
                            string updateQuery = $"UPDATE urunler SET stok = stok + {cikarilacakMiktar} WHERE urunadi = '{urunAdi}'";

                            using (SqlConnection connection = new SqlConnection(connectionString1))
                            {
                                SqlCommand command = new SqlCommand(updateQuery, connection);
                                connection.Open();
                                command.ExecuteNonQuery();
                            }

                            
                            decimal toplamFiyatDG2 = 0;
                            foreach (DataGridViewRow row in dataGridView2.Rows)
                            {
                                toplamFiyatDG2 += Convert.ToDecimal(row.Cells["Fiyat"].Value);
                            }
                            label7.Text = $"Toplam Fiyat: {toplamFiyatDG2:C}";

                           

                            inputBox.Close();
                        }
                        else
                        {
                            MessageBox.Show("Lütfen geçerli bir değer giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };

                    inputBox.ShowDialog();
                }

                else
                {
                    
                    string connectionString1 = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
                    string updateQuery = $"UPDATE urunler SET stok = stok + {alinanMiktar} WHERE urunadi = '{urunAdi}'";

                    using (SqlConnection connection = new SqlConnection(connectionString1))
                    {
                        SqlCommand command = new SqlCommand(updateQuery, connection);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    
                    dataGridView2.Rows.Remove(selectedRow);

                    
                    decimal toplamFiyatDG2 = 0;
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        toplamFiyatDG2 += Convert.ToDecimal(row.Cells["Fiyat"].Value);
                    }

                    
                    label7.Text = $"Toplam Fiyat: {toplamFiyatDG2:C}";

                    
                    MessageBox.Show("Ürün başarıyla sepetten çıkarıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir ürün seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
            string selectQuery = "SELECT * FROM urunler";
            using (SqlConnection connection5 = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection5);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DataTable satinAlinanUrunler = new DataTable();
            satinAlinanUrunler.Columns.Add("Ürün Adı", typeof(string));
            satinAlinanUrunler.Columns.Add("Fiyat", typeof(decimal));
            satinAlinanUrunler.Columns.Add("Alınan Miktar", typeof(int));

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                
                if (!row.IsNewRow && row.Cells["UrunAdi"].Value != null && row.Cells["Fiyat"].Value != null && row.Cells["AlinanMiktar"].Value != null)
                {
                    string urunAdi = row.Cells["UrunAdi"].Value.ToString();
                    decimal fiyat = Convert.ToDecimal(row.Cells["Fiyat"].Value);
                    int alinanMiktar = Convert.ToInt32(row.Cells["AlinanMiktar"].Value);

                    satinAlinanUrunler.Rows.Add(urunAdi, fiyat, alinanMiktar);
                }
            }

            string label7Text = label7.Text;

            Form2 form2 = new Form2(satinAlinanUrunler, label7Text);
            form2.Show();

        }

        private void button7_Click(object sender, EventArgs e)
        {
            
            string urunID = textBox5.Text;
            string urunAdi = textBox6.Text;
            int stok;
            decimal fiyat;

            
            if (string.IsNullOrEmpty(urunID))
            {
                MessageBox.Show("Lütfen bir ürün ID'si girin.");
                return;
            }

            
            if (string.IsNullOrEmpty(urunAdi))
            {
                MessageBox.Show("Lütfen bir ürün adı girin.");
                return;
            }

            
            if (!int.TryParse(textBox7.Text, out stok) || stok < 0)
            {
                MessageBox.Show("Geçersiz stok miktarı. Lütfen pozitif bir tamsayı girin.");
                return;
            }

            if (!decimal.TryParse(textBox8.Text, out fiyat) || fiyat <= 0)
            {
                MessageBox.Show("Geçersiz fiyat. Lütfen pozitif bir fiyat girin.");
                return;
            }

            if (pictureBox2.Image == null)
            {
                MessageBox.Show("Lütfen bir resim seçin.");
                return;
            }

            
            byte[] resimBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                pictureBox2.Image.Save(ms, ImageFormat.Jpeg); 
                resimBytes = ms.ToArray();
            }

            
            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";
            string checkQuery = "SELECT COUNT(*) FROM urunler WHERE urunid = @UrunID";

            int existingCount;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@UrunID", urunID);
                connection.Open();
                existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
            }

            
            if (existingCount == 0)
            {
                MessageBox.Show("Belirtilen ID'ye sahip bir ürün bulunamadı.");
                return;
            }

            
            string updateQuery = "UPDATE urunler SET urunadi = @UrunAdi, stok = @Stok, fiyat = @Fiyat, resim = @Resim WHERE urunid = @UrunID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@UrunID", urunID);
                command.Parameters.AddWithValue("@UrunAdi", urunAdi);
                command.Parameters.AddWithValue("@Stok", stok);
                command.Parameters.AddWithValue("@Fiyat", fiyat);
                command.Parameters.AddWithValue("@Resim", resimBytes);

                connection.Open();
                int affectedRows = command.ExecuteNonQuery();

                
                if (affectedRows > 0)
                {
                    MessageBox.Show("Ürün bilgileri başarıyla güncellendi.");
                }
                else
                {
                    MessageBox.Show("Ürün bilgileri güncellenirken bir hata oluştu.");
                }
                string selectQuery = "SELECT * FROM urunler";
                using (SqlConnection connection5 = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection5);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                textBox5.Clear();
                textBox6.Clear();
                textBox7.Clear();
                textBox8.Clear();
                pictureBox2.Image = null;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp|Tüm Dosyalar|*.*";
            openFileDialog.Title = "Resim Seç";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image = new Bitmap(openFileDialog.FileName);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=DESKTOP-HSG03TQ;Initial Catalog=marketotomasyon;Integrated Security=True";

            
            string urunID = textBox9.Text;

            
            if (string.IsNullOrEmpty(urunID))
            {
                MessageBox.Show("Lütfen bir ürün ID'si girin.");
                return;
            }

            
            string deleteQuery = "DELETE FROM urunler WHERE urunid = @UrunID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(deleteQuery, connection);
                command.Parameters.AddWithValue("@UrunID", urunID);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Ürün başarıyla silindi.");
                }
                else
                {
                    MessageBox.Show("Belirtilen ID'ye sahip bir ürün bulunamadı.");
                }
                string selectQuery = "SELECT * FROM urunler";
                using (SqlConnection connection5 = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection5);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }

        }
    }

}



    
    

