using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MMOItemKnowledgeBase
{
    public partial class MainForm : Form
    {
        private DatabaseManager dbManager;
        private ThemeManager themeManager;
        private const int SearchDelay = 400;
        private int lastSortedColumn = -1;
        private SortOrder sortOrder = SortOrder.Ascending;

        public MainForm()
        {
            InitializeComponent();
            
            themeManager = new ThemeManager(this);
            themeManager.InitializeTheme();

            itemListView.DrawColumnHeader += ListView_DrawColumnHeader;
            itemListView.DrawItem += ListView_DrawItem;
            itemListView.DrawSubItem += ListView_DrawSubItem;

            dbManager = new DatabaseManager();
            dbManager.InitializeDatabase();
            dbManager.LoadAllData();
            
            InitializeCategoryGroups();
            UpdateItemList();
            PopulateCategoryTree();
        }
        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
{
    if (themeManager.CurrentTheme == ThemeManager.ColorTheme.Dark)
    {
        e.Graphics.FillRectangle(new SolidBrush(themeManager.DarkColumnHeaderColor), e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, themeManager.DarkForeColor, 
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }
    else
    {
        e.DrawDefault = true;
    }
}

private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
{
    e.DrawDefault = true;
}

private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
{
    e.DrawDefault = true;
}
        private void InitializeCategoryGroups()
        {
            dbManager.CategoryGroups.Add("Weapons", new List<string> {
                "axe", "twohand", "lance", "dual", "rod", "staff",
                "bow", "circle","chain", "blaster","gauntlet", "shuriken", "glaive"
            });

            dbManager.CategoryGroups.Add("Armor", new List<string> {
                "bodyRobe", "handRobe", "feetRobe", 
                "bodyLeather", "handLeather", "feetLeather",
                "bodyMail", "handMail", "feetMail",
                "underwear"
            });

            dbManager.CategoryGroups.Add("jewelry", new List<string> {
                "ring", "earring", "necklace", "belt", "brooch", "accessoryFace"
            });

            dbManager.CategoryGroups.Add("Other", new List<string> {
                "crest", "skillbook", "combat", "quest"
            });
        }

        private void PopulateCategoryTree()
        {
            categoryTreeView.BeginUpdate();
            categoryTreeView.Nodes.Clear();

            foreach (var group in dbManager.CategoryGroups)
            {
                TreeNode groupNode = new TreeNode(group.Key);
                groupNode.Tag = group.Key;

                foreach (var category in group.Value)
                {
                    TreeNode categoryNode = new TreeNode(category);
                    categoryNode.Tag = category;
                    groupNode.Nodes.Add(categoryNode);
                }

                categoryTreeView.Nodes.Add(groupNode);
                groupNode.Expand();
            }

            categoryTreeView.EndUpdate();
        }

        private CancellationTokenSource searchCancellationTokenSource;
private async void UpdateItemList()
{
    // Cancel previous search if it's still running
    searchCancellationTokenSource?.Cancel();
    searchCancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = searchCancellationTokenSource.Token;

    itemListView.BeginUpdate();
    try
    {
        itemListView.Items.Clear();
        imageList.Images.Clear();
        statusLabel.Text = "Searching...";

        string searchText = searchTextBox.Text.Trim();
        bool isNumeric = int.TryParse(searchText, out int searchId);

        // Get selected categories
        List<string> selectedCategories = new List<string>();
        foreach (TreeNode groupNode in categoryTreeView.Nodes)
        {
            foreach (TreeNode categoryNode in groupNode.Nodes)
            {
                if (categoryNode.Checked)
                {
                    selectedCategories.Add(categoryNode.Tag.ToString());
                }
            }
        }

        try
        {
            var searchResult = await dbManager.SearchItemsAsync(
                searchText, 
                isNumeric, 
                searchId, 
                selectedCategories,  // Pass the categories here
                cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            // Add items to ListView in batches
            const int batchSize = 100;
            for (int i = 0; i < searchResult.Items.Count; i += batchSize)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var batch = searchResult.Items.Skip(i).Take(batchSize).ToList();
                await AddItemsToListView(batch, cancellationToken);
                
                statusLabel.Text = $"Loaded {Math.Min(i + batchSize, searchResult.Items.Count)} of {searchResult.Items.Count} items...";
                await Task.Delay(1); // Yield to UI thread
            }

            statusLabel.Text = $"Showing {itemListView.Items.Count} items" +
                (searchResult.IsLimited ? " (limited to 500 when no search/filter is active)" : "");
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled - ignore
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Error: " + ex.Message;
        }
    }
    finally
    {
        itemListView.EndUpdate();
    }
}

        private async Task AddItemsToListView(List<ItemInfo> items, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    var listViewItems = new List<ListViewItem>();

                    foreach (var item in items)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        ListViewItem listItem = new ListViewItem(item.Id.ToString());
                        listItem.SubItems.Add(item.Name);
                        listItem.SubItems.Add(item.Level > 0 ? item.Level.ToString() : "N/A");

                        if (item.HasEquipmentStats)
                        {
                            listItem.SubItems.Add(item.Balance);
                            listItem.SubItems.Add(item.Defense.ToString());
                            listItem.SubItems.Add(item.Impact);
                            listItem.SubItems.Add(item.MaxAttack.ToString());
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                listItem.SubItems.Add("-");
                            }
                        }

                        listItem.ForeColor = themeManager.GetRarityColor(item.RareGrade);
                        listItem.Tag = item.Tooltip;

                        if (item.IconImage != null)
                        {
                            // Add to image list on UI thread
                            this.Invoke((Action)(() =>
                            {
                                imageList.Images.Add(item.Id.ToString(), item.IconImage);
                            }));
                            listItem.ImageKey = item.Id.ToString();
                        }

                        listViewItems.Add(listItem);
                    }

                    // Add all items to ListView at once on UI thread
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        this.Invoke((Action)(() =>
                        {
                            itemListView.Items.AddRange(listViewItems.ToArray());
                        }));
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation during list view population
            }
        }

        private void itemListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lastSortedColumn)
            {
                sortOrder = sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                lastSortedColumn = e.Column;
                sortOrder = SortOrder.Ascending;
            }

            itemListView.ListViewItemSorter = new ListViewItemComparer(e.Column, sortOrder);
            itemListView.Sort();
        }

        private void themeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            themeManager.ToggleTheme();
            UpdateItemList();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Interval = SearchDelay;
            searchTimer.Start();
        }

        private void searchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            UpdateItemList();
        }

        private void itemListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemListView.SelectedItems.Count == 0)
            {
                descriptionTextBox.Clear();
                return;
            }

            string htmlDescription = itemListView.SelectedItems[0].Tag as string ?? "No description available.";
            DisplayHtmlDescription(htmlDescription);
        }
        private void DisplayHtmlDescription(string html)
        {
            try
            {
                string rtf = HtmlToRtfConverter.ConvertSimpleHtmlToRtf(html);
                descriptionTextBox.Rtf = rtf;
            }
            catch
            {
                // Fallback to plain text if conversion fails
                descriptionTextBox.Text = StripHtmlTags(html);
            }
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return Regex.Replace(html, "<.*?>", string.Empty);
        }
        private void categoryTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                UpdateItemList();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dbManager?.Dispose();
            base.OnFormClosing(e);
        }
    }
}