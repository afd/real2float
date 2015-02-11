clear all;
close all;

p = 53;
n = 3; m = 6;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x1 = 15 * xscale(1); x2 = 15 * xscale(2); x3 = 15 * xscale(3);
f = -x1*x2 - 2 * x2 * x3 - x1 - x3;
fhat = (((-x1 * x2 * (1e0 + eps(1)) - 2 * x2 * (1e0 + eps(2)) * x3 * (1e0 + eps(4))) * (1e0 + eps(5)) - x1) * (1e0 + eps(3)) - x3) * (1e0 + eps(6));
r = f - fhat;
S = [1 - xscale(1)^2 >= 0; 1 - xscale(2)^2 >= 0; 1 - xscale(3)^2 >= 0];
B = [1 - eps(1)^2 >= 0; 1 - eps(2)^2 >= 0; 1 - eps(3)^2 >= 0; 1 - eps(4)^2 >= 0; 1 - eps(5)^2 >= 0; 1 - eps(6)^2 >= 0];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2, 1e-9);
