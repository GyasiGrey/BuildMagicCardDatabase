using AccessLayer;
using HtmlAgilityPack;
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

            db.nonquery("TRUNCATE TABLE Cards");

            int PageNum = 1;
            CardService service = new CardService();
            Console.WriteLine("Getting Page " + PageNum);
            Exceptional<List<Card>> result = service.Where(X => X.Page, PageNum).All();

            while (result.IsSuccess)
            {
                var value = result.Value;

                if(PageNum > result.PagingInfo.TotalPages)
                {
                    break;
                }

                foreach(Card c in value)
                {
                    //If the MultiverseId is NULL that means the card is not on Gatherer.
                    //This only happens with a small number of reprint sets so it won't matter for this project
                    if (c.MultiverseId != null)
                    {
                        float? rating = null;

                        //Scrape the rating off Gatherer
                        string GathererURL = String.Format("https://gatherer.wizards.com/pages/card/Details.aspx?multiverseid={0}", c.MultiverseId);
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(GathererURL);

                        HtmlNodeCollection ratingCollection = doc.DocumentNode.SelectNodes("//span[@class='textRatingValue']");
                        HtmlNodeCollection ratingNumVotes = doc.DocumentNode.SelectNodes("//span[@class='totalVotesValue']");

                        //If there are 0 votes, set rating to NULL
                        if (ratingNumVotes.Count > 0)
                        {
                            int numVotesTmp;
                            if (int.TryParse(ratingNumVotes[0].InnerText, out numVotesTmp))
                            {
                                if (numVotesTmp > 0)
                                {
                                    if (ratingCollection.Count > 0)
                                    {
                                        float ratingTmp;
                                        if (float.TryParse(ratingCollection[0].InnerText, out ratingTmp))
                                        {
                                            rating = ratingTmp;
                                        }
                                    }
                                }
                            }
                        }

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
                                value = c.Text.Replace("\n", " ")
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
                            },
                            new DBParameter()
                            {
                                name = "@Rating",
                                type = System.Data.SqlDbType.Decimal,
                                value = rating
                            }
                        };

                        Console.WriteLine("Inserting " + c.Name);
                        db.nonquery("INSERT INTO Cards (Name, RulesText, CMC, Power, Toughness, Type, MultiverseId, Rating) VALUES (@Name, @RulesText, @CMC, @Power, @Toughness, @Type, @MultiverseId, @Rating)", parameters);
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
