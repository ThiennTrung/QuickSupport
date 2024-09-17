using DevExpress.XtraGrid.Views.Grid;
using QuickSupport_v2.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSupport_v2.Function
{
    static class Helper
    {
        public static void BindingData2gridview(SqlConnection connection,List<QuerySql> querySqls, DevExpress.XtraGrid.GridControl gridControl
            , FPT.Framework.Data.DataObject param, string KeyQuery, bool BestFitColumns = true)
        {
            var MainView = gridControl.MainView as GridView;
            MainView.Columns.Clear();
            QuerySql obj = querySqls.Where(x => x.code.Equals(KeyQuery)).First();
            string queryString = obj.query;
            DataTable source = DbTool.DbTool.Query(connection, queryString, param);
            gridControl.DataSource = source;
            if (BestFitColumns)
            {
                MainView.BestFitColumns();
            }
        }
    }
}
