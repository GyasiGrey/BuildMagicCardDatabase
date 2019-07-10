using System;
using System.Data;

namespace AccessLayer
{
    public class DBParameter
    {
        public SqlDbType type;
        public Object value;
        public string name;
        public bool output;
        public bool isCSV;
        public SqlDbType csvType;
    }
}