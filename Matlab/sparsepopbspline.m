clear all;
close all;
p = 40;
delta = 2^(-p);
m = 4;
epsbnd = m * delta/(1 - m * delta);
n = 2;
x = sdpvar(n,1);
u = x(1); eps = x(2);
p = -(1+eps)*u^3/6;
%g = [u*(1 - u)>=0; epsbnd^2 - eps^2>=0]; % -2.6012634e-06
%g = [u>=0; 1 - u>=0; epsbnd - eps>=0; eps + epsbnd>=0]; % -3.1672259e-14
%g = [u>=0; 1 - u>=0; epsbnd^2 - eps^2>=0]; % infeasible
g = [u*(1-u)>=0;  epsbnd - eps>=0; eps + epsbnd>=0]; %-6.0265799e-16
optimize(g,p,sdpsettings('solver','sparsepop','sparsepop.relaxOrder',2,'sparsepop.SDPsolver','sdpa'));
