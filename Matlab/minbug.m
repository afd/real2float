clear all;
close all;
m = 2;
eps = sdpvar(m,1);
f = sum(eps);
[c v] = coefficients(f, eps(1:m-1));
sdisplay(v)
[c v] = coefficients(f, eps(1:m));
sdisplay(v)
