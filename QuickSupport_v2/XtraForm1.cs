using DevExpress.XtraBars.Alerter;
using DevExpress.XtraGrid.Views.Base;
using Newtonsoft.Json;
using QuickSupport_v2.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Configuration;
using QuickSupport_v2.Function;
using FPT.Framework.Data;
using System.IO.Compression;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net.Http;

namespace QuickSupport_v2
{
    public partial class XtraForm1 : DevExpress.XtraEditors.XtraForm
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<ObjConnect> _ProConnects = new List<ObjConnect>();
        private List<ObjConnect> _StaConnects = new List<ObjConnect>();
        private List<ObjConnect> ProConnects
        {
            get
            {
                if (_ProConnects.Count == 0)
                {
                    var server = from HospitalSetting s in section.Hospitals select s;
                    var a = server.Where(x => x.GROUP.Equals("CONNECTIONSTRING") && x.KEY.EndsWith("PRO") && !string.IsNullOrWhiteSpace(x.TEXT)).ToList();
                    foreach (var item in a)
                    {
                        ObjConnect obj = new ObjConnect(item.KEY, item.TEXT, item.VALUE, item.BENHVIEN_ID);
                        _ProConnects.Add(obj);
                    }
                }
                return _ProConnects;
            }
        }
        private List<ObjConnect> StaConnects
        {
            get
            {
                if (_StaConnects.Count == 0)
                {
                    var server = from HospitalSetting s in section.Hospitals select s;
                    var a = server.Where(x => x.GROUP.Equals("CONNECTIONSTRING") && (x.KEY.EndsWith("STA") || x.KEY.EndsWith("DEV")) && !string.IsNullOrWhiteSpace(x.TEXT)).ToList();
                    foreach (var item in a)
                    {
                        ObjConnect obj = new ObjConnect(item.KEY, item.TEXT, item.VALUE, item.BENHVIEN_ID);
                        _StaConnects.Add(obj);
                    }
                }
                return _StaConnects;
            }
        }
        public HospitalSettings section
        {
            get
            {
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("HospitalSettings") as HospitalSettings;
            }
            set { }

        }
        public string MABENHVIEN = string.Empty;
        public string myConnectionVal = string.Empty;
        public List<QuerySql> querySqls = new List<QuerySql>();
        public SqlConnection connection = null;
        static readonly Regex trimmer = new Regex(@"\s\s+");

        static string[] stringSeparators = new string[] { "]," };
        StringBuilder _content = new StringBuilder();
        static readonly string formatResult = "<body><b>-- {0}</b>{2}{1}{2}{2}</boby>";
        List<Result> _result;

        //string _path = Path.Combine(Environment.CurrentDirectory, @"FileData\\Data.xlsx");
        //static List<ColumnData> _dataSource;
        public XtraForm1()
        {
            InitializeComponent();
            comboBox1.DisplayMember = "display";
            comboBox1.ValueMember = "value";
            toggleSwitch1_Toggled(null, null);
            //if (_dataSource == null)
            //    _dataSource = GetDataSource(_path);
            button50.Visible = false;
        }

