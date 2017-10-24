using System.Diagnostics;

namespace TMFGmail.AppExtension
{
    public static class Search
    {
        public static string Query(this string query)
        {
            if (query == "Everyday")
            {
                return "CAS Gateway has:attachment newer_than:1d";
            }
            else if (query == "Every week")
            {
                return "CAS Gateway has:attachment newer_than:7d";
            }
            else if (query == "Every month")
            {
                return "CAS Gateway has:attachment newer_than:1m";
            }else if (query == "Every year")
            {
                return "CAS Gateway has:attachment newer_than:1y";
            }
            else if (query == "Custom")
            {
                Debug.WriteLine(SearchDate.DateFrom);
                Debug.WriteLine(SearchDate.DateTo);
                return "CAS Gateway has:attachment after:"+ SearchDate.DateFrom +"before:"+ SearchDate.DateTo;
            }
            return "CAS Gateway has:attachment newer_than:1d"; 
        }
    }
}
