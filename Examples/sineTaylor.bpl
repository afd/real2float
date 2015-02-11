
procedure sineTaylor(x : real) returns (r : real) {

  r := x - (x*x*x)/6.0 + (x*x*x*x*x)/120.0 - (x*x*x*x*x*x*x)/5040.0 ;

}