        private void toggleSwitch1_Toggled(object sender, EventArgs e)
        {

            comboBox1.DataSource = toggleSwitch1.IsOn ? StaConnects : ProConnects;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            slk_kho.Properties.DataSource = null;
            var ComboItem = comboBox1.SelectedItem as ObjConnect;
            myConnectionVal = ComboItem.value.ToString();
            if (!string.IsNullOrEmpty(myConnectionVal))
            {
                connection = new SqlConnection(myConnectionVal);
                MABENHVIEN = ComboItem.Benhvien_id;
                labelControl1.Text = "BV: " + MABENHVIEN;
            }
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (textEdit1.Text.Length > 1 && textEdit1.Text.Length <= 6)
                return;
            if (checkEdit1.Checked)
            {
                textEdit1.Text = string.Concat("0", MABENHVIEN, ".", textEdit1.Text);
            }
            else
            {
                if (textEdit1.Text.Length == 0)
                    return;
                if (textEdit1.Text.Substring(1, 5) == MABENHVIEN)
                {
                    textEdit1.Text = textEdit1.Text.Replace(string.Concat("0", MABENHVIEN, "."), string.Empty);
                }
            }
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textEdit1.Enabled = true;
            if (radioGroup1.SelectedIndex == 0 )
            {
                checkEdit1.Enabled = true;
                BENHNHAN_ID.Enabled = false;
                TIEPNHAN_ID.Enabled = false;
                BENHAN_ID.Enabled = false;
                simpleButton1_Click(null, null);
            }
            else if (radioGroup1.SelectedIndex == 1 || radioGroup1.SelectedIndex == 3 || radioGroup1.SelectedIndex == 4)
            {
                checkEdit1.Enabled = false;
                checkEdit1.Checked = false;
                BENHNHAN_ID.Enabled = false;
                TIEPNHAN_ID.Enabled = false;
                BENHAN_ID.Enabled = false;
                simpleButton1_Click(null, null);
            }
            else
            {
                textEdit1.Enabled = false;
                BENHNHAN_ID.Text = null;
                TIEPNHAN_ID.Text = null;
                BENHAN_ID.Text = null;
                BENHNHAN_ID.Enabled = true;
                TIEPNHAN_ID.Enabled = true;
                BENHAN_ID.Enabled = true;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(myConnectionVal))
                return;
            if (radioGroup1.SelectedIndex == 0 || radioGroup1.SelectedIndex == 1)
            {
                Clear();
                string txt = radioGroup1.SelectedIndex == 0 ? "infoBN" : "infoBN1";
                QuerySql obj = querySqls.Where(x => x.code.Equals(txt)).First();
                if (obj == null) return;

                string MAYTE = radioGroup1.SelectedIndex == 0 ? textEdit1.Text.ToString().Replace("\n", "") : null;
                string SOBENHAN = radioGroup1.SelectedIndex == 1 ? textEdit1.Text.ToString().Replace("\n", "") : null;

                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["MAYTE"] = MAYTE;
                param["SOBENHAN"] = SOBENHAN;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl1.DataSource = source;
                //gridView1.Columns["TIEPNHAN_ID"].Visible = false;
                gridView1.Columns["BENHNHAN_ID"].Visible = false;
                //gridView1.Columns["BENHAN_ID"].Visible = false;
            }
            else if (radioGroup1.SelectedIndex == 3)
            {
                Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("infoBN2")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["MAYTE"] = textEdit1.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl1.DataSource = source;

            }
            else if (radioGroup1.SelectedIndex == 4)
            {
                Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("infoBN3")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["MAYTE"] = textEdit1.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl1.DataSource = source;

            }
            else
            {
                if (string.IsNullOrEmpty(BENHNHAN_ID.Text) && string.IsNullOrEmpty(TIEPNHAN_ID.Text) && string.IsNullOrEmpty(BENHAN_ID.Text))
                {
                    MessagesBox("Nhập thông tin cần tìm vô kìa", false);
                    return;
                }
                // Search by ID
                QuerySql obj = querySqls.Where(x => x.code.Equals("infoBNByID")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                if (!string.IsNullOrEmpty(BENHNHAN_ID.Text))
                    queryString += " AND BN.BENHNHAN_ID = @BENHNHAN_ID";
                if (!string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                    queryString += " AND TN.TIEPNHAN_ID = @TIEPNHAN_ID";
                if (!string.IsNullOrEmpty(BENHAN_ID.Text))
                    queryString += " AND BA.BENHAN_ID = @BENHAN_ID";

                param["BENHNHAN_ID"] = string.IsNullOrEmpty(BENHNHAN_ID.Text) ? null : BENHNHAN_ID.Text;
                param["TIEPNHAN_ID"] = string.IsNullOrEmpty(TIEPNHAN_ID.Text) ? null : TIEPNHAN_ID.Text;
                param["BENHAN_ID"] = string.IsNullOrEmpty(BENHAN_ID.Text) ? null : BENHAN_ID.Text;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl1.DataSource = source;
                //gridView1.Columns["TIEPNHAN_ID"].Visible = false;
                gridView1.Columns["BENHNHAN_ID"].Visible = false;
                //gridView1.Columns["BENHAN_ID"].Visible = false;
            }
        }
        private async void XtraForm1_Load(object sender, EventArgs e)
        {
            string json = string.Empty;
            bool flag = section.Hospitals.Getvalue("ISONL_SQLJSON").VALUE.ToBool();
            if (flag)
            {
                try
                {
                    string apiUrl = section.Hospitals.Getvalue("API_SQLJSON").VALUE.ToString();
                    string Master = section.Hospitals.Getvalue("API_MASTER").VALUE.ToString();
                    string Access = section.Hospitals.Getvalue("API_ACCSESS").VALUE.ToString();
                    json = await FetchDataFromAPIWithHeaders(apiUrl, Master, Access);
                    querySqls = JsonConvert.DeserializeObject<List<QuerySql>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            else
            {
                string filePath = Path.Combine(Environment.CurrentDirectory, @"FileData\\SqlQuery.json");
                try
                {
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        json = r.ReadToEnd();
                        querySqls = JsonConvert.DeserializeObject<List<QuerySql>>(json);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            
        }
        static async Task<string> FetchDataFromAPIWithHeaders(string apiUrl,string Master,string Access)
        {
            string responseData = string.Empty;
            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Add headers to the HttpClient instance
                    client.DefaultRequestHeaders.Add("X-Master-Key", Master);
                    client.DefaultRequestHeaders.Add("X-Access-Key", Access);
                    client.DefaultRequestHeaders.Add("X-BIN-META", "false");

                    // Send GET request to the API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    //Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return responseData;
        }
        //private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        //{
        //    GridView gridView1 = gridControl1.MainView as GridView;
        //    if (gridView1.FocusedRowHandle >= 0)
        //    {
        //        BENHNHAN_ID.Text = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "BENHNHAN_ID").ToString();
        //        TIEPNHAN_ID.Text = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "TIEPNHAN_ID").ToString();
        //        BENHAN_ID.Text = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "BENHAN_ID").ToString();
        //        string TENBENHNHAN = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "TENBENHNHAN").ToString();

        //        if (_dataTable.Rows.Count > 0)
        //        {
        //            var rowsToUpdate = _dataTable.Select($"BENHNHAN_ID = {gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "BENHNHAN_ID")}").FirstOrDefault();

        //            if (rowsToUpdate != null)
        //            {
        //                rowsToUpdate["MAYTE"] = textEdit1.Text;
        //                rowsToUpdate["TENBENHNHAN"] = TENBENHNHAN;
        //                rowsToUpdate["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
        //                rowsToUpdate["BENHAN_ID"] = BENHAN_ID.Text;
        //                rowsToUpdate["BENHNHAN_ID"] = BENHNHAN_ID.Text;
        //                rowsToUpdate["SITE"] = comboBox1.Text;
        //                rowsToUpdate["NOTE"] = textBox14.Text;
        //            }
        //            else
        //            {
        //                DataRow newRow = _dataTable.NewRow();
        //                newRow["MAYTE"] = textEdit1.Text;
        //                newRow["TENBENHNHAN"] = TENBENHNHAN;
        //                newRow["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
        //                newRow["BENHAN_ID"] = BENHAN_ID.Text;
        //                newRow["BENHNHAN_ID"] = BENHNHAN_ID.Text;
        //                newRow["SITE"] = comboBox1.Text;
        //                newRow["NOTE"] = textBox14.Text;
        //                _dataTable.Rows.Add(newRow);
        //            }
        //        }
        //        else
        //        {
        //            DataRow newRow = _dataTable.NewRow();
        //            newRow["MAYTE"] = textEdit1.Text;
        //            newRow["TENBENHNHAN"] = TENBENHNHAN;
        //            newRow["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
        //            newRow["BENHAN_ID"] = BENHAN_ID.Text;
        //            newRow["BENHNHAN_ID"] = BENHNHAN_ID.Text;
        //            newRow["SITE"] = comboBox1.Text;
        //            newRow["NOTE"] = textBox14.Text;
        //            _dataTable.Rows.Add(newRow);
        //        }
        //        //gridControl15.RefreshDataSource();
        //    }
        //}
        private void navigationPage2_CustomButtonClick_1(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            if (e.Button == navigationPage2.CustomHeaderButtons[1]) // search
            {
                //gridView2.Columns.Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("ALLVIENPHI")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["BHYT"] = navigationPage2.CustomHeaderButtons[0].IsChecked;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl2.DataSource = source;
                gridView2.BestFitColumns();
                QuerySql obj1 = querySqls.Where(x => x.code.Equals("SUMVP")).First();
                queryString = obj1.query;

                DataTable source1 = DbTool.DbTool.Query(connection, queryString, param);

                var culture = new CultureInfo("vi-VN");
                culture.NumberFormat.NumberDecimalSeparator = ".";
                culture.NumberFormat.NumberGroupSeparator = ",";

                label5.Text = source1.Rows[0].Field<decimal>("sTHANHTIEN_DOANHTHU").ToString("N", culture);
                label8.Text = source1.Rows[0].Field<decimal>("sBHYT_THANHTIEN_CHITRA").ToString("N", culture);
                label11.Text = source1.Rows[0].Field<decimal>("sBHTN_THANHTIEN_CHITRA").ToString("N", culture);
                label14.Text = source1.Rows[0].Field<decimal>("sGIATRI_BENHNHAN_DATHANHTOAN").ToString("N", culture);
                label16.Text = source1.Rows[0].Field<decimal>("sMIENGIAM_GIATRI").ToString("N", culture);
                label18.Text = source1.Rows[0].Field<decimal>("sBENHNHAN_PHAITHANHTOAN").ToString("N", culture);
                label20.Text = source1.Rows[0].Field<decimal>("sTAMUNG").ToString("N", culture);
                label27.Text = source1.Rows[0].Field<decimal>("sCONLAI").ToString("N", culture);
                label29.Text = source1.Rows[0].Field<decimal>("sGOI").ToString("N", culture);
                //gridView2.Columns["BENHNHAN_PHAITHANHTOAN"].Summary.Clear();
                //DevExpress.XtraGrid.GridColumnSummaryItem item1 = new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "BENHNHAN_PHAITHANHTOAN", "{0:#,##0.00}");
                //gridView2.Columns["BENHNHAN_PHAITHANHTOAN"].Summary.Add(item1);

                //gridView2.Columns["REF_TABLE"].Group();
            }
            else if (e.Button == navigationPage2.CustomHeaderButtons[2]) // Tính lại chi phí
            {
                QuerySql obj = querySqls.Where(x => x.code.Equals("TINHALL")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["BENHAN_ID"] = BENHAN_ID.Text;
                param["BENHVIEN_ID"] = MABENHVIEN;
                try
                {
                    DbTool.DbTool.ExcuteStored(connection, queryString, param);
                    MessagesBox("DONE", true);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (e.Button == navigationPage2.CustomHeaderButtons[3]) // Chưa có số lô
            {
                //gridView2.Columns.Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("CHUACOLOVP")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }

                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl2.DataSource = source;
            }
            else if (e.Button == navigationPage2.CustomHeaderButtons[4])
            {
                QuerySql obj1 = querySqls.Where(x => x.code.Equals("CSRIPTVP")).First();
                string queryString = obj1.query.Replace("@TIEPNHAN_ID", TIEPNHAN_ID.Text);
                System.Windows.Forms.Clipboard.SetText(queryString);
                this.MessagesBox("COPY SCRIPT TT_VIENPHI", true);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(BENHNHAN_ID.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(BENHNHAN_ID.Text);
            //this.MessagesBox("COPY BENHNHAN_ID", true);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(TIEPNHAN_ID.Text);
            //this.MessagesBox("COPY TIEPNHAN_ID", true);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(BENHAN_ID.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(BENHAN_ID.Text);
            //this.MessagesBox("COPY BENHAN_ID", true);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _result = Generate(textBox1.Text);
            _content.Clear();
            foreach (Result item in _result)
            {
                _content.Append(string.Format(formatResult, item.IdDao, item.Query, KEYGLOBAL.NewLine));
            }
            webBrowser1.DocumentText = _content.ToString().ToUpper();
        }



        #region priavate
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DbTool.DbTool.CloseConn(connection);
        }
        private void Clear()
        {
            BENHAN_ID.Text = string.Empty;
            BENHNHAN_ID.Text = string.Empty;
            TIEPNHAN_ID.Text = string.Empty;
            //dataGridView5.DataSource = null;
            //dataGridView7.DataSource = null;
            //dataGridView4.DataSource = null;
            //dataGridView3.DataSource = null;
            //dataGridView8.DataSource = null;
            gridView3.Columns.Clear();
            gridView4.Columns.Clear();
            gridControl2.DataSource = null;
        }
        private void MessagesBox(string text, bool success)
        {
            AlertInfo aInfo = new AlertInfo("Thông báo", text)
            {
                AutoCloseFormOnClick = true
            };

            if (success)
            {
                alertControl2.FormLocation = AlertFormLocation.TopRight;
                alertControl2.AutoFormDelay = 1000;
                alertControl2.Show(this, aInfo);
            }
            else
            {
                alertControl1.FormLocation = AlertFormLocation.TopRight;
                alertControl1.AutoFormDelay = 1000;
                alertControl1.Show(this, aInfo);
            }
        }
        //public static string GetDDLName(string columnName)
        //{
        //    string result = string.Empty;
        //    if (_dataSource != null)
        //    {
        //        ColumnData column = _dataSource.Find(x => x.ColumnName.ToUpper() == columnName.ToUpper());
        //        if (column != null)
        //            result = column.DDLName.ToUpper();
        //        else
        //            result = "NVARCHAR(500)";
        //    }
        //    return result;
        //}

        //public static List<ColumnData> GetDataSource(string fileName)
        //{
        //    try
        //    {
        //        List<ColumnData> listData = new List<ColumnData>();
        //        if (System.IO.File.Exists(fileName))
        //        {
        //            System.Data.DataTable data = ReadExcel(fileName);
        //            if (data != null && data.Rows.Count > 0)
        //            {
        //                listData.AddRange(ConvertDataTable<ColumnData>(data));
        //            }
        //        }

        //        return listData;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public static System.Data.DataTable ReadExcel(string fileName)
        //{
        //    var a = readfileExcel.GetDataTableFromExcel(fileName);
        //    return a.Tables[0];
        //}
        public static List<T> ConvertDataTable<T>(System.Data.DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public static List<Result> Generate(string input)
        {
            List<Result> _resultAll = new List<Result>();
            try
            {
                input = input.ToUpper();
                Result result1 = new Result();
                StringReader reader = new StringReader(input);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                ineligible:
                    if (string.IsNullOrEmpty(result1.IdDao))
                    {
                        result1.TextInput.AppendFormat("{0}{1}", line, KEYGLOBAL.NewLine);
                        if (line.Contains(QuickSupport_v2.Model.KEY.Key1))
                        {
                            string idDao = GetStringFromTo(line, QuickSupport_v2.Model.KEY.Key, QuickSupport_v2.Model.KEY.Key1);
                            if (!string.IsNullOrEmpty(idDao))
                            {
                                string sql1 = GetStringFromTo(line, QuickSupport_v2.Model.KEY.Key1, "").Replace(KEYGLOBAL.Char_OpenSquareBrackets, "").Replace(KEYGLOBAL.Char_CloseSquareBrackets, "").Trim();
                                if (!string.IsNullOrEmpty(sql1))
                                {
                                    sql1 = trimmer.Replace(sql1, " ");
                                    result1.Query.Append(sql1);
                                    result1.IdDao = idDao;
                                    _resultAll.Add(result1);
                                    continue;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(result1.IdDao))
                        {
                            result1.Query.Append("Unable to compile!");
                            _resultAll.Add(result1);
                        }
                        result1 = new Result();
                    }
                    else
                    {
                        if (line.Contains(QuickSupport_v2.Model.KEY.Key2) && GetStringFromTo(line, QuickSupport_v2.Model.KEY.Key, QuickSupport_v2.Model.KEY.Key2) == result1.IdDao)
                        {
                            string param = GetStringFromTo(line, QuickSupport_v2.Model.KEY.Key2, string.Empty);
                            string[] param1 = param.Split(stringSeparators, StringSplitOptions.None);
                            for (int i = param1.Length - 1; i >= 0; i--)
                            {
                                string[] str1 = param1[i].Replace(KEYGLOBAL.Char_CloseSquareBrackets, "").Replace(KEYGLOBAL.Char_OpenSquareBrackets, "").Split(new string[] { KEYGLOBAL.Char_EqualSign }, StringSplitOptions.None);
                                if (str1.Length > 1)
                                {
                                    string[] str2 = str1[1].Split(new string[] { "," }, StringSplitOptions.None);
                                    if (str2.Length > 0)
                                    {
                                        string ddlName = "NVARCHAR(500)";
                                        string value = string.Empty;
                                        if (str2.Length > 1)
                                        {
                                            value = str2.Length > 1 ? str2[1].Trim() : string.Empty;
                                            value = FormatValue(value, ddlName);
                                        }
                                        if (string.IsNullOrEmpty(value)) value = "''";
                                        result1.Query.Insert(0, string.Format("DECLARE {0} {1} = {2}; {3}", str1[0].Trim(), ddlName, value, KEYGLOBAL.NewLine));
                                    }
                                }
                            }
                            result1.TextInput.AppendFormat("{0}{1}", line, KEYGLOBAL.NewLine);
                            result1 = new Result();
                            continue;
                        }
                        else
                        {
                            result1 = new Result();
                            goto ineligible;
                        }
                    }
                };
                return _resultAll;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static string FormatValue(string value, string ddlName)
        {
            string result = value;
            if (!DATATYPE.TYPENUMBERIC.Any(ddlName.Contains) && !DATATYPE.VALUEERROR.Any(value.Contains))
            {
                if (ddlName.Contains("NVARCHAR"))
                    result = string.Format("N'{0}'", value);
                else
                    result = string.Format("'{0}'", value);
            }
            return result;
        }

        private static string GetStringFromTo(string line, string keyFrom, string keyTo)
        {
            string result = string.Empty;
            int index = line.IndexOf(keyFrom);
            if (index >= 0)
            {
                int pFrom = index + keyFrom.Length;
                int pTo = line.Length;
                if (!string.IsNullOrEmpty(keyTo))
                {
                    pTo = line.IndexOf(keyTo);
                }
                result = line.Substring(pFrom, pTo - pFrom).Trim();
            }
            return result;
        }
        public void Copyfile(string FolderName, string SiteName)
        {
            string PATH_SOURCE = section.Hospitals.Getvalue("PATH_SOURCE").VALUE.ToString();
            string PATH_TARGET = section.Hospitals.Getvalue("PATH_TARGET").VALUE.ToString();
            PATH_SOURCE += FolderName;

            Directory.CreateDirectory(PATH_TARGET);

            try
            {
                foreach (var file in Directory.GetFiles(PATH_SOURCE))
                    System.IO.File.Copy(file, Path.Combine(PATH_TARGET, Path.GetFileName(file)), true);
                this.MessagesBox(string.Format("Đã chuyển qua {0} ", SiteName), true);
            }
            catch (IOException iox)
            {
                this.MessagesBox(iox.Message, false);
                return;
            }
        }
        #endregion

        private void navigationPage1_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            string action = string.Empty;
            int index = navigationPage1.CustomHeaderButtons.IndexOf(e.Button);
            bool checkBA = true;

            switch (index)
            {
                case 0:
                    action = "GETSINHIEU";
                    checkBA = false;
                    break;
                case 1:
                    action = "TTKHAMBENH";
                    checkBA = false;
                    break;
                case 2:
                    action = "DV_CK";
                    checkBA = false;
                    break;
                case 3:
                    action = "SGIUONG";
                    break;
                case 4:
                    action = "SXNBHYT";
                    checkBA = false;
                    break;
                case 5:
                    action = "sHD";
                    checkBA = false;
                    break;
                case 6:
                    action = "sDDT";
                    break;
                case 7:
                    action = "SMIENGIAM";
                    checkBA = false;
                    break;
                case 8:
                    action = "SDATANOITRU";
                    break;
                case 9:
                    action = "SGOI";
                    checkBA = false;
                    break;
                default:
                    break;
            }
            gridView3.Columns.Clear();
            QuerySql obj = querySqls.Where(x => x.code.Equals(action)).First();
            if (obj == null || (string.IsNullOrEmpty(BENHAN_ID.Text) && checkBA) || (string.IsNullOrEmpty(TIEPNHAN_ID.Text) && !checkBA))
            {
                string mess = checkBA ? "Chọn bệnh án đi" : "Chọn tiếp nhận đi";
                MessagesBox(mess, false);
                return;
            }
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;

            param["BENHAN_ID"] = BENHAN_ID.Text;
            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
            param["BENHNHAN_ID"] = BENHNHAN_ID.Text;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl3.DataSource = source;
            if (index == 7 && source.Rows.Count > 0 )
            {
                gridView3.Columns["LOAI"].Group();
                gridView3.Columns["REF_TABLE"].Group();
            }
        }

        private void navigationPage3_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            if (e.Button == navigationPage3.CustomHeaderButtons[0])
            {
                gridView4.Columns.Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("DVChuaHT")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }

                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl4.DataSource = source;
            }
            else if (e.Button == navigationPage3.CustomHeaderButtons[1])
            {
                gridView4.Columns.Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("AllDV")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["MALOAI"] = "";

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl4.DataSource = source;
                //gridView4.Columns["MALOAI"].Visible = false;
                gridView4.Columns["LOAI"].Group();
            }
        }

        private void navigationPage4_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            if (e.Button == navigationPage4.CustomHeaderButtons[0])
            {
                if (string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                QuerySql obj = querySqls.Where(x => x.code.Equals("THETIEPNHAN")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl5.DataSource = source;
                source = null;

                obj = querySqls.Where(x => x.code.Equals("THEBHYT")).First();
                source = DbTool.DbTool.Query(connection, obj.query, param);
                //gridControl6.DataSource = source;
                gridControl27.DataSource = source;
            }
            else if (e.Button == navigationPage4.CustomHeaderButtons[1])
            {
                DialogResult popup = MessageBox.Show("Có chắc chắn muốn cập nhật không?", "Cập nhật thông tin thẻ", MessageBoxButtons.YesNo);

                if (popup == DialogResult.No) { return; }

                bool flag  = false;
              
                // Cập nhật TT_TIEPNHAN
                DataRow dr1;
                for (int i = 0; i < gridView5.RowCount; i++)
                {
                    dr1 = gridView5.GetDataRow(i);
                    if (dr1 != null)
                    {
                        if (dr1.RowState == DataRowState.Modified)
                        {
                            string queryString = "UPDATE TT_TIEPNHAN set SOBHYT = @SOBHYT ,BHYTTUNGAY = @BHYTTUNGAY ,BHYTDENNGAY = @BHYTDENNGAY ,NGAYBATDAU5NAM  = @NGAYBATDAU5NAM ,BENHVIEN_KCB = @BENHVIEN_KCB,DOITUONG_ID = @DOITUONG_ID WHERE TIEPNHAN_ID = @TIEPNHAN_ID";
                            FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                            param["SOBHYT"] = dr1["SOBHYT"];
                            param["BHYTTUNGAY"] = dr1["BHYTTUNGAY"];
                            param["BHYTDENNGAY"] = dr1["BHYTDENNGAY"];
                            param["TIEPNHAN_ID"] = dr1["TIEPNHAN_ID"];
                            param["NGAYBATDAU5NAM"] = dr1["NGAYBATDAU5NAM"];
                            param["BENHVIEN_KCB"] = dr1["BENHVIEN_KCB"];
                            param["DOITUONG_ID"] = dr1["DOITUONG_ID"];
                            flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString, param);
                        }
                    }
                }

                // Cập nhật TT_TIEPNHAN_SOTHE
                DataRow dr;
                for (int i = 0; i < gridView27.RowCount; i++)
                {
                    dr = gridView27.GetDataRow(i);
                    if (dr != null)
                    {
                        if (dr.RowState == DataRowState.Modified)
                        {
                            string queryString = "UPDATE  TT_TIEPNHAN_SOTHEBHYT SET SOBHYT = @SOBHYT, BHYTTUNGAY = @BHYTTUNGAY, BHYTDENNGAY =@BHYTDENNGAY,TAMNGUNG = @TAMNGUNG,NGAYBATDAU5NAM = @NGAYBATDAU5NAM, BENHVIEN_KCB = @BENHVIEN_KCB WHERE SOBHYT_HUONG_ID = @SOBHYT_HUONG_ID and TIEPNHAN_ID = @TIEPNHAN_ID";
                            FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                            param["SOBHYT"] = dr["SOBHYT"];
                            param["BHYTTUNGAY"] = dr["BHYTTUNGAY"];
                            param["BHYTDENNGAY"] = dr["BHYTDENNGAY"];
                            param["SOBHYT_HUONG_ID"] = dr["SOBHYT_HUONG_ID"];
                            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                            param["TAMNGUNG"] = dr["TAMNGUNG"];
                            param["NGAYBATDAU5NAM"] = dr["NGAYBATDAU5NAM"];
                            param["BENHVIEN_KCB"] = dr["BENHVIEN_KCB"];
                            flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString, param);
                        }
                    }
                }

                string mess = flag ? "ĐÃ CẬP NHẬT" : "LỖI KHI CẬP NHẬT";
                MessagesBox(mess, flag);
            }
            else if (e.Button == navigationPage4.CustomHeaderButtons[2])
            {
                string text = string.Empty;
                QuerySql obj1 = querySqls.Where(x => x.code.Equals("THETIEPNHAN")).First();
                text = obj1.query.Replace("@TIEPNHAN_ID", TIEPNHAN_ID.Text) + Environment.NewLine + Environment.NewLine;

                obj1 = querySqls.Where(x => x.code.Equals("THEBHYT")).First();
                text += obj1.query.Replace("@TIEPNHAN_ID", TIEPNHAN_ID.Text);
                System.Windows.Forms.Clipboard.SetText(text);
                this.MessagesBox("Đã copy SCRIPT", true);
            }
            else if (e.Button == navigationPage4.CustomHeaderButtons[3])
            {
                if (gridView27.SelectedRowsCount == 0)
                {
                    MessagesBox("Hãy chọn thẻ cần cập nhật", false);
                    return;
                }
                DialogResult popup = MessageBox.Show("Có chắc chắn muốn cập nhật không?", "Cập nhật thông tin thẻ", MessageBoxButtons.YesNo);
                if (popup == DialogResult.No) { return; }

                string queryString = "UPDATE TT_VIENPHI SET SOBHYT_HUONG_ID = @SOBHYT_HUONG_ID  WHERE DOITUONG_HUONG_ID in (38, 1043, 1044, 1045, 1054) and TIEPNHAN_ID = @TIEPNHAN_ID AND SOBHYT_HUONG_ID != @SOBHYT_HUONG_ID";
                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["SOBHYT_HUONG_ID"] = gridView27.GetRowCellValue(gridView27.GetSelectedRows().FirstOrDefault(), "SOBHYT_HUONG_ID").ToString();

                bool flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString, param);
                string mess = flag ? "ĐÃ CẬP NHẬT" : "LỖI KHI CẬP NHẬT";
                MessagesBox(mess, flag);
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string action = radioGroup3.SelectedIndex == 0 ? "sREPORT" : "sSTORED";
            QuerySql obj = querySqls.Where(x => x.code.Equals(action)).First();
            if (obj == null || string.IsNullOrEmpty(textEdit2.Text))
            {
                MessagesBox("Nhập thông tin cần tìm vô kìa", false);
                return;
            }

            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;
            param["KEY"] = textEdit2.Text;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl7.DataSource = source;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();

            string REPORT_FILE = gridView7.GetRowCellValue(gridView7.FocusedRowHandle, "REPORT_FILE").ToString();
            if (string.IsNullOrEmpty(REPORT_FILE) || string.IsNullOrEmpty(path))
                return;
            string remoteUri = linkLabel1.Text;
            string fileName = REPORT_FILE;
            string dest = path + string.Format(@"\{0}", fileName);
            try
            {
                DialogResult dialogResult = DialogResult.Cancel;
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteUri, dest);
                    dialogResult = MessageBox.Show(string.Format("Successfully downloaded file.\nDestination: {0}", dest), "MỞ FILE ĐÃ TẢI", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                }
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start(dest);
                }
                else if (dialogResult == DialogResult.No)
                {
                    Process.Start(path);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (tabPane1.SelectedPageIndex == 0) // Nội trú
            {
                gridView8.Columns.Clear();
                gridView9.Columns.Clear();
                gridView10.Columns.Clear();
                QuerySql obj = querySqls.Where(x => x.code.Equals("SKHAMBENH_ID")).First();
                if (obj == null || string.IsNullOrEmpty(BENHAN_ID.Text))
                {
                    MessagesBox("Chọn bệnh án đi", false);
                    return;
                }

                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["BENHAN_ID"] = BENHAN_ID.Text;
                DateTime? dt = null;
                if (dateTimePicker1.Checked)
                    dt = dateTimePicker1.Value;
                param["THOIGIANKHAM"] = dt;
                param["THUOC"] = checkBox9.Checked;
                param["VTYT"] = checkBox10.Checked;


                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl8.DataSource = source;
                gridView8.Columns["TRANGTHAIYLENH"].Visible = false;
                //gridView8.Columns["KHAMBENH_ID"].Visible = false;
                //gridView1.Columns["BENHAN_ID"].Visible = false;
                button50.Visible = false;
                gridView8.BestFitColumns();
            }
            else if (tabPane1.SelectedPageIndex == 1) // Ngoại trú
            {
                gridView12.Columns.Clear();
                gridView13.Columns.Clear();
                gridView14.Columns.Clear();

                QuerySql obj = querySqls.Where(x => x.code.Equals("SKBNGOAITRU")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DateTime? dt = null;
                if (dateTimePicker1.Checked)
                    dt = dateTimePicker1.Value;
                param["THOIGIANKHAM"] = dt;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl12.DataSource = source;
                gridView12.Columns["KHAMBENH_ID"].Visible = false;
            }
            else if (tabPane1.SelectedPageIndex == 2) // PT/TT
            {
                gridView17.Columns.Clear();

                QuerySql obj = querySqls.Where(x => x.code.Equals("THUOCPTTT")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DateTime? dt = null;
                if (dateTimePicker1.Checked)
                    dt = dateTimePicker1.Value;
                param["THOIGIANKHAM"] = dt;
                param["THUOC"] = checkBox9.Checked;
                param["VTYT"] = checkBox10.Checked;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl16.DataSource = source;
                gridView16.Columns["TENDICHVU"].Group();
            }
            else if (tabPane1.SelectedPageIndex == 3) // PT/TT
            {
                gridView20.Columns.Clear();

                QuerySql obj = querySqls.Where(x => x.code.Equals("THUOCCLS")).First();
                if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DateTime? dt = null;
                if (dateTimePicker1.Checked)
                    dt = dateTimePicker1.Value;
                param["THOIGIANKHAM"] = dt;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl19.DataSource = source;
                gridView19.Columns["TENDICHVU"].Group();
            }
        }

        //private void gridView8_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        //{
        //    if (gridView8.FocusedRowHandle >= 0)
        //    {
        //        var Khambenh_id = gridView8.GetRowCellValue(gridView8.FocusedRowHandle, "KHAMBENH_ID").ToString();

        //        QuerySql obj = querySqls.Where(x => x.code.Equals("STHUOCNOITRU")).First();
        //        string queryString = obj.query;
        //        FPT.Framework.Data.DataObject param = obj.param;

        //        param["KHAMBENH_ID"] = Khambenh_id;
        //        param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
        //        DataTable source = DbTool.DbTool.Query(connection, queryString, param);
        //        gridControl9.DataSource = source;
        //        gridControl10.DataSource = null;
        //    }
        //}

        private void gridControl9_DoubleClick(object sender, EventArgs e)
        {
            List<QuerySql> lstquery = new List<QuerySql>();
            var ToaThuoc_id = gridView9.GetRowCellValue(gridView9.FocusedRowHandle, "TOATHUOC_ID").ToString();
            QuerySql obj0 = querySqls.Where(x => x.code.Equals("STHYL")).First();
            QuerySql obj1 = querySqls.Where(x => x.code.Equals("sTGLIEU")).First();
            QuerySql obj2 = querySqls.Where(x => x.code.Equals("sCTX13")).First();
            QuerySql obj3 = querySqls.Where(x => x.code.Equals("sTHUOCNGUNG")).First();
            QuerySql obj4 = querySqls.Where(x => x.code.Equals("INSERTTHYL")).First();
            lstquery.Add(obj0);
            lstquery.Add(obj1);
            lstquery.Add(obj2);
            lstquery.Add(obj3);
            lstquery.Add(obj4);
            THYL f2 = new THYL
            {
                TOATHUOC_ID = ToaThuoc_id,
                BENHVIEN_ID = MABENHVIEN,
                SQuery = lstquery,
                connection = connection
            };
            f2.Show();
            return;
        }


        private void tabPane1_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
        {
            this.button8.Enabled = false;
            this.button10.Enabled = false;
            button50.Enabled = false;
            checkBox9.Enabled = true;
            checkBox10.Enabled = true;
            checkBox9.Checked = true;
            checkBox10.Checked = true;
            if (tabPane1.SelectedPageIndex == 0)
            {
                this.button8.Enabled = true;
                this.button10.Enabled = true;
                button50.Enabled = true;
            }
            else if (tabPane1.SelectedPageIndex == 1 || tabPane1.SelectedPageIndex == 3)
            {
                checkBox9.Checked = false;
                checkBox10.Checked = false;
                checkBox9.Enabled = false;
                checkBox10.Enabled = false;
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            gridControl8.DataSource = null;
            gridControl10.DataSource = null;
            gridView8.Columns.Clear();
            gridView9.Columns.Clear();
            gridView10.Columns.Clear();
            QuerySql obj = querySqls.Where(x => x.code.Equals("SCXUATTH")).First();
            if (obj == null || string.IsNullOrEmpty(BENHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }

            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;
            param["BENHAN_ID"] = BENHAN_ID.Text;
            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;

            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl9.DataSource = source;
            button50.Visible = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            QuerySql obj = querySqls.Where(x => x.code.Equals("SDANGTT")).First();
            if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }
            gridControl8.DataSource = null;
            gridControl10.DataSource = null;
            gridView8.Columns.Clear();
            gridView9.Columns.Clear();
            gridView10.Columns.Clear();
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;

            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl9.DataSource = source;
            button50.Visible = source.Rows.Count > 0;
        }

        //private void button31_Click(object sender, EventArgs e)
        //{
        //    if (gridView9.RowCount == 0)
        //        return;
        //    string lstToathuoc_id = "";
        //    for (int i = 0; i < gridView9.DataRowCount; i++)
        //    {
        //        lstToathuoc_id += gridView9.GetRowCellValue(i, "TOATHUOC_ID").ToString() + ",";
        //    }
        //    QuerySql obj = querySqls.Where(x => x.code.Equals("SCRIPTDANGTT")).First();
        //    string queryString = obj.query.Replace("@SLTTOATHUOC_ID", lstToathuoc_id.Substring(0, lstToathuoc_id.Length - 1)).Replace('"', ' ');
        //    System.Windows.Forms.Clipboard.SetText(queryString);
        //}

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void groupControl1_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            if (e.Button == groupControl1.CustomHeaderButtons[0])
            {
                ConfigForm f = new ConfigForm();
                f.ShowDialog();
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                section = configFile.GetSection("HospitalSettings") as HospitalSettings;
                textBox2.Text = section.Hospitals.Getvalue("NOTE1").VALUE.ToString();
                textBox4.Text = section.Hospitals.Getvalue("NOTE2").VALUE.ToString();
                textBox5.Text = section.Hospitals.Getvalue("NOTE3").VALUE.ToString();
                textBox6.Text = section.Hospitals.Getvalue("NOTE4").VALUE.ToString();

                textBox10.Text = section.Hospitals.Getvalue("NOTE5").VALUE.ToString();
                textBox11.Text = section.Hospitals.Getvalue("NOTE6").VALUE.ToString();

            }
            else if (e.Button == groupControl1.CustomHeaderButtons[1])
            {
                string link = section.Hospitals.Getvalue("SOURCE_LOG").VALUE.ToString();
                Process.Start(link);
            }
            else if (e.Button == groupControl1.CustomHeaderButtons[2])
            {
                string link = section.Hospitals.Getvalue("PATH_SOURCE").VALUE.ToString();
                Process.Start(link);
            }
            else if (e.Button == groupControl1.CustomHeaderButtons[3])
            {
                string link = section.Hospitals.Getvalue("HM_SOURCE").VALUE.ToString();
                Process.Start(link);
            }
        }
        private void navigationPane1_SelectedPageIndexChanged(object sender, EventArgs e)
        {
            if (navigationPane1.SelectedPageIndex == 7)
            {
                textBox2.Text = section.Hospitals.Getvalue("NOTE1").VALUE.ToString();
                textBox4.Text = section.Hospitals.Getvalue("NOTE2").VALUE.ToString();
                textBox5.Text = section.Hospitals.Getvalue("NOTE3").VALUE.ToString();
                textBox6.Text = section.Hospitals.Getvalue("NOTE4").VALUE.ToString();

                textBox10.Text = section.Hospitals.Getvalue("NOTE5").VALUE.ToString();
                textBox11.Text = section.Hospitals.Getvalue("NOTE6").VALUE.ToString();
                toggleSwitch3_Toggled(null, null);

                DataTable dt = new DataTable();
                dt.Columns.Add("TEXT");
                dt.Columns.Add("KEY");
                dt.Columns.Add("VALUE");
                dt.Columns.Add("ENVI");
                dt.Columns.Add("ISPRO");

                var server = from HospitalSetting s in section.Hospitals select s;
                var a = server.Where(x => x.GROUP.Equals("RUN_HIS")).ToList();
                foreach (var item in a)
                {
                    DataRow _ravi = dt.NewRow();
                    _ravi["TEXT"] = item.TEXT;
                    _ravi["KEY"] = item.KEY;
                    _ravi["VALUE"] = item.VALUE;
                    _ravi["ENVI"] = item.KEY.EndsWith("PRO") ? "PRODUCTION" : "STAGING";
                    _ravi["ISPRO"] = item.KEY.EndsWith("PRO") ? true : false;
                    dt.Rows.Add(_ravi);
                }
                gridControl29.DataSource = dt;
                gridView30.Columns["ENVI"].Group();
            }
            else if (navigationPane1.SelectedPageIndex == 9)
            {
                //webBrowser6.DocumentText = null;
                string storedname = string.IsNullOrEmpty(BENHAN_ID.Text) ? "eClaim_NgoaiTru_TT130" : "eClaim_NoiTru_DTNT_TT130";

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(GenScriptStored(storedname));
                stringBuilder.Append("<br><br>");
                stringBuilder.Append(GenScriptStored("eClaim_TT130_4_12"));
                stringBuilder.Append("<br><br>");

                webBrowser6.DocumentText = stringBuilder.ToString();
            }
            else
            {
                //System.Configuration.Configuration _configFile = null;
                //System.ComponentModel.BindingList<HospitalSetting> _Hospitals = new System.ComponentModel.BindingList<HospitalSetting>();
                //_configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //if (_configFile.GetSection("HospitalSettings") is HospitalSettings section)
                //{
                //    foreach (HospitalSetting hospital in (ConfigurationElementCollection)section.Hospitals)
                //    {
                //        _Hospitals.Add(hospital);
                //    }
                //}
                //_Hospitals.AllowNew = true;
                //_Hospitals.AllowEdit = true;

            }
        }
        private string GenScriptStored(string storedname)
        {
            string text = "EXECUTE " + storedname + " ";
            QuerySql obj = new QuerySql();
            string queryString = "SELECT PARAMETER_NAME,DATA_TYPE FROM information_schema.parameters WHERE specific_name = @STORED_NAME";
            var param = new FPT.Framework.Data.DataObject();
            param["STORED_NAME"] = storedname;

            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            string value = string.Empty;

            string[] number_type = { "bigint", "bit", "decimal", "int", "numeric", "smallint", "tinyint" };

            foreach (DataRow item in source.Rows)
            {
                switch (item["PARAMETER_NAME"].ToString().ToUpper())
                {
                    case "@MABENHVIEN":
                    case "@BENHVIEN_ID":
                        value = "'" + MABENHVIEN + "'";
                        break;
                    case "@TIEPNHAN_ID":
                    case "@SOTIEPNHAN":
                        value = TIEPNHAN_ID.Text;
                        break;
                    case "@BENHAN_ID":
                        value = BENHAN_ID.Text;
                        break;
                    case "@BENHNHAN_ID":
                        value = BENHNHAN_ID.Text;
                        break;
                    case "@DEBUG":
                        value = "1";
                        break;
                    case "@TUNGAY":
                        value = "'" + DateTime.Now.ToString("yyyy-MM-dd 00:00") + "'";
                        break;
                    case "@DENNGAY":
                        value = "'" + DateTime.Now.ToString("yyyy-MM-dd 23:59") + "'";
                        break;
                    default:
                        value = number_type.Any(item["DATA_TYPE"].ToString().Contains) ? string.Empty : "'" + string.Empty + "'";
                        break;
                }

                text += item["PARAMETER_NAME"].ToString() + " = " + value + ", ";
            }
            return text.Substring(0, text.Length - 2);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox2.Text);
        }

        private void button35_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox4.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox4.Text);
        }

        private void button38_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox5.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox5.Text);
        }

        private void button37_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox6.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox6.Text);
        }

        private void button41_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox11.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox11.Text);
        }



        private void button40_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox10.Text))
                return;
            System.Windows.Forms.Clipboard.SetText(textBox10.Text);
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1)
            {
                this.textBox7.Text = null; // Database
                this.textBox8.Text = null; // remote
                this.textBox9.Text = null; // Mã BV
                this.linkLabel3.Text = null;
                this.linkLabel2.Text = null;
                return;
            }
            var reader = new AppSettingsReader();
            string code = comboBox2.SelectedValue.ToString();

