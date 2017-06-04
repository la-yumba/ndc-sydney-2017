using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using static System.Console;

using LaYumba.Functional;
using static LaYumba.Functional.F;

using Rates = System.Collections.Immutable.ImmutableDictionary<string, decimal>;

public static class CurrencyLookup
{
   // stateless program
   public static void Stateless()
   {
      WriteLine("Enter a currency pair like 'EURUSD', or 'q' to quit");
      for (string input; (input = ReadLine().ToUpper()) != "Q";)
         WriteLine(GetRate(input));
   }

   // stateful equivalent, no mutation, uses recursion
   public static void Stateful()
   {
      WriteLine("Enter a currency pair like 'EURUSD', or 'q' to quit");
      MainRec(Rates.Empty);
   }

   static void MainRec(Rates cache)
   {
      var input = ReadLine().ToUpper();
      if (input == "Q") return;

      var (rate, newState) = GetRate(input, cache);
      WriteLine(rate);
      MainRec(newState); // recursively calls itself with the new state
   }

   // non-recursive version, uses a loop instead
   public static void MainNonRec()
   {
      WriteLine("Enter a currency pair like 'EURUSD', or 'q' to quit");
      var state = Rates.Empty;

      for (string input; (input = ReadLine().ToUpper()) != "Q";)
      {
         var (rate, newState) = GetRate(input, state);
         state = newState;
         WriteLine(rate);
      }
   }

   // non-recursive version, uses a fold instead
   public static void Fold()
   {
      WriteLine("Enter a currency pair like 'EURUSD', or 'q' to quit");
      Inputs().Aggregate(Rates.Empty, 
         (cache, input) =>
         {
            var (rate, newState) = GetRate(input, cache);
            WriteLine(rate);
            return newState;               
         });
   }

   static IEnumerable<string> Inputs()
   {
      for (string input; (input = ReadLine().ToUpper()) != "Q";)
         yield return input;
   }

   // gets the FX rate for the given currency pair
   public static decimal GetRate(string ccyPair)
   {
      WriteLine($"fetching rate...");
      var uri = $"http://finance.yahoo.com/d/quotes.csv?f=l1&s={ccyPair}=X";
      var request = new HttpClient().GetStringAsync(uri);
      return decimal.Parse(request.Result.Trim());
   }

   // stateful version of the above, takes a cache 
   static (decimal, Rates) GetRate(string ccyPair, Rates cache)
   {
      if (cache.ContainsKey(ccyPair))
         return (cache[ccyPair], cache);

      var rate = GetRate(ccyPair);
      return (rate, cache.Add(ccyPair, rate));
   }

   // same as above, but unit-testable: inject the function that performs the remote lookup
   static (decimal, Rates) GetRate
      (Func<string, decimal> getRate, string ccyPair, Rates cache)
   {
      if (cache.ContainsKey(ccyPair))
         return (cache[ccyPair], cache);

      var rate = getRate(ccyPair);
      return (rate, cache.Add(ccyPair, rate));
   }
}
