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


      OldBackup Backup = new OldBackup();
      Backup.Visit(prog);

      FloatConverter Converter = new FloatConverter();
      Converter.Visit(prog);

      Converter.GetResultBound(3, Backup);

      prog.AddTopLevelDeclarations(Converter.GetEpsilons());
      Converter.GetFreshPrecision();
      prog.AddTopLevelDeclarations(Converter.GetPrecisions());
      Converter.GetAxiomPrecision(3);
      Converter.GetEpsilonBounds(Converter.GetEpsilons());
      foreach (Axiom axiom in Converter.GetAxioms())
      {
        prog.AddTopLevelDeclaration(axiom);
      }

      using (TokenTextWriter writer = new TokenTextWriter("output.bpl", false)) {
        prog.Emit(writer);
      }

    }
  }
  class OldBackup : StandardVisitor {

    private Expr[] OldRhss = new Expr[1];

    public Expr[] GetOldRhss() {
      return OldRhss;
    }

    public override Cmd VisitAssignCmd(AssignCmd node)
    {
      if (node.Lhss.Count == 1) {
        Expr e = node.Rhss[0];
        OldRhss[0] = e; 
      }
      return base.VisitAssignCmd(node);
    }    
  }

  class FloatConverter : StandardVisitor {

    private List<Constant> Epsilons = new List<Constant>();

    private List<Constant> Precisions = new List<Constant>();

    public List<Axiom> Axioms = new List<Axiom>();

    public List<Constant> GetEpsilons() {
      return Epsilons;
    }
    public List<Constant> GetPrecisions() {
      return Precisions;
    }
    public List<Axiom> GetAxioms() {
      return Axioms;
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

   private int Count1 (List<Constant> l)
   {
     return l.Count() + 1;
   }

    private Expr GetFreshEpsilon ()
    {
      Epsilons.Add(new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "_eps" + Count1(Epsilons), Microsoft.Boogie.Type.Real), false));
      return Expr.Ident(Epsilons.Last());
    }
    public void GetFreshPrecision () {
      Precisions.Add(new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "delta", Microsoft.Boogie.Type.Real), false));
    }
    public void GetAxiomPrecision (int precision) {
      Expr prec = Expr.Literal(BigDec.FromString("2e-"+precision));
      Expr axiomExpr = Expr.Eq (Expr.Ident(Precisions.Last()),prec);
      Axioms.Add(new Axiom(Token.NoToken, axiomExpr));
    }

    public void GetEpsilonBounds(List<Constant> terms)
    {
        Expr axiomExpr = BinaryTreeAnd(terms, 0, terms.Count - 1);
        Axioms.Add(new Axiom(Token.NoToken, axiomExpr));
    }
    
    public void GetResultBound(int precision, OldBackup Backup) {
        Expr rhs = Expr.Literal(BigDec.FromString("2e-"+precision));
        Console.WriteLine("{0}", Backup.GetOldRhss().Last());
        Expr lhs = (Backup.GetOldRhss())[0];
        Expr axiomExpr = Expr.Ge(lhs,rhs);
        Axioms.Add(new Axiom(Token.NoToken, axiomExpr));
    }

    private Expr BinaryTreeAnd(List<Constant> terms, int start, int end)
    {
      Expr eps = Expr.Ident(terms[start]);
      Expr delta = Expr.Ident(Precisions.Last());
      Expr ineq = Expr.Ge(Expr.Mul(delta,delta),Expr.Mul(eps,eps));
      Expr eps1 = Expr.Ident(terms[end]);
      Expr ineq1 = Expr.Ge(Expr.Mul(delta,delta),Expr.Mul(eps1,eps1));

        if (start > end)
            return Expr.True;
        if (start == end)
            return ineq;
        if (start + 1 == end) {
            return Expr.And(ineq, ineq1);
        }
        var mid = (start + end) / 2;
        return Expr.And(BinaryTreeAnd(terms, start, mid), BinaryTreeAnd(terms, mid + 1, end));
    }

    

  }

}
