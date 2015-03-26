Real2Float
==============

*real2float* is a set of libraries to analyze floating-point errors of polynomial programs with semidefinite programming. 
The tool requires Mono, Boogie2 and MATLAB toolboxes (Yalmip, SparsePOP and SeDuMi).


Requirements
------------

Mono        [Download](http://www.mono-project.com/download)

Boogie2     [Download](http://boogie.codeplex.com)

MATLAB      [Download](https://fr.mathworks.com/programs/trials/trial_request.html)

Yalmip      [Download](http://users.isy.liu.se/johanl/yalmip/pmwiki.php?n=Main.Download)

SparsePOP   [Download](http://sourceforge.net/projects/sparsepop)

SeDuMi      [Download](http://sedumi.ie.lehigh.edu/downloads)


Compilation
-----------

Go the `Source/` directory and execute the following command:

`% xbuild Real2Float.sln`


Documentation
-------------

Go to the `Examples/` directory and see the content of the rigidbody1.bpl program, encoded in Boogie language:

`procedure rigidbody1 (x1 : real, x2 : real, x3 : real)`
 
` returns (f : real)`

`requires -15.0 <= x1 && x1 <= 15.0;`
`requires -15.0 <= x2 && x2 <= 15.0;`
`requires -15.0 <= x3 && x3 <= 15.0;`

`{`
`  f := -x1 * x2 - 2.0 * x2 * x3 - x1 - x3;`
`}`

This program returns the result `-x1 * x2 - 2.0 * x2 * x3 - x1 - x3`, assuming that each variable lies in `[-15, 15]`. However, when taking into account floating-point error (e.g. with double precision), the actual computed result is different.

To perform this error analysis, one first executes the following command:

`$ ../Source/bin/Debug/Real2Float.exe /bits:53 /resultPrecision:1e-12 rigidbody1.bpl >> ../Matlab/rigidbody1out.m`

This generates a second Boogie program `output.bpl`, whose purpose is to verify whether the absolute error (on the result computation) is less than `1e-12` for double precision (64 bits with 53 bits of significand precision).

The file `output.bpl` displays the floating-point result `f_float` associated to the exact result:

`f_float := (((-x1 * x2 * (1e0 + eps0) - 2 * x2 * (1e0 + eps1) * x3 * (1e0 + eps3)) * (1e0 + eps4) - x1) * (1e0 + eps2) - x3) * (1e0 + eps5);`.

Note that error variables `eps0,...,eps5` have been freshly generated for each operation `*, +, -` used to define `f` and is absolutely bounded by `2^(-53)` (double precision).

In the `Matlab/` directory, the script file `rigidbody1out.m` details how to analyze the error `f - f_float` using sparse sums of squares and semidefinite programming. Executing this script allows to generate certifates that the bound is less than `1e-12`. 

Further developments include automatic Matlab script generation and execution.


Warning
-------

*This is a preliminary implementation and the probability of finding bugs is high.*

Do not hesitate to submit bug report of pull requests.