            switch (code)
            {
                case "DN_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("DN_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("DN_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("DN_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("DN_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "DN_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("DN_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("DN_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = string.Empty; // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("DN_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "DL_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("DL_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("DL_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("DL_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("DL_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "DL_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("DL_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("DL_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = string.Empty; // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("DL_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "CL_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("CL_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("CL_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("CL_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("CL_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "CL_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("CL_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("CL_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = string.Empty; // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("CL_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;

                case "SG_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("SG_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("SG_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("SG_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("SG_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "SG_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("SG_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("SG_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("SG_STA_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("SG_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "MAT_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("MAT_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("MAT_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("MAT_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("MAT_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "MAT_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("MAT_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("MAT_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("MAT_STA_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("MAT_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "VINH_PRO":
                    this.textBox7.Text = section.Hospitals.Getvalue("VINH_PRO_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("VINH_PRO_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("VINH_PRO_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("VINH_PRO_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "VINH_STA":
                    this.textBox7.Text = section.Hospitals.Getvalue("VINH_STA_DATABASE").VALUE.ToString(); // Database
                    this.textBox8.Text = section.Hospitals.Getvalue("VINH_STA_SERVER_APP").VALUE.ToString(); // remote
                    this.textBox9.Text = section.Hospitals.Getvalue(code).BENHVIEN_ID.ToString(); // Mã BV
                    this.linkLabel3.Text = section.Hospitals.Getvalue("VINH_STA_REPORT").VALUE.ToString(); // report
                    this.linkLabel2.Text = section.Hospitals.Getvalue("VINH_STA_DOWNLOAD").VALUE.ToString(); // download
                    break;
                case "VANPHUC_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("VANPHUC_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "VANPHUC_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("VANPHUC_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "THUDUC_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("THUDUC_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "THUDUC_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("THUDUC_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "DONGNAI_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("DONGNAI_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "DONGNAI_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("DONGNAI_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "BINHDUONG_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("BINHDUONG_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "BINHDUONG_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("BINHDUONG_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "BINHDUONGC_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("BINHDUONGC_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "BINHDUONGC_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("BINHDUONGC_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "PDR_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("PDR_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "PDR_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("PDR_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "TMSGC_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("TMSGC_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "TMSGC_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("TMSGC_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "HMMH_PRO":
                    this.textBox8.Text = section.Hospitals.Getvalue("HMMH_PRO_SERVER_APP").VALUE.ToString(); // remote
                    break;
                case "HMMH_STA":
                    this.textBox8.Text = section.Hospitals.Getvalue("HMMH_STA_SERVER_APP").VALUE.ToString(); // remote
                    break;
                default:
                    this.textBox7.Text = null; // Database
                    this.textBox8.Text = null; // remote
                    this.textBox9.Text = null; // Mã BV
                    this.linkLabel3.Text = null;
                    this.linkLabel2.Text = null;
                    break;
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0)
            {
                string connString = section.Hospitals.Getvalue(comboBox2.SelectedValue.ToString()).VALUE.ToString();
                if (!string.IsNullOrEmpty(connString))
                    System.Windows.Forms.Clipboard.SetText(connString);
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox8.Text))
                return;
            Clipboard.SetText(textBox8.Text.ToString());
        }

        private void button34_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox7.Text))
                return;
            Clipboard.SetText(textBox7.Text.ToString());
        }

        private void button39_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox9.Text))
                return;
            Clipboard.SetText(textBox9.Text.ToString());
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.linkLabel3.Text))
                return;
            Process.Start(this.linkLabel3.Text);
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.linkLabel2.Text))
                return;
            Process.Start(this.linkLabel2.Text);
        }

        private void gridView13_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (gridView13.FocusedRowHandle >= 0)
            {
                List<QuerySql> lstquery = new List<QuerySql>();
                var ToaThuoc_id = gridView13.GetRowCellValue(gridView13.FocusedRowHandle, "TOATHUOC_ID").ToString();

                QuerySql obj = querySqls.Where(x => x.code.Equals("SCHUNGTUNT")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["TOATHUOC_ID"] = ToaThuoc_id;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl14.DataSource = source;
            }
            else { gridControl14.DataSource = null; }
        }

        private void gridView16_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (gridView16.FocusedRowHandle >= 0)
            {
                List<QuerySql> lstquery = new List<QuerySql>();
                var PHAUTHUAT_VTYT_ID = gridView16.GetRowCellValue(gridView16.FocusedRowHandle, "PHAUTHUAT_VTYT_ID").ToString();

                QuerySql obj = querySqls.Where(x => x.code.Equals("SCHUNGTUPTTT")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["PHAUTHUAT_VTYT_ID"] = PHAUTHUAT_VTYT_ID;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl17.DataSource = source;
            }
            else { gridControl17.DataSource = null; }
        }

        private void gridView19_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (gridView19.FocusedRowHandle >= 0)
            {
                List<QuerySql> lstquery = new List<QuerySql>();
                var CLSKETQUA_VTYT_ID = gridView19.GetRowCellValue(gridView19.FocusedRowHandle, "CLSKETQUA_VTYT_ID").ToString();

                QuerySql obj = querySqls.Where(x => x.code.Equals("SCHUNGTUCLS")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["CLSKETQUA_VTYT_ID"] = CLSKETQUA_VTYT_ID;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl20.DataSource = source;
            }
            else { gridControl20.DataSource = null; }
        }
        private void button44_Click(object sender, EventArgs e)
        {
            QuerySql obj = querySqls.Where(x => x.code.Equals("UPDSAIKHO")).First();
            if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;
            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
            try
            {
                DbTool.DbTool.Query(connection, queryString, param);
                MessagesBox("DONE", true);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void button45_Click(object sender, EventArgs e)
        {
            Copyfile("CLDEV", "CL DEV");
        }

        private void gridControl8_Click(object sender, EventArgs e)
        {
            if (gridView8.GetSelectedRows().FirstOrDefault() >= 0)
            {
                var Khambenh_id = gridView8.GetRowCellValue(gridView8.GetSelectedRows().FirstOrDefault(), "KHAMBENH_ID").ToString();
                textBox16.Text = gridView8.GetRowCellValue(gridView8.GetSelectedRows().FirstOrDefault(), "TRANGTHAIYLENH").ToString();
                QuerySql obj = querySqls.Where(x => x.code.Equals("STHUOCNOITRU")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["KHAMBENH_ID"] = Khambenh_id;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl9.DataSource = source;
                gridControl10.DataSource = null;
            }
        }
        private void button46_Click(object sender, EventArgs e)
        {
            gridView18.Columns.Clear();
            string action = string.Empty;
            //if (!string.IsNullOrEmpty(textBox14.Text)) // search text
            //{
            //    action = "sDIC";
            //}
            //else
            //{
            //if (radioGroup2.SelectedIndex == 0)
            //{
            //    action = "GETDICHGQ";
            //}
            //else if (radioGroup2.SelectedIndex == 1)
            //{
            //    action = "GETDICCK";
            //}
            //else if (radioGroup2.SelectedIndex == 2)
            //{
            //    action = "GETDICLBA";
            //}
            //}

            switch (radioGroup2.SelectedIndex)
            {
                case 0: action = "GETDICHGQ";
                    break;
                case 1: action = "sDIC";
                    break;
                case 2: action = "GETDICCK";
                    break;
                case 3: action = "GETDICLBA";
                    break;
                case 4:
                    action = "GETKD";
                    break;
                case 5:
                    action = "GETPB";
                    break;
                case 6:
                    action = "GETCACHE";
                    break;
                case 7:
                    action = "GETAPPSETTING";
                    break;
                case 8:
                    action = "SDUOC";
                    break;
                case 9:
                    action = "SDICHVU";
                    break;

            }

            QuerySql obj = querySqls.Where(x => x.code.Equals(action)).First();
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;
            param["TEXT"] = textBox14.Text;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl18.DataSource = source;
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {
            //GridView gridView1 = gridControl1.MainView as GridView;
            if (gridView1.GetSelectedRows().FirstOrDefault() >= 0)
            {
                BENHNHAN_ID.Text = gridView1.GetRowCellValue(gridView1.GetSelectedRows().FirstOrDefault(), "BENHNHAN_ID").ToString();
                TIEPNHAN_ID.Text = gridView1.GetRowCellValue(gridView1.GetSelectedRows().FirstOrDefault(), "TIEPNHAN_ID").ToString();
                BENHAN_ID.Text = gridView1.GetRowCellValue(gridView1.GetSelectedRows().FirstOrDefault(), "BENHAN_ID").ToString();
                string TENBENHNHAN = gridView1.GetRowCellValue(gridView1.GetSelectedRows().FirstOrDefault(), "TENBENHNHAN").ToString();
            }
        }

        private void gridControl9_Click(object sender, EventArgs e)
        {
            if (gridView9.GetSelectedRows().FirstOrDefault() >= 0)
            {
                List<QuerySql> lstquery = new List<QuerySql>();
                var ToaThuoc_id = gridView9.GetRowCellValue(gridView9.GetSelectedRows().FirstOrDefault(), "TOATHUOC_ID").ToString();
                QuerySql obj = querySqls.Where(x => x.code.Equals("SCHUNGTU")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["TOATHUOC_ID"] = ToaThuoc_id;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl10.DataSource = source;
            }
            else { gridControl10.DataSource = null; }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (gridView7.GetSelectedRows().FirstOrDefault() >= 0)
            {
                string STORED_NAME = gridView7.GetRowCellValue(gridView7.GetSelectedRows().FirstOrDefault(), "PROCEDURE_FILE").ToString();
                System.Windows.Forms.Clipboard.SetText(STORED_NAME.Trim());
            }
        }

        private void radioGroup2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (radioGroup2.SelectedIndex)
            {
                case 0:
                case 2:
                case 3:
                    textBox14.Enabled = false;
                    break;
                default: textBox14.Enabled = true; break;
            }
        }

        private void navigationPage9_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            if (e.Button == navigationPage9.CustomHeaderButtons[0])
            {
                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["BENHNHAN_ID"] = BENHNHAN_ID.Text;
                param["MABENHVIEN"] = MABENHVIEN;
                param["DEBUG"] = 1;

                string action;
                if (string.IsNullOrEmpty(BENHAN_ID.Text))
                {
                    action = "eClaim_NgoaiTru_TT130";
                }
                else
                {
                    action = "eClaim_NoiTru_DTNT_TT130";
                    param["BENHAN_ID"] = BENHAN_ID.Text;
                }

                DataSet source = DbTool.DbTool.QueryStored(connection, action, param);
                gridControl15.DataSource = source.Tables[0];
                gridControl22.DataSource = source.Tables[1];
                gridControl23.DataSource = source.Tables[2];



                gridView15.Columns["T_BHTT"].AppearanceCell.BackColor = Color.GreenYellow;
                gridView15.Columns["T_BNTT"].AppearanceCell.BackColor = Color.GreenYellow;
                gridView15.Columns["T_BNCCT"].AppearanceCell.BackColor = Color.GreenYellow;
                gridView15.Columns["T_TONGCHI_BH"].AppearanceCell.BackColor = Color.GreenYellow;
                gridView15.Columns["T_THUOC"].AppearanceCell.BackColor = Color.Yellow;
                gridView15.Columns["T_VTYT"].AppearanceCell.BackColor = Color.Yellow;

                param.Remove("MABENHVIEN");
                param.Remove("DEBUG");
                if (!param.ContainsKey("BENHAN_ID"))
                    param["BENHAN_ID"] = BENHAN_ID.Text;

                DataSet sourc1e = DbTool.DbTool.QueryStored(connection, "eClaim_TT130_4_12", param);

                System.Data.DataColumnCollection columns = sourc1e.Tables[0].Columns;
                gridControl21.DataSource = columns.Contains("XML4_RESULT") ? XmlToDataset(sourc1e.Tables[0].Rows[0].Field<string>("XML4_RESULT")) : null;
                gridControl24.DataSource = columns.Contains("XML5_RESULT") ? XmlToDataset(sourc1e.Tables[0].Rows[0].Field<string>("XML5_RESULT")) : null;

                //giấy ra viện
                webBrowser2.DocumentText = columns.Contains("XML7_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML7_RESULT")) : null;
                //Tóm tắt HSBA
                webBrowser3.DocumentText = columns.Contains("XML8_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML8_RESULT")) : null;
                webBrowser4.DocumentText = columns.Contains("XML9_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML9_RESULT")) : null;
                webBrowser7.DocumentText = columns.Contains("XML10_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML10_RESULT")) : null;
                webBrowser5.DocumentText = columns.Contains("XML11_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML11_RESULT")) : null;

                webBrowser8.DocumentText = columns.Contains("XML13_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML13_RESULT")) : null;
                webBrowser9.DocumentText = columns.Contains("XML14_RESULT") ? Beautify(sourc1e.Tables[0].Rows[0].Field<string>("XML14_RESULT")) : null;

