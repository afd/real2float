clear all;
close all;

delta = 2^(-53);
sdpvar x1_scale x2_scale x3_scale;
sdpvar eps0 eps1 eps2 eps3 eps4 eps5;
x1 = ((15e0 - -15e0) / 2) * x1_scale + (15e0 + -15e0)/2;
x2 = ((15e0 - -15e0) / 2) * x2_scale + (15e0 + -15e0)/2;
x3 = ((15e0 - -15e0) / 2) * x3_scale + (15e0 + -15e0)/2;
x1_float = x1;
x2_float = x2;
x3_float = x3;
f = -x1 * x2 - 2e0 * x2 * x3 - x1 - x3;
f_float = (((-x1_float * x2_float * (1e0 + eps0) - 2e0 * x2_float * (1e0 + eps1) * x3_float * (1e0 + eps2)) * (1e0 + eps3) - x1_float) * (1e0 + eps4) - x3_float) * (1e0 + eps5);
[ lower_real_f, upper_real_f, lower_error_f, upper_error_f, l, h] = boundfperror (delta, [ x1_scale; x2_scale; x3_scale], [ eps0; eps1; eps2; eps3; eps4; eps5 ], f, f_float);
