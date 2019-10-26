using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;
namespace practice1
{
    public partial class Form1 : Form
    {
        String headerID;
        MySqlConnection connection = new MySqlConnection("Server=localhost;Database=practice1;Uid=root;Pwd=vroxine;");
        String sql;
        MySqlCommand cmd;
        public Form1()
        {
            InitializeComponent();
            try
            {
                connection.Open();
                getHeaderData();
            }
            catch (MySqlException err)
            {
                MessageBox.Show(err.Message, err.Number.ToString());
            }
        }

        private void getHeaderData()
        {
            try
            {
                if (stockTable.Items.Count > 0)
                    stockTable.Items.Clear();
                sql = $"select * from practice1.header";
                cmd = new MySqlCommand(sql, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader.GetInt16(0).ToString());
                    item.SubItems.Add(reader.GetString(1));
                    stockTable.Items.Add(item);
                }
                cmd.Dispose();
                reader.Dispose();
                if (stockTableDetail.Items.Count > 0)
                    stockTableDetail.Items.Clear();
                
            }
            catch (MySqlException err)
            {
                MessageBox.Show(err.Message, err.Number.ToString());
            }

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(keteranganTbox.Text))
                {
                    sql = $"insert into header(`name`) values('{keteranganTbox.Text}')";
                    cmd = new MySqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    getHeaderData();
                }
            }
            catch (MySqlException err)
            {
                MessageBox.Show(err.Message, err.Number.ToString());
            }
        }

        private void tabDetailShow()
        {
            try
            {
                sql = $"select name from stock";
                cmd = new MySqlCommand(sql, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())                
                    comboBox1.Items.Add(reader.GetString(0));
                
                cmd.Dispose();
                reader.Dispose();
            }
            catch (MySqlException err)
            {
                MessageBox.Show(err.Message, err.Number.ToString());
            }
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 1)
            {
                if (stockTable.SelectedItems.Count > 0)
                {
                    tabControl1.SelectTab(1);
                    headerID = stockTable.SelectedItems[0].Text;
                    Console.WriteLine(headerID);
                    tabDetailShow();
                    getTransactionData();
                } else
                {
                    MessageBox.Show("No item selected");
                    tabControl1.SelectTab(0);
                }
            }
        }

        private void tabDetailSave_Click(object sender, EventArgs e)
        {
            int thisQty = Convert.ToInt16(qtyTbox.Value);
            int thisPrice = Convert.ToInt16(priceTbox.Text);
            int hid = Convert.ToInt16(headerID);
            int sid = comboBox1.SelectedIndex+1;
            sql = "insert into transaction(`sid`,`hid`,`price`,`qty`) " + 
                  $"values({sid},{hid},{thisPrice},{thisQty})"  ;
            Console.WriteLine(sql);
            cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            getTransactionData();
        }

        private void getTransactionData()
        {
            if (stockTableDetail.Items.Count > 0)
                stockTableDetail.Items.Clear();
            sql = "SELECT t1.*  , t2.`name` , t3.`name` FROM `transaction` t1 INNER JOIN `header` t2 on t1.hid = t2.id INNER JOIN `stock` t3 on t1.sid = t3.id";
            cmd = new MySqlCommand(sql, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ListViewItem item = new ListViewItem(reader.GetInt16(0).ToString());
                for (int i = 1; i <= 6; i++)
                {
                    item.SubItems.Add(reader.GetValue(i).ToString());
                }
                stockTableDetail.Items.Add(item);
            }
            reader.Dispose();
            cmd.Dispose();
        }

        private void readFile(String filePath)
        {
            byte[] buffer;
            FileInfo fi = new FileInfo(filePath);
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  
                buffer = new byte[length];            
                int count;                            
                int sum = 0;     
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  
            }
            finally
            {
                fileStream.Close();
            }
            String fileText = Encoding.UTF8.GetString(buffer);            
            string[] splitText = fileText.Split(new[] { Environment.NewLine },StringSplitOptions.None);
            List<string> lines = splitText.ToList<string>();
            if (lines[0] == "ADD HEADER:")
            {
                lines.RemoveAt(0);
                foreach (string el in lines)
                {
                    sql = $"insert into header(`name`) values('{el}')";
                    cmd = new MySqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            else MessageBox.Show("Wrong File", "ERROR");
            getHeaderData();
        }

        private void writeFIle(String path)
        {
            FileStream fs = File.Create(path + @"\header.txt");
            using (StreamWriter writer = new StreamWriter(fs))
            {
                sql = "select * from header";
                cmd = new MySqlCommand(sql, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                
                writer.WriteLine("ID,NAME");
                while (reader.Read())
                {
                    writer.WriteLine($"{reader.GetInt16(0).ToString()},{reader.GetString(1)}");
                }
                reader.Dispose();
                cmd.Dispose();
            }
            fs.Dispose();
            fs = File.Create(path + @"\stock.txt");
            using (StreamWriter writer = new StreamWriter(fs))
            {
                sql = "select * from stock";
                cmd = new MySqlCommand(sql, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                writer.WriteLine("ID,NAME");
                while (reader.Read())
                {
                    writer.WriteLine($"{reader.GetInt16(0).ToString()},{reader.GetString(1)}");
                }
                reader.Dispose();
                cmd.Dispose();
            }
            fs.Dispose();
            fs = File.Create(path + @"\transaction.txt");
            using (StreamWriter writer = new StreamWriter(fs))
            {
                sql = "select * from transaction";
                cmd = new MySqlCommand(sql, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                writer.WriteLine("ID,StockID,HeaderID,Price,Quantity");
                while (reader.Read())
                {
                    for (int i = 0; i <= 4; i++)
                        writer.Write(reader.GetValue(i).ToString()+",");
                    writer.WriteLine();
                }
                reader.Dispose();
                cmd.Dispose();
            }
        }

        private void Import_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            readFile(openFile.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            string folderPath = folderBrowserDialog1.SelectedPath;
            writeFIle(folderPath);
            
        }
    }
}
