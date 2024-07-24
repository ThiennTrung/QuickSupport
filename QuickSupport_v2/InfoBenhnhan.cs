using DevExpress.XtraEditors;
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
        public InfoBenhnhan()
        {
            InitializeComponent();
           

        }

        private void InfoBenhnhan_Load(object sender, EventArgs e)
        {
            string queryString = "SELECT BN.MAYTE ,BN.TENBENHNHAN ,G.DESCRIPTION AS  GIOITINH ,CASE WHEN BN.NGAYGIOSINH is NOT NULL THEN convert(VARCHAR(10),BN.NGAYGIOSINH,103) ELSE cast(BN.NAMSINH AS VARCHAR(10)) END AS  NGAYSINH ,dbo.TinhTuoi_V3(BN.NAMSINH,BN.NGAYGIOSINH) AS TUOI ,BN.SODIENTHOAI ,BN.SONHA ,BN.DIACHI ,BN.DIACHITHUONGTRU ,BN.DIACHILIENLAC ,BN.XAPHUONG_ID ,ld.DICTIONARY_NAME AS NGHENGHIEP ,ld1.DICTIONARY_NAME AS DANTOC ,ld2.DICTIONARY_NAME AS QUOCGIA ,ld3.DICTIONARY_NAME as NHOMMAU ,ld4.DICTIONARY_NAME AS HONNHAN ,BN.CMND ,FORMAT(BN.NGAYCAPCMND, 'dd/MM/yyyy HH:mm:ss') as NGAYCAPCMND ,BN.NOICAPGIAYTO_TEXT FROM TT_BENHNHAN BN INNER JOIN TM_GENDER G on BN.GIOITINH = G.ID LEFT join LST_DICTIONARY ld ON BN.NGHENGHIEP_ID = ld.DICTIONARY_ID and ld.DICTIONARY_TYPE_CODE = 'NgheNghiep' LEFT JOIN LST_DICTIONARY ld1 ON BN.DANTOC_ID = ld1.DICTIONARY_ID and ld1.DICTIONARY_TYPE_CODE = 'DanToc' LEFT JOIN LST_DICTIONARY ld2 ON BN.QUOCTICH_ID = ld2.DICTIONARY_ID and ld2.DICTIONARY_TYPE_CODE = 'QuocGia' LEFT JOIN LST_DICTIONARY ld3 on BN.NHOMMAU_ID = ld3.DICTIONARY_ID and ld3.DICTIONARY_TYPE_CODE = 'NhomMau' LEFT join LST_DICTIONARY ld4 ON BN.TINHTRANGHONNHAN_ID = ld4.DICTIONARY_ID where BN.BENHNHAN_ID  =362339";
            //FPT.Framework.Data.DataObject param = new FPT.Framework.Data.DataObject();
            //param["TOATHUOC_ID"] = null;

            DataTable source = DbTool.DbTool.Query(connection, queryString, null);
            gridControl1.DataSource = source;

        }
    }
}