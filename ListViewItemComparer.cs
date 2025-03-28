using System.Collections;

namespace MMOItemKnowledgeBase
{
    public class ListViewItemComparer : IComparer
    {
        private readonly int col;
        private readonly SortOrder order;
        private readonly bool isNumericColumn;

        public ListViewItemComparer(int column, SortOrder sortOrder)
        {
            col = column;
            order = sortOrder;

            // Determine if this is a numeric column based on its header text
            // Add more column headers as needed
            isNumericColumn = column == 0 || column == 2 || column == 3 || column == 4 || column == 5 || column == 6 || column == 7;
        }

        public int Compare(object x, object y)
        {
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;

            string textX = itemX.SubItems[col].Text;
            string textY = itemY.SubItems[col].Text;

            // Handle empty or dash values
            bool isXEmpty = string.IsNullOrEmpty(textX) || textX == "-";
            bool isYEmpty = string.IsNullOrEmpty(textY) || textY == "-";

            if (isNumericColumn)
            {
                // For numeric columns, treat empty/dash as 0
                int xValue = isXEmpty ? 0 : ParseValue(textX);
                int yValue = isYEmpty ? 0 : ParseValue(textY);

                int result = xValue.CompareTo(yValue);
                return order == SortOrder.Ascending ? result : -result;
            }
            else
            {
                // For non-numeric columns, keep original behavior
                if (isXEmpty && isYEmpty) return 0;
                if (isXEmpty) return order == SortOrder.Ascending ? 1 : -1;
                if (isYEmpty) return order == SortOrder.Ascending ? -1 : 1;

                int result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
                return order == SortOrder.Ascending ? result : -result;
            }
        }

        private int ParseValue(string text)
        {
            if (int.TryParse(text, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}