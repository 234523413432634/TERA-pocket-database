using System.Runtime.InteropServices;


namespace MMOItemKnowledgeBase
{
    public class ThemeManager
    {
        private readonly MainForm mainForm;
        private ColorTheme currentTheme = ColorTheme.Dark;

        // Theme color definitions
        private readonly Color darkBackColor = Color.FromArgb(45, 45, 48);
        private readonly Color darkForeColor = Color.WhiteSmoke;
        private readonly Color darkControlColor = Color.FromArgb(63, 63, 70);
        private readonly Color darkListViewBackColor = Color.FromArgb(37, 37, 38);
        private readonly Color darkListViewForeColor = Color.White;
        private readonly Color darkTextBoxBackColor = Color.FromArgb(30, 30, 30);
        private readonly Color darkTextBoxForeColor = Color.White;
        private readonly Color darkTreeViewBackColor = Color.FromArgb(45, 45, 48);
        private readonly Color darkTreeViewForeColor = Color.White;
        private readonly Color darkColumnHeaderColor = Color.FromArgb(63, 63, 70);
        private readonly Color darkMenuBackColor = Color.FromArgb(45, 45, 48);
        private readonly Color darkMenuForeColor = Color.White;
        public ColorTheme CurrentTheme => currentTheme;
        public Color DarkColumnHeaderColor => darkColumnHeaderColor;
        public Color DarkForeColor => darkForeColor;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public enum ColorTheme { Light, Dark }

        public ThemeManager(MainForm form)
        {
            mainForm = form;
        }

        public void InitializeTheme()
        {
            ApplyTheme(currentTheme);
        }

        public void ToggleTheme()
        {
            currentTheme = currentTheme == ColorTheme.Light ? ColorTheme.Dark : ColorTheme.Light;
            ApplyTheme(currentTheme);
        }

        public Color GetRarityColor(int rareGrade)
        {
            if (currentTheme == ColorTheme.Dark)
            {
                switch (rareGrade)
                {
                    case 1: return Color.LimeGreen;
                    case 2: return Color.DodgerBlue;
                    case 3: return Color.Orange;
                    case 4: return Color.Fuchsia;
                    default: return darkListViewForeColor;
                }
            }
            else
            {
                switch (rareGrade)
                {
                    case 1: return Color.FromArgb(0, 128, 0);    // Dark Green
                    case 2: return Color.FromArgb(0, 0, 200);     // Dark Blue
                    case 3: return Color.FromArgb(200, 120, 0);   // Dark Orange
                    case 4: return Color.FromArgb(180, 0, 180);   // Dark Purple
                    default: return SystemColors.WindowText;
                }
            }
        }

        private void ApplyTheme(ColorTheme theme)
        {
            if (theme == ColorTheme.Dark)
            {
                // Apply dark theme to all controls
                ApplyDarkTheme(mainForm);

                // Set dark title bar
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                    var useDarkMode = 1;
                    DwmSetWindowAttribute(mainForm.Handle, attribute, ref useDarkMode, sizeof(int));
                }
            }
            else
            {
                // Apply light theme to all controls
                ApplyLightTheme(mainForm);

                // Set light title bar
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                    var useDarkMode = 0;
                    DwmSetWindowAttribute(mainForm.Handle, attribute, ref useDarkMode, sizeof(int));
                }
            }

            mainForm.Refresh();
        }
        public void ApplyDarkThemeToComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = darkTextBoxBackColor;
            comboBox.ForeColor = darkTextBoxForeColor;
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        public void ApplyLightThemeToComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = SystemColors.Window;
            comboBox.ForeColor = SystemColors.WindowText;
            comboBox.FlatStyle = FlatStyle.Standard;
        }

        private void ApplyDarkTheme(Control control)
        {
            // Apply to the control itself
            if (control is Form form)
            {
                form.BackColor = darkBackColor;
                form.ForeColor = darkForeColor;
            }
            else if (control is TextBoxBase textBox)
            {
                textBox.BackColor = darkTextBoxBackColor;
                textBox.ForeColor = darkTextBoxForeColor;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ListView listView)
            {
                listView.BackColor = darkListViewBackColor;
                listView.ForeColor = darkListViewForeColor;
                listView.BorderStyle = BorderStyle.FixedSingle;
                listView.OwnerDraw = true;
            }
            else if (control is TreeView treeView)
            {
                treeView.BackColor = darkTreeViewBackColor;
                treeView.ForeColor = darkTreeViewForeColor;
            }
            else if (control is MenuStrip menu)
            {
                menu.BackColor = darkMenuBackColor;
                menu.ForeColor = darkMenuForeColor;
            }
            else if (control is StatusStrip status)
            {
                status.BackColor = darkMenuBackColor;
                status.ForeColor = darkMenuForeColor;
            }
            else if (control is SplitContainer split)
            {
                split.BackColor = darkBackColor;
                split.Panel1.BackColor = darkBackColor;
                split.Panel2.BackColor = darkBackColor;
            }
            else if (control is RichTextBox rtb)
            {
                rtb.BackColor = darkTextBoxBackColor;
                rtb.ForeColor = darkTextBoxForeColor;
                rtb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                ApplyDarkThemeToComboBox(comboBox);
            }
            // Apply to all child controls
            foreach (Control child in control.Controls)
            {
                ApplyDarkTheme(child);
            }
        }

        private void ApplyLightTheme(Control control)
        {
            // Apply to the control itself
            if (control is Form form)
            {
                form.BackColor = SystemColors.Control;
                form.ForeColor = SystemColors.ControlText;
            }
            else if (control is TextBoxBase textBox)
            {
                textBox.BackColor = SystemColors.Window;
                textBox.ForeColor = SystemColors.WindowText;
                textBox.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (control is ListView listView)
            {
                listView.BackColor = SystemColors.Window;
                listView.ForeColor = SystemColors.WindowText;
                listView.BorderStyle = BorderStyle.Fixed3D;
                listView.OwnerDraw = false;
            }
            else if (control is TreeView treeView)
            {
                treeView.BackColor = SystemColors.Window;
                treeView.ForeColor = SystemColors.WindowText;
            }
            else if (control is MenuStrip menu)
            {
                menu.BackColor = SystemColors.MenuBar;
                menu.ForeColor = SystemColors.MenuText;
            }
            else if (control is StatusStrip status)
            {
                status.BackColor = SystemColors.MenuBar;
                status.ForeColor = SystemColors.MenuText;
            }
            else if (control is SplitContainer split)
            {
                split.BackColor = SystemColors.Control;
                split.Panel1.BackColor = SystemColors.Control;
                split.Panel2.BackColor = SystemColors.Control;
            }
            else if (control is RichTextBox rtb)
            {
                rtb.BackColor = SystemColors.Window;
                rtb.ForeColor = SystemColors.WindowText;
                rtb.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (control is ComboBox comboBox)
            {
                ApplyLightThemeToComboBox(comboBox);
            }

            // Apply to all child controls
            foreach (Control child in control.Controls)
            {
                ApplyLightTheme(child);
            }
        }
    }
}