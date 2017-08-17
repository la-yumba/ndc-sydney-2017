using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;
using LaYumba.Functional;

using System.Threading.Tasks;

using System.Reflection;

using static Dsl.StdOut.Factory;
using Unit = System.ValueTuple;

public static partial class Program
{
   public static void Main(string[] args)
   {
      var programs = new Dictionary<string, Action>
      {
         ["stateless"] = CurrencyLookup.Stateless,
         ["stateful"] = CurrencyLookup.Stateful,
         ["fold"] = CurrencyLookup.Fold,
         ["free"] = FreeProgram.Run,
         ["recur"] = Recur,
         ["recurFree"] = RecurFree,
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
