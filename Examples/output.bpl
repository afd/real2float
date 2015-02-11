procedure mytriangle(x1: real, x2: real, x3: real) returns (r: real, r$float: real);
  ensures -1e-12 <= r - r$float && r - r$float <= 1e-12;



implementation mytriangle(x1: real, x2: real, x3: real) returns (r: real, r$float: real)
{
  var x1$float: real;
  var x2$float: real;
  var x3$float: real;

    x1$float := x1;
    x2$float := x2;
    x3$float := x3;
    r, r$float := (x1 + x2 + x3) * x1 * x2 * x3, ((x1 + x2) * (1e0 + eps(1)) + x3) * (1e0 + eps(2)) * x1 * (1e0 + eps(3)) * x2 * (1e0 + eps(4)) * x3 * (1e0 + eps(5));
}



const eps(1): real;

axiom eps(1) * eps(1) <= delta * delta;

const eps(2): real;

axiom eps(2) * eps(2) <= delta * delta;

const eps(3): real;

axiom eps(3) * eps(3) <= delta * delta;

const eps(4): real;

axiom eps(4) * eps(4) <= delta * delta;

const eps(5): real;

axiom eps(5) * eps(5) <= delta * delta;

const delta: real;

axiom delta == 2e-53;
