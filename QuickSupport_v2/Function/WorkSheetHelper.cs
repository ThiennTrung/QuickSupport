using DevExpress.Spreadsheet;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms;

namespace QuickSupport_v2.Function
{
    public static class WorkSheetHelper
    {
        public static void ExportGridViewsToExcel(GridView gridView1, GridView gridView2, string filePath)
        {
            // Create a new workbook
            Workbook workbook = new Workbook();

            // Add and populate the first sheet
            Worksheet sheet1 = workbook.Worksheets.Add("Sheet11");
            PopulateWorksheetFromGridView(sheet1, gridView1);

            // Add and populate the second sheet
            Worksheet sheet2 = workbook.Worksheets.Add("Sheet22");
            PopulateWorksheetFromGridView(sheet2, gridView2);

            // Save the workbook
            workbook.SaveDocument(filePath, DocumentFormat.Xlsx);
            MessageBox.Show("Excel file created successfully at " + filePath);
        }

        public static void PopulateWorksheetFromGridView(Worksheet sheet, GridView gridView)
        {
            // Populate headers
            for (int col = 0; col < gridView.Columns.Count; col++)
            {
                sheet.Cells[0, col].Value = gridView.Columns[col].FieldName;
                sheet.Cells[0, col].FillColor = System.Drawing.Color.LightGray;
                sheet.Cells[0, col].Font.Bold = true;
            }

            // Populate data
            for (int row = 0; row < gridView.RowCount; row++)
            {
                for (int col = 0; col < gridView.Columns.Count; col++)
                {
                    object cellValue = gridView.GetRowCellValue(row, gridView.Columns[col]);
                    sheet.Cells[row + 1, col].Value = cellValue?.ToString();
                }
            }
        }

    }
}
