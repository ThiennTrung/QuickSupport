using DevExpress.XtraGrid.Views.Grid;

namespace QuickSupport_v2
{
    public class CustomGridView
    {
        private static CustomGridView _instance;
        private CustomGridView() { }
        public static CustomGridView Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new CustomGridView();
                return _instance;
            }
            private set { _instance = value; }
        }
        public void CustomDrawRowIndicalor(RowIndicatorCustomDrawEventArgs e)
        {
            if (!e.Info.IsRowIndicator || e.RowHandle < 0) return;
            e.Info.DisplayText = (e.RowHandle + 1).ToString();
        }
    }
}
