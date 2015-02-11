
procedure sqroot(x : real) returns (r : real) {

  r := 1.0 + 0.5*x - 0.125*x*x + 0.0625*x*x*x - 0.0390625*x*x*x*x;

}
