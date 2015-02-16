clear all;
close all;

delta = 2^(-53);
sdpvar eps0 eps1 eps2 eps3 eps4 eps5;
sdpvar xscale1 xscale2 xscale3;
xscale = [xscale1; xscale2; xscale3];
eps = [eps0; eps1; eps2; eps3; eps4; eps5];

x1 = 15 * xscale1; x2 = 15 * xscale2; x3 = 15 * xscale3;

f = -x1*x2 - 2 * x2 * x3 - x1 - x3;
f_float = (((-x1 * x2 * (1e0 + eps0) - 2 * x2 * (1e0 + eps1) * x3 * (1e0 + eps3)) * (1e0 + eps4) - x1) * (1e0 + eps2) - x3) * (1e0 + eps5);

[mf, Mf, mr, Mr] = boundfperror (delta, xscale, eps, f, f_float);
