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

      var Converter = new FloatConverter(prog);
      Converter.ApplyTransformation();

      using (TokenTextWriter writer = new TokenTextWriter("output.bpl", false)) {
        prog.Emit(writer);
      }

    }


  }

  class FloatConverter : Duplicator {

    private Program Prog;
    private Implementation Impl;
    private int EpsilonCounter = 0;

    private List<Constant> Precisions = new List<Constant>();

    public List<Axiom> Axioms = new List<Axiom>();

    public List<Constant> GetPrecisions() {
      return Precisions;
    }
    public List<Axiom> GetAxioms() {
      return Axioms;
    }

    public FloatConverter(Program Prog) {
      this.Prog = Prog;
      // For now, assume that the input has a single implementation
      Debug.Assert(Prog.Implementations.Count() == 1);
      this.Impl = Prog.Implementations.ToList()[0];
    }

    public void ApplyTransformation()
    {
      DuplicateLocalVariablesAndParameters();

      // For now assume that there is a single block of statements,
      // with no stuctured statements (i.e., no if or while)
      Debug.Assert(Impl.StructuredStmts.BigBlocks.Count() == 1);
      var BigBlock = Impl.StructuredStmts.BigBlocks.ToList()[0];
      // This asserts that there is no if or while
      Debug.Assert(BigBlock.ec == null);

      DuplicateAssignments(BigBlock);

      InitialiseFloatParameterVariables(BigBlock);

      SpecifyResultBound(3);

      AddPrecision(3);



      /*prog.AddTopLevelDeclarations(Converter.GetPrecisions());
      Converter.GetAxiomPrecision(3);
      //Converter.GetEpsilonBounds(Converter.GetEpsilons());
      foreach (Axiom axiom in Converter.GetAxioms())
      {
        prog.AddTopLevelDeclaration(axiom);
      }*/

    }

    private void DuplicateAssignments(BigBlock bb)
    {
      List<Cmd> NewCmds = new List<Cmd>();
      foreach(Cmd c in bb.simpleCmds) {
        var Assign = c as AssignCmd;
        Debug.Assert(Assign != null);

        Debug.Assert(Assign.Lhss.Count() == 1);
        Debug.Assert(Assign.Rhss.Count() == 1);

        var Lhs = (SimpleAssignLhs)Assign.Lhss[0];
        var Rhs = Assign.Rhss[0];

        var FloatLhs = new SimpleAssignLhs(Token.NoToken,
          Expr.Ident(ConvertToFloatName(Lhs.AssignedVariable.Name), Lhs.AssignedVariable.Type));
        var FloatRhs = (Expr)Rhs.Clone();
        FloatRhs = VisitExpr(FloatRhs);

        NewCmds.Add(new AssignCmd(Token.NoToken,
          new List<AssignLhs> { Lhs, FloatLhs }, new List<Expr> { Rhs, FloatRhs }));
      }
      bb.simpleCmds = NewCmds;
    }

    private void DuplicateLocalVariablesAndParameters()
    {

      // Make a $float local variable to duplicate each local variable
      // and input parameter.
      foreach(var v in Impl.LocVars.ToList().Union(Impl.InParams.ToList())) {
        Impl.LocVars.Add(
          new LocalVariable(Token.NoToken, new TypedIdent(Token.NoToken,
            ConvertToFloatName(v.Name), v.TypedIdent.Type)));
      }

      // Also duplicate each output parameter
      // to have a $float counterpart
      foreach(var v in Impl.OutParams.ToList()) {
        Variable NewOutParam = new LocalVariable(Token.NoToken, new TypedIdent(Token.NoToken,
            ConvertToFloatName(v.Name), v.TypedIdent.Type));
        Impl.OutParams.Add(NewOutParam);
        Impl.Proc.OutParams.Add(NewOutParam);
      }

    }

    private void InitialiseFloatParameterVariables(BigBlock BigBlock)
    {
      // If p is a parameter, we want initially to set
      // p$float := p
      var ReversedParams = Impl.InParams.ToList();
      ReversedParams.Reverse();
      foreach(var p in ReversedParams) {
        BigBlock.simpleCmds.Insert(0, new AssignCmd(Token.NoToken,
          new List<AssignLhs> {
            new SimpleAssignLhs(Token.NoToken, Expr.Ident(
              ConvertToFloatName(p.Name), p.TypedIdent.Type))
          }, 
          new List<Expr> { Expr.Ident(p) }));
      }
    }

    private static string ConvertToFloatName(string name)
    {
      return name + "$float";
    }

    private static bool IsFloatName(string name) {
      return name.EndsWith("$float");
    }

    private static string ConvertFromFloatName(string name)
    {
      Debug.Assert(IsFloatName(name));
      return name.Substring(0, name.Count() - "$float".Count());
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
          NAryExpr TransformedExpr =
            new NAryExpr(Token.NoToken, new BinaryOperator(Token.NoToken, Binop.Op),
              new List<Expr> { VisitExpr(node.Args[0]), VisitExpr(node.Args[1]) });
          return Expr.Mul(TransformedExpr, Expr.Add(Expr.Literal(
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
      var Epsilon = new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "_eps" + EpsilonCounter, Microsoft.Boogie.Type.Real), false);
      Prog.AddTopLevelDeclaration(Epsilon);

      Prog.AddTopLevelDeclaration(new Axiom(Token.NoToken,
        Expr.Le(Expr.Mul(Expr.Ident(Epsilon), Expr.Ident(Epsilon)),
          Expr.Mul(Expr.Ident("delta", Microsoft.Boogie.Type.Real),
                   Expr.Ident("delta", Microsoft.Boogie.Type.Real)))));
      EpsilonCounter++;
      return Expr.Ident(Epsilon);
    }

    public void AddPrecision (int bits) {
      var Delta = new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "delta", Microsoft.Boogie.Type.Real), false);
      Prog.AddTopLevelDeclaration(Delta);
      Expr prec = Expr.Literal(BigDec.FromString("2e-"+bits));
      Prog.AddTopLevelDeclaration(new Axiom(Token.NoToken, Expr.Eq (Expr.Ident(Delta), prec)));
    }

    public void GetEpsilonBounds(List<Constant> terms)
    {
        Expr axiomExpr = BinaryTreeAnd(terms, 0, terms.Count - 1);
        Axioms.Add(new Axiom(Token.NoToken, axiomExpr));
    }
    
    public void SpecifyResultBound(int precision) {
      // Expr rhs = Expr.Literal(BigDec.FromString("2e-"+precision));
      foreach(var r in Impl.OutParams) {
        if(!IsFloatName(r.Name)) {
          Impl.Proc.Ensures.Add(new Ensures(false,
            Expr.Ge(Expr.Ident(r), Expr.Ident(ConvertToFloatName(r.Name), r.TypedIdent.Type))));
        }
      }
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
