using System;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Kadastr.CommonUtils;
using Kadastr.WebApp.Code.Helpers;
using Kadastr.WebApp.Code;


namespace Kadastr.WebApp.Statics
{
    public class TemplateFactory
    {
        public static void PrintTemplateFile()
        {
            ExportFileInfo info = HttpContext.Current.Session["ExportFileInfo"] as ExportFileInfo;
            if (info == null)
                throw new InvalidCastException("Отсутсвует информация о выгружаемых данных!");

            string fileName = HttpUtility.UrlPathEncode(info.FileName);

            if (info.IsExcelExport)
            {
                ExcelExportHelper.DoExport(info.ExportData, fileName);
                return;
            }

            string allText = info.FileDataAsBase64String;
            HttpContext.Current.Session[SessionViewstateConstants.ExportFileInfo] = null;
            byte[] buf = Convert.FromBase64String(allText);
            var response = HttpContext.Current.Response;
            response.Clear();
            response.ClearContent();
            response.ClearHeaders();
            response.HeaderEncoding = Encoding.UTF8;
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            response.ContentType = "application/octet-stream";
            response.AddHeader("Content-Length", buf.Length.ToString());
            response.BinaryWrite(buf);
            response.End();
        }
    }
}