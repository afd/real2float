using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Basetypes;

namespace Real2Float
{
  class Translator
  {
    static void Main(string[] args)
    {
      
      CommandLineOptions.Install(new CommandLineOptions());

      var filename = args[0];

      Program prog = ExecutionEngine.ParseBoogieProgram(
        new List<string> { filename }, false);
      if (prog == null)
        Environment.Exit(1);

      LinearTypeChecker UnusedLinearTypeChecker;
      MoverTypeChecker UnusedMoverTypeChecker;

      ExecutionEngine.ResolveAndTypecheck(prog, filename, out UnusedLinearTypeChecker, out UnusedMoverTypeChecker);

      FloatConverter Converter = new FloatConverter();

      Converter.Visit(prog);

      prog.AddTopLevelDeclarations(Converter.GetEpsilons());

      using (TokenTextWriter writer = new TokenTextWriter("output.bpl", false)) {
        prog.Emit(writer);
      }

    }
  }

  class FloatConverter : StandardVisitor {

    private List<Constant> Epsilons = new List<Constant>();

    public List<Constant> GetEpsilons() {
      return Epsilons;
    }

    public override Expr VisitNAryExpr(NAryExpr node)
    {
      var Binop = node.Fun as BinaryOperator;
      if (Binop == null) {
        return base.VisitNAryExpr(node);
      }

      switch (Binop.Op) {
        case BinaryOperator.Opcode.Add:
        case BinaryOperator.Opcode.Mul:
        case BinaryOperator.Opcode.Sub:
        case BinaryOperator.Opcode.Div:
          node.Args[0] = VisitExpr(node.Args[0]);
          node.Args[1] = VisitExpr(node.Args[1]);
          return Expr.Mul(node, Expr.Add(Expr.Literal(
            BigDec.FromString("1.0")), GetFreshEpsilon()));
      }
      return base.VisitNAryExpr(node);
    }

    private Expr GetFreshEpsilon()
    {
      Epsilons.Add(new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "_eps" + Epsilons.Count(), Microsoft.Boogie.Type.Real), false));
      return Expr.Ident(Epsilons.Last());
    }


  }

}
