using AccessLayer;
using MtgApiManager.Lib.Core;
using MtgApiManager.Lib.Model;
using MtgApiManager.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildMagicCardDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = new Database();

            CardService service = new CardService();
            Exceptional<List<Card>> result = service.All();
            if(result.IsSuccess)
            {
                var value = result.Value;

                foreach(Card c in value)
                {
                    DBParameter[] parameters = new DBParameter[]
                    {
                        new DBParameter()
                        {
                            name = "@Name",
                            type = System.Data.SqlDbType.VarChar,
                            value = c.Name
                        },
                        new DBParameter()
                        {
                            name = "@RulesText",
                            type = System.Data.SqlDbType.VarChar,
                            value = c.Text
                        },
                        new DBParameter()
                        {
                            name = "@CMC",
                            type = System.Data.SqlDbType.Int,
                            value = c.Cmc
                        },
                        new DBParameter()
                        {
                            name = "@Power",
                            type = System.Data.SqlDbType.Int,
                            value = c.Power
                        },
                        new DBParameter()
                        {
                            name = "@Toughness",
                            type = System.Data.SqlDbType.Int,
                            value = c.Toughness
                        },
                        new DBParameter()
                        {
                            name = "@Type",
                            type = System.Data.SqlDbType.VarChar,
                            value = c.Type
                        }
                    };

                    db.nonquery("INSERT INTO Cards Name, RulesText, CMC, Power, Toughness, Type VALUES (@Name, @RulesText, @CMC, @Power, @Toughness)", parameters);
                }
            }
            else
            {
                var exception = result.Exception;
            }
        }
    }
}
