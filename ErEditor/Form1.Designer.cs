namespace ErEditor
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            navigatorTreeView1 = new ErEditor.UI.NavigatorTreeView();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            createSchemaToolstripMenuItem = new ToolStripMenuItem();
            saveSchemaToolStripMenuItem = new ToolStripMenuItem();
            openSchemaToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            diagramPanel1 = new ErEditor.UI.DiagramPanel();
            panel1 = new Panel();
            groupBox3 = new GroupBox();
            splitter2 = new Splitter();
            groupBox2 = new GroupBox();
            toolStrip3 = new ToolStrip();
            splitter1 = new Splitter();
            groupBox1 = new GroupBox();
            toolStrip2 = new ToolStrip();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // navigatorTreeView1
            // 
            navigatorTreeView1.BorderStyle = BorderStyle.None;
            navigatorTreeView1.Dock = DockStyle.Fill;
            navigatorTreeView1.ImageIndex = 0;
            navigatorTreeView1.Location = new Point(29, 19);
            navigatorTreeView1.Name = "navigatorTreeView1";
            navigatorTreeView1.SelectedImageIndex = 0;
            navigatorTreeView1.SelectedNode = null;
            navigatorTreeView1.Size = new Size(266, 596);
            navigatorTreeView1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1252, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createSchemaToolstripMenuItem, saveSchemaToolStripMenuItem, openSchemaToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(48, 20);
            fileToolStripMenuItem.Text = "Файл";
            // 
            // createSchemaToolstripMenuItem
            // 
            createSchemaToolstripMenuItem.Name = "createSchemaToolstripMenuItem";
            createSchemaToolstripMenuItem.Size = new Size(169, 22);
            createSchemaToolstripMenuItem.Text = "Создать схему";
            createSchemaToolstripMenuItem.Click += createSchemaToolstripMenuItem_Click;
            // 
            // saveSchemaToolStripMenuItem
            // 
            saveSchemaToolStripMenuItem.Name = "saveSchemaToolStripMenuItem";
            saveSchemaToolStripMenuItem.Size = new Size(169, 22);
            saveSchemaToolStripMenuItem.Text = "Сохранить схему";
            saveSchemaToolStripMenuItem.Click += saveSchemaToolStripMenuItem_Click;
            // 
            // openSchemaToolStripMenuItem
            // 
            openSchemaToolStripMenuItem.Name = "openSchemaToolStripMenuItem";
            openSchemaToolStripMenuItem.Size = new Size(169, 22);
            openSchemaToolStripMenuItem.Text = "Открыть схему";
            openSchemaToolStripMenuItem.Click += openSchemaToolStripMenuItem_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1252, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // diagramPanel1
            // 
            diagramPanel1.BackColor = Color.White;
            diagramPanel1.Diagram = null;
            diagramPanel1.Dock = DockStyle.Fill;
            diagramPanel1.Location = new Point(29, 19);
            diagramPanel1.Name = "diagramPanel1";
            diagramPanel1.Size = new Size(660, 596);
            diagramPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(groupBox3);
            panel1.Controls.Add(splitter2);
            panel1.Controls.Add(groupBox2);
            panel1.Controls.Add(splitter1);
            panel1.Controls.Add(groupBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 49);
            panel1.Name = "panel1";
            panel1.Size = new Size(1252, 618);
            panel1.TabIndex = 5;
            // 
            // groupBox3
            // 
            groupBox3.Dock = DockStyle.Fill;
            groupBox3.Location = new Point(301, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(256, 618);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "groupBox3";
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Right;
            splitter2.Location = new Point(557, 0);
            splitter2.Name = "splitter2";
            splitter2.Size = new Size(3, 618);
            splitter2.TabIndex = 3;
            splitter2.TabStop = false;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(diagramPanel1);
            groupBox2.Controls.Add(toolStrip3);
            groupBox2.Dock = DockStyle.Right;
            groupBox2.Location = new Point(560, 0);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(692, 618);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "groupBox2";
            // 
            // toolStrip3
            // 
            toolStrip3.Dock = DockStyle.Left;
            toolStrip3.Location = new Point(3, 19);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new Size(26, 596);
            toolStrip3.TabIndex = 5;
            toolStrip3.Text = "toolStrip3";
            // 
            // splitter1
            // 
            splitter1.Location = new Point(298, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 618);
            splitter1.TabIndex = 1;
            splitter1.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(navigatorTreeView1);
            groupBox1.Controls.Add(toolStrip2);
            groupBox1.Dock = DockStyle.Left;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(298, 618);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // toolStrip2
            // 
            toolStrip2.Dock = DockStyle.Left;
            toolStrip2.Location = new Point(3, 19);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(26, 596);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1252, 667);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainWindow";
            Text = "Редактор ER-схем";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UI.NavigatorTreeView navigatorTreeView1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem createSchemaToolstripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripMenuItem saveSchemaToolStripMenuItem;
        private ToolStripMenuItem openSchemaToolStripMenuItem;
        private UI.DiagramPanel diagramPanel1;
        private Panel panel1;
        private GroupBox groupBox1;
        private Splitter splitter1;
        private GroupBox groupBox3;
        private Splitter splitter2;
        private GroupBox groupBox2;
        private ToolStrip toolStrip2;
        private ToolStrip toolStrip3;
    }
}
