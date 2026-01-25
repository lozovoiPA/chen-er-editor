namespace ErEditor
{
    partial class TestForm
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
            roleView1 = new ErEditor.UI.RoleView();
            mappingView1 = new ErEditor.UI.MappingView();
            SuspendLayout();
            // 
            // roleView1
            // 
            roleView1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            roleView1.ColumnCount = 2;
            roleView1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            roleView1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.6666641F));
            roleView1.Element = null;
            roleView1.Location = new Point(75, 54);
            roleView1.Name = "roleView1";
            roleView1.RowCount = 2;
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle());
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            roleView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            roleView1.Schema = null;
            roleView1.Size = new Size(303, 277);
            roleView1.TabIndex = 0;
            // 
            // mappingView1
            // 
            mappingView1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            mappingView1.ColumnCount = 2;
            mappingView1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.3452377F));
            mappingView1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55.6547623F));
            mappingView1.Element = null;
            mappingView1.Location = new Point(436, 144);
            mappingView1.Name = "mappingView1";
            mappingView1.RowCount = 2;
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle());
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mappingView1.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            mappingView1.Schema = null;
            mappingView1.Size = new Size(337, 258);
            mappingView1.TabIndex = 1;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(mappingView1);
            Controls.Add(roleView1);
            Name = "TestForm";
            Text = "TestForm";
            ResumeLayout(false);
        }

        #endregion

        private UI.RoleView roleView1;
        private UI.MappingView mappingView1;
    }
}