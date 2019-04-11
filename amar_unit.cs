using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Data.OleDb;
using DataGridView_Import_Excel.Properties;

namespace DataGridView_Import_Excel
{
    public partial class Form1 : Form
    {
        private const string Excel03ConString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";
        private const string Excel07ConString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";
        private static int _index = 1;
        private int _counterError;
        private int _counterCodePersenloi;
        List<string> _lstPersonliCode;
        List<string> _lstPersonliCodeError;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            btn_save.Enabled = true;
            string filePath = openFileDialog1.FileName;
            string extension = Path.GetExtension(filePath);
            const string header = "no";
            string sheetName="";
            string conStr = string.Empty;
            switch (extension)
            {

                case ".xls": //Excel 97-03
                    conStr = string.Format(Excel03ConString, filePath, header);
                    break;

                case ".xlsx": //Excel 07
                    conStr = string.Format(Excel07ConString, filePath, header);
                    break;
            }

            //Get the name of the First Sheet.
            using (OleDbConnection connection = new OleDbConnection(conStr))
            {
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = connection;
                    connection.Open();
                    DataTable dtExcelSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dtExcelSchema != null) sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    connection.Close();
                }
            }

            //Read Data from the First Sheet.
            using (OleDbConnection connection = new OleDbConnection(conStr))
            {
                using (OleDbCommand command = new OleDbCommand())
                {
                    using (OleDbDataAdapter oda = new OleDbDataAdapter())
                    {
                        DataTable dt = new DataTable();
                        command.CommandText = "SELECT * From [" + sheetName + "]";
                        command.Connection = connection;
                        connection.Open();
                        oda.SelectCommand = command;
                        oda.Fill(dt);
                        connection.Close();
                        dataGridView1.DataSource = dt;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btn_save.Enabled = false;
        }
        
        private void btn_save_Click(object sender, EventArgs e)
        {

            try
            {
                if (dataGridView1.Rows.Count < 1)
                {
                    return;
                }
                string indexStr = numericUpDown1.Text;
                _index = Int32.Parse(indexStr);
                _index = _index - 1;
                saveFileDialog1.InitialDirectory = @"C:\";
                saveFileDialog1.Title = Resources.txt_dilog_title;
                saveFileDialog1.CheckPathExists = true;
                saveFileDialog1.DefaultExt = "txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = Resources.txt_format;
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = "ltms_amar.txt";
                int counter = 0;
                int list45Counter = 0;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string savePath = saveFileDialog1.FileName;
                    _lstPersonliCode = new List<string>();
                    _lstPersonliCodeError = new List<string>();

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {



                        if (!string.IsNullOrEmpty(dataGridView1.Rows[i].Cells[_index].Value.ToString().Trim()))
                        {


                            string temPersonliCode = dataGridView1.Rows[i].Cells[_index].Value.ToString().Trim();

                            if (temPersonliCode.Length != 8 || _lstPersonliCode.Contains(temPersonliCode))
                            {
                                int radifError = i + 1;
                                _lstPersonliCodeError.Add(temPersonliCode + "(ردیف :" + radifError + "),");
                                _counterError++;

                            }
                            else
                            {
                                _lstPersonliCode.Add(temPersonliCode + ",");
                                counter++;
                                _counterCodePersenloi++;

                                if (counter == 45)
                                {
                                    list45Counter++;
                                    _lstPersonliCode.Add(Environment.NewLine);
                                    _lstPersonliCode.Add(Environment.NewLine + "---------------------------<<- : تکمیل لیست شماره  " + list45Counter + "  ->>--------------------------\n" + Environment.NewLine);
                                    counter = 0;


                                }
                            }
                        }
                    }

                    if (!File.Exists(savePath))
                    {
                        using (StreamWriter sw = File.CreateText(savePath))
                        {
                            sw.Write("----------------------------------------<< **** گزارش جزییات فایل بررسی شده **** >>-----------------------------------\n" + Environment.NewLine);
                            sw.WriteLine("تعداد کدهای خطا در فایل بررسی شده : " + _counterError);
                            sw.WriteLine("تعداد کدهای صحیح در فایل بررسی شده : " + _counterCodePersenloi + Environment.NewLine);

                            sw.Write("---------------------------------------------- << گزارش کدهای اشتباه  >> -------------------------------------------\n" + Environment.NewLine);
                            sw.WriteLine(" کدهای پرسنلی دارای اشکال  : " + Environment.NewLine);
                            int lineWith = 0;
                            foreach (string s in _lstPersonliCodeError)
                            {
                                sw.Write(s);
                                lineWith++;
                                if (lineWith == 10)
                                {
                                    lineWith = 0;
                                    sw.WriteLine("");
                                }
                            }
                            sw.Write(Environment.NewLine + "---------------------------------<< پایان محدوده گزارش جزییات خطاهای بررسی شده  >>--------------------------------------------" + Environment.NewLine);


                        }
                        using (StreamWriter sw = File.AppendText(savePath))
                        {
                            sw.Write(Environment.NewLine + "----------------------------<< کدهای پرسنلی صحیح برای درج در سایت  >>---------------------------\n" + Environment.NewLine);
                            int lineWith = 0;
                            foreach (string s in _lstPersonliCode)
                            {
                                sw.Write(s);
                                lineWith++;
                                if (lineWith == 10)
                                {
                                    lineWith = 0;
                                    sw.WriteLine("");
                                }
                            }
                        }
                        
                        string txtInfo = string.Format("تعداد کدهای ثبت شده در فایل خروجی  : {0}", _counterCodePersenloi);
                        MessageBox.Show(txtInfo, "توجه",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                        dataGridView1.DataSource = null;
                        _counterCodePersenloi = 0;
                        _counterError = 0;

                        Process.Start(savePath);
                    }

                }

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message, "خطا",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Exclamation);
            }


        }


        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            string indexStr = numericUpDown1.Text;
            _index = Int32.Parse(indexStr);


        }
    }
}
