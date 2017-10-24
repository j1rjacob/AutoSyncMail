using System;

namespace SimpleAPI
{
    public static class Subject
    {
        public static string GetSubject(this string subject)
        {
            string[] separator1 = { " " };
            string[] results;

            results = subject.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
            return results[2].Replace(":", "");
        }
    }
}
