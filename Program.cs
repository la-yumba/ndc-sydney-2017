using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;
using LaYumba.Functional;

using System.Reflection;

public class Program
{
   public static void Main(string[] args)
   {
      var programs = new Dictionary<string, Action>
      {
         ["stateless"] = CurrencyLookup.Main_Stateless,
         ["stateful"] = CurrencyLookup.Main_Stateful,
         ["fold"] = CurrencyLookup.Main_Fold,
      };

      if (args.Length > 0)
         programs.Lookup(args[0])
            .Match(
               None: () => WriteLine($"Unknown option: '{args[0]}'"),
               Some: (main) => main()
            );

      else WriteLine($"What class do you want to start?");
   }
}
