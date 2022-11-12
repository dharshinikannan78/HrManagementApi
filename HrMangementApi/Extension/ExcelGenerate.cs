using ClosedXML.Excel;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;

namespace HrMangementApi.Extension
{
    public static class ExcelGenerate
    {
        public static byte[] CreateExcel<T>(this List<T> list)
        {
            Type tType = typeof(T);
            var attribute = tType.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                           .Cast<DisplayNameAttribute>().FirstOrDefault();
            var sheetName = attribute == null ? "Sheet" : attribute.DisplayName;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            PropertyInfo[] headerInfo = tType.GetProperties();
            foreach (var property in headerInfo)
            {
                attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                           .Cast<DisplayNameAttribute>().FirstOrDefault();

                if (property.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)) continue;

                headers.Add(property.Name, attribute == null ? property.Name : attribute.DisplayName);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                // Add Header Columns
                var count = 1;
                foreach (var entry in headers)
                {
                    worksheet.Cell(1, count).Value = entry.Value;
                    count++;
                }

                // Add Value
                int rowNumber = 2;
                for (int row = 0; row < list.Count; row++)
                {
                    var item = list[row];
                    int column = 1;
                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        var y = typeof(T).InvokeMember(entry.Key.ToString(), BindingFlags.GetProperty, null, item, null);
                        worksheet.Cell(rowNumber, column).Value = (y == null) ? "" : y.ToString();
                        column++;
                    }
                    rowNumber++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return content;
                }
            }

        }

    }
}
