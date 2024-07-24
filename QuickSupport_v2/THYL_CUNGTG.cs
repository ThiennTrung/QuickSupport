using System;
using System.Data;

namespace QuickSupport_v2
{
    public partial class THYL_CUNGTG : DevExpress.XtraEditors.XtraUserControl
    {
        public DataTable data { get; set; }
        public THYL_CUNGTG()
        {
            InitializeComponent();

        }
        private void THYL_CUNGTG_Load(object sender, EventArgs e)
        {
            gridControl1.DataSource = data;
            gridView1.Columns["DIENGIAI"].Group();
        }
    }
}
