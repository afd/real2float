procedure triangle(a : real, b : real, c : real) returns (s : real, r : real) {

  s := (a + b + c)/2.0;
  r := s * (s - a) * (s - b) * (s - c);

}
