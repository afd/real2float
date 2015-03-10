using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Basetypes;

namespace Real2Float
{
  class FloatConverter : Duplicator {

    private Program Prog;
    private Implementation Impl;
    private int EpsilonCounter = 0;
    private int Bits;
    private BigDec ResultPrecision;

    public FloatConverter(Program Prog, int Bits, BigDec UserPrecision) {
      this.Prog = Prog;
      // For now, assume that the input has a single implementation
      Debug.Assert(Prog.Implementations.Count() == 1);
      this.Impl = Prog.Implementations.ToList()[0];
      this.Bits = Bits;
      this.ResultPrecision = UserPrecision;
    }

    public void ApplyTransformation()
    {
      DuplicateLocalVariablesAndParameters();

      Impl.StructuredStmts = DuplicateStmtList(Impl.StructuredStmts);

      InitialiseFloatParameterVariables(Impl.StructuredStmts.BigBlocks[0]);

      SpecifyResultBound();

      AddPrecision();

    }

    private StmtList DuplicateStmtList(StmtList SL)
    {
        IList<BigBlock> NewBigBlocks = new List<BigBlock>();
        foreach (var BB in SL.BigBlocks)
        {
            NewBigBlocks.Add(BB);
        }
        foreach(var NewBB in SL.BigBlocks.Select
            (Item => ConvertBigBlock(Item))) {
            NewBigBlocks.Add(NewBB);
        }
        return new StmtList(NewBigBlocks, SL.EndCurly);
    }

    private BigBlock ConvertBigBlock(BigBlock bb)
    {
        List<Cmd> NewCmds = new List<Cmd>();
        foreach (Cmd c in bb.simpleCmds)
        {
            var Assign = c as AssignCmd;
            Debug.Assert(Assign != null);

            Debug.Assert(Assign.Lhss.Count() == 1);
            Debug.Assert(Assign.Rhss.Count() == 1);

            var Lhs = (SimpleAssignLhs)Assign.Lhss[0];
            var Rhs = Assign.Rhss[0];

            var FloatLhs = new SimpleAssignLhs(Token.NoToken,
              Expr.Ident(ConvertToFloatName(Lhs.AssignedVariable.Name), Lhs.AssignedVariable.Type));
            var FloatRhs = VisitExpr(Rhs);

            NewCmds.Add(new AssignCmd(Token.NoToken,
              new List<AssignLhs> { FloatLhs }, new List<Expr> { FloatRhs }));
        }

        StructuredCmd NewEC = null;
        if (bb.ec is IfCmd)
        {
            IfCmd IfCmd = bb.ec as IfCmd;
            Debug.Assert(IfCmd.elseIf == null);
            NewEC = new IfCmd(Token.NoToken, VisitExpr(IfCmd.Guard),
                ConvertStmtList(IfCmd.thn), null, ConvertStmtList(IfCmd.elseBlock));
        }
        else
        {
            Debug.Assert(bb.ec == null);
        }

        Debug.Assert(bb.tc == null);
        return new BigBlock(Token.NoToken, null, NewCmds, NewEC, null);
    }

    private StmtList ConvertStmtList(StmtList SL)
    {
        if (SL == null)
        {
            return null;
        }
        IList<BigBlock> NewBigBlocks = new List<BigBlock>();
        foreach (var NewBB in SL.BigBlocks.Select
            (Item => ConvertBigBlock(Item)))
        {
            NewBigBlocks.Add(NewBB);
        }
        return new StmtList(NewBigBlocks, SL.EndCurly);
    }

    private void DuplicateLocalVariablesAndParameters()
    {

      // Make a _float local variable to duplicate each local variable
      // and input parameter.
      foreach(var v in Impl.LocVars.ToList().Union(Impl.InParams.ToList())) {
        Impl.LocVars.Add(
          new LocalVariable(Token.NoToken, new TypedIdent(Token.NoToken,
            ConvertToFloatName(v.Name), v.TypedIdent.Type)));
      }

      // Also duplicate each output parameter
      // to have a _float counterpart
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
      // p_float := p
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
      return name + "_float";
    }

    private static bool IsFloatName(string name) {
      return name.EndsWith("_float");
    }

    private static string ConvertFromFloatName(string name)
    {
      Debug.Assert(IsFloatName(name));
      return name.Substring(0, name.Count() - "_float".Count());
    }

    public override Expr VisitIdentifierExpr(IdentifierExpr node)
    {
      return Expr.Ident(ConvertToFloatName(node.Decl.Name), node.Decl.TypedIdent.Type);
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

    private Expr GetFreshEpsilon ()
    {
      var Epsilon = new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "eps" + EpsilonCounter, Microsoft.Boogie.Type.Real), false);
      Prog.AddTopLevelDeclaration(Epsilon);

      Prog.AddTopLevelDeclaration(new Axiom(Token.NoToken,
        Expr.Le(Expr.Mul(Expr.Ident(Epsilon), Expr.Ident(Epsilon)),
          Expr.Mul(Expr.Ident("delta", Microsoft.Boogie.Type.Real),
                   Expr.Ident("delta", Microsoft.Boogie.Type.Real)))));
      EpsilonCounter++;
      return Expr.Ident(Epsilon);
    }

