clear all;
close all;

p = 53;
d = 1e-1;
n = 3; m = 5;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x1 = 12 * xscale(1) + 5; x2 = 12 * xscale(2) + 5; x3 = 12 * xscale(3) + 5; 
f = (x1 + x2 + x3) * x1 * x2 * x3;
fhat = ((x1 + x2) * (1e0 + eps(1)) + x3) * (1e0 + eps(2)) * x1 * (1e0 + eps(3)) * x2 * (1e0 + eps(4)) * x3 * (1e0 + eps(5));
r = f - fhat;
S = [1 - xscale.^2 >= 0; x1 >= d; x2 >= d; x3 >= d];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2, 1e-7);
