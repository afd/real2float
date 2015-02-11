function [l, h] = lhdecompose (pol, eps)
[c v] = coefficients (pol,eps);
n = length(c);
l = 0; h = 0;
for alpha =1:n
  if degree (v(alpha)) == 1
    l = l + c(alpha)*v(alpha);
  else
    h = h + c(alpha)*v(alpha);
  end
end
