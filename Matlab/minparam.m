function [pk, obj] = minparam (x, y, f, g, k, degp)
m = length(g);
[p, pk] = polynomial(y, degp);
obj = 0;
for i = 1:(degp + 1)
  obj = obj + pk(i) / i
end
cstr = []; coeffsos = []; qk = 0;
for j = 1:m
  gj = g(j); dj = 2 * ceil(degree(gj)/2);
  [s,c] = polynomial([x; y], 2 * k - dj); 
  coeffsos = [coeffsos; c];
  cstr = [cstr sos(s)];
  qk = qk + s * gj;
end
K = [sos(f - p - qk), cstr];
solvesos(K, -obj, sdpsettings('solver','mosek'), [coeffsos; obj]);
obj = double(-obj);
pk = double(pk);
