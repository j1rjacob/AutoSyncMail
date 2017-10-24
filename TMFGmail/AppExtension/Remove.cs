using System.IO;

namespace TMFGmail.AppExtension
{
    public static class Remove
    {
        public static void RemoveRAW(this string path)
        {
            string filesToDelete = @"*RAW*.csv";  
            string[] fileList = Directory.GetFiles(path, filesToDelete);
            foreach (string file in fileList)
            {
                //Debug.WriteLine(file + " was deleted");
                File.Delete(file);
            }
        }
    }
}
