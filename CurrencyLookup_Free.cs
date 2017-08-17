using System;
using System.Linq;
using System.Net.Http;
using static System.Console;

using LaYumba.Functional;
using static LaYumba.Functional.F;
using Unit = System.ValueTuple;

using Rates = System.Collections.Immutable.ImmutableDictionary<string, decimal>;
using static Dsl.StdOut.Factory;
using static Dsl.Http.Factory;

namespace Dsl
{
   namespace StdOut
   {
      class Ask {}

      class Tell
      {
         public string Message { get; }
         public Tell(string message) => Message = message;
      }
      
      static class Factory
      {
         public static Free<string> Ask()
            => Free.Of<string>(new Ask());

         public static Free<Unit> Tell(string message)
            => Free.Of<Unit>(new Tell(message));   
      }
   }

   namespace Http
   {      
      class Get
      {
         public string Url { get; }
         public Get(string url) => Url = url;
      }

      static class Factory
      {
         public static Free<string> Get(string url)
            => Free.Of<string>(new Get(url));
      }
   }
}

public static class FreeProgram
{
   public static void Run() =>
      MainF.Run(Interpreter);

   public static Func<object, object> Interpreter => cmd =>
   {
      switch (cmd)
      {
         case Dsl.StdOut.Ask _:
            return ReadLine().ToUpper();
         
         case Dsl.StdOut.Tell tell:
            WriteLine(tell.Message);
            return Unit();

         case Dsl.Http.Get @get:
            return new HttpClient().GetStringAsync(@get.Url).Result.Trim();

         default: 
            throw new Exception("Invalid command");
      }
   };

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
         from _ in Tell("fetching rate...")
         from s in Get($"http://finance.yahoo.com/d/quotes.csv?f=l1&s={ccyPair}=X")
         let rate = decimal.Parse(s)
         select (rate, cache.Add(ccyPair, rate));

      Free<Unit> process(string ccyPair) =>
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
            : process(input)
         select _;
   }
}
