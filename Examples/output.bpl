procedure rigidbody1(x1: real, x2: real, x3: real) returns (f: real, f_float: real);
  requires -15e0 <= x1 && x1 <= 15e0;
  requires -15e0 <= x2 && x2 <= 15e0;
  requires -15e0 <= x3 && x3 <= 15e0;
  ensures f > 225e0;
  ensures -1e-12 <= f - f_float && f - f_float <= 1e-12;



implementation rigidbody1(x1: real, x2: real, x3: real) returns (f: real, f_float: real)
{
  var x1_float: real;
  var x2_float: real;
  var x3_float: real;

    x1_float := x1;
    x2_float := x2;
    x3_float := x3;
    f := -x1 * x2 - 2e0 * x2 * x3 - x1 - x3;

    f_float := (((-x1_float * (1e0 + eps0) * x2_float * (1e0 + eps1) - 2e0 * x2_float * (1e0 + eps2) * x3_float * (1e0 + eps3)) * (1e0 + eps4) - x1_float) * (1e0 + eps5) - x3_float) * (1e0 + eps6);
}



const eps0: real;

axiom eps0 * eps0 <= delta * delta;

const eps1: real;

axiom eps1 * eps1 <= delta * delta;

const eps2: real;

axiom eps2 * eps2 <= delta * delta;

const eps3: real;

axiom eps3 * eps3 <= delta * delta;

const eps4: real;

axiom eps4 * eps4 <= delta * delta;

const eps5: real;

axiom eps5 * eps5 <= delta * delta;

const eps6: real;

axiom eps6 * eps6 <= delta * delta;

const delta: real;

axiom delta == 2e-53;
