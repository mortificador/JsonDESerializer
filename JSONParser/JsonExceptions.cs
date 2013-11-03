using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace JSONParser
{
    class JsonBadFormattedException : Exception
    {
        public JsonBadFormattedException(int index)
        {
            Console.Write("Bad format exception on index: " + index);
        }

        public JsonBadFormattedException(int index, string message)
        {
            Console.Write(message + ": " + index.ToString());
        }
    }
}
