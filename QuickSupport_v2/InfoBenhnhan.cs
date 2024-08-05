using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using QuickSupport_v2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickSupport_v2
{
    public partial class InfoBenhnhan : DevExpress.XtraEditors.XtraForm
    {
        public List<QuerySql> SQuery { get; set; }
        public SqlConnection connection { get; set; }

        public int BENHNHAN_ID { get; set; }
        public int TIEPNHAN_ID { get; set; }
        public int BENHAN_ID { get; set; }

        public InfoBenhnhan()
        {
            InitializeComponent();
           

        }

        private void InfoBenhnhan_Load(object sender, EventArgs e)
        {
            FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
            param["BENHNHAN_ID"] = BENHNHAN_ID;
            param["TIEPNHAN_ID"] = TIEPNHAN_ID;
            param["BENHAN_ID"] = BENHAN_ID;

            // bệnh nhân
            QuerySql obj = SQuery.Where(x => x.code.Equals("CHITIETBENHNHAN")).First();
            string queryString = obj.query;


            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl2.DataSource = source;
            RepositoryItemMemoEdit riMemoEdit = new RepositoryItemMemoEdit();
            riMemoEdit.WordWrap = true;
            gridControl2.RepositoryItems.Add(riMemoEdit);
            layoutView2.Columns["DIACHILIENLAC"].ColumnEdit = riMemoEdit;
            layoutView2.Columns["DIACHITHUONGTRU"].ColumnEdit = riMemoEdit;
            layoutView2.Columns["DIACHI"].ColumnEdit = riMemoEdit;


            // TIẾP nhận
            QuerySql obj1 = SQuery.Where(x => x.code.Equals("CHITIETTIEPNHAN")).First();
            string queryString1 = obj1.query;
            gridControl1.DataSource = DbTool.DbTool.Query(connection, queryString1, param);
        }
    }
}