clear all;
close all;

p = 11;
n = 1; m = 14;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x = 0.5 * xscale + 0.5;
f = 1e0 + 5e-1 * x - 125e-3 * x * x + 625e-4 * x * x * x - 390625e-7 * x * x * x * x;
fhat = ((((1e0 + 5e-1 * x * (1e0 + eps(1))) * (1e0 + eps(2)) - 125e-3 * x * (1e0 + eps(3)) * x * (1e0 + eps(4))) * (1e0 + eps(5)) + 625e-4 * x * (1e0 + eps(6)) * x * (1e0 + eps(7)) * x * (1e0 + eps(8))) * (1e0 + eps(9)) - 390625e-7 * x * (1e0 + eps(10)) * x * (1e0 + eps(11)) * x * (1e0 + eps(12)) * x * (1e0 + eps(13))) * (1e0 + eps(14));
r = f - fhat;
S = [0.001 - xscale(1)^2 >= 0];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2, 1e-7);
