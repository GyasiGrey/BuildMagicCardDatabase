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
            CardService service = new CardService();
            Exceptional<List<Card>> result = service.All();
            if(result.IsSuccess)
            {
                var value = result.Value;
            }
            else
            {
                var exception = result.Exception;
            }
        }
    }
}
