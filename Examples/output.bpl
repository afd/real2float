procedure rigidbody1(x1: real, x2: real, x3: real) returns (f: real, f$float: real);
  requires -15e0 <= x1 && x1 <= 15e0;
  requires -15e0 <= x2 && x2 <= 15e0;
  requires -15e0 <= x3 && x3 <= 15e0;
  ensures -1e-12 <= f - f$float && f - f$float <= 1e-12;



implementation rigidbody1(x1: real, x2: real, x3: real) returns (f: real, f$float: real)
{
  var x1$float: real;
  var x2$float: real;
  var x3$float: real;

    x1$float := x1;
    x2$float := x2;
    x3$float := x3;
    f, f$float := -x1 * x2 - 2e0 * x2 * x3 - x1 - x3, (((-x1 * x2 * (1e0 + _eps0) - 2e0 * x2 * (1e0 + _eps1) * x3 * (1e0 + _eps2)) * (1e0 + _eps3) - x1) * (1e0 + _eps4) - x3) * (1e0 + _eps5);
}



const _eps0: real;

axiom _eps0 * _eps0 <= delta * delta;

const _eps1: real;

axiom _eps1 * _eps1 <= delta * delta;

const _eps2: real;

axiom _eps2 * _eps2 <= delta * delta;

const _eps3: real;

axiom _eps3 * _eps3 <= delta * delta;

const _eps4: real;

axiom _eps4 * _eps4 <= delta * delta;

const _eps5: real;

axiom _eps5 * _eps5 <= delta * delta;

const delta: real;

axiom delta == 2e-53;
