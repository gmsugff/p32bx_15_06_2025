using System.Data.SQLite;

namespace p32bxdateBasse
{
    internal class Program
    {
        
            private const string DbFile = "coffee_shop.db";
            private static string ConnectionString => $"Data Source={DbFile};Version=3;";

            static void Main()
            {
                CreateDatabaseAndTable();

                Console.WriteLine("Попытка подключения к базе данных...");
                if (TryConnect())
                {
                    Console.WriteLine("Подключение успешно.");
                }
                else
                {
                    Console.WriteLine("Не удалось подключиться к базе данных.");
                    return;
                }

                // Для демонстрации добавим пару записей (если таблица пуста)
                InsertSampleDataIfEmpty();

                while (true)
                {
                    Console.WriteLine("\nВыберите действие:");
                    Console.WriteLine("1 - Показать всю информацию");
                    Console.WriteLine("2 - Показать названия всех сортов кофе");
                    Console.WriteLine("3 - Показать кофе вида арабика");
                    Console.WriteLine("4 - Показать кофе вида робуста");
                    Console.WriteLine("5 - Показать купажи/бленды");
                    Console.WriteLine("6 - Показать сорта с количеством <= 200 грамм");
                    Console.WriteLine("7 - Показать минимальную себестоимость");
                    Console.WriteLine("8 - Показать максимальную себестоимость");
                    Console.WriteLine("9 - Показать среднюю себестоимость");
                    Console.WriteLine("10 - Кол-во сортов с минимальной себестоимостью");
                    Console.WriteLine("11 - Кол-во сортов с максимальной себестоимостью");
                    Console.WriteLine("12 - Кол-во арабики, робусты и купажей");
                    Console.WriteLine("0 - Выход");

                    Console.Write("Введите номер: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1": ShowAllCoffee(); break;
                        case "2": ShowAllCoffeeNames(); break;
                        case "3": ShowCoffeeByType("арабика"); break;
                        case "4": ShowCoffeeByType("робуста"); break;
                        case "5": ShowCoffeeByType("купаж"); break;
                        case "6": ShowCoffeeByQuantity(200); break;
                        case "7": ShowMinCost(); break;
                        case "8": ShowMaxCost(); break;
                        case "9": ShowAvgCost(); break;
                        case "10": ShowCountByMinCost(); break;
                        case "11": ShowCountByMaxCost(); break;
                        case "12": ShowCountByTypes(); break;
                        case "0": return;
                        default: Console.WriteLine("Неверный ввод."); break;
                    }
                }
            }

            static void CreateDatabaseAndTable()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = @"
            CREATE TABLE IF NOT EXISTS Coffee (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Country TEXT NOT NULL,
                Type TEXT NOT NULL CHECK(Type IN ('арабика', 'робуста', 'купаж')),
                Description TEXT,
                QuantityGrams INTEGER NOT NULL CHECK(QuantityGrams >= 0),
                Cost REAL NOT NULL CHECK(Cost >= 0)
            );";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            static bool TryConnect()
            {
                try
                {
                    using var conn = new SQLiteConnection(ConnectionString);
                    conn.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}");
                    return false;
                }
            }

