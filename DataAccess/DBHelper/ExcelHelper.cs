using System.Data;
using System.IO;
using ExcelDataReader;

public static class ExcelHelper
{
    public static DataTable ReadExcelToDataTable(string filePath)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                return result.Tables[0]; // Trả về sheet đầu tiên
            }
        }
    }
}