                //string storedname = string.IsNullOrEmpty(BENHAN_ID.Text) ? "eClaim_NgoaiTru_TT130" : "eClaim_NoiTru_DTNT_TT130";

                //StringBuilder stringBuilder = new StringBuilder();
                //stringBuilder.Append(GenScriptStored(storedname));
                //stringBuilder.Append("<br><br>");
                //stringBuilder.Append(GenScriptStored("eClaim_TT130_4_12"));
                //stringBuilder.Append("<br><br>");

                //webBrowser6.DocumentText = stringBuilder.ToString();

                gridView15.BestFitColumns();
                gridView22.BestFitColumns();
                gridView23.BestFitColumns();
                gridView21.BestFitColumns();
                gridView24.BestFitColumns();

                //List<string> reqColumn = new List<string>();
                //foreach (var item in gridView15.Columns)
                //{
                //    if(item.)
                //}

            }
            else if (e.Button == navigationPage9.CustomHeaderButtons[1])  
            {
                if (string.IsNullOrEmpty(TIEPNHAN_ID.Text))
                {
                    MessagesBox("Chọn tiếp nhận đi", false);
                    return;
                }
                String queryString1 = "SELECT TIEPNHAN_ID ,MA_LK ,STT ,MA_BN ,HO_TEN ,SO_CCCD ,NGAY_SINH ,GIOI_TINH ,MA_THE_BHYT ,MA_DKBD ,GT_THE_TU ,GT_THE_DEN ,MA_DOITUONG_KCB ,NGAY_VAO ,NGAY_VAO_NOI_TRU ,LY_DO_VNT ,MA_LY_DO_VNT ,MA_LOAI_KCB ,MA_CSKCB ,MA_DICH_VU ,TEN_DICH_VU ,MA_THUOC ,TEN_THUOC ,MA_VAT_TU ,TEN_VAT_TU ,NGAY_YL ,DU_PHONG ,EXPORT ,SOLAN AS SOLAN_XUATFILE ,THOIGIANCHECKIN AS THOIGIAN_XUAT ,SOLAN_CHECKIN ,BENHAN_ID ,NGAYTAO FROM ECLAIM_CHECKIN where TIEPNHAN_ID = " + TIEPNHAN_ID.Text + " ORDER BY SOLAN_CHECKIN";
                DataTable source1 = DbTool.DbTool.Query(connection, queryString1, null);
                gridControl34.DataSource = source1;
                gridView35.BestFitColumns();
            }
            else if (e.Button == navigationPage9.CustomHeaderButtons[2])
            {
                string filePath = @"E:\file.xlsx";
                WorkSheetHelper.ExportGridViewsToExcel(gridView22,gridView23,filePath);
                //ExportDataGridViewsToExcel(dataGridView1, dataGridView2, filePath);
            }
        }

        private string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                CloseOutput = true
            };
            using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
        private string Beautify(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
                return null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            StringBuilder sb = new StringBuilder();
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                CloseOutput = true
            };
            using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
            {
                xmlDoc.Save(writer);
            }
            return sb.ToString();
        }
        private DataTable XmlToDataset(string xmlstring)
        {
            if (string.IsNullOrEmpty(xmlstring))
                return null;
            //xmlstring.Replace("","").Replace("","");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlstring);
            XmlReader xmlReader = new XmlNodeReader(xmlDoc);
            DataSet ds = new DataSet();
            ;
            ds.ReadXml(xmlReader);
            return ds.Tables[1];
        }
        private void button47_Click(object sender, EventArgs e)
        {
            Copyfile("DNDEV", "DN DEV");
        }

        private void button48_Click(object sender, EventArgs e)
        {
            Copyfile("DLDEV", "DL DEV");
        }

        //private void toggleSwitch2_Toggled(object sender, EventArgs e)
        //{
        //    string PATH_SOURCE = section.Hospitals.Getvalue("PATH_SOURCE").VALUE.ToString() + "LOGIN\\";
        //    string PATH_TARGET = section.Hospitals.Getvalue("VMLOGIN_TARGET").VALUE.ToString();

        //    PATH_SOURCE += toggleSwitch2.IsOn ? "VMLoginOn.cs" : "VMLoginOff.cs";
        //    string mess = toggleSwitch2.IsOn ? "AUTO LOGIN ON" : "AUTO LOGIN OFF";

        //    try
        //    {
        //        File.Copy(PATH_SOURCE, Path.Combine(PATH_TARGET, Path.GetFileName("VMLogin.cs")), true);
        //        this.MessagesBox(mess,true);
        //    }
        //    catch (IOException iox)
        //    {
        //        this.MessagesBox(iox.Message, false);
        //        return;
        //    }
        //}

        private void button49_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox8.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox5.Text))
                return;
            var rdcProcess = new Process
            {
                StartInfo =
            {
                FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe"),
                Arguments = String.Format(@"/generic:TERMSRV/{0} /user:{1} /pass:{2}",
                            textBox8.Text,
                            textBox2.Text,
                            textBox5.Text),
                            WindowStyle = ProcessWindowStyle.Hidden
            }
            };
            rdcProcess.Start();
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
            rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            rdcProcess.StartInfo.Arguments = String.Format("/f /v {0}", textBox8.Text);
            rdcProcess.Start();
        }

        private void toggleSwitch3_Toggled(object sender, EventArgs e)
        {
            var lstPRO = new List<ObjConnect>
            {
                new ObjConnect("SG_PRO", "Sài Gòn PRO"),

                new ObjConnect("MAT_PRO", "Mắt PRO"),

                new ObjConnect("VINH_PRO", "Vinh PRO"),

                new ObjConnect("DN_PRO", "Đà Nẵng PRO"),

                new ObjConnect("DL_PRO", "Đà Lạt PRO"),

                new ObjConnect("CL_PRO", "Cửu Long PRO"),

                new ObjConnect("VANPHUC_PRO", "Vạn Phúc 2 PRO"),
                new ObjConnect("THUDUC_PRO", "Thủ Đức PRO"),
                new ObjConnect("DONGNAI_PRO", "Đồng Nai PRO"),
                new ObjConnect("BINHDUONG_PRO", "Bình Dương PRO"),
                new ObjConnect("BINHDUONGC_PRO", "Bình Dương C PRO"),
                new ObjConnect("PDR_PRO", "Pandora PRO"),
                new ObjConnect("TMSGC_PRO", "TMSGC PRO"),
                new ObjConnect("HMMH_PRO", "Minh Hải PRO"),

            };
            var lstSTA = new List<ObjConnect>
            {

                new ObjConnect("SG_STA", "Sài Gòn STA"),

                new ObjConnect("MAT_STA", "Mắt STA"),

                new ObjConnect("VINH_STA", "Vinh STA"),

                new ObjConnect("DN_STA", "Đà Nẵng STA"),

                new ObjConnect("DL_STA", "Đà Lạt STA"),

                new ObjConnect("CL_STA", "Cửu Long STA"),

                new ObjConnect("VANPHUC_STA", "Vạn Phúc 2 STA"),
                new ObjConnect("THUDUC_STA", "Thủ Đức STA"),
                new ObjConnect("DONGNAI_STA", "Đồng Nai STA"),
                new ObjConnect("BINHDUONG_STA", "Bình Dương STA"),
                new ObjConnect("BINHDUONGC_STA", "Bình Dương C STA"),
                new ObjConnect("PDR_STA", "Pandora STA"),
                new ObjConnect("TMSGC_STA", "TMSGC STA"),
                new ObjConnect("HMMH_STA", "Minh Hải STA"),
            };


            comboBox2.DisplayMember = "display";
            comboBox2.ValueMember = "code";
            comboBox2.DataSource = toggleSwitch3.IsOn ? lstSTA : lstPRO;
            this.comboBox2.SelectedIndex = -1;
        }

        private void button50_Click(object sender, EventArgs e)
        {
            if (gridView9.RowCount == 0)
                return;
            string lstToathuoc_id = "";
            string thuoc = "";
            for (int i = 0; i < gridView9.DataRowCount; i++)
            {
                lstToathuoc_id += gridView9.GetRowCellValue(i, "TOATHUOC_ID").ToString() + ",";
                thuoc += gridView9.GetRowCellValue(i, "TENTHUOC").ToString() + "\n";

            }
            QuerySql obj = querySqls.Where(x => x.code.Equals("DELTHYL")).First();
            string queryString = obj.query.Replace("@SLTTOATHUOC_ID", lstToathuoc_id.Substring(0, lstToathuoc_id.Length - 1)).Replace('"', ' ');
            bool flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString);
            string mess = flag ? "ĐÃ XÓA" : "LỖI KHI XÓA";
            MessagesBox(mess, flag);

            button10_Click(null, null);
            System.Windows.Forms.Clipboard.SetText(thuoc);
        }

        private void gridControl12_Click(object sender, EventArgs e)
        {
            if (gridView12.GetSelectedRows().FirstOrDefault() >= 0)
            {
                var Khambenh_id = gridView12.GetRowCellValue(gridView12.GetSelectedRows().FirstOrDefault(), "KHAMBENH_ID").ToString();

                QuerySql obj = querySqls.Where(x => x.code.Equals("THUOCNGOAITRU")).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;

                param["KHAMBENH_ID"] = Khambenh_id;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl13.DataSource = source;
                gridControl14.DataSource = null;
                gridView13.Columns["LOAITOATHUOC"].Group();
            }
        }
        private void gridControl7_Click(object sender, EventArgs e)
        {
            if (gridView7.GetSelectedRows().FirstOrDefault() >= 0)
            {
                var server = from HospitalSetting s in section.Hospitals select s;
                string REPORT_FILE = gridView7.GetRowCellValue(gridView7.GetSelectedRows().FirstOrDefault(), "REPORT_FILE").ToString();
                string STORED_NAME = gridView7.GetRowCellValue(gridView7.GetSelectedRows().FirstOrDefault(), "PROCEDURE_FILE").ToString();
                string link = string.Empty;
                switch (MABENHVIEN)
                {
                    case "68038":
                        link = section.Hospitals.Getvalue("DL_PRO_REPORT").VALUE.ToString();
                        break;
                    case "92088":
                        link = section.Hospitals.Getvalue("CL_PRO_REPORT").VALUE.ToString();
                        break;
                    case "48072":
                        link = section.Hospitals.Getvalue("DN_PRO_REPORT").VALUE.ToString();
                        break;
                    case "79071":
                        link = section.Hospitals.Getvalue("SG_PRO_REPORT").VALUE.ToString();
                        break;
                    case "79976":
                        link = section.Hospitals.Getvalue("MAT_PRO_REPORT").VALUE.ToString();
                        break;
                    case "40574":
                        link = section.Hospitals.Getvalue("VINH_PRO_REPORT").VALUE.ToString();
                        break;
                    default:
                        link = server.Where(x => x.GROUP.Equals("INFOMATION") && x.KEY.EndsWith("REPORT") && x.BENHVIEN_ID.Equals(MABENHVIEN)).FirstOrDefault().VALUE;
                        break;
                }
                if (toggleSwitch1.IsOn)
                {
                    linkLabel1.Text = string.Empty;
                }
                else
                {
                    linkLabel1.Text = radioGroup3.SelectedIndex == 0 ? link + REPORT_FILE : string.Empty;
                }    
                textBox3.Text = GenScriptStored(STORED_NAME);
            }
            else
            {
                textBox3.Text = string.Empty;
            }
        }

        private void gridView30_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (e.Column.Name == "RUN")
            {
                string link = gridView30.GetRowCellValue(gridView30.GetSelectedRows().FirstOrDefault(), "VALUE").ToString();
                if (!string.IsNullOrEmpty(link))
                    Process.Start(link);
            }
            else if (e.Column.Name == "CONF")
            {
                string CODE = gridView30.GetRowCellValue(gridView30.GetSelectedRows().FirstOrDefault(), "KEY").ToString();
                string TEXT = gridView30.GetRowCellValue(gridView30.GetSelectedRows().FirstOrDefault(), "TEXT").ToString();
                Copyfile(CODE, TEXT);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var index = comboBox5.SelectedIndex;

            if (string.IsNullOrEmpty(textBox18.Text) && index != 4 && index >= 0)
            {
                MessagesBox("Nhập mã chứng từ", false);
                return;
            }

            if (string.IsNullOrEmpty(textEdit3.Text) && (index == 4 || index < 0))
            {
                MessagesBox("Nhập lô cần tìm", false);
                return;
            }
            string queryCode = string.Empty;
            gridView26.Columns.Clear();
            //0 Nhập - Xuất
            //1 Phiếu lĩnh
            //2 Ký gửi
            //3 Mua hàng
            //4 NNCC
            switch (index)
            {
                case 0:
                    queryCode = "GETCTBYMA0";
                    break;
                case 1:
                    queryCode = "GETCTBYMA1";
                    break;
                case 2:
                    queryCode = "GETCTBYMA2";
                    break;
                case 3:
                    queryCode = "GETCTBYMA3";
                    break;
                case 4:
                    queryCode = "GETCTBYMA4";
                    break;
                default:
                    queryCode = "GETSOLO";
                    break;
            }

            QuerySql obj = querySqls.Where(x => x.code.Equals(queryCode)).First();
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;
            param["MACHUNGTU"] = textBox18.Text;
            param["SOLONHAP"] = textEdit3.Text;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl26.DataSource = source;
            gridView26.BestFitColumns();
            if (gridView26.Columns.ColumnByName("CHUNGTU") != null)
                gridView26.Columns["CHUNGTU"].Group();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox18.Text))
            {
                MessagesBox("Nhập thông tin vô kìa", false);
                return;
            }
            if (!textBox18.Text.Contains("X13"))
            {
                MessagesBox("Không phải chứng từ xuất X13", false);
                return;
            }
            DialogResult dr = MessageBox.Show("Có chắn là muốn cập nhập diễn giải chứng từ xuất tự động không?", "Cập nhật diễn giải của chứng từ", MessageBoxButtons.YesNo);

            if (dr == DialogResult.No) { return; }
            if (dr == DialogResult.Yes)
            {
                string MACHUNGTU = textBox18.Text.Trim();
                string DIENGIAI = checkBox2.Checked ? string.Empty : "Xuất tự động từ Y lệnh thuốc";
                string query = string.Format("UPDATE TT_DUOC_CHUNGTU set DIENGIAINGHIEPVUPHATSINH = N'{0}', NGUOICAPNHAT_ID = 1, NGAYCAPNHAT = GETDATE() where MACHUNGTU = '{1}' and MUCDICHCHUNGTU_CODE = 'X13'", DIENGIAI, MACHUNGTU);

                bool flag = DbTool.DbTool.ExecuteNonQuery(connection, query);
                string mess = flag ? "ĐÃ CẬP NHẬT" : "LỖI KHI CẬP NHẬT";
                MessageBox.Show(mess);

            }


        }

        private void gridControl4_Click(object sender, EventArgs e)
        {
            if (gridView4.GetSelectedRows().FirstOrDefault() >= 0)
            {

                string action = string.Empty;

                string Loai = gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "MALOAI").ToString();
                string DVYEUCAU_ID = gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "DVYEUCAU_ID").ToString();
                string TRANGTHAI = gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "TRANGTHAI").ToString();
                comboBox3.SelectedIndex = comboBox3.Items.IndexOf(TRANGTHAI);
                switch (Loai)
                {
                    case "01":
                    case "02":
                    case "03":
                    case "07":
                        action = "Get_CLSKETQUA";
                        break;
                    case "05":
                    case "06":
                        action = "GET_PTTT";
                        break;
                    case "04":
                        if (gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "KHAMCK").TryConvertTo<bool>())
                        {
                            action = "GET_KHAMCK";
                        }
                        else
                        {
                            action = "GET_KB";
                        }
                        break;
                    default:
                        action = "GET_DVKKQ";
                        break;
                        
                }
                gridView11.Columns.Clear();
                if (string.IsNullOrEmpty(action))
                    return;
                QuerySql obj = querySqls.Where(x => x.code.Equals(action)).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = obj.param;


                param["DVYEUCAU_ID"] = DVYEUCAU_ID;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl11.DataSource = source;
            }
            else { gridControl11.DataSource = null; }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (gridView4.GetSelectedRows().Count() <= 0)
                return;
            if (string.IsNullOrEmpty(TIEPNHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }
            if (comboBox3.SelectedIndex < 0)
            {
                MessagesBox("Chưa chọn trạng thái", false);
                return;
            }
            FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
            param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
            param["TRANGTHAI"] = comboBox3.Text;
            param["DVYEUCAU_ID"] = gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "DVYEUCAU_ID").ToString();
            string query = string.Format("UPDATE TT_DVYEUCAU set TRANGTHAI = @TRANGTHAI WHERE DVYEUCAU_ID = @DVYEUCAU_ID AND TIEPNHAN_ID = @TIEPNHAN_ID");

            bool flag = DbTool.DbTool.ExecuteNonQuery(connection, query, param);
            if (comboBox3.Text == "CHUAKETQUA")
            {
                string query1 = string.Format("UPDATE TT_DVYEUCAU set DALAYMAU = '0' WHERE DVYEUCAU_ID = @DVYEUCAU_ID AND TIEPNHAN_ID = @TIEPNHAN_ID AND DALAYMAU = '1'");
                DbTool.DbTool.ExecuteNonQuery(connection, query1, param);
                log.InfoFormat("{0}: {1}", System.Environment.MachineName, query1);

                string query2 = string.Format("UPDATE TT_VIENPHI set DATHUCHIEN = '0' where REF_ID = @DVYEUCAU_ID AND REF_TABLE = 'TT_DVYEUCAU' AND TIEPNHAN_ID = @TIEPNHAN_ID");
                DbTool.DbTool.ExecuteNonQuery(connection, query2, param);
                log.InfoFormat("{0}: {1}", System.Environment.MachineName, query2);
            }
            
            string mess = flag ? "CẬP NHẬT THÀNH CÔNG" : "CÓ LỖI KHI CẬP NHẬT";
            MessagesBox(mess, flag);
            log.InfoFormat("{0}: {1}", System.Environment.MachineName, query);
            log.InfoFormat("PARAM: {0}", JsonConvert.SerializeObject(param));
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            if (gridView4.GetSelectedRows().Count() <= 0)
                return;
            if (string.IsNullOrEmpty(TIEPNHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }
            DialogResult dr = MessageBox.Show(string.Format("Có chắn là muốn cập nhật {0} dòng chưa?", gridView4.GetSelectedRows().Count()),
                     "CHẮC CHƯA ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) { return; }
            if (dr == DialogResult.Yes)
            {
                var a = gridView4.GetSelectedRows();
                string DvyeucauID = string.Empty;
                string query = string.Format("update TT_DVYEUCAU set DUOCPHEPTHUCHIEN = '1' WHERE TIEPNHAN_ID = {0} AND DVYEUCAU_ID IN (@LSTID)", TIEPNHAN_ID.Text);
                foreach (var i in a)
                {
                    DvyeucauID += gridView4.GetRowCellValue(i, "DVYEUCAU_ID").ToString() + ",";
                }
                string queryString = query.Replace("@LSTID", DvyeucauID.Substring(0, DvyeucauID.Length - 1)).Replace('"', ' ');
                bool flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString);
                string mess = flag ? "CẬP NHẬT THÀNH CÔNG" : "CÓ LỖI KHI CẬP NHẬT";
                MessagesBox(mess, flag);
                log.InfoFormat("{0}: {1}", System.Environment.MachineName,queryString);
            }

        }

        private void btn_LayKho_Click(object sender, EventArgs e)
        {
            String queryString1 = "SELECT tk.KHODUOC_ID AS ID ,tk.MAKHO ,tk.TENKHO,ld.DICTIONARY_NAME AS LOAIKHO FROM dbo.TM_KHODUOC AS tk LEFT join LST_DICTIONARY ld on tk.LOAIKHO_ID = ld.DICTIONARY_ID and ld.DICTIONARY_TYPE_CODE = 'LoaiKhoDuoc' WHERE tk.TAMNGUNG != 1  ORDER BY tk.KHODUOC_ID";
            DataTable source1 = DbTool.DbTool.Query(connection, queryString1, null);
            slk_kho.Properties.DataSource = source1;
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(slk_kho.EditValue.ToString()))
            {
                MessagesBox("Chưa chọn kho", false);
                return;
            }
            if (string.IsNullOrEmpty(Maduoctxt.Text))
            {
                MessagesBox("Chưa nhập mã dược", false);
                return;
            }

            QuerySql obj = querySqls.Where(x => x.code.Equals("MERGTONKHO")).First();
            FPT.Framework.Data.DataObject param = obj.param;
            param["MADUOC"] = Maduoctxt.Text;
            param["KHODUOC_ID"] = slk_kho.EditValue.ToString();

            bool flag = DbTool.DbTool.ExecuteNonQuery(connection, obj.query, param);
            string mess = flag ? "ĐÃ CẬP NHẬT, LOAD LẠI TỒN KHO" : "LỖI KHI CẬP NHẬT";
            MessagesBox(mess, flag);
        }

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    //var index = comboBox6.SelectedIndex;
        //    string queryString = "SELECT KB.KHAMBENH_ID AS KB_ID,FORMAT(case WHEN kb.ISYLENHVTYT = '1' THEN KB.THOIGIANBATDAU else  KB.THOIGIANKHAM END,'dd/MM/yyyy HH:mm:ss') THOIGIAN,tnt.TIEPNHAN_ID,tb.MAYTE,KB.TRANGTHAIYLENH AS TT, tnt.SOLUONGTHUCLINH AS SL FROM TT_NOITRU_KHAMBENH KB LEFT JOIN TT_NOITRU_TOATHUOC tnt ON KB.KHAMBENH_ID = tnt.KHAMBENH_ID LEFT join TT_TIEPNHAN tt on tnt.TIEPNHAN_ID = tt.TIEPNHAN_ID LEFT join TT_BENHNHAN tb ON tt.BENHNHAN_ID = tb.BENHNHAN_ID WHERE tnt.TOATHUOC_ID = @REF_ID";
        //    FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
        //    param["REF_ID"] = textBox19.Text;
        //    param["REF_TABLE"] = "";

        //    DataTable source = DbTool.DbTool.Query(connection, queryString, param);
        //    gridControl30.DataSource = source;
        //    gridView31.BestFitColumns();

        //}

        private void slk_kho_EditValueChanged(object sender, EventArgs e)
        {
            if (slk_kho.EditValue + "" == "")
            {
                txtKho.Text = "";

            }
            else
                txtKho.Text = slk_kho.EditValue.ToString();
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtKho.Text) && string.IsNullOrEmpty(Maduoctxt.Text))
            {
                MessagesBox("Chọn kho hoặc dược", false);
                return;
            }
            gridView33.Columns.Clear();

            QuerySql obj = querySqls.Where(x => x.code.Equals("GETSLTONKHO")).First();
            FPT.Framework.Data.DataObject param = obj.param;
            param["MADUOC"] = string.IsNullOrEmpty(Maduoctxt.Text) ? null : Maduoctxt.Text;
            param["KHODUOC_ID"] = string.IsNullOrEmpty(txtKho.Text) ? null : slk_kho.EditValue.ToString();


            string queryString = obj.query + (checkBox1.Checked ? " and KD.SOLUONG > 0 " : "");
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl32.DataSource = source;

            gridView33.BestFitColumns();
            gridView33.Columns["TENDUOCDAYDU"].Group();

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                string queryString = "SELECT DICTIONARY_ID,DICTIONARY_NAME FROM LST_DICTIONARY where DICTIONARY_TYPE_CODE = 'GiaiQuyetKhamBenh' and ENABLED = '1' ORDER BY IDX";
                DataTable source = DbTool.DbTool.Query(connection, queryString, null);

                comboBox4.DataSource = source;
                comboBox4.DisplayMember = "DICTIONARY_NAME";
                comboBox4.ValueMember = "DICTIONARY_ID";
            }
            else { comboBox4.DataSource = null;}
        }

        private void simpleButton11_Click(object sender, EventArgs e)
        {
            if (gridView4.SelectedRowsCount == 0)
            {
                MessagesBox("Hãy chọn công khám cần cập nhật", false);
                return;
            }
            if(!gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "MALOAI").ToString().Equals("04"))
            {
                MessagesBox("Dịch vụ đang chọn không phải khám bệnh", false);
                return;
            }
            if(comboBox4.SelectedIndex < 0)
            {
                MessagesBox("Chưa chọn hướng cần cập nhật", false);
                return;
            }
            DialogResult dr = MessageBox.Show("Đã kiểm tra kĩ dữ liệu trước khi cập nhật chưa?", "Cập nhật hướng giải quyết khám bệnh", MessageBoxButtons.YesNo);

            if (dr == DialogResult.No) { return; }
            if (dr == DialogResult.Yes)
            {
                string ID = comboBox4.SelectedValue.ToString();
                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param["HUONGGIAIQUYET_ID"] = ID;
                param["TIEPNHAN_ID"] = TIEPNHAN_ID.Text;
                param["DVYEUCAU_ID"] = gridView4.GetRowCellValue(gridView4.GetSelectedRows().FirstOrDefault(), "DVYEUCAU_ID").ToString();

                string query = "UPDATE TT_NGOAITRU_KHAMBENH SET HUONGGIAIQUYET_ID =  @HUONGGIAIQUYET_ID where TIEPNHAN_ID = @TIEPNHAN_ID AND KHAMBENH_ID = (SELECT TOP 1  KHAMBENH_NGOAITRU_ID FROM TT_DVYEUCAU where DVYEUCAU_ID = @DVYEUCAU_ID AND TIEPNHAN_ID = @TIEPNHAN_ID)";

                bool flag = DbTool.DbTool.ExecuteNonQuery(connection, query, param);
                string mess = flag ? "ĐÃ CẬP NHẬT" : "LỖI KHI CẬP NHẬT";
                MessageBox.Show(mess);
                gridView11.Columns.Clear();
                log.InfoFormat("{0}: {1}", System.Environment.MachineName, query);
                log.InfoFormat("PARAM: {0}", JsonConvert.SerializeObject(param));
            }

        }
        private void gridView2_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            CustomGridView.Instance.CustomDrawRowIndicalor(e);
        }

        private void gridView4_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            CustomGridView.Instance.CustomDrawRowIndicalor(e);
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            gridView28.Columns.Clear();
            gridView6.Columns.Clear();

            if (string.IsNullOrEmpty(slk_kho.EditValue.ToString()))
            {
                MessagesBox("Chưa chọn kho", false);
                return;
            }
            if(string.IsNullOrEmpty(Maduoctxt.Text))
            {
                MessagesBox("Chưa nhập mã dược", false);
                return;
            }
            QuerySql obj = querySqls.Where(x => x.code.Equals("TIMTHUOCLECH")).First();
            FPT.Framework.Data.DataObject param = obj.param;


            param["SOLONHAP_ID"] = null;
            param["MADUOC"] = Maduoctxt.Text;
            param["KHOXUAT_ID"] = txtKho.Text;


            string queryString = obj.query + (checknhaptx.Checked ? " WHERE (x.soluong+ISNULL(t.soluong,0))<>n.soluong " : "");
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl28.DataSource = source;
        }

        private void gridControl28_Click(object sender, EventArgs e)
        {
            if (gridView28.FocusedRowHandle >= 0)
            {
                string TOATHUOCNOITRU_ID = gridView28.GetRowCellValue(gridView28.FocusedRowHandle, "TOATHUOCNOITRU_ID").ToString();

                QuerySql obj = querySqls.Where(x => x.code.Equals("sTHUOCBYCT")).First();
                FPT.Framework.Data.DataObject param = obj.param;
                param["SLTTOATHUOC_ID"] = TOATHUOCNOITRU_ID;
                string queryString = obj.query;
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl6.DataSource = source;

            }
            else
            {
                gridControl6.DataSource = null;
            }
        }

        private void gridControl32_Click(object sender, EventArgs e)
        {
            if (gridView33.SelectedRowsCount != 0)
            {
                string HOSOTHAU_CHITIET_ID = string.Empty;

                foreach (var i in gridView33.GetSelectedRows())
                {
                    if (i < 0) continue;
                    if (string.IsNullOrEmpty(gridView33.GetRowCellValue(i, "HOSOTHAU_CHITIET_ID").ToString())) continue;

                    HOSOTHAU_CHITIET_ID += gridView33.GetRowCellValue(i, "HOSOTHAU_CHITIET_ID").ToString() + ",";
                }
                if (string.IsNullOrEmpty(HOSOTHAU_CHITIET_ID))
                {
                    return;
                }
                string queryString = string.Format("SELECT HST.THAU_ID,HST.HOSOTHAU_ID,tt.SOQUYETDINH,HST.LOAITHAU,HSTCT.DUOC_ID" +
                    ",HSTCT.DONGIATHAU,HSTCT.DONGIAMUA,HSTCT.BHYT,HSTCT.TENBHYT,HSTCT.MA_HOATCHAT_PL05,HSTCT.THONGTINTHAU FROM  " +
                    "TT_HOSOTHAU_CHITIET HSTCT INNER JOIN TT_HOSOTHAU HST ON HSTCT.HOSOTHAU_ID = HST.HOSOTHAU_ID " +
                    "INNER JOIN TM_THAU tt on HST.THAU_ID = tt.THAU_ID where HSTCT.HOSOTHAU_CHITIET_ID in ({0})", HOSOTHAU_CHITIET_ID.Substring(0, HOSOTHAU_CHITIET_ID.Length - 1));
                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param["HOSOTHAU_CHITIET_ID"] = HOSOTHAU_CHITIET_ID;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl25.DataSource = source;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Text = checkBox2.Checked ? "Bỏ xuất tự động" : "Xuất tự động";
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            Process notePad = new Process();
            notePad.StartInfo.FileName = "WinMerge.exe";
            notePad.StartInfo.Arguments ="trung";
            notePad.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FrmNoteUpdate f2 = new FrmNoteUpdate();
            DevExpress.XtraEditors.XtraDialog.Show(f2, "Có gì mới", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            QuerySql obj = querySqls.Where(x => x.code.Equals("THUOCCHUALINH")).First();
            if (obj == null || string.IsNullOrEmpty(TIEPNHAN_ID.Text))
            {
                MessagesBox("Chọn tiếp nhận đi", false);
                return;
            }
            if (gridView8.GetSelectedRows().Count() <= 0)
            {
                return;
            }

            gridControl10.DataSource = null;
            gridView9.Columns.Clear();
            gridView10.Columns.Clear();
            string queryString = obj.query;
            FPT.Framework.Data.DataObject param = obj.param;

            param["KHAMBENH_ID"] = gridView8.GetRowCellValue(gridView8.GetSelectedRows().FirstOrDefault(), "KHAMBENH_ID").ToString();
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl9.DataSource = source;
        }

        private void simpleButton9_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(slk_kho.EditValue.ToString()))
            {
                MessagesBox("Chưa chọn kho", false);
                return;
            }
            if (string.IsNullOrEmpty(Maduoctxt.Text))
            {
                MessagesBox("Chưa nhập mã dược", false);
                return;
            }
            if (checkBox4.Checked)
            {
                QuerySql obj = querySqls.Where(x => x.code.Equals("SUMCHUNGTU")).First();
                FPT.Framework.Data.DataObject param = obj.param;
                param["MADUOC"] = Maduoctxt.Text;
                param["KHODUOC_ID"] = slk_kho.EditValue.ToString();
                param["SOLONHAP"] = textEdit4.Text;
                string queryString = obj.query;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl33.DataSource = source;
            }
            else
            {
                if (!dateTimePicker2.Checked || dateTimePicker3.Checked == null)
                {
                    MessagesBox("Chưa chọn thời gian", false);
                    return;
                }
                QuerySql obj = querySqls.Where(x => x.code.Equals("SLISTCHUNGTU")).First();
                FPT.Framework.Data.DataObject param = obj.param;
                param["MADUOC"] = Maduoctxt.Text;
                param["KHODUOC_ID"] = slk_kho.EditValue.ToString();
                param["SOLONHAP"] = textEdit4.Text;
                param["TUNGAY"] = dateTimePicker2.Value;
                param["DENNGAY"] = dateTimePicker3.Value;
                string queryString = obj.query;

                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl33.DataSource = source;
            }
        }

        private void gridView3_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            CustomGridView.Instance.CustomDrawRowIndicalor(e);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            panel4.Visible = !checkBox4.Checked;

            if (!dateTimePicker2.Checked)
                dateTimePicker2.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!dateTimePicker3.Checked)
                dateTimePicker3.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
        }

        private void gridControl26_Click(object sender, EventArgs e)
        {
            if (gridView26.GetSelectedRows().FirstOrDefault() >= 0)
            {
                string code = string.Empty;
                string ref_table = gridView26.GetRowCellValue(gridView26.GetSelectedRows().FirstOrDefault(), "REF_TABLE").ToString();
                switch (ref_table)
                {
                    case "TT_NOITRU_TOATHUOC":
                        code = "THUOCNT_BYCT";
                        break;
                    case "TT_NGOAITRU_TOATHUOC":
                        code = "THUOCNGT_BYCT";
                        break;
                    case "TT_PHAUTHUAT_VTYT":
                        code = "THUOCPT_BYCT";
                        break;
                    case "TT_CLSKETQUA_VTYT":
                        code = "THUOCCLS_BYCT";
                        break;
                }

                if (string.IsNullOrEmpty(code))
                {
                    gridControl30.DataSource = null;
                    return;
                }

                QuerySql obj = querySqls.Where(x => x.code.Equals(code)).First();
                string queryString = obj.query;
                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param["REF_ID"] = gridView26.GetRowCellValue(gridView26.GetSelectedRows().FirstOrDefault(), "REF_ID").ToString();
                param["REF_TABLE"] = ref_table;


                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                gridControl30.DataSource = source;
                gridView31.BestFitColumns();
            }
            else { gridControl30.DataSource = null; }
        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            InfoBenhnhan form = new InfoBenhnhan
            {
                //SQuery = lstquery,
                connection = connection
            };
            form.Show();
        }
    }
}