            static void InsertSampleDataIfEmpty()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string checkSql = "SELECT COUNT(*) FROM Coffee;";
                using var checkCmd = new SQLiteCommand(checkSql, conn);
                long count = (long)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    string insertSql = @"
                INSERT INTO Coffee (Name, Country, Type, Description, QuantityGrams, Cost) VALUES
                ('Эфиопия Сидамо', 'Эфиопия', 'арабика', 'Яркий цветочный аромат', 250, 500.0),
                ('Бразилия Сантос', 'Бразилия', 'робуста', 'Насыщенный вкус с шоколадными нотами', 150, 350.0),
                ('Купаж Арома', 'Колумбия', 'купаж', 'Сбалансированный вкус арабики и робусты', 300, 450.0);
            ";
                    using var insertCmd = new SQLiteCommand(insertSql, conn);
                    insertCmd.ExecuteNonQuery();
                    Console.WriteLine("Добавлены примерные данные.");
                }
            }

            static void ShowAllCoffee()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT * FROM Coffee;";
                using var cmd = new SQLiteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("Вся информация о кофе:");
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]}, Название: {reader["Name"]}, Страна: {reader["Country"]}, Вид: {reader["Type"]}, Описание: {reader["Description"]}, Кол-во (г): {reader["QuantityGrams"]}, Себестоимость: {reader["Cost"]}");
                }
            }

            static void ShowAllCoffeeNames()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT Name FROM Coffee;";
                using var cmd = new SQLiteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("Названия всех сортов кофе:");
                while (reader.Read())
                {
                    Console.WriteLine(reader["Name"]);
                }
            }

            static void ShowCoffeeByType(string type)
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT * FROM Coffee WHERE Type = @type;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@type", type);

                using var reader = cmd.ExecuteReader();

                Console.WriteLine($"Кофе вида {type}:");
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]}, Название: {reader["Name"]}, Страна: {reader["Country"]}, Описание: {reader["Description"]}, Кол-во (г): {reader["QuantityGrams"]}, Себестоимость: {reader["Cost"]}");
                }
            }

            static void ShowCoffeeByQuantity(int maxQuantity)
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT Name FROM Coffee WHERE QuantityGrams <= @maxQty;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maxQty", maxQuantity);

                using var reader = cmd.ExecuteReader();

                Console.WriteLine($"Сорта кофе с количеством не больше {maxQuantity} грамм:");
                while (reader.Read())
                {
                    Console.WriteLine(reader["Name"]);
                }
            }

            static void ShowMinCost()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT MIN(Cost) FROM Coffee;";
                using var cmd = new SQLiteCommand(sql, conn);

                var minCost = cmd.ExecuteScalar();
                Console.WriteLine($"Минимальная себестоимость: {minCost}");
            }

            static void ShowMaxCost()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT MAX(Cost) FROM Coffee;";
                using var cmd = new SQLiteCommand(sql, conn);

                var maxCost = cmd.ExecuteScalar();
                Console.WriteLine($"Максимальная себестоимость: {maxCost}");
            }

            static void ShowAvgCost()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "SELECT AVG(Cost) FROM Coffee;";
                using var cmd = new SQLiteCommand(sql, conn);

                var avgCost = cmd.ExecuteScalar();
                Console.WriteLine($"Средняя себестоимость: {avgCost:F2}");
            }

            static void ShowCountByMinCost()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string minSql = "SELECT MIN(Cost) FROM Coffee;";
                using var minCmd = new SQLiteCommand(minSql, conn);
                var minCost = Convert.ToDouble(minCmd.ExecuteScalar());

                string countSql = "SELECT COUNT(*) FROM Coffee WHERE Cost = @minCost;";
                using var countCmd = new SQLiteCommand(countSql, conn);
                countCmd.Parameters.AddWithValue("@minCost", minCost);

                var count = countCmd.ExecuteScalar();
                Console.WriteLine($"Количество сортов с минимальной себестоимостью ({minCost}): {count}");
            }

            static void ShowCountByMaxCost()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string maxSql = "SELECT MAX(Cost) FROM Coffee;";
                using var maxCmd = new SQLiteCommand(maxSql, conn);
                var maxCost = Convert.ToDouble(maxCmd.ExecuteScalar());

                string countSql = "SELECT COUNT(*) FROM Coffee WHERE Cost = @maxCost;";
                using var countCmd = new SQLiteCommand(countSql, conn);
                countCmd.Parameters.AddWithValue("@maxCost", maxCost);

                var count = countCmd.ExecuteScalar();
                Console.WriteLine($"Количество сортов с максимальной себестоимостью ({maxCost}): {count}");
            }

            static void ShowCountByTypes()
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = @"
            SELECT Type, COUNT(*) as Count
            FROM Coffee
            GROUP BY Type;
        ";

                using var cmd = new SQLiteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("Количество сортов по видам кофе:");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Type"]}: {reader["Count"]}");
                }
            }

        }
    }

