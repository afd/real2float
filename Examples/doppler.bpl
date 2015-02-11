

procedure doppler(t1 : real, u : real, v : real, T : real) returns (r : real) {

  r := -t1*v/((t1 + u)*(t1 + u));

}
