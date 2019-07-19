using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace AccessLayer
{
    public class Database
    {
        private SqlConnection connection;

        private bool open()
        {
            bool retval = true;

            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    //Try to open the connection to the DB
                    connection = openHelper();
                }
                catch
                {
                    try
                    {
                        //If the first one fails, sleep for 5 seconds and try again
                        System.Threading.Thread.Sleep(5000);
                        connection = openHelper();
                    }
                    catch
                    {
                        //If the second one fails sleep for 5 seconds, try again and allow any failure to bubble up
                        System.Threading.Thread.Sleep(5000);
                        connection = openHelper();
                    }
                }
            }

            return retval;
        }

        private SqlConnection openHelper()
        {
            SqlConnection con;
            con = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ConnectionString);
            con.Open();

            return con;
        }

        public void nonquery(String SQL)
        {
            DBParameter[] p = new DBParameter[0];
            nonquery(SQL, p);
        }

        public void nonquery(String SQL, DBParameter[] parameters)
        {
            open();

            using (connection)
            {
                using(SqlCommand command = new SqlCommand(SQL, connection))
                {
                    foreach (DBParameter p in parameters)
                    {
                        command.Parameters.Add(p.name, p.type);
                        if (p.value == null)
                        {
                            command.Parameters[p.name].Value = DBNull.Value;
                        }
                        else
                        {
                            command.Parameters[p.name].Value = p.value;
                        }

                        if (p.output)
                        {
                            command.Parameters[p.name].Direction = ParameterDirection.InputOutput;
                        }
                    }

                    command.ExecuteNonQuery();

                    foreach(DBParameter p in parameters)
                    {
                        if(p.output)
                        {
                            p.value = command.Parameters[p.name].Value;
                        }
                    }
                }
            }
        }

        public List<List<DBParameter>> query(String SQL)
        {
            DBParameter[] p = new DBParameter[0];
            return query(SQL, p);
        }

        public List<List<DBParameter>> query(String SQL, DBParameter[] parameters)
        {
            List<List<DBParameter>> totalOutput = new List<List<DBParameter>>();

            open();

            //Check if any of the parameters are CSV 
            foreach(DBParameter p in parameters)
            {
                if(p.isCSV)
                {
                    //Re-constitute the SQL string to have all the new params
                    string[] tokens = ((string)p.value).Split(',');
                    StringBuilder strongbad = new StringBuilder();
                    int i = 0;

                    //Add the each token as a parameter
                    foreach (string t in tokens)
                    {
                        strongbad.Append(p.name + i + ", ");
                        i++;
                    }

                    //Remove the last ", " from strongbad
                    strongbad.Remove(strongbad.Length - 2, 2);

                    //Put the new list of params in the SQL string
                    SQL = SQL.Replace(p.name, strongbad.ToString());
                }
            }

            using (connection)
            {
                using(SqlCommand command = new SqlCommand(SQL, connection))
                {
                    foreach (DBParameter p in parameters)
                    {
                        if (p.isCSV)
                        {
                            //This parameter is a CSV (e.g. WHERE IN (x,y,z))
                            //Each value in the where statement needs to be a parameter
                            string[] tokens = ((string)p.value).Split(',');
                            int i = 0;

                            //Add the each token as a parameter
                            foreach(string t in tokens)
                            {
                                command.Parameters.Add(p.name + i, p.csvType);

                                if (t == null)
                                {
                                    command.Parameters[p.name + i].Value = DBNull.Value;
                                }
                                else
                                {
                                    command.Parameters[p.name + i].Value = t;
                                }

                                i++;
                            }
                        }
                        else
                        {
                            command.Parameters.Add(p.name, p.type);

                            if (p.value == null)
                            {
                                command.Parameters[p.name].Value = DBNull.Value;
                            }
                            else
                            {
                                command.Parameters[p.name].Value = p.value;
                            }

                            if (p.output)
                            {
                                command.Parameters[p.name].Direction = ParameterDirection.InputOutput;
                            }
                        }
                    }

                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int len = reader.FieldCount;
                            List<DBParameter> output = new List<DBParameter>();

                            for (int i = 0; i < len; i++)
                            {
                                DBParameter p = new DBParameter();

                                p.value = reader.GetValue(i);
                                p.name = reader.GetName(i);

                                output.Add(p);
                            }

                            totalOutput.Add(output);
                        }

                        foreach (DBParameter p in parameters)
                        {
                            if (p.output)
                            {
                                p.value = command.Parameters[p.name].Value;
                            }
                        }
                    }
                }
            }

            return totalOutput;
        }

        public static double? DBOutputDouble(Object o)
        {
            double? retval;

            if (o == DBNull.Value)
            {
                retval = null;
            }
            else
            {
                retval = (double?)o;
            }

            return retval;
        }

        public static int? DBOutputInt(Object o)
        {
            int? retval;

            if(o == DBNull.Value)
            {
                retval = null;
            }
            else
            {
                retval = (int?)o;
            }

            return retval;
        }

        public static string DBOutputString(Object o)
        {
            string retval;

            if (o == DBNull.Value)
            {
                retval = "";
            }
            else
            {
                retval = (string)o;
            }

            return retval;
        }
    }
}
