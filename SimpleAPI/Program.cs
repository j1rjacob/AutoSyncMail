using System;
using System.IO;

namespace SimpleAPI
{
    class Program
    {
      static void Main(string[] args)
        {
            //Initialize Directory
            try
            {
                LoadDirectory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //Subfolders
            LoadSubfolders();



            
            Console.WriteLine("Done Bye!");
            Console.ReadLine();
        }

        private static void LoadSubfolders()
        {
            var filename = "D039726BB19C";
            var root = @"F:\TMFRoot\";

            if (!Directory.Exists(root + filename))
            {
                Directory.CreateDirectory(root + filename);
                //CopyRDSFile();
            }
        }

        private static void CopyRDSFile()
        {
            throw new NotImplementedException();
        }

        private static void LoadDirectory()
        {
            var temp = @"F:\TMFTemp\";
            var root = @"F:\TMFRoot\";
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
                Directory.CreateDirectory(temp);
            }
            else
            {
                Directory.CreateDirectory(temp);
            }

            //Create Once
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                Directory.CreateDirectory(root+"Logs");
            }
        }
    }
}
