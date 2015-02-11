function [obj] = minsos (x, xcliques, p, g, gcliques, k)
n = length(x); m = length(g);
%mons = monpowers(2,2 * k);
obj = sdpvar(1,1);

cstr = []; coeffsos = []; qk = 0; qsparse = 0;
for j = 1:m
  gj = g(j); dj = 2 * ceil(degree(gj)/2);
  [s,c] = polynomial([x(gcliques(j,:))], 2 * k - dj); 
  coeffsos = [coeffsos; c];
  cstr = [cstr sos(s)];
  qk = qk + s * gj;
end
for j = 1:size(xcliques,1)
  [s,c] = polynomial([x(xcliques(j,:))], 2 * k); 
  coeffsos = [coeffsos; c];
  cstr = [cstr sos(s)];
  qsparse = qsparse + s;
end

K = [coefficients(p - obj - qsparse - qk, [x]) == 0, cstr];
optimize(K, -obj, sdpsettings('solver','mosek'), [coeffsos; obj]);
obj = double(obj);
