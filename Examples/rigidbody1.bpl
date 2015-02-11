procedure rigidbody1(x1 : real, x2 : real, x3 : real) returns (r : real) {
  assume (-15.0 <= x1 && x1 <= 15.0 && -15.0 <= x2 && x2 <= 15.0 && -15.0 <= x3 && x3 <= 15.0);
  f := -x1*x2 - 2.0 * x2 * x3 - x1 - x3;
}
