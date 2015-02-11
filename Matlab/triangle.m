clear all;
close all;

p = 53; d = 1e-6; % Bnd = 1.7595e-06
n = 3; m = 8;
xscale = sdpvar(n,1);
eps = sdpvar(m,1);
a = 4 * xscale(1)+5; b = 4 * xscale(2) + 5; c = 4 * xscale(3) + 5;

s = (a + b + c) / 2e0;
f = s * (s - a) * (s - b) * (s - c);
%sdpvar sscale;
%shat = 15/4 * sscale + 21/4 * sscale;
shat = ((a + b) * (1e0 + eps(1)) + c) * (1e0 + eps(2)) / 2e0;
fhat = shat * (shat - a) * (1e0 + eps(3)) * (1e0 + eps(4)) * (shat - b) * (1e0 + eps(5)) * (1e0 + eps(6)) * (shat - c) * (1e0 + eps(7)) * (1e0 + eps(8));

r = f - fhat;
S = [1 - xscale.^2 >= 0; a + b >= c + d; a + c >= b + d; b + c >= a + d];
B = [1 - eps.^2>=0];
% B = [1 - eps.^2>=0; shat >= ((a + b) * (1e0 + eps(1)) + c) * (1e0 + eps(2)) / 2e0; shat <= ((a + b) * (1e0 + eps(1)) + c) * (1e0 + eps(2)) / 2e0; 1 - sscale^2 >=0];
[mf, Mf, mr, Mr] = boundfperror (p, xscale, eps, S, B, f, r, 2, 1e-7);
