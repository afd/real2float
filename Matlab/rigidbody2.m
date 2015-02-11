clear all;
close all;

p = 53;
n = 3; m = 14;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x1 = 15 * xscale(1); x2 = 15 * xscale(2); x3 = 15 * xscale(3);
f = 2e0 * x1 * x2 * x3 + 3e0 * x3 * x3 - x2 * x1 * x2 * x3 + 3e0 * x3 * x3 - x2;
fhat = ((((2e0 * x1 * (1e0 + eps(1)) * x2 * (1e0 + eps(2)) * x3 * (1e0 + eps(3)) + 3e0 * x3 * (1e0 + eps(4)) * x3 * (1e0 + eps(5))) * (1e0 + eps(6)) - x2 * x1 * (1e0 + eps(7)) * x2 * (1e0 + eps(8)) * x3 * (1e0 + eps(9))) * (1e0 + eps(10)) + 3e0 * x3 * (1e0 + eps(11)) * x3 * (1e0 + eps(12))) * (1e0 + eps(13)) - x2) * (1e0 + eps(14));

r = f - fhat;
S = [1 - xscale.^2 >= 0];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2, 1e-7);
