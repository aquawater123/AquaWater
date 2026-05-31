using System;
using System.Collections.Generic;
using System.Text;
using C1.Win.C1FlexGrid;
using CarlosAg.ExcelXmlWriter;


namespace XMLEXCEL
{

    public class ShengComparer : System.Collections.IComparer//实现自定义排序接口
    {
        private static int SortCol = 0;
        private static int sortOrderModifier = 1;
        int System.Collections.IComparer.Compare(object x, object y)
        {

            Row left = (Row)x;
            Row right = (Row)y;
            int L, R;
            L = getIndex(left[SortCol].ToString());
            R = getIndex(right[SortCol].ToString());
            int CompareResult;
            CompareResult = L.CompareTo(R);
            return CompareResult * sortOrderModifier;
        }
        public ShengComparer(int col_Sort, int sortOrder)
        {
            SortCol = col_Sort;
            sortOrderModifier = sortOrder;
        }

        int getIndex(string Name)
        {
            if (Name.Length > 2)
            {
                Name = Name.Remove(2);
            }
            switch (Name)
            {
                case "北京":
                    return 1;
                case "天津":
                    return 2;
                case "河北":
                    return 3;
                case "山西":
                    return 4;
                case "内蒙":
                    return 5;
                case "辽宁":
                    return 6;
                case "吉林":
                    return 7;
                case "黑龙":
                    return 8;
                case "上海":
                    return 9;
                case "江苏":
                    return 10;
                case "浙江":
                    return 11;
                case "安徽":
                    return 12;
                case "福建":
                    return 13;
                case "江西":
                    return 14;
                case "山东":
                    return 15;
                case "河南":
                    return 16;
                case "湖北":
                    return 17;
                case "湖南":
                    return 18;
                case "广东":
                    return 19;
                case "广西":
                    return 20;
                case "海南":
                    return 21;
                case "重庆":
                    return 22;
                case "四川":
                    return 23;
                case "贵州":
                    return 24;
                case "云南":
                    return 25;
                case "西藏":
                    return 26;
                case "陕西":
                    return 27;
                case "甘肃":
                    return 28;
                case "青海":
                    return 29;
                case "宁夏":
                    return 30;
                case "新疆":
                    return 31;
                case "兵团":
                    return 32;
                default:
                    return 33;
            }
        }

    }


    class FlexGridExcel : C1FlexGrid
    {
        public FlexGridExcel()
            : base()
        {

        }

