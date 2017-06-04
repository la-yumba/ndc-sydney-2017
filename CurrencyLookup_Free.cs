using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using static System.Console;
using LaYumba.Functional;
using static LaYumba.Functional.F;
using Unit = System.ValueTuple;

using Rates = System.Collections.Immutable.ImmutableDictionary<string, decimal>;
using Dsl;
using static Dsl.Factory;

namespace Dsl
{
   class Ask {}

   class Tell
   {
      public string Message { get; }
      public Tell(string message) => Message = message;
   }

   class GetRate
   {
      public string CcyPair { get; }
      public GetRate(string ccyPair) => CcyPair = ccyPair;
   }

   static class Factory
   {
      public static Free<string> Ask()
         => Free.Of<string>(new Ask());

      public static Free<Unit> Tell(string message)
         => Free.Of<Unit>(new Tell(message));   

      public static Free<decimal> GetRate(string ccyPair)
         => Free.Of<decimal>(new GetRate(ccyPair));   
   }
}

public class CurrencyLookup_Free
{
   public static void Main_Free() =>
      MainF.Run(cmd =>
      {
         switch (cmd)
         {
            case Ask _:
               return ReadLine().ToUpper();
            
            case Tell tell:
               WriteLine(tell.Message);
               return Unit();

            case GetRate getRate:
               return FxApi.GetRate(getRate.CcyPair);

            default: 
               throw new Exception("Invalid command");
         }
      });

   public static Free<Unit> MainF =>
      from _  in Tell("Enter a currency pair like 'EURUSD', or 'q' to quit")
      from __ in MainRec(Rates.Empty)
      select _;

   public static Free<Unit> MainRec(Rates cache)
   {
      Free<Unit> done = Free.Return(Unit());

      Free<(decimal Rate, Rates NewState)> retrieveLocally(string ccyPair) =>
         Free.Return((cache[ccyPair], cache));
       
      Free<(decimal Rate, Rates NewState)> retrieveRemotely(string ccyPair) =>
         from rate in GetRate(ccyPair)
         select (rate, cache.Add(ccyPair, rate));

      Free<Unit> notDone(string ccyPair) =>
         from t in cache.ContainsKey(ccyPair)
                   ? retrieveLocally(ccyPair)
                   : retrieveRemotely(ccyPair) 
         from _  in Tell(t.Rate.ToString())
         from __ in MainRec(t.NewState)
         select _;

      return 
         from input in Ask()
         from _ in (input == "Q")
            ? done
            : notDone(input)
         select _;
   }
}

