using ErEditor.UI.NavigatorTreeClasses;

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
            navigatorTreeView1 = new ErEditor.UI.NavigatorTreeClasses.NavigatorTreeView();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            createSchemaToolstripMenuItem = new ToolStripMenuItem();
            saveSchemaToolStripMenuItem = new ToolStripMenuItem();
            openSchemaToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            diagramPanel1 = new ErEditor.UI.DiagramPanel();
            panel1 = new Panel();
            groupBox3 = new GroupBox();
            elementPropertiesPanel1 = new ErEditor.UI.ElementPropertiesPanelClasses.ElementPropertiesPanel();
            splitter2 = new Splitter();
            groupBox2 = new GroupBox();
            toolStrip3 = new ToolStrip();
            splitter1 = new Splitter();
            groupBox1 = new GroupBox();
            toolStrip2 = new ToolStrip();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // navigatorTreeView1
            // 
            navigatorTreeView1.BackColor = SystemColors.Window;
            navigatorTreeView1.BorderStyle = BorderStyle.None;
            navigatorTreeView1.Dock = DockStyle.Fill;
            navigatorTreeView1.Enabled = false;
            navigatorTreeView1.ImageIndex = 0;
            navigatorTreeView1.Location = new Point(27, 16);
            navigatorTreeView1.Name = "navigatorTreeView1";
            navigatorTreeView1.SelectedImageIndex = 0;
            navigatorTreeView1.SelectedNode = null;
            navigatorTreeView1.Size = new Size(270, 602);
            navigatorTreeView1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(3, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1249, 24);
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
            toolStrip1.Location = new Point(3, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1249, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // diagramPanel1
            // 
            diagramPanel1.BackColor = SystemColors.ControlLight;
            diagramPanel1.Dock = DockStyle.Fill;
            diagramPanel1.Location = new Point(29, 16);
            diagramPanel1.Name = "diagramPanel1";
            diagramPanel1.Size = new Size(570, 602);
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
            panel1.Location = new Point(3, 49);
            panel1.Name = "panel1";
            panel1.Size = new Size(1249, 618);
            panel1.TabIndex = 5;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(elementPropertiesPanel1);
            groupBox3.Dock = DockStyle.Fill;
            groupBox3.Location = new Point(301, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(1, 0, 0, 0);
            groupBox3.Size = new Size(346, 618);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Свойства";
            // 
            // elementPropertiesPanel1
            // 
            elementPropertiesPanel1.BackColor = SystemColors.ControlLight;
            elementPropertiesPanel1.Dock = DockStyle.Fill;
            elementPropertiesPanel1.Location = new Point(1, 16);
            elementPropertiesPanel1.Margin = new Padding(0);
            elementPropertiesPanel1.Name = "elementPropertiesPanel1";
            elementPropertiesPanel1.Size = new Size(345, 602);
            elementPropertiesPanel1.TabIndex = 0;
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Right;
            splitter2.Location = new Point(647, 0);
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
            groupBox2.Location = new Point(650, 0);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 0, 0, 0);
            groupBox2.Size = new Size(599, 618);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Диаграммер";
            // 
            // toolStrip3
            // 
            toolStrip3.BackColor = SystemColors.Control;
            toolStrip3.Dock = DockStyle.Left;
            toolStrip3.Location = new Point(3, 16);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new Size(26, 602);
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
            groupBox1.Padding = new Padding(1, 0, 1, 0);
            groupBox1.Size = new Size(298, 618);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Навигатор";
            // 
            // toolStrip2
            // 
            toolStrip2.BackColor = SystemColors.Control;
            toolStrip2.Dock = DockStyle.Left;
            toolStrip2.Location = new Point(1, 16);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(26, 602);
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
            Padding = new Padding(3, 0, 0, 0);
            Text = "Редактор ER-схем";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NavigatorTreeView navigatorTreeView1;
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
        private UI.ElementPropertiesPanelClasses.ElementPropertiesPanel elementPropertiesPanel1;
    }
}
