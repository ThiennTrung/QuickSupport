namespace QuickSupport_v2
{
    partial class InfoBenhnhan
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.layoutView2 = new DevExpress.XtraGrid.Views.Layout.LayoutView();
            this.layoutViewCard2 = new DevExpress.XtraGrid.Views.Layout.LayoutViewCard();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.layoutView1 = new DevExpress.XtraGrid.Views.Layout.LayoutView();
            this.layoutViewCard1 = new DevExpress.XtraGrid.Views.Layout.LayoutViewCard();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutViewCard2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutViewCard1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.panel1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1057, 646);
            this.panelControl1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gridControl2);
            this.panel1.Controls.Add(this.gridControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(430, 642);
            this.panel1.TabIndex = 1;
            // 
            // gridControl2
            // 
            this.gridControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl2.Location = new System.Drawing.Point(0, 0);
            this.gridControl2.MainView = this.layoutView2;
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(430, 446);
            this.gridControl2.TabIndex = 2;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.layoutView2});
            // 
            // layoutView2
            // 
            this.layoutView2.Appearance.CardCaption.ForeColor = System.Drawing.Color.Black;
            this.layoutView2.Appearance.CardCaption.Options.UseForeColor = true;
            this.layoutView2.Appearance.FieldValue.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.layoutView2.Appearance.FieldValue.ForeColor = System.Drawing.Color.Blue;
            this.layoutView2.Appearance.FieldValue.Options.UseFont = true;
            this.layoutView2.Appearance.FieldValue.Options.UseForeColor = true;
            this.layoutView2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.layoutView2.CardMinSize = new System.Drawing.Size(340, 350);
            this.layoutView2.DetailHeight = 800;
            this.layoutView2.GridControl = this.gridControl2;
            this.layoutView2.Name = "layoutView2";
            this.layoutView2.OptionsBehavior.AllowExpandCollapse = false;
            this.layoutView2.OptionsBehavior.AllowRuntimeCustomization = false;
            this.layoutView2.OptionsBehavior.Editable = false;
            this.layoutView2.OptionsBehavior.ReadOnly = true;
            this.layoutView2.OptionsBehavior.ScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Auto;
            this.layoutView2.OptionsCustomization.AllowFilter = false;
            this.layoutView2.OptionsCustomization.AllowSort = false;
            this.layoutView2.OptionsCustomization.ShowGroupCardCaptions = false;
            this.layoutView2.OptionsCustomization.ShowGroupCardIndents = false;
            this.layoutView2.OptionsCustomization.ShowGroupCards = false;
            this.layoutView2.OptionsCustomization.ShowGroupFields = false;
            this.layoutView2.OptionsCustomization.ShowGroupHiddenItems = false;
            this.layoutView2.OptionsCustomization.ShowGroupLayout = false;
            this.layoutView2.OptionsCustomization.ShowGroupLayoutTreeView = false;
            this.layoutView2.OptionsCustomization.ShowGroupView = false;
            this.layoutView2.OptionsCustomization.ShowResetShrinkButtons = false;
            this.layoutView2.OptionsCustomization.ShowSaveLoadLayoutButtons = false;
            this.layoutView2.OptionsMultiRecordMode.StretchCardToViewHeight = true;
            this.layoutView2.OptionsMultiRecordMode.StretchCardToViewWidth = true;
            this.layoutView2.OptionsSelection.MultiSelect = true;
            this.layoutView2.OptionsView.PartialCardsSimpleScrolling = DevExpress.Utils.DefaultBoolean.True;
            this.layoutView2.OptionsView.ShowCardCaption = false;
            this.layoutView2.OptionsView.ShowCardExpandButton = false;
            this.layoutView2.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.layoutView2.OptionsView.ShowViewCaption = true;
            this.layoutView2.TemplateCard = this.layoutViewCard2;
            this.layoutView2.ViewCaption = "THÔNG TIN BỆNH NHÂN";
            // 
            // layoutViewCard2
            // 
            this.layoutViewCard2.CustomizationFormText = "TemplateCard";
            this.layoutViewCard2.GroupBordersVisible = false;
            this.layoutViewCard2.HeaderButtonsLocation = DevExpress.Utils.GroupElementLocation.AfterText;
            this.layoutViewCard2.Name = "layoutViewTemplateCard";
            this.layoutViewCard2.OptionsItemText.TextToControlDistance = 5;
            this.layoutViewCard2.Text = "TemplateCard";
            this.layoutViewCard2.TextLocation = DevExpress.Utils.Locations.Default;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridControl1.Location = new System.Drawing.Point(0, 446);
            this.gridControl1.MainView = this.layoutView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(430, 196);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.layoutView1});
            // 
            // layoutView1
            // 
            this.layoutView1.Appearance.CardCaption.ForeColor = System.Drawing.Color.Black;
            this.layoutView1.Appearance.CardCaption.Options.UseForeColor = true;
            this.layoutView1.Appearance.FieldValue.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.layoutView1.Appearance.FieldValue.ForeColor = System.Drawing.Color.Blue;
            this.layoutView1.Appearance.FieldValue.Options.UseFont = true;
            this.layoutView1.Appearance.FieldValue.Options.UseForeColor = true;
            this.layoutView1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.layoutView1.CardMinSize = new System.Drawing.Size(340, 500);
            this.layoutView1.DetailHeight = 800;
            this.layoutView1.GridControl = this.gridControl1;
            this.layoutView1.Name = "layoutView1";
            this.layoutView1.OptionsBehavior.AllowExpandCollapse = false;
            this.layoutView1.OptionsBehavior.AllowRuntimeCustomization = false;
            this.layoutView1.OptionsBehavior.Editable = false;
            this.layoutView1.OptionsBehavior.ReadOnly = true;
            this.layoutView1.OptionsCustomization.AllowFilter = false;
            this.layoutView1.OptionsCustomization.AllowSort = false;
            this.layoutView1.OptionsCustomization.ShowGroupCardCaptions = false;
            this.layoutView1.OptionsCustomization.ShowGroupCardIndents = false;
            this.layoutView1.OptionsCustomization.ShowGroupCards = false;
            this.layoutView1.OptionsCustomization.ShowGroupFields = false;
            this.layoutView1.OptionsCustomization.ShowGroupHiddenItems = false;
            this.layoutView1.OptionsCustomization.ShowGroupLayout = false;
            this.layoutView1.OptionsCustomization.ShowGroupLayoutTreeView = false;
            this.layoutView1.OptionsCustomization.ShowGroupView = false;
            this.layoutView1.OptionsCustomization.ShowResetShrinkButtons = false;
            this.layoutView1.OptionsCustomization.ShowSaveLoadLayoutButtons = false;
            this.layoutView1.OptionsMultiRecordMode.StretchCardToViewHeight = true;
            this.layoutView1.OptionsMultiRecordMode.StretchCardToViewWidth = true;
            this.layoutView1.OptionsSelection.MultiSelect = true;
            this.layoutView1.OptionsView.ShowCardCaption = false;
            this.layoutView1.OptionsView.ShowCardExpandButton = false;
            this.layoutView1.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.layoutView1.OptionsView.ShowViewCaption = true;
            this.layoutView1.TemplateCard = this.layoutViewCard1;
            this.layoutView1.ViewCaption = "THÔNG TIN BỆNH NHÂN";
            // 
            // layoutViewCard1
            // 
            this.layoutViewCard1.CustomizationFormText = "TemplateCard";
            this.layoutViewCard1.GroupBordersVisible = false;
            this.layoutViewCard1.HeaderButtonsLocation = DevExpress.Utils.GroupElementLocation.AfterText;
            this.layoutViewCard1.Name = "layoutViewCard1";
            this.layoutViewCard1.OptionsItemText.TextToControlDistance = 5;
            this.layoutViewCard1.Text = "TemplateCard";
            this.layoutViewCard1.TextLocation = DevExpress.Utils.Locations.Default;
            // 
            // InfoBenhnhan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 646);
            this.Controls.Add(this.panelControl1);
            this.MaximizeBox = false;
            this.Name = "InfoBenhnhan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thông tin chi tiết";
            this.Load += new System.EventHandler(this.InfoBenhnhan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutViewCard2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutViewCard1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Layout.LayoutView layoutView2;
        private DevExpress.XtraGrid.Views.Layout.LayoutViewCard layoutViewCard2;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Layout.LayoutView layoutView1;
        private DevExpress.XtraGrid.Views.Layout.LayoutViewCard layoutViewCard1;
    }
}