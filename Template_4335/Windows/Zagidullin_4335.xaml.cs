using Microsoft.Win32;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Template_4335.Models;
using System.IO;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;

namespace Template_4335.Windows
{
    /// <summary>
    /// Логика взаимодействия для Zagidullin_4335.xaml
    /// </summary>
    public partial class Zagidullin_4335 : System.Windows.Window
    {
        public Zagidullin_4335()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DefaultExt = ".xls;*.xlsx",
                Filter = "файл Excel (Spisok.xlsx)|*.xlsx",
                Title = "Выберите файл базы данных"
            };
            if (!(openFileDialog.ShowDialog() == true))
                return;

            string[,] list;
            Microsoft.Office.Interop.Excel.Application ObjWorkExcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(openFileDialog.FileName);
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ObjWorkExcel.Sheets[1];
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell);
            int _columns = (int)lastCell.Column;
            int _rows = (int)lastCell.Row - 3;
            list = new string[_rows, _columns];
            for (int j = 0; j < _columns; j++)
                for (int i = 0; i < _rows; i++)
                    list[i, j] = ObjWorkSheet.Cells[i + 1, j + 1].Text;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            ObjWorkExcel.Quit();
            GC.Collect();

            using (UserContext db = new UserContext())
            {
                for (int i = 1; i < _rows; i++)
                {
                    User user = new User() { Id = int.Parse(list[i, 1]), Name = list[i, 0], Email = list[i, 8], Street = list[i, 5] };
                    db.Users.Add(user);
                }
                db.SaveChanges();
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //var streets = new List<string>();
            //using (UserContext db = new UserContext())
            //{
            //    for (int i = 0; i < db.Users.Count(); i++)
            //    {
            //        if (!streets.Contains(db.Users.ToList()[i].Street))
            //        {
            //            streets.Add(db.Users.ToList()[i].Street);
            //        }

            //    }

            //    for (int i = 0; i < streets.Count; i++)
            //    {
            //        db.Streets.Add(new Streets() { StreetName = streets[i] });
            //    }
            //    db.SaveChanges();
            //}

            List<User> allUsers;
            List<Streets> allStreets;

            using (UserContext db = new UserContext())
            {
                allUsers = db.Users.ToList().OrderBy(x => x.Name).ToList();
                allStreets = db.Streets.ToList().OrderBy(x => x.StreetName).ToList();
            }

            var app = new Microsoft.Office.Interop.Excel.Application();
            app.SheetsInNewWorkbook = allStreets.Count();
            Microsoft.Office.Interop.Excel.Workbook workbook = app.Workbooks.Add(Type.Missing);
            
            for (int i = 0; i < allStreets.Count; i++)
            {
                int startRowIndex = 1;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = app.Worksheets.Item[i + 1];
                worksheet.Name = allStreets[i].StreetName;
                worksheet.Cells[1][2] = "Код клиента";
                worksheet.Cells[2][2] = "ФИО";
                worksheet.Cells[3][2] = "E-mail";
                startRowIndex++;

                var usersCategories = allUsers.GroupBy(s => s.Street).ToList();

                foreach (var users in usersCategories)
                {
                    if (users.Key == allStreets[i].StreetName)
                    {
                        Microsoft.Office.Interop.Excel.Range headerRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[3][1]];
                        headerRange.Merge();
                        headerRange.Value = allStreets[i].StreetName;
                        headerRange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        foreach (User user in allUsers)
                        {
                            if (user.Street == users.Key)
                            {
                                worksheet.Cells[1][startRowIndex] = user.Id;
                                worksheet.Cells[2][startRowIndex] = user.Name;
                                worksheet.Cells[3][startRowIndex] = user.Email;
                                startRowIndex++;
                            }
                        }
                        worksheet.Cells[1][startRowIndex].Font.Bold = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                Microsoft.Office.Interop.Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[3][startRowIndex - 1]];
                rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = 
                    rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].LineStyle = 
                    rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].LineStyle = 
                    rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = 
                    rangeBorders.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                    Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous; 
                worksheet.Columns.AutoFit();
            }
            app.Visible = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (UserContext db = new UserContext())
            {
                var json = File.ReadAllText("D:\\Учеба\\4335\\2_семестр\\исрпо\\ЛР3\\Импорт\\3.json");
                var clients = JsonConvert.DeserializeObject<List<User>>(json);

                foreach (var client in clients)
                {
                    User dbClient = new User
                    {
                        Id = client.Id,
                        CodeClient = client.CodeClient,
                        Name = client.Name,
                        Email = client.Email,
                        Street = client.Street,
                    };
                    db.Users.Add(dbClient);
                }
                db.SaveChanges();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            using (UserContext db = new UserContext())
            {
                // Группировка по улицам
                var clientsByStreet = db.Users.GroupBy(c => c.Street);

                var wordApp = new Application();
                var document = wordApp.Documents.Add();

                // Для каждой улицы создаем новую страницу и добавляем на нее список клиентов
                foreach (var streetGroup in clientsByStreet)
                {
                    // Отсортировать клиентов по ФИО
                    var sortedClients = streetGroup.OrderBy(c => c.Name);

                    // Добавляем новую страницу
                    document.Words.Last.InsertBreak(WdBreakType.wdPageBreak);

                    // Добавляем заголовок с названием улицы
                    document.Words.Last.Text = streetGroup.Key;
                    document.Words.Last.set_Style(WdBuiltinStyle.wdStyleHeading1);

                    // Добавляем таблицу со списком клиентов
                    var clientsTable = document.Tables.Add(document.Words.Last, sortedClients.Count() + 1, 3);
                    clientsTable.Borders.Enable = 1;

                    // Заполняем заголовки столбцов таблицы
                    clientsTable.Cell(1, 1).Range.Text = "ФИО";
                    clientsTable.Cell(1, 2).Range.Text = "Код клиента";
                    clientsTable.Cell(1, 3).Range.Text = "E-mail";

                    // Заполняем ячейки таблицы данными клиентов
                    for (int i = 0; i < sortedClients.Count(); i++)
                    {
                        var client = sortedClients.ElementAt(i);
                        clientsTable.Cell(i + 2, 1).Range.Text = client.Name;
                        clientsTable.Cell(i + 2, 2).Range.Text = client.CodeClient;
                        clientsTable.Cell(i + 2, 3).Range.Text = client.Email;
                    }
                }

                // Сохраняем документ
                document.SaveAs("D:\\Учеба\\4335\\2_семестр\\исрпо\\ЛР3\\doc.docx");
                document.Close();
                wordApp.Quit();
            }
        }
    }
}
