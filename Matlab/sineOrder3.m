clear all;
close all;

p = 24;
n = 1; m = 5;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
x = 2 * xscale;
f =  954929658551372e-15 * x - 12900613773279798e-17 * x * x * x;
fhat =  (954929658551372e-15 * x * (1e0 + eps(1)) - 12900613773279798e-17 * x * (1e0 + eps(2)) * x * (1e0 + eps(3)) * x * (1e0 + eps(4))) * (1e0 + eps(5));
r = f - fhat;
S = [1 - xscale(1)^2 >= 0];
B = [1 - eps.^2>=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 3, 1e-9);
