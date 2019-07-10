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

            int PageNum = 1;
            CardService service = new CardService();
            Console.WriteLine("Getting Page " + PageNum);
            Exceptional<List<Card>> result = service.Where(X => X.Page, PageNum).All();

            while (result.IsSuccess)
            {
                var value = result.Value;

                foreach(Card c in value)
                {
                    //If the MultiverseId is NULL that means the card is not on Gatherer.
                    //This only happens with a small number of reprint sets so it won't matter for this project
                    if (c.MultiverseId != null)
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
                                type = System.Data.SqlDbType.VarChar,
                                value = c.Power
                            },
                            new DBParameter()
                            {
                                name = "@Toughness",
                                type = System.Data.SqlDbType.VarChar,
                                value = c.Toughness
                            },
                            new DBParameter()
                            {
                                name = "@Type",
                                type = System.Data.SqlDbType.VarChar,
                                value = c.Type
                            },
                            new DBParameter()
                            {
                                name = "@MultiverseId",
                                type = System.Data.SqlDbType.Int,
                                value = c.MultiverseId
                            }
                        };

                        Console.WriteLine("Inserting " + c.Name);
                        db.nonquery("INSERT INTO Cards (Name, RulesText, CMC, Power, Toughness, Type, MultiverseId) VALUES (@Name, @RulesText, @CMC, @Power, @Toughness, @Type, @MultiverseId)", parameters);
                    }
                }

                //Increment the Page
                PageNum++;
                Console.WriteLine("Getting Page " + PageNum);
                //Get the next page
                result = service.Where(X => X.Page, PageNum).All();
            }
        }
    }
}
