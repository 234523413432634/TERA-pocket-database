namespace MMOItemKnowledgeBase
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem themeToolStripMenuItem;
        private System.Windows.Forms.Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            searchTextBox = new TextBox();
            itemListView = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            imageList = new ImageList(components);
            descriptionTextBox = new RichTextBox();
            searchTimer = new System.Windows.Forms.Timer(components);
            categoryTreeView = new TreeView();
            splitContainer1 = new SplitContainer();
            statusLabel = new Label();
            menuStrip1 = new MenuStrip();
            themeToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // searchTextBox
            // 
            searchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchTextBox.Location = new Point(16, 35);
            searchTextBox.Margin = new Padding(4, 5, 4, 5);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new Size(1260, 27);
            searchTextBox.TabIndex = 0;
            searchTextBox.TextChanged += searchTextBox_TextChanged;
            // 
            // itemListView
            // 
            itemListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemListView.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
            itemListView.FullRowSelect = true;
            itemListView.Location = new Point(4, 0);
            itemListView.Margin = new Padding(4, 5, 4, 5);
            itemListView.MultiSelect = false;
            itemListView.Name = "itemListView";
            itemListView.Size = new Size(969, 537);
            itemListView.SmallImageList = imageList;
            itemListView.TabIndex = 1;
            itemListView.UseCompatibleStateImageBehavior = false;
            itemListView.View = View.Details;
            itemListView.ColumnClick += itemListView_ColumnClick;
            itemListView.SelectedIndexChanged += itemListView_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "ID";
            columnHeader1.Width = 90;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Name";
            columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Level";
            columnHeader3.Width = 50;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Balance";
            columnHeader4.Width = 70;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Defense";
            columnHeader5.Width = 70;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "Impact";
            columnHeader6.Width = 70;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Attack";
            columnHeader7.Width = 80;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(32, 32);
            imageList.TransparentColor = Color.Transparent;
            // 
            // descriptionTextBox
            // 
            descriptionTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            descriptionTextBox.Location = new Point(15, 644);
            descriptionTextBox.Margin = new Padding(4, 5, 4, 5);
            descriptionTextBox.Name = "descriptionTextBox";
            descriptionTextBox.ReadOnly = true;
            descriptionTextBox.Size = new Size(1261, 96);
            descriptionTextBox.TabIndex = 2;
            descriptionTextBox.Text = "";
            // 
            // searchTimer
            // 
            searchTimer.Interval = 300;
            searchTimer.Tick += searchTimer_Tick;
            // 
            // categoryTreeView
            // 
            categoryTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            categoryTreeView.CheckBoxes = true;
            categoryTreeView.Location = new Point(0, 0);
            categoryTreeView.Margin = new Padding(4, 5, 4, 5);
            categoryTreeView.Name = "categoryTreeView";
            categoryTreeView.Size = new Size(276, 537);
            categoryTreeView.TabIndex = 3;
            categoryTreeView.AfterCheck += categoryTreeView_AfterCheck;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(16, 86);
            splitContainer1.Margin = new Padding(4, 5, 4, 5);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(categoryTreeView);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(itemListView);
            splitContainer1.Size = new Size(1262, 549);
            splitContainer1.SplitterDistance = 282;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 4;
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(16, 745);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(0, 20);
            statusLabel.TabIndex = 6;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = SystemColors.MenuBar;
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { themeToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(8, 3, 0, 3);
            menuStrip1.Size = new Size(1294, 30);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // themeToolStripMenuItem
            // 
            themeToolStripMenuItem.Name = "themeToolStripMenuItem";
            themeToolStripMenuItem.Size = new Size(122, 24);
            themeToolStripMenuItem.Text = "🌙 Dark Mode";
            themeToolStripMenuItem.Click += themeToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1294, 782);
            Controls.Add(statusLabel);
            Controls.Add(menuStrip1);
            Controls.Add(splitContainer1);
            Controls.Add(descriptionTextBox);
            Controls.Add(searchTextBox);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(1061, 590);
            Name = "MainForm";
            Text = "TERA pocket database";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.ListView itemListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.RichTextBox descriptionTextBox;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Timer searchTimer;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.TreeView categoryTreeView;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}