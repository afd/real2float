clear all;
close all;

p = 11;
n = 1; m = 15;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x = 1.57079632679 * xscale;
f = x - x * x * x / 6e0 + x * x * x * x * x / 12e1 - x * x * x * x * x * x * x / 504e1;
fhat = (((x - x * x * (1e0 + eps(1)) * x * (1e0 + eps(2)) / 6e0) * (1e0 + eps(3)) + x * x * (1e0 + eps(4)) * x * (1e0 + eps(5)) * x * (1e0 + eps(6)) * x * (1e0 + eps(7)) / 12e1) * (1e0 + eps(8)) - x * x * (1e0 + eps(9)) * x * (1e0 + eps(10)) * x * (1e0 + eps(11)) * x * (1e0 + eps(12)) * x * (1e0 + eps(13)) * x * (1e0 + eps(14)) / 504e1) * (1e0 + eps(15));
r = f - fhat;
S = [1 - xscale(1)^2 >= 0];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr, l, h] = boundfperror (p, xscale, eps, S, B, f, r, 4, 1e-7);
