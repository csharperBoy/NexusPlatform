using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Exporter.Excel
{

    public class ExcelExporter
    {
        /// <summary>
        /// ذخیره‌سازی یک لیست از اشیاء در مسیر مشخص (اکسل)
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="data">لیست داده‌ها</param>
        /// <param name="filePath">مسیر کامل فایل خروجی (مثلاً C:\output.xlsx)</param>
        /// <param name="sheetName">نام برگه (اختیاری)</param>
        public static void ExportToExcel<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1")
        {
            if (data == null || !data.Any())
                throw new ArgumentException("لیست داده خالی است.");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // دریافت خصوصیات کلاس
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Where(p => p.CanRead)
                                              .ToArray();

                    // نوشتن هدرها (ردیف اول)
                    for (int col = 0; col < properties.Length; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = properties[col].Name;
                        // تنظیم پررنگ برای هدر (اختیاری)
                        //worksheet.Cell(1, col + 1).Style.Font.Bold = true;
                    }

                    // نوشتن داده‌ها (از ردیف دوم)
                    int row = 2;
                    foreach (var item in data)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var value = properties[col].GetValue(item) ?? string.Empty;
                            worksheet.Cell(row, col + 1).Value = value.ToString();
                        }
                        row++;
                    }

                    // تنظیم خودکار عرض ستون‌ها
                    //worksheet.Columns().AdjustToContents();

                    // ذخیره فایل
                    workbook.SaveAs(filePath);
                    Console.WriteLine($"فایل اکسل با موفقیت در {filePath} ذخیره شد.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در ذخیره‌سازی اکسل: {ex.Message}");
                throw;
            }
        }
    }
}
