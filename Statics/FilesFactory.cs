using System;
using System.IO;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace Kadastr.WebApp.Statics
{
    public class FilesFactory
    {
        /// <summary>
        /// Сохранение файла на сервере
        /// </summary>
        /// <param name="file">файл</param>
        /// <param name="path">путь</param>
        public static void SavingFileToServer(FileUpload file, ref string path)
        {
            // путь к папке где временно будет храниться бд фиас
            //string dirPath = System.Web.HttpContext.Current.Server.MapPath("~").ToString();
            //if (dirPath.Trim().EndsWith(@"\"))
            //    dirPath = "C:\\" + "TempDataBase\\";
            //else 
            //     dirPath = dirPath + "\\TempDataBase\\";

            // получаем путь к папке
            path = GetPathToFolder();

            // проверка, есть ли папка, если нет то создаем
            CheckingFolderOnDisk(path);

            // имя загруженного файла
            string fileName = file.FileName;

            // добавляем название файла к пути его хранения
            fileName = path + fileName;

            // сохраняем его на сервера
            file.SaveAs(fileName);
        }

        /// <summary>
        /// Метод очистки папки от файлов
        /// </summary>
        /// <param name="path">путь к папке</param>
        public static void ClearingDirectoryFromFolder()
        {
            // получаем путь к папке
            string path = GetPathToFolder();

            // получаем путь к папке
            DirectoryInfo di = new DirectoryInfo(path);

            //проверка, есть ли папка в нужном месте
            if (!CheckingFolderOnDisk(path))
            {
                return;
            }

            // удаляем все файлы (папки) в папке
            foreach (FileInfo delfile in di.GetFiles())
            {
                delfile.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }         
        }

        /// <summary>
        /// Метод для проверки присутствия папки на диске
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool CheckingFolderOnDisk(string path)
        {
            // получаем путь к папке
            DirectoryInfo di = new DirectoryInfo(path);

            //проверка, есть ли папка в нужном месте
            if (!Directory.Exists(path))
            {
                // создаем папку и выходим из функции
                di = Directory.CreateDirectory(path);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Метод для получения адреса папки, куда будут сохраняться данные
        /// </summary>
        /// <returns></returns>
        private static string GetPathToFolder()
        {
            if (WebConfigurationManager.AppSettings["FilesDirectory"] != null)
            {
                return WebConfigurationManager.AppSettings["FilesDirectory"].ToString();
            }
            else
            {
                return "C:\\TempDataBase\\";
            }
        }
    }
}