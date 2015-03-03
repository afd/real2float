using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Boogie;
using Microsoft.Basetypes;

namespace Real2Float
{
  class Driver
  {
    static void Main(string[] args)
    {
      Real2FloatCommandLineOptions Clo = new Real2FloatCommandLineOptions();
      CommandLineOptions.Install(Clo);
      if(!Clo.Parse(args)) {
        Environment.Exit(1);
      }

      if(Clo.Bits <= 0) {
        Console.WriteLine("You must specify a non-negative bit-width n via /bits:n");
        Environment.Exit(1);
      }

      if(Clo.ResultPrecision == null) {
        Console.WriteLine("You must specify a result precision d via /resultPrecision:d");
        Environment.Exit(1);
      }

      BigDec ResultPrecision;
      try {
        ResultPrecision = BigDec.FromString(Clo.ResultPrecision);
      } catch (System.FormatException) {
        Console.WriteLine("Invalid format given for /resultPrecision");
        Environment.Exit(1);
        return;
      }

      Debug.Assert(Clo.Files.Count() == 1);
      var filename = Clo.Files[0];

      Program prog = ExecutionEngine.ParseBoogieProgram(
        new List<string> { filename }, false);
      if (prog == null)
        Environment.Exit(1);

      LinearTypeChecker UnusedLinearTypeChecker;
      MoverTypeChecker UnusedMoverTypeChecker;
      ExecutionEngine.ResolveAndTypecheck(prog, filename, out UnusedLinearTypeChecker, out UnusedMoverTypeChecker);

      var Converter = new FloatConverter(prog, Clo.Bits, ResultPrecision);
      Converter.ApplyTransformation();

      // At this point, prog contains the float encoding

      using (TokenTextWriter writer = new TokenTextWriter("output.bpl", false)) {
        prog.Emit(writer);
      }

      // Next step: generate Matlab
      Converter.DumpMatlab();



    }

  }

}
