function [mf, Mf, mr, Mr, l, h] = boundfperror (delta, x, eps, f, fhat)

S = [1 - x.^2 >= 0]; B = [1 - eps.^2>=0];
r = f - fhat;
relaxOrder = 2;
SDPsolverEpsilon = 1e-9;

sdpsolver = 'sedumi';
sol = optimize(S, f, sdpsettings('sos.congruence',2, 'solver','sparsepop','sparsepop.relaxOrder',3,'sparsepop.SDPsolver',sdpsolver,'sparsepop.mex',1,'sparsepop.sparseSW',1,'sparsepop.scalingSW',1,'sparsepop.boundSW',2, 'sparsepop.SDPsolverSW',1,'savesolveroutput',1, 'sparsepop.SDPsolverEpsilon',SDPsolverEpsilon));
mf = sol.solveroutput.SDPobjValue;
sol = optimize(S, -f, sdpsettings('sos.congruence',2, 'solver','sparsepop','sparsepop.relaxOrder',2,'sparsepop.SDPsolver',sdpsolver,'sparsepop.mex',1,'sparsepop.sparseSW',1,'sparsepop.scalingSW',1,'sparsepop.boundSW',2, 'sparsepop.SDPsolverSW',1,'savesolveroutput',1,'sparsepop.SDPsolverEpsilon',SDPsolverEpsilon));
Mf = - sol.solveroutput.SDPobjValue;

mr = 0; Mr = 0;
tic
[ralpha monsalpha] = coefficients(r, [x]);
toc
nmons = length(ralpha);
l = 0; h = 0;
tic
for alpha = 1:nmons
  [lalpha, halpha] = lhdecompose (ralpha(alpha), [eps]);
  l = l + lalpha*monsalpha(alpha); h = h + halpha*monsalpha(alpha);
end
toc
sol = optimize([S; B], l, sdpsettings('solver','sparsepop','sparsepop.relaxOrder',relaxOrder,'sparsepop.SDPsolver',sdpsolver,'sparsepop.mex',1,'sparsepop.sparseSW',1,'sparsepop.scalingSW',1,'sparsepop.boundSW',2, 'sparsepop.SDPsolverSW',1,'savesolveroutput',1,'sparsepop.SDPsolverEpsilon',SDPsolverEpsilon,'sparsepop.reduceMomentMatSW',1,'sparsepop.multiCliquesFactor',1));
mr = delta * sol.solveroutput.SDPobjValue;
sol = optimize([S; B], -l, sdpsettings('sos.congruence',2,'solver','sparsepop','sparsepop.relaxOrder',relaxOrder,'sparsepop.SDPsolver',sdpsolver,'sparsepop.mex',1,'sparsepop.sparseSW',1,'sparsepop.scalingSW',1,'sparsepop.boundSW',2, 'sparsepop.SDPsolverSW',1,'savesolveroutput',1,'sparsepop.SDPsolverEpsilon',SDPsolverEpsilon));
Mr = - delta * sol.solveroutput.SDPobjValue;

