using System.Data.SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace MMOItemKnowledgeBase
{
    public class DatabaseManager : IDisposable
    {
        private const string IconsFolderPath = "icons";
        private SQLiteConnection dbConnection;
        private string currentDatabasePath;
        private string currentDatabaseFolder => Path.GetDirectoryName(currentDatabasePath);

        public Dictionary<string, List<string>> CategoryGroups { get; } = new Dictionary<string, List<string>>();

        public static List<(string Name, string Path)> GetAvailableDatabases()
        {
            var databases = new List<(string, string)>();

            // Look for folders starting with "X. Name" pattern
            foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory()))
            {
                var dirName = Path.GetFileName(dir);
                var match = Regex.Match(dirName, @"^\d+\.\s*(.+)$");
                if (match.Success)
                {
                    string dbName = match.Groups[1].Value;
                    string dbPath = Path.Combine(dir, "ItemDatabase.sqlite");
                    databases.Add((dbName, dbPath));
                }
            }

            return databases;
        }
        public void InitializeDatabase(string databasePath)
        {
            currentDatabasePath = databasePath;
            bool createNew = !File.Exists(databasePath);

            // Close existing connection if any
            dbConnection?.Close();
            dbConnection?.Dispose();

            dbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3;");
            dbConnection.Open();

            if (createNew)
            {
                CreateDatabaseTables();
            }
        }

        public void LoadAllData()
        {
            if (IsDatabaseEmpty())
            {
                LoadEquipmentData();
                LoadItemData();
                LoadLocalizationData();
            }
        }

        public async Task<SearchResult> SearchItemsAsync(string searchText, bool isNumeric, int searchId,
            List<string> selectedCategories, CancellationToken cancellationToken)
        {
            var result = new SearchResult();
            var items = new List<ItemInfo>();

            string query = @"
            SELECT i.Id, i.Icon, i.Level, l.Name, l.Tooltip, i.LinkEquipmentId,
                   e.Balance, e.Defense, e.Impact, e.MaxAttack, i.RareGrade
            FROM Items i
            JOIN LocalizedItems l ON i.Id = l.Id
            LEFT JOIN EquipmentStats e ON i.LinkEquipmentId = e.EquipmentId
            WHERE ((@isNumeric = 1 AND i.Id = @searchId) OR
                  (@isNumeric = 0 AND l.Name COLLATE NOCASE LIKE @searchText))";

            if (selectedCategories.Count > 0)
            {
                query += " AND i.Category IN (" + string.Join(",", selectedCategories.Select(c => $"'{c}'")) + ")";
            }

            bool shouldLimitResults = string.IsNullOrEmpty(searchText) && selectedCategories.Count == 0;
            if (shouldLimitResults)
            {
                query += " ORDER BY i.Id LIMIT 500";
                result.IsLimited = true;
            }
            else
            {
                query += " ORDER BY i.Id";
            }

            await Task.Run(() =>
            {
                using (var cmd = new SQLiteCommand(query, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@isNumeric", isNumeric ? 1 : 0);
                    cmd.Parameters.AddWithValue("@searchId", searchId);
                    cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            var item = new ItemInfo
                            {
                                Id = reader.GetInt32(0),
                                Icon = reader.GetString(1),
                                Level = reader.GetInt32(2),
                                Name = reader.GetString(3),
                                Tooltip = reader.GetString(4),
                                LinkEquipmentId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                RareGrade = reader.GetInt32(10)
                            };

                            if (!reader.IsDBNull(6))
                            {
                                item.HasEquipmentStats = true;
                                item.Balance = reader.GetString(6);
                                item.Defense = reader.GetInt32(7);
                                item.Impact = reader.GetString(8);
                                item.MaxAttack = reader.GetInt32(9);
                            }

                            items.Add(item);
                        }
                    }
                }
            }, cancellationToken);

            // Load icons in parallel with throttling
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Task.Run(() =>
            {
                Parallel.ForEach(items, options, item =>
                {
                    try
                    {
                        string iconPath = Path.Combine(IconsFolderPath, item.Icon.Replace('.', '\\') + ".png");
                        if (File.Exists(iconPath))
                        {
                            item.IconImage = Image.FromFile(iconPath);
                        }
                    }
                    catch { /* Ignore icon loading errors */ }
                });
            }, cancellationToken);

            result.Items = items;
            return result;
        }

        private void CreateDatabaseTables()
        {
            using (var cmd = new SQLiteCommand(dbConnection))
            {
                cmd.CommandText = @"
                CREATE TABLE Items (
                    Id INTEGER PRIMARY KEY,
                    NameKey TEXT,
                    Icon TEXT,
                    Level INTEGER,
                    LinkEquipmentId INTEGER,
                    Category TEXT,
                    RareGrade INTEGER
                );
                CREATE TABLE LocalizedItems (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT,
                    Tooltip TEXT
                );
                CREATE TABLE EquipmentStats (
                    EquipmentId INTEGER PRIMARY KEY,
                    Balance TEXT,
                    Defense INTEGER,
                    Impact TEXT,
                    MaxAttack INTEGER
                );
                CREATE INDEX idx_LocalizedItems_Name ON LocalizedItems(Name);
                CREATE INDEX idx_Items_Id ON Items(Id);
                CREATE INDEX idx_Items_LinkEquipmentId ON Items(LinkEquipmentId);
                CREATE INDEX idx_Items_Category ON Items(Category);";
                cmd.ExecuteNonQuery();
            }
        }

        private bool IsDatabaseEmpty()
        {
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Items", dbConnection))
            {
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
        }

        private void LoadEquipmentData()
        {
            string equipmentDataFile = Path.Combine(currentDatabaseFolder, "EquipmentData", "EquipmentData-00000.xml");
            if (!File.Exists(equipmentDataFile))
            {
                MessageBox.Show($"Equipment data file not found at: {equipmentDataFile}");
                return;
            }
            try
            {
                XDocument doc = XDocument.Load(equipmentDataFile);
                XNamespace ns = "https://vezel.dev/novadrop/dc/EquipmentData";

                using (var transaction = dbConnection.BeginTransaction())
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO EquipmentStats (EquipmentId, Balance, Defense, Impact, MaxAttack) " +
                    "VALUES (@id, @balance, @defense, @impact, @maxAttack)",
                    dbConnection))
                {
                    var idParam = cmd.Parameters.Add("@id", System.Data.DbType.Int32);
                    var balanceParam = cmd.Parameters.Add("@balance", System.Data.DbType.String);
                    var defenseParam = cmd.Parameters.Add("@defense", System.Data.DbType.Int32);
                    var impactParam = cmd.Parameters.Add("@impact", System.Data.DbType.String);
                    var maxAttackParam = cmd.Parameters.Add("@maxAttack", System.Data.DbType.Int32);

                    foreach (XElement equipElement in doc.Descendants(ns + "Equipment"))
                    {
                        try
                        {
                            // Skip if missing required attributes
                            if (equipElement.Attribute("equipmentId") == null ||
                                equipElement.Attribute("balance") == null ||
                                equipElement.Attribute("def") == null ||
                                equipElement.Attribute("impact") == null ||
                                equipElement.Attribute("maxAtk") == null)
                            {
                                continue;
                            }

                            idParam.Value = int.Parse(equipElement.Attribute("equipmentId").Value);
                            balanceParam.Value = equipElement.Attribute("balance").Value;
                            defenseParam.Value = int.Parse(equipElement.Attribute("def").Value);
                            impactParam.Value = equipElement.Attribute("impact").Value;
                            maxAttackParam.Value = int.Parse(equipElement.Attribute("maxAtk").Value);

                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            // Log or handle individual item errors without stopping the whole process
                            Debug.WriteLine($"Error processing equipment: {equipElement}. Error: {ex.Message}");
                        }
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment data: {ex.Message}");
            }
        }

        private void LoadItemData()
        {
            string itemDataFolder = Path.Combine(currentDatabaseFolder, "ItemData");
            string[] itemDataFiles = Directory.GetFiles(itemDataFolder, "ItemData-*.xml");
            if (itemDataFiles.Length == 0)
            {
                MessageBox.Show($"No ItemData XML files found in: {itemDataFolder}");
                return;
            }
            using (var transaction = dbConnection.BeginTransaction())
            using (var cmd = new SQLiteCommand(
                "INSERT INTO Items (Id, NameKey, Icon, Level, LinkEquipmentId, Category, RareGrade) " +
                "VALUES (@id, @nameKey, @icon, @level, @linkEquipmentId, @category, @rareGrade)",
                dbConnection))
            {
                var idParam = cmd.Parameters.Add("@id", System.Data.DbType.Int32);
                var nameKeyParam = cmd.Parameters.Add("@nameKey", System.Data.DbType.String);
                var iconParam = cmd.Parameters.Add("@icon", System.Data.DbType.String);
                var levelParam = cmd.Parameters.Add("@level", System.Data.DbType.Int32);
                var linkEquipmentIdParam = cmd.Parameters.Add("@linkEquipmentId", System.Data.DbType.Int32);
                var categoryParam = cmd.Parameters.Add("@category", System.Data.DbType.String);
                var rareGradeParam = cmd.Parameters.Add("@rareGrade", System.Data.DbType.Int32);

                foreach (string file in itemDataFiles)
                {
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        XNamespace ns = "https://vezel.dev/novadrop/dc/ItemData";

                        foreach (XElement itemElement in doc.Descendants(ns + "Item"))
                        {
                            try
                            {
                                // Skip if missing required attributes
                                if (itemElement.Attribute("id") == null ||
                                    itemElement.Attribute("name") == null ||
                                    itemElement.Attribute("icon") == null ||
                                    itemElement.Attribute("rareGrade") == null)
                                {
                                    continue;
                                }

                                idParam.Value = int.Parse(itemElement.Attribute("id").Value);
                                nameKeyParam.Value = itemElement.Attribute("name").Value;
                                iconParam.Value = itemElement.Attribute("icon").Value;
                                levelParam.Value = itemElement.Attribute("level")?.Value != null ?
                                    int.Parse(itemElement.Attribute("level").Value) : 0;
                                linkEquipmentIdParam.Value = itemElement.Attribute("linkEquipmentId")?.Value != null ?
                                    int.Parse(itemElement.Attribute("linkEquipmentId").Value) : 0;
                                categoryParam.Value = itemElement.Attribute("category")?.Value ?? string.Empty;
                                rareGradeParam.Value = int.Parse(itemElement.Attribute("rareGrade").Value);

                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing item: {itemElement}. Error: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading item data from {file}: {ex.Message}");
                    }
                }
                transaction.Commit();
            }
        }

        private void LoadLocalizationData()
        {
            string localizationFolder = Path.Combine(currentDatabaseFolder, "StrSheet_Item");
            string[] localizationFiles = Directory.GetFiles(localizationFolder, "StrSheet_Item-*.xml");
            if (localizationFiles.Length == 0)
            {
                MessageBox.Show($"No localization XML files found in: {localizationFolder}");
                return;
            }
            using (var transaction = dbConnection.BeginTransaction())
            using (var cmd = new SQLiteCommand(
                "INSERT INTO LocalizedItems (Id, Name, Tooltip) VALUES (@id, @name, @tooltip)",
                dbConnection))
            {
                var idParam = cmd.Parameters.Add("@id", System.Data.DbType.Int32);
                var nameParam = cmd.Parameters.Add("@name", System.Data.DbType.String);
                var tooltipParam = cmd.Parameters.Add("@tooltip", System.Data.DbType.String);

                foreach (string file in localizationFiles)
                {
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        XNamespace ns = "https://vezel.dev/novadrop/dc/StrSheet_Item";

                        foreach (XElement stringElement in doc.Descendants(ns + "String"))
                        {
                            try
                            {
                                // Skip if missing required attributes
                                if (stringElement.Attribute("id") == null ||
                                    stringElement.Attribute("string") == null)
                                {
                                    continue;
                                }

                                idParam.Value = int.Parse(stringElement.Attribute("id").Value);
                                nameParam.Value = stringElement.Attribute("string").Value;
                                tooltipParam.Value = stringElement.Attribute("toolTip")?.Value ?? string.Empty;

                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing localization string: {stringElement}. Error: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading localization data from {file}: {ex.Message}");
                    }
                }
                transaction.Commit();
            }
        }

        public void Dispose()
        {
            dbConnection?.Close();
            dbConnection?.Dispose();
        }
    }

    public class ItemInfo
    {
        public int Id { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public int LinkEquipmentId { get; set; }
        public int RareGrade { get; set; }
        public bool HasEquipmentStats { get; set; }
        public string Balance { get; set; }
        public int Defense { get; set; }
        public string Impact { get; set; }
        public int MaxAttack { get; set; }
        public Image IconImage { get; set; }
    }

    public class SearchResult
    {
        public List<ItemInfo> Items { get; set; }
        public bool IsLimited { get; set; }
    }
}