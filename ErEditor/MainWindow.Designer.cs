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
            navigatorTreeView1 = new NavigatorTreeView();
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
            правкаToolStripMenuItem = new ToolStripMenuItem();
            iconDropDownButton1 = new FontAwesome.Sharp.IconDropDownButton();
            iconDropDownButton2 = new FontAwesome.Sharp.IconDropDownButton();
            iconToolStripButton1 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton2 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton3 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton4 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton5 = new FontAwesome.Sharp.IconToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            iconToolStripButton6 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton7 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton8 = new FontAwesome.Sharp.IconToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            iconToolStripButton9 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton10 = new FontAwesome.Sharp.IconToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            iconToolStripButton11 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton12 = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButton13 = new FontAwesome.Sharp.IconToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            iconToolStripButton14 = new FontAwesome.Sharp.IconToolStripButton();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            toolStrip3.SuspendLayout();
            groupBox1.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // navigatorTreeView1
            // 
            navigatorTreeView1.BackColor = SystemColors.Window;
            navigatorTreeView1.BorderStyle = BorderStyle.None;
            navigatorTreeView1.Dock = DockStyle.Fill;
            navigatorTreeView1.Enabled = false;
            navigatorTreeView1.ImageIndex = 0;
            navigatorTreeView1.Location = new Point(25, 16);
            navigatorTreeView1.Name = "navigatorTreeView1";
            navigatorTreeView1.SelectedImageIndex = 0;
            navigatorTreeView1.SelectedNode = null;
            navigatorTreeView1.Size = new Size(272, 602);
            navigatorTreeView1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, правкаToolStripMenuItem });
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
            toolStrip1.Items.AddRange(new ToolStripItem[] { iconDropDownButton1, iconDropDownButton2, iconToolStripButton1 });
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
            diagramPanel1.Location = new Point(27, 16);
            diagramPanel1.Name = "diagramPanel1";
            diagramPanel1.Size = new Size(572, 602);
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
            toolStrip3.Items.AddRange(new ToolStripItem[] { iconToolStripButton10, toolStripSeparator3, iconToolStripButton11, iconToolStripButton12, iconToolStripButton13, toolStripSeparator4, iconToolStripButton14 });
            toolStrip3.Location = new Point(3, 16);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new Size(24, 602);
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
            toolStrip2.Items.AddRange(new ToolStripItem[] { iconToolStripButton2, iconToolStripButton3, iconToolStripButton4, iconToolStripButton5, toolStripSeparator1, iconToolStripButton6, iconToolStripButton7, iconToolStripButton8, toolStripSeparator2, iconToolStripButton9 });
            toolStrip2.Location = new Point(1, 16);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(24, 602);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // правкаToolStripMenuItem
            // 
            правкаToolStripMenuItem.Name = "правкаToolStripMenuItem";
            правкаToolStripMenuItem.Size = new Size(59, 20);
            правкаToolStripMenuItem.Text = "Правка";
            // 
            // iconDropDownButton1
            // 
            iconDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconDropDownButton1.IconChar = FontAwesome.Sharp.IconChar.File;
            iconDropDownButton1.IconColor = Color.Black;
            iconDropDownButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconDropDownButton1.ImageTransparentColor = Color.Magenta;
            iconDropDownButton1.Name = "iconDropDownButton1";
            iconDropDownButton1.Size = new Size(29, 22);
            iconDropDownButton1.Text = "iconDropDownButton1";
            // 
            // iconDropDownButton2
            // 
            iconDropDownButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconDropDownButton2.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            iconDropDownButton2.IconColor = Color.Orange;
            iconDropDownButton2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconDropDownButton2.IconSize = 60;
            iconDropDownButton2.ImageTransparentColor = Color.Magenta;
            iconDropDownButton2.Name = "iconDropDownButton2";
            iconDropDownButton2.Size = new Size(29, 22);
            iconDropDownButton2.Text = "iconDropDownButton2";
            // 
            // iconToolStripButton1
            // 
            iconToolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            iconToolStripButton1.IconChar = FontAwesome.Sharp.IconChar.Save;
            iconToolStripButton1.IconColor = Color.Black;
            iconToolStripButton1.IconFont = FontAwesome.Sharp.IconFont.Regular;
            iconToolStripButton1.IconSize = 60;
            iconToolStripButton1.ImageTransparentColor = Color.Magenta;
            iconToolStripButton1.Name = "iconToolStripButton1";
            iconToolStripButton1.Size = new Size(23, 22);
            iconToolStripButton1.Text = "iconToolStripButton1";
            // 
            // iconToolStripButton2
            // 
            iconToolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton2.IconChar = FontAwesome.Sharp.IconChar.Add;
            iconToolStripButton2.IconColor = Color.Black;
            iconToolStripButton2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton2.ImageTransparentColor = Color.Magenta;
            iconToolStripButton2.Name = "iconToolStripButton2";
            iconToolStripButton2.Size = new Size(29, 20);
            iconToolStripButton2.Text = "iconToolStripButton2";
            // 
            // iconToolStripButton3
            // 
            iconToolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton3.IconChar = FontAwesome.Sharp.IconChar.E;
            iconToolStripButton3.IconColor = Color.Black;
            iconToolStripButton3.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton3.ImageTransparentColor = Color.Magenta;
            iconToolStripButton3.Name = "iconToolStripButton3";
            iconToolStripButton3.Size = new Size(29, 20);
            iconToolStripButton3.Text = "iconToolStripButton3";
            // 
            // iconToolStripButton4
            // 
            iconToolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton4.IconChar = FontAwesome.Sharp.IconChar.R;
            iconToolStripButton4.IconColor = Color.Black;
            iconToolStripButton4.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton4.ImageTransparentColor = Color.Magenta;
            iconToolStripButton4.Name = "iconToolStripButton4";
            iconToolStripButton4.Size = new Size(29, 20);
            iconToolStripButton4.Text = "iconToolStripButton4";
            // 
            // iconToolStripButton5
            // 
            iconToolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton5.IconChar = FontAwesome.Sharp.IconChar.V;
            iconToolStripButton5.IconColor = Color.Black;
            iconToolStripButton5.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton5.ImageTransparentColor = Color.Magenta;
            iconToolStripButton5.Name = "iconToolStripButton5";
            iconToolStripButton5.Size = new Size(29, 20);
            iconToolStripButton5.Text = "iconToolStripButton5";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(29, 6);
            // 
            // iconToolStripButton6
            // 
            iconToolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton6.IconChar = FontAwesome.Sharp.IconChar.Copy;
            iconToolStripButton6.IconColor = Color.Black;
            iconToolStripButton6.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton6.IconSize = 60;
            iconToolStripButton6.ImageTransparentColor = Color.Magenta;
            iconToolStripButton6.Name = "iconToolStripButton6";
            iconToolStripButton6.Size = new Size(29, 20);
            iconToolStripButton6.Text = "iconToolStripButton6";
            // 
            // iconToolStripButton7
            // 
            iconToolStripButton7.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton7.IconChar = FontAwesome.Sharp.IconChar.FileClipboard;
            iconToolStripButton7.IconColor = Color.Black;
            iconToolStripButton7.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton7.IconSize = 60;
            iconToolStripButton7.ImageTransparentColor = Color.Magenta;
            iconToolStripButton7.Name = "iconToolStripButton7";
            iconToolStripButton7.Size = new Size(29, 20);
            iconToolStripButton7.Text = "iconToolStripButton7";
            // 
            // iconToolStripButton8
            // 
            iconToolStripButton8.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton8.IconChar = FontAwesome.Sharp.IconChar.Clone;
            iconToolStripButton8.IconColor = Color.Black;
            iconToolStripButton8.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton8.IconSize = 60;
            iconToolStripButton8.ImageTransparentColor = Color.Magenta;
            iconToolStripButton8.Name = "iconToolStripButton8";
            iconToolStripButton8.Size = new Size(21, 20);
            iconToolStripButton8.Text = "iconToolStripButton8";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(29, 6);
            // 
            // iconToolStripButton9
            // 
            iconToolStripButton9.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton9.IconChar = FontAwesome.Sharp.IconChar.Trash;
            iconToolStripButton9.IconColor = Color.Black;
            iconToolStripButton9.IconFont = FontAwesome.Sharp.IconFont.Solid;
            iconToolStripButton9.IconSize = 60;
            iconToolStripButton9.ImageTransparentColor = Color.Magenta;
            iconToolStripButton9.Name = "iconToolStripButton9";
            iconToolStripButton9.Size = new Size(29, 20);
            iconToolStripButton9.Text = "iconToolStripButton9";
            // 
            // iconToolStripButton10
            // 
            iconToolStripButton10.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton10.IconChar = FontAwesome.Sharp.IconChar.MousePointer;
            iconToolStripButton10.IconColor = Color.Black;
            iconToolStripButton10.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton10.ImageTransparentColor = Color.Magenta;
            iconToolStripButton10.Name = "iconToolStripButton10";
            iconToolStripButton10.Size = new Size(29, 20);
            iconToolStripButton10.Text = "iconToolStripButton10";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(29, 6);
            // 
            // iconToolStripButton11
            // 
            iconToolStripButton11.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton11.IconChar = FontAwesome.Sharp.IconChar.SquareFull;
            iconToolStripButton11.IconColor = Color.Black;
            iconToolStripButton11.IconFont = FontAwesome.Sharp.IconFont.Solid;
            iconToolStripButton11.ImageTransparentColor = Color.Magenta;
            iconToolStripButton11.Name = "iconToolStripButton11";
            iconToolStripButton11.Size = new Size(29, 20);
            iconToolStripButton11.Text = "iconToolStripButton11";
            // 
            // iconToolStripButton12
            // 
            iconToolStripButton12.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton12.Font = new Font("Arial Narrow", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            iconToolStripButton12.IconChar = FontAwesome.Sharp.IconChar.Diamond;
            iconToolStripButton12.IconColor = Color.Black;
            iconToolStripButton12.IconFont = FontAwesome.Sharp.IconFont.Solid;
            iconToolStripButton12.IconSize = 60;
            iconToolStripButton12.ImageTransparentColor = Color.Magenta;
            iconToolStripButton12.Name = "iconToolStripButton12";
            iconToolStripButton12.Size = new Size(29, 20);
            iconToolStripButton12.Text = "iconToolStripButton12";
            // 
            // iconToolStripButton13
            // 
            iconToolStripButton13.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton13.IconChar = FontAwesome.Sharp.IconChar.Slash;
            iconToolStripButton13.IconColor = Color.Black;
            iconToolStripButton13.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton13.IconSize = 60;
            iconToolStripButton13.ImageTransparentColor = Color.Magenta;
            iconToolStripButton13.Name = "iconToolStripButton13";
            iconToolStripButton13.Size = new Size(21, 20);
            iconToolStripButton13.Text = "iconToolStripButton13";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(29, 6);
            // 
            // iconToolStripButton14
            // 
            iconToolStripButton14.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButton14.IconChar = FontAwesome.Sharp.IconChar.X;
            iconToolStripButton14.IconColor = Color.Black;
            iconToolStripButton14.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton14.IconSize = 60;
            iconToolStripButton14.ImageTransparentColor = Color.Magenta;
            iconToolStripButton14.Name = "iconToolStripButton14";
            iconToolStripButton14.Size = new Size(21, 20);
            iconToolStripButton14.Text = "iconToolStripButton14";
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
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
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
        private ToolStripMenuItem правкаToolStripMenuItem;
        private FontAwesome.Sharp.IconDropDownButton iconDropDownButton1;
        private FontAwesome.Sharp.IconDropDownButton iconDropDownButton2;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton1;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton2;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton3;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton4;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton5;
        private ToolStripSeparator toolStripSeparator1;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton6;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton7;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton8;
        private ToolStripSeparator toolStripSeparator2;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton9;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton10;
        private ToolStripSeparator toolStripSeparator3;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton11;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton12;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton13;
        private ToolStripSeparator toolStripSeparator4;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton14;
    }
}
