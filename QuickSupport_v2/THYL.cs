using QuickSupport_v2.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace QuickSupport_v2
{
    public partial class THYL : DevExpress.XtraEditors.XtraForm
    {
        public string TOATHUOC_ID { get; set; }
        public string BENHVIEN_ID { get; set; }
        public List<QuerySql> SQuery { get; set; }
        public SqlConnection connection { get; set; }
        public THYL()
        {
            InitializeComponent();
        }
        private void THYL_Load(object sender, EventArgs e)
        {
            string queryString = SQuery[0].query;
            FPT.Framework.Data.DataObject param = SQuery[0].param;
            param["TOATHUOC_ID"] = TOATHUOC_ID;

            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl1.DataSource = source;

            string queryString1 = SQuery[1].query;
            DataTable source1 = DbTool.DbTool.Query(connection, queryString1, param);
            gridControl8.DataSource = source1;

            string queryString2 = SQuery[2].query;
            DataTable source2 = DbTool.DbTool.Query(connection, queryString2, param);
            gridControl2.DataSource = source2;

            string queryString3 = SQuery[3].query;
            DataTable source3 = DbTool.DbTool.Query(connection, queryString3, param);
            gridControl3.DataSource = source3;
        }

        private void button49_Click(object sender, EventArgs e)
        {
            if (gridView1.RowCount == 0 && string.IsNullOrEmpty(textBox7.Text))
            {
                MessageBox.Show("Nhập người thực hiện vô");
                return;
            }
            string queryString3 = SQuery[4].query;

            FPT.Framework.Data.DataObject param = SQuery[4].param;
            param["TOATHUOC_ID"] = TOATHUOC_ID;
            param["BENHVIEN_ID"] = BENHVIEN_ID;
            param["NGUOITHUCHIEN"] = textBox7.Text;
            try
            {
                DbTool.DbTool.ExcuteStored(connection, queryString3, param);

                DataTable source = DbTool.DbTool.Query(connection, SQuery[0].query, new FPT.Framework.Data.DataObject() { ["TOATHUOC_ID"] = TOATHUOC_ID });
                gridControl1.DataSource = source;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string thyl_ID = string.Empty;
            var collection = gridView1.GetSelectedRows();
            if (collection.Count() > 0)
            {
                if (gridView1.GetRowCellValue(collection.First(), "TRANGTHAI").ToString().Equals("NGUNGSUDUNG"))
                {
                    MessageBox.Show("KHÔNG CẦN KIỂM TRA DỮ LIỆU NGƯNG THUỐC (KHÔNG XÓA ĐƯỢC DÒNG NÀY) !");
                    return;
                }
                thyl_ID = gridView1.GetRowCellValue(collection.First(), "ID").ToString();

                FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
                param.Add("THYL_ID", thyl_ID);
                param.Add("TOATHUOC_ID", TOATHUOC_ID);
                string queryString = "DECLARE @DIENGIAI NVARCHAR(50) DECLARE @NGAYTAO DATETIME DECLARE @LIEUTHUCHIEN INT DECLARE @KHAMBENH_ID INT = (SELECT top 1 KHAMBENH_ID FROM TT_NOITRU_TOATHUOC WHERE TOATHUOC_ID = @TOATHUOC_ID) SELECT @NGAYTAO =NGAYTAO, @LIEUTHUCHIEN = a.LIEUTHUCHIEN,@DIENGIAI =CONCAT(N'Liều ',LIEUTHUCHIEN,N' trùng thời gian với các thuốc: ') FROM TT_NOITRU_THUCHIENYLENH a WHERE a.THYL_ID  = @THYL_ID SELECT @DIENGIAI AS DIENGIAI,td.TENDUOCDAYDU,CONVERT(VARCHAR(5),CAST(a.THOIGIANTHUCHIEN AS TIME), 108) AS TGTHUCHIEN,LIEUTHUCHIEN as LIEU, a.TOATHUOC_ID FROM TT_NOITRU_THUCHIENYLENH a INNER JOIN TT_NOITRU_TOATHUOC tnt ON a.TOATHUOC_ID = tnt.TOATHUOC_ID INNER JOIN TM_DUOC td on tnt.DUOC_ID = td.DUOC_ID where a.KHAMBENH_ID = @KHAMBENH_ID AND CONVERT(VARCHAR,a.NGAYTAO,120) = CONVERT(VARCHAR,@NGAYTAO,120) AND a.TOATHUOC_ID <> @TOATHUOC_ID";
                DataTable source = DbTool.DbTool.Query(connection, queryString, param);
                THYL_CUNGTG f2 = new THYL_CUNGTG
                {
                    data = source,
                };
                DevExpress.XtraEditors.XtraDialog.Show(f2, "Các thuốc có cùng thời gian thực hiện", MessageBoxButtons.OK);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TOATHUOC_ID))
                return;
            var collection = gridView1.GetSelectedRows();
            if (collection.Count() > 0)
            {
                DialogResult dr = MessageBox.Show(string.Format("Có chắn là muốn xóa chưa {0} dòng chưa?", collection.Count()),
                      "Xóa thực hiện y lệnh", MessageBoxButtons.YesNo);

                if (dr == DialogResult.No) { return; }
                if (dr == DialogResult.Yes)
                {
                    string lstthyl_ID = string.Empty;
                    string query = string.Format("DELETE TT_NOITRU_THUCHIENYLENH WHERE THYL_ID IN (@LSTID) AND TRANGTHAITHUCHIENYLENH != 'NGUNGSUDUNG' AND TOATHUOC_ID = {0}", TOATHUOC_ID);
                    foreach (var i in collection)
                    {
                        lstthyl_ID += gridView1.GetRowCellValue(i, "ID").ToString() + ",";
                    }
                    string queryString = query.Replace("@LSTID", lstthyl_ID.Substring(0, lstthyl_ID.Length - 1)).Replace('"', ' ');
                    bool flag = DbTool.DbTool.ExecuteNonQuery(connection, queryString);
                    string mess = flag ? "ĐÃ XÓA" : "LỖI KHI XÓA";
                    MessageBox.Show(mess);

                    FPT.Framework.Data.DataObject param = SQuery[0].param;
                    param["TOATHUOC_ID"] = TOATHUOC_ID;

                    DataTable source = DbTool.DbTool.Query(connection, SQuery[0].query, param);
                    gridControl1.DataSource = source;
                }
            }
        }
    }
}