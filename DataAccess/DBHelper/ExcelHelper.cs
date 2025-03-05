using System;
using System.Data;
using System.IO;
using System.Linq;
using ExcelDataReader;

public static class ExcelHelper
{
    public static DataTable ReadExcelToDataTable(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Khong ton tai file: {filePath}");
        }

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true // Đảm bảo dòng đầu tiên là tiêu đề cột
                    }
                });

                if (result.Tables.Count == 0)
                {
                    throw new Exception("File Excel khong chua du lieu hop le.");
                }

                DataTable dataTable = result.Tables[0];

                // In danh sách cột để kiểm tra
                Console.WriteLine("Các cột thực tế trong file Excel:");
                foreach (DataColumn column in dataTable.Columns)
                {
                    Console.WriteLine($"- '{column.ColumnName}'");
                }

                // Kiểm tra xem cột 'Username' có tồn tại không
                if (!dataTable.Columns.Cast<DataColumn>().Any(c => c.ColumnName.Trim() == "Username"))
                {
                    throw new Exception("File khong chua cot Username hoac dang co khoang trang.");
                }

                return dataTable;
            }
        }
    }
}