    public void AddPrecision () {
      var Delta = new Constant(Token.NoToken, new TypedIdent(Token.NoToken,
          "delta", Microsoft.Boogie.Type.Real), false);
      Prog.AddTopLevelDeclaration(Delta);
      Expr prec = Expr.Literal(BigDec.FromString("2e-" + Bits));
      Prog.AddTopLevelDeclaration(new Axiom(Token.NoToken, Expr.Eq (Expr.Ident(Delta), prec)));
    }
    
    public void SpecifyResultBound() {
      Expr AllowedDifference = Expr.Literal(ResultPrecision);
      foreach(var r in Impl.OutParams) {
        if(!IsFloatName(r.Name)) {
          var rFloat = Expr.Ident(ConvertToFloatName(r.Name), r.TypedIdent.Type);
          Expr Difference = Expr.Sub(Expr.Ident(r), rFloat);
          Impl.Proc.Ensures.Add(new Ensures(false,
            Expr.And(Expr.Le(Expr.Neg(AllowedDifference), Difference),
                     Expr.Le(Difference, AllowedDifference))));
        }
      }
    }

    internal void DumpMatlab()
    {
      Debug.Assert(Prog.Implementations.Count() == 1);
      Implementation Impl = Prog.Implementations.ToList()[0];

      Console.WriteLine("clear all;");
      Console.WriteLine("close all;");
      Console.WriteLine("");
      Console.WriteLine("delta = 2^(-" + Bits + ");");

      Console.Write("sdpvar");
      foreach (var InParam in Impl.InParams)
      {
        Console.Write(" " + InParam.Name + "_scale");
      }
      Console.WriteLine(";");

      Console.Write("sdpvar");
      foreach (var Epsilon in Prog.TopLevelDeclarations.OfType<Constant>().Where(Item => Item.Name.StartsWith("eps")))
      {
        Console.Write(" " + Epsilon.Name);
      }
      Console.WriteLine(";");

      // Output code to relate x to x_scale for each parameter
      foreach (Requires r in Impl.Proc.Requires)
      {
        Variable Var;
        Expr Lower;
        Expr Upper;
        GetVariableAndBounds(r.Condition, out Var, out Lower, out Upper);
        Console.Out.WriteLine(Var.Name + " = ((" + Upper + " - " + Lower + ") / 2)) * " + Var.Name + "_scale" + " + (" + Upper + " + " + Lower + ")/2;");
      }

      // Output MatLab code for the program statements
      foreach (var bb in Impl.StructuredStmts.BigBlocks)
      {
        foreach (var c in bb.simpleCmds)
        {
          MatLabPrinter MLP = new MatLabPrinter();
          MLP.Visit(c);
        }
      }

      // Output the optimization problem in terms of result variables
      IEnumerable<string> NonFloatOutputs = Impl.OutParams.Where(Item => !(Item.Name.EndsWith("_float"))).Select(Item => Item.Name);
      Debug.Assert(Impl.OutParams.Count() == 2*NonFloatOutputs.Count());
      foreach (var result in NonFloatOutputs) {
        Console.Write("[ ");
        Console.Write("lower_real_" + result + ", ");
        Console.Write("upper_real_" + result + ", ");
        Console.Write("lower_error_" + result + ", ");
        Console.Write("upper_error_" + result + " ");
        Console.Write("] = boundfperror (delta, [ ");
        bool first = true;
        foreach(var scaled in Impl.InParams.Select(Item => Item.Name + "_scale")) {
          if(!first) {
            Console.Write("; ");
          }
          first = false;
          Console.Write(scaled);
        }
        Console.Write("], [ ");
        first = true;
        foreach (var Epsilon in Prog.TopLevelDeclarations.OfType<Constant>().
          Where(Item => Item.Name.StartsWith("eps"))) {
          if(!first) {
            Console.Write("; ");
          }
          first = false;
          Console.Write(Epsilon.Name);
        }
        Console.WriteLine(" ], " + result + ", " + result + "_float);");
      }

    }

    private void GetVariableAndBounds(Expr Condition, out Variable Var, out Expr Lower, out Expr Upper)
    {
      NAryExpr Conjunction = Condition as NAryExpr;
      Debug.Assert(Conjunction != null);
      Debug.Assert(Conjunction.Args.Count() == 2);
      Debug.Assert(Conjunction.Fun is BinaryOperator);
      Debug.Assert((Conjunction.Fun as BinaryOperator).Op == BinaryOperator.Opcode.And);

      {
        NAryExpr LHS = Conjunction.Args[0] as NAryExpr;
        Debug.Assert(LHS != null);
        Debug.Assert(LHS.Args.Count() == 2);
        Debug.Assert(LHS.Fun is BinaryOperator);
        Debug.Assert((LHS.Fun as BinaryOperator).Op == BinaryOperator.Opcode.Le);
        IdentifierExpr Identifier = LHS.Args[1] as IdentifierExpr;
        Debug.Assert(Identifier != null);
        Var = Identifier.Decl;
        Lower = LHS.Args[0];
      }

      {
        NAryExpr RHS = Conjunction.Args[1] as NAryExpr;
        Debug.Assert(RHS != null);
        Debug.Assert(RHS.Args.Count() == 2);
        Debug.Assert(RHS.Fun is BinaryOperator);
        Debug.Assert((RHS.Fun as BinaryOperator).Op == BinaryOperator.Opcode.Le);
        IdentifierExpr OtherIdentifier = RHS.Args[0] as IdentifierExpr;
        Debug.Assert(Var == OtherIdentifier.Decl);
        Upper = RHS.Args[1];
      }
    }


  }
}
