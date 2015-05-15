
procedure turbine (r : real, v : real, w : real) returns (res : real) {

  res := 3.0 + 2.0/(r*r) - 0.125*(3.0-2.0*v)*(w*w*r*r)/(1.0-v) - 4.5;
}
