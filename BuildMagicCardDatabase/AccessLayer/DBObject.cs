using System;
using System.Collections.Generic;

namespace AccessLayer
{
    public abstract class DBObject
    {
        public List<List<DBParameter>> rawObjects;
        public List<Object> genericObjects;
        private Database db = null;

        protected void getData(String SQL)
        {
            if (db == null)
            {
                db = new Database();
            }

            rawObjects = db.query(SQL);
        }

        protected void getData(String SQL, DBParameter[] parameters)
        {
            if (db == null)
            {
                db = new Database();
            }

            rawObjects = db.query(SQL, parameters);
        }
    }
}