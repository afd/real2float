using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;

namespace Real2Float
{
  class Real2FloatCommandLineOptions : CommandLineOptions
  {

    public int Bits = 0;
    public string ResultPrecision = null;

    protected override bool ParseOption(string name, CommandLineOptionEngine.CommandLineParseState ps) {
      if (name == "bits") {
        ps.GetNumericArgument(ref Bits);
        return true;
      }
      if (name == "resultPrecision") {
        if (ps.ConfirmArgumentCount(1)) {
          ResultPrecision = ps.args[ps.i];
        }
        return true;
      }
      
      return base.ParseOption(name, ps);
    }

  }
}
