procedure rigidbody1(x1 : real, x2 : real, x3 : real, x4:real) 
 returns (f : real)
requires -15e0 <= x1 && x1 <= 15e0;
requires -15e0 <= x2 && x2 <= 15e0;
requires -15e0 <= x3 && x3 <= 15e0;
ensures f > 225.0;
 {
  f := x1* x4* ( - x1 +  x2 +  x3  - x4 ) + x2* (x1  -  x2 +  x3 +  x4) +  x3* (x1 +  x2  -  x3 +  x4 ) - x2* x3* x4  -  x1* x3-  x1* x2  - x4;
}

