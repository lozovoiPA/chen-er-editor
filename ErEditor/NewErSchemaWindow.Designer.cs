namespace ErEditor
{
    partial class NewErSchemaWindow
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
            groupBox1 = new GroupBox();
            schemaNameTextBox = new TextBox();
            createButton = new Button();
            cancelButton = new Button();
            groupBox2 = new GroupBox();
            fileNameTextBox = new TextBox();
            groupBox3 = new GroupBox();
            folderBrowserDialogButton = new Button();
            filePathTextBox = new TextBox();
            folderBrowserDialog1 = new FolderBrowserDialog();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(schemaNameTextBox);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(306, 49);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Название схемы";
            // 
            // schemaNameTextBox
            // 
            schemaNameTextBox.Location = new Point(8, 20);
            schemaNameTextBox.Name = "schemaNameTextBox";
            schemaNameTextBox.Size = new Size(292, 23);
            schemaNameTextBox.TabIndex = 0;
            schemaNameTextBox.Text = "Тестовая схема";
            // 
            // createButton
            // 
            createButton.Location = new Point(337, 12);
            createButton.Name = "createButton";
            createButton.Size = new Size(102, 27);
            createButton.TabIndex = 3;
            createButton.Text = "Создать";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(337, 45);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(102, 27);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(fileNameTextBox);
            groupBox2.Location = new Point(12, 67);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(306, 49);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Название файла";
            // 
            // fileNameTextBox
            // 
            fileNameTextBox.Location = new Point(8, 20);
            fileNameTextBox.Name = "fileNameTextBox";
            fileNameTextBox.Size = new Size(292, 23);
            fileNameTextBox.TabIndex = 0;
            fileNameTextBox.Text = "test_schema";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(folderBrowserDialogButton);
            groupBox3.Controls.Add(filePathTextBox);
            groupBox3.Location = new Point(12, 122);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(427, 49);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Путь";
            // 
            // folderBrowserDialogButton
            // 
            folderBrowserDialogButton.Location = new Point(397, 19);
            folderBrowserDialogButton.Name = "folderBrowserDialogButton";
            folderBrowserDialogButton.Size = new Size(24, 25);
            folderBrowserDialogButton.TabIndex = 1;
            folderBrowserDialogButton.Text = "...";
            folderBrowserDialogButton.UseVisualStyleBackColor = true;
            folderBrowserDialogButton.Click += folderBrowserDialogButton_Click;
            // 
            // filePathTextBox
            // 
            filePathTextBox.Location = new Point(8, 20);
            filePathTextBox.Name = "filePathTextBox";
            filePathTextBox.Size = new Size(388, 23);
            filePathTextBox.TabIndex = 0;
            filePathTextBox.Text = "test_schema";
            // 
            // NewErSchemaWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(451, 189);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(cancelButton);
            Controls.Add(createButton);
            Controls.Add(groupBox1);
            Name = "NewErSchemaWindow";
            Text = "Новая схема";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Button createButton;
        private Button cancelButton;
        private TextBox schemaNameTextBox;
        private GroupBox groupBox2;
        private TextBox fileNameTextBox;
        private GroupBox groupBox3;
        private TextBox filePathTextBox;
        private Button folderBrowserDialogButton;
        private FolderBrowserDialog folderBrowserDialog1;
    }
}