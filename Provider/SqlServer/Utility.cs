using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCreator
{
    public static class UtilitySqlServer
    {
        public static string BuildConnectionString(string server, string database, string username, string password)
        {
            return string.Format("data source={0};initial catalog={1};integrated security=false;persist security info=True;User ID={2};Password={3}", server, database, username, password); 
        }
    }
}
