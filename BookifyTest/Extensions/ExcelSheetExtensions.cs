using ClosedXML.Excel;

namespace Bookify.Web.Extensions
{
    public static class ExcelSheetExtensions
    {
        public static void AddHeader(this IXLWorksheet sheet, string[] headerCells)
        {

            for (int i = 0; i < headerCells.Length; i++)
            {
                sheet.Cell(4, i + 1).SetValue(headerCells[i]);
            }
            //var header = sheet.Row(4);
            //header.Style.Fill.BackgroundColor = XLColor.Black;
            //header.Style.Font.FontColor = XLColor.White;
            //header.Style.Font.SetBold();
        }
        public static void AddStyles(this IXLWorksheet sheet)
        {
            sheet.ColumnsUsed().AdjustToContents();
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.CellsUsed().Style.Border.OutsideBorderColor = XLColor.Black;
        }
        public static void AddToLocalImage(this IXLWorksheet sheet, string imagePath)
        {
            sheet.AddPicture(imagePath).MoveTo(sheet.Cell("A1")).Scale(0.2);
        }
        public static void AddTable(this IXLWorksheet sheet, int numberOfRows, int NumberOfColumns)
        {
            var range = sheet.Range(4, 1, numberOfRows + 4, NumberOfColumns);
            var table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium16;
            table.ShowAutoFilter = false;
        }
    }
}
