clear all;
close all;

p = 53;
n = 4; m = 6;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
v = (20000-20)/2 * xscale(2) + (20000 + 20)/2; u = 100 * xscale(1); T = 40 * xscale(3)+10;
t1 = 331.4 + 0.6*T;
% 313.4 <= t1 <= 361.4 
% z = 1/(t1 + u)^2 => 1/(461.4^2) <= z <= 1/(213.4^2)
z = (1/(213.4^2) - 1/(461.4^2))/2 * xscale(4) + (1/(213.4^2) + 1/(461.4^2))/2;
f = -t1*v*z;
fhat =  -t1 * v * (1e0 + _eps0) / ((t1 + u) * (1e0 + _eps1) * (t1 + u) * (1e0 + _eps2) * (1e0 + _eps3))



r = f - fhat;
S = [1 - xscale(1)^2 >= 0; 1 - xscale(2)^2 >= 0; 1 - xscale(3)^2 >= 0;  1 - xscale(4)^2 >= 0; z * (t1 + u)^2 >= 1; z * (t1 + u)^2 <= 1];
B = [1 - eps(1)^2 >= 0; 1 - eps(2)^2 >= 0; 1 - eps(3)^2 >= 0; 1 - eps(4)^2 >= 0; 1 - eps(5)^2 >= 0; 1 - eps(6)^2 >= 0];
B = [1 - eps(1)^2 >= 0; 1 - eps(2)^2 >= 0; 1 - eps(3)^2 >= 0; 1 - eps(4)^2 >= 0; 1 - eps(5)^2 >= 0; 1 - eps(6)^2 >= 0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2);
