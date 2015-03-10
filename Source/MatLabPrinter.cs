using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using System.Diagnostics;

namespace Real2Float
{
  class MatLabPrinter : StandardVisitor
  {

    public override Cmd VisitAssignCmd(AssignCmd node)
    {
      Debug.Assert(node.Lhss.Count() == 1);
      var Lhs = node.Lhss[0] as SimpleAssignLhs;
      Console.Write(Lhs.AssignedVariable);
      Console.Write(" = ");
      Debug.Assert(node.Rhss.Count() == 1);
      Console.Write(node.Rhss[0]);
      Console.WriteLine(";");
      return base.VisitAssignCmd(node);
    }
  }
}
