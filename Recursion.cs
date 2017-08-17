using System;
using static System.Console;
using LaYumba.Functional;
using static LaYumba.Functional.F;

using static Dsl.StdOut.Factory;
using Unit = System.ValueTuple;

public static partial class Program
{
   const int BIG_NUMBER = 1000000;

   // recursive call blows the stack

   static void Recur() => Recur(BIG_NUMBER);

   static void Recur(int n)
   {
      if (n == 0) return;
      Write(n);
      Write("-");
      Recur(n - 1);
   }

   // with free monads it's stack-safe

   static void RecurFree() 
      => RecurFree(BIG_NUMBER).Run(FreeProgram.Interpreter);

   static Free<Unit> RecurFree(int n) =>
      (n == 0) 
         ? Free.Return(Unit())
         : from _ in Tell(n.ToString())
           from __ in RecurFree(n - 1)
           select _;
}
