using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementlibrary;

namespace TestDLLConsoleApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string message = string.Empty;
            int retVal = UserAuthentication.Authenticated("guoqing", "getech", ref message);
            Console.WriteLine(message);

            Console.ReadLine();
        }
    }
}
