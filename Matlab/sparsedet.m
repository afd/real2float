% Minimize (a * b * (1 + delta1) + c * d * (1 + delta2))(1 + delta3)
% 0 <= a, b, c, d <= 1 | deltai | <= Delta = 2^{-m}, m = 24 (C single prec) or m = 54 (double)
% Problem: sdp solver with single precision

clear all;
close all;
m = 6;
n = 7;
x = sdpvar(n,1);
a = x(1); b = x(2); c = x(3); d = x(4);
delta1 = x(5); delta2 = x(6); delta3 = x(7);
p = a * b + c * d - (a * b * (1 + delta1) + c * d * (1 + delta2))*(1 + delta3);
c1 = [1 2 5 7]; c2 = [3 4 6 7];
xcliques = [c1; c2];
%g = [2 - norm(x([1:2]),2)^2 - delta1^2 + 2^(-2 * m) - delta3^2 + 2^(-2 * m) ; 2 - norm(x([3:4]),2)^2 - delta2^2 + 2^(-2 * m) - delta3^2 + 2^(-2 * m)];
g = [2 - norm(x([1:2]),2)^2; 2 - norm(x([3:4]),2)^2; - delta1^2 + 2^(-2 * m); - delta2^2 + 2^(-2 * m); - delta3^2 + 2^(-2 * m)];
gcliques = [c1; c2; c1; c2; c1];
%g = [2 - norm(x(1:4),2)^2; - delta1^2 + 2^(-2 * m); -delta2^2 + 2^(-2 * m); -delta3^2 + 2^(-2 * m)];
m2 = sparseminsos (x, xcliques, p, g, gcliques, 2);
%m3 = sparseminsos (x, xcliques, p, gcliques, 3);
%m4 = sparseminsos (x, xcliques, p, gcliques, 4);