        public bool SaveXMLExcel(string _fileName, string _sheetName, string _title, bool _IsAutoFitWidth)
        {
            string fileName = string.IsNullOrEmpty(_fileName) ? (string.IsNullOrEmpty(_title) ? "未命名文件" : _title) : _fileName;
            try
            {

                Workbook _book = new Workbook();
                InitializeBook(_book, _title);
                SetStyles(_book.Styles);

                //  Worksheet _sheet = book.Worksheets.Add(sheetName);

                SetSheels(_book.Worksheets, _sheetName, _title, _IsAutoFitWidth);

                _book.Save(fileName);
                //Process.Start(filename);  
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 初始化Excel Workbook
        /// </summary>
        /// <param >book</param>
        private void InitializeBook(Workbook book, string _title)
        {

            // Specify which Sheet should be opened and the size of window by default
            book.ExcelWorkbook.ActiveSheetIndex = 1;
            book.Properties.Author = "全国灌区灌溉用水有效利用系数测算分析信息管理系统";
            book.Properties.Title = _title;
            book.Properties.Created = DateTime.Now;
            book.Properties.Company = "中国水科院水资源所节水室";
            book.ExcelWorkbook.WindowHeight = 13500;
            book.ExcelWorkbook.WindowWidth = 17100;
            book.ExcelWorkbook.WindowTopX = 360;
            book.ExcelWorkbook.WindowTopY = 75;

            //book.ExcelWorkbook.ProtectWindows = false;
            //book.ExcelWorkbook.ProtectStructure = false;
        }
        private void SetStyles(WorksheetStyleCollection styles)
        {
            // -----------------------------------------------
            //  Default
            // -----------------------------------------------
            WorksheetStyle Default = styles.Add("Default");
            Default.Name = "Normal";
            Default.Font.FontName = "宋体";
            Default.Font.Size = 12;
            Default.Alignment.Vertical = StyleVerticalAlignment.Center;
            // -----------------------------------------------
            //  TitleStyle
            // -----------------------------------------------
            WorksheetStyle TitleStyle = styles.Add("TitleStyle");
            TitleStyle.Font.Bold = true;
            TitleStyle.Font.FontName = "宋体";
            TitleStyle.Font.Size = 16;
            TitleStyle.Alignment.Horizontal = StyleHorizontalAlignment.Center;
            TitleStyle.Alignment.Vertical = StyleVerticalAlignment.Center;
            // -----------------------------------------------
            //  FieldStyle
            // -----------------------------------------------
            WorksheetStyle FieldStyle = styles.Add("FieldStyle");
            //FieldStyle.Font.Bold = true;
            FieldStyle.Font.FontName = "宋体";
            FieldStyle.Font.Size = 12;
            FieldStyle.Alignment.Horizontal = StyleHorizontalAlignment.Center;
            FieldStyle.Borders.Add(StylePosition.Bottom, LineStyleOption.Continuous, 1);
            FieldStyle.Borders.Add(StylePosition.Left, LineStyleOption.Continuous, 1);
            FieldStyle.Borders.Add(StylePosition.Right, LineStyleOption.Continuous, 1);
            FieldStyle.Borders.Add(StylePosition.Top, LineStyleOption.Continuous, 1);
            // -----------------------------------------------
            //   HeaderStyle 
            // -----------------------------------------------  
            WorksheetStyle HeaderStyle = styles.Add("HeaderStyle");
            HeaderStyle.Font.FontName = "宋体";
            HeaderStyle.Font.Size = 16;
            HeaderStyle.Font.Bold = true;
            HeaderStyle.Alignment.Horizontal = StyleHorizontalAlignment.Center;
            FieldStyle.Alignment.Vertical = StyleVerticalAlignment.Center;
            //HeaderStyle.Font.Color = "White";
            //HeaderStyle.Interior.Color = "Blue";
            //HeaderStyle.Interior.Pattern = StyleInteriorPattern.DiagCross;
            HeaderStyle.Borders.Add(StylePosition.Top, LineStyleOption.Continuous, 2);
            HeaderStyle.Borders.Add(StylePosition.Bottom, LineStyleOption.Continuous, 1);
            HeaderStyle.Borders.Add(StylePosition.Left, LineStyleOption.Continuous, 2);
            HeaderStyle.Borders.Add(StylePosition.Right, LineStyleOption.Continuous, 2);
            // -----------------------------------------------
            //  MyDataStyle
            // -----------------------------------------------
            WorksheetStyle MyDataStyle = styles.Add("MyDataStyle");
            //FieldStyle.Font.Bold = true;
            MyDataStyle.Font.FontName = "宋体";
            MyDataStyle.Font.Size = 12;
            MyDataStyle.Alignment.Horizontal = StyleHorizontalAlignment.Center;
            MyDataStyle.Borders.Add(StylePosition.Bottom, LineStyleOption.Continuous, 1);
            MyDataStyle.Borders.Add(StylePosition.Left, LineStyleOption.Continuous, 1);
            MyDataStyle.Borders.Add(StylePosition.Right, LineStyleOption.Continuous, 1);
            MyDataStyle.Borders.Add(StylePosition.Top, LineStyleOption.Continuous, 1);
        }

        private void MergeCell(Worksheet sheet, int row0, int col0, int row1, int col1, bool isLastCol)
        {
            int i, j;

            try
            {
                for (i = row1; i >= row0; i--)
                {
                    for (j = col1; j > col0; j--)
                    {
                        sheet.Table.Rows[row1 + 1].Cells.RemoveAt(j);
                    }
                    if (!isLastCol)
                    {
                        if (sheet.Table.Rows[i + 1].Cells[j + 1].Index == 0)
                        {
                            sheet.Table.Rows[i + 1].Cells[j + 1].Index = col1 + 2;
                        }
                    }
                    if (i != row0)
                    {
                        sheet.Table.Rows[i + 1].Cells.RemoveAt(j);
                    }
                    else
                    {
                        sheet.Table.Rows[i + 1].Cells[j].Index = j + 1;
                    }
                }
                sheet.Table.Rows[row0 + 1].Cells[col0].MergeAcross = col1 - col0;
                sheet.Table.Rows[row0 + 1].Cells[col0].MergeDown = row1 - row0;
            }
            catch (Exception e)
            {
                // string sss = e.ToString();
            }

        }

        private void SetSheels(WorksheetCollection sheets, string _sheetName, string _title, bool IsAutoFitWidth)
        {

            //// we can optionally set some column settings
            //sheet.Table.Columns.Add(new WorksheetColumn(150));
            //sheet.Table.Columns.Add(new WorksheetColumn(100));

            //WorksheetRow row = sheet.Table.Rows.Add();
            //row.Cells.Add(new WorksheetCell("Header 1", "TitleStyle"));
            //row.Cells.Add(new WorksheetCell("Header 2", "TitleStyle"));
            //WorksheetCell cell = row.Cells.Add("Header 3");
            //cell.MergeAcross = 1;            // Merge two cells together

            //cell.StyleID = "TitleStyle";

            //row = sheet.Table.Rows.Add();
            //// Skip one row, and add some text
            //row.Index = 3;
            //row.Cells.Add("Data");
            //row.Cells.Add("Data 1");
            //row.Cells.Add("Data 2");
            //row.Cells.Add("Data 3");

            //// Generate 30 rows
            //for (int i = 0; i < 30; i++)
            //{
            //    row = sheet.Table.Rows.Add();
            //    row.Cells.Add("Row " + i.ToString());
            //    row.Cells.Add(new WorksheetCell(i.ToString(), DataType.Number));
            //}

            //// Add a Hyperlink
            //row = sheet.Table.Rows.Add();
            //cell = row.Cells.Add();
            //cell.Data.Text = "Carlos Aguilar Mares";
            //cell.HRef = "http://www.carlosag.net";

            //// Add a Formula for the above 30 rows
            //cell = row.Cells.Add();
            //cell.Formula = "=SUM(R[-30]C:R[-1]C)";

            //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            Worksheet sheet = sheets.Add(_sheetName);
            sheet.Table.DefaultRowHeight = 14.25F;
            sheet.Table.DefaultColumnWidth = 54F;
            sheet.Table.FullColumns = 1;
            sheet.Table.FullRows = 1;

            // -----------------------------------------------
            WorksheetRow row = null;
            WorksheetCell cell = null;
            CellRange mergeRang;
            #region 大标题
            row = sheet.Table.Rows.Add();
            row.AutoFitHeight = true;
            row.Height = 30;
            cell = row.Cells.Add();
            cell.StyleID = "HeaderStyle";
            cell.Data.Type = DataType.String;
            cell.Data.Text = _title;
            //cell.MergeAcross = _dataTable.Columns.Count - 1;
            cell.MergeAcross = Cols.Count - 1;
            #endregion

            //foreach (DataColumn dc in _dataTable.Columns)//初始化列宽度集合
            //{
            //    _maxLengthOfField[dc.ColumnName] = 0;
            //}

            //-----------------------------------------------字段

            for (int i = 0; i < Rows.Count; i++)
            {
                row = sheet.Table.Rows.Add();
                row.AutoFitHeight = true;
                for (int j = 0; j < Cols.Count; j++)
                {
                    cell = row.Cells.Add();
                    if (base[i, j] == null)
                    {
                        cell.Data.Text = string.Empty;
                    }
                    else
                    {
                        cell.Data.Text = base[i, j].ToString();
                        cell.Data.Type = TypeConvert(base[i, j].GetType()); //// DataType.String;//
                        cell.StyleID = "MyDataStyle";
                    }


                }
            }
            for (int i = Rows.Count - 1; i >= 0; i--)
            {
                for (int j = Cols.Count - 1; j >= 0; j--)
                {
                    mergeRang = base.GetMergedRange(i, j);

                    if (i == mergeRang.r1 && j == mergeRang.c1 && (i < mergeRang.r2 || j < mergeRang.c2))
                    {
                        if (mergeRang.c2 == Cols.Count - 1)
                        {
                            MergeCell(sheet, mergeRang.r1, mergeRang.c1, mergeRang.r2, mergeRang.c2, true);
                        }
                        else
                        {
                            MergeCell(sheet, mergeRang.r1, mergeRang.c1, mergeRang.r2, mergeRang.c2, false);
                        }
                    }
                }
            }





            #region 字段标题行
            //if (_columnNamesCollection.Count != 0)
            //{
            //    for (int i = 0; i < _columnNamesCollection.Count; i++)
            //    {
            //        row = sheet.Table.Rows.Add();
            //        row.AutoFitHeight = true;
            //        int j = 0;
            //        foreach (ExcelColumn ec in _columnNamesCollection)
            //        {
            //            cell = row.Cells.Add();
            //            cell.Data.Text = ec.Name;
            //            cell.Data.Type = DataType.String;
            //            if (i != _columnNamesCollection.Count - 1)
            //            {
            //                cell.MergeAcross = ec.MergeAcross;
            //                cell.StyleID = "FieldStyle";
            //            }
            //            else//最下层标题行
            //            {
            //                cell.StyleID = "LastFieldStyle";
            //                _maxLengthOfField[_dataTable.Columns[j++].ColumnName] =
            //                    GetMaxLength(_maxLengthOfField[_dataTable.Columns[j++].ColumnName], ec.Name);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    row = sheet.Table.Rows.Add();
            //    row.AutoFitHeight = true;
            //    foreach (DataColumn dc in _dataTable.Columns)
            //    {
            //        cell = row.Cells.Add();
            //        cell.Data.Text = dc.ColumnName;
            //        cell.Data.Type = DataType.String;
            //        cell.StyleID = "FieldStyle";
            //        _maxLengthOfField[dc.ColumnName] = GetMaxLength(_maxLengthOfField[dc.ColumnName], dc.ColumnName);
            //    }
            //}

            #endregion
            // -----------------------------------------------
            #region 数据行


            //string dcValueS = null;
            //foreach (DataRow dr in _dataTable.Rows)
            //{
            //    row = sheet.Table.Rows.Add();
            //    row.AutoFitHeight = true;
            //    foreach (DataColumn dc in _dataTable.Columns)
            //    {
            //        dcValueO = dr[dc];
            //        if (dcValueO == DBNull.Value)
            //            dcValueS = string.Empty;
            //        else
            //            dcValueS = dcValueO.ToString();
            //        cell = row.Cells.Add();
            //        cell.Data.Text = dcValueS;
            //        cell.Data.Type = TypeConvert(dc.DataType);
            //        cell.StyleID = "DataStyle";
            //        if (_isAutoFitWidth || _columnNamesCollection.Count == 0)
            //        {
            //            _maxLengthOfField[dc.ColumnName] = GetMaxLength(_maxLengthOfField[dc.ColumnName], dcValueS);
            //        }
            //    }
            //}
            #endregion
            // -----------------------------------------------
            #region 设置列
            WorksheetColumn column = null;
            for (int i = 0; i < Cols.Count; i++)
            {
                column = new WorksheetColumn();
                if (IsAutoFitWidth)
                {
                    column.AutoFitWidth = true;
                }
                else
                {
                    column.Width = base.Cols[i].Width;
                }

                sheet.Table.Columns.Add(column);
            }



            //WorksheetColumn column = null;
            //if (!_isAutoFitWidth && _columnNamesCollection.Count != 0)
            //{
            //    foreach (ExcelColumn ec in _columnNamesCollection[_columnNamesCollection.Count - 1])
            //    {
            //        column = new WorksheetColumn();
            //        column.AutoFitWidth = false;
            //        column.Width = ec.Width;
            //        sheet.Table.Columns.Add(column);
            //    }
            //}
            //else
            //{
            //    foreach (DataColumn dc in _dataTable.Columns)
            //    {
            //        column = new WorksheetColumn();
            //        column.AutoFitWidth = false;
            //        column.Width = _maxLengthOfField[dc.ColumnName] * 7;
            //        sheet.Table.Columns.Add(column);
            //    }
            //}
            #endregion
            //  Options
            // -----------------------------------------------


            sheet.Options.Selected = true;
            sheet.Options.ProtectObjects = false;
            sheet.Options.ProtectScenarios = false;
            sheet.Options.Print.PaperSizeIndex = 9;
            sheet.Options.Print.HorizontalResolution = 300;
            sheet.Options.Print.VerticalResolution = 300;
            sheet.Options.Print.ValidPrinterInfo = true;
        }


        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param ></param>
        /// <returns></returns>
        private DataType TypeConvert(Type type)
        {

            switch (type.Name)
            {

                case "Decimal":

                case "Double":

                case "Single":

                    return DataType.Number;

                case "Int16":

                case "Int32":

                case "Int64":

                case "SByte":

                case "UInt16":

                case "UInt32":

                case "UInt64":

                    return DataType.Number;

                case "String":

                    return DataType.String;

                case "DateTime":

                    return DataType.String;
                //case "Boolean":
                //   return DataType .Boolean;
                default:

                    return DataType.String;

            }

        }


        public void ShengSort(int startRow, int SortCol, int sortOrder)
        {
            ShengComparer thisShengComparer = new ShengComparer(SortCol, sortOrder);
            Sort(startRow, Rows.Count - startRow, thisShengComparer);
        }



        //public int ShengSort(int startRow,int startCol)
        //{
        //    int i, j, k;
        //    i = startRow;
        //    j = startRow + 1;
        //    Row tempRow;
        //    for (; i < Rows.Count; i++)
        //    {
        //        for (; j < Rows.Count; j++)
        //        {
        //            if (getIndex(Rows[j - 1][startCol].ToString()) > getIndex(Rows[j][startCol].ToString()))
        //            {
        //                tempRow = Rows[j - 1];
        //                Rows[j - 1] = Rows[j];
        //                Rows[j] = tempRow;



        //            }
        //        }
        //    }
        //}

        int getIndex(string Name)
        {
            switch (Name)
            {
                case "北京":
                    return 1;
                case "天津":
                    return 2;
                case "河北":
                    return 3;
                case "山西":
                    return 4;
                case "内蒙古":
                    return 5;
                case "辽宁":
                    return 6;
                case "吉林":
                    return 7;
                case "黑龙江":
                    return 8;
                case "上海":
                    return 9;
                case "江苏":
                    return 10;
                case "浙江":
                    return 11;
                case "安徽":
                    return 12;
                case "福建":
                    return 13;
                case "江西":
                    return 14;
                case "山东":
                    return 15;
                case "河南":
                    return 16;
                case "湖北":
                    return 17;
                case "湖南":
                    return 18;
                case "广东":
                    return 19;
                case "广西":
                    return 20;
                case "海南":
                    return 21;
                case "重庆":
                    return 22;
                case "四川":
                    return 23;
                case "贵州":
                    return 24;
                case "云南":
                    return 25;
                case "西藏":
                    return 26;
                case "陕西":
                    return 27;
                case "甘肃":
                    return 28;
                case "青海":
                    return 39;
                case "宁夏":
                    return 30;
                case "新疆":
                    return 31;
                case "兵团":
                    return 32;
                default:
                    return 33;

            }
        }

        private enum ShengNameList
        {
            北京,
            天津,
            河北,
            山西,
            内蒙古,
            辽宁,
            吉林,
            黑龙江,
            上海,
            江苏,
            浙江,
            安徽,
            福建,
            江西,
            山东,
            河南,
            湖北,
            湖南,
            广东,
            广西,
            海南,
            重庆,
            四川,
            贵州,
            云南,
            西藏,
            陕西,
            甘肃,
            青海,
            宁夏,
            新疆,
            兵团
        }
    }

}
