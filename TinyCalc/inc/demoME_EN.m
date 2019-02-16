% Demo Jasymca Octave Mode
p = 5000;
printf("Welcome to the Live-Demonstration of Jasymca\n");
pause(200);
printf("Interrupt anytime using Control-c or Menu: Run->Interrupt.\n");
printf("\n\n");
pause(2000);
printf("Jasymca is an interactive Calculator for Java-Platforms\n");
pause(2000);
printf("\n\n\n");
printf("Calculate mathematical expressions\n");
printf(">>2^75-1\n");
format short;
2^75-1,
pause(p);
printf("\n\n\n");
printf("Calculations use 15-16 decimal digits.\n");
printf(">>format long\n");
format long;
printf(">>2^75-1\n");
2^75-1,
pause(p);
printf("\n\n\n");
printf("or they are exact if 'rat' is used:\n");
printf(">>rat(2)^75-1\n");
rat(2)^75-1,
pause(p);

format short;
printf("\n\n\n");
printf("Greatest common denominator:\n");
printf(">>gcd(2891951703861, 289169475515193915)\n");
gcd(2891951703861, 289169475515193915),
pause(p);
printf("\n\n\n");
printf("works also with symbolic expressions:\n");
printf(">>syms x,z\n");
syms x,z
printf(">>gcd(z*x^5-z, x^2-2*x+1)\n");
gcd(z*x^5-z,x^2-2*x+1),
pause(p);
printf("\n\n\n");
printf("Symbolic polynomials;\n");
printf(">> syms x\n");
syms x
printf(">> y=(x-1)*(x+2)*(x-1/3)*(x+25)\n");
y=(x-1)*(x+2)*(x-1/3)*(x+25),
pause(p);
printf("\n\n\n");
printf("Finding roots;\n");
printf(">> allroots( y )\n");
allroots( y ),
pause(p);
printf("\n\n\n");
printf("Decompose complex expressions:\n");
printf(">> syms x;\n");
syms x;
printf(">> y=(3+i*x)/(2-i*x)\n");
y=(3+i*x)/(2-i*x),
pause(p);
printf("\n\n\n");
printf("Into realpart:\n");
printf(">> realpart(y)\n");
realpart(y),
pause(p);
printf("\n\n\n");
printf("and imaginary part:\n");
printf(">> imagpart(y)\n");
imagpart(y),
pause(p);
printf("\n\n\n");
printf("Working with matrices:\n");
printf(">> A = hilb(4)\n");
A = hilb(4),
pause(p);
printf("\n\n\n");
printf("Inverse:\n");
printf(">> B = inv(A)\n");
B = inv(A),
pause(p);
printf("\n\n\n");
printf("Multiplication:\n");
printf(">> B * A\n");
B * A,
pause(p);
printf("\n\n\n");
printf("Determinante:\n");
printf(">> det( B )\n");
det( B ),
pause(p);
printf("\n\n\n");
printf("System of symbolic linear equations::\n");
printf(">> syms x,y;\n");
syms x,y;
printf(">> A=[x,-2,0;1 3*y 4;1 2 0]\n");
A=[x,-2,0;1 3*y 4;1 2 0],
pause(p);
printf("\n");
printf("The system of equation's matrix A depends on symbolic variables x and y.\n");
pause(p);
printf("\n");
printf("The right-hand-side of the equations:\n");
printf(">>b = [1 -2  2 ]\n");
b = [1 -2  2 ],
pause(p);
printf("\n");
printf("\n\nFinally, the three solutions:\n");
printf(">> trigrat( linsolve( rat(A), b) )\n ");
trigrat( linsolve( rat(A), b) ),

pause(p);
pause(p);
printf("\n\n\n");
printf("Symbolic calculus:\n");
printf(">> syms x;\n");
syms x;
printf(">> y=(x^3+2*x^2-x+1)/((x+i)*(x-i)*(x+3))\n");
y=(x^3+2*x^2-x+1)/((x+i)*(x-i)*(x+3)),       
printf("\n\n\n");
pause(p);
printf(">> integrate(y,x)\n");
integrate(y,x),
pause(p);
printf("\n\n\n");
printf("and differentiation:\n");
printf(">>  diff(ans,x)\n");
diff(ans,x),
pause(p);

printf("\n\n\n");
printf("Trigonometric simplifications:\n");
printf(">> syms x;\n");
syms x;
printf(">> sin(x)^2+sin(x+2*pi/3)^2+sin(x+4*pi/3)^2\n");
sin(x)^2+sin(x+2*pi/3)^2+sin(x+4*pi/3)^2,
printf("\n\n\n");
pause(p);
printf("This is why we use three-phase current for power transmission:\n");
printf(">> trigrat(ans)\n");
trigrat(ans),
pause(p);

printf("\n\n\n");
printf("Programmable, e.g. loops:\n");
pause(p);
printf("\n");
printf("We calculate the first 5 Taylorpolynomials of log(1+x):\n");
pause(p);
printf("\n");
printf(">> syms x;\n");
pause(p);
printf(">>for (k=1:5)\n>");
pause(p);
printf("y = rat( taylor( log(1+x),x,0,k ));\n>" );
pause(p);
printf("printf('degree %f: %f',k,y)\n>");
pause(p);
printf("end\n\n");
pause(p);
syms x; 
for (k=1:5) y = rat(taylor( log(1+x),x,0,k )); printf('degree %f: %f\n',k,y); end,
pause(p);
printf("\n\n\n");
printf("And many more options.\n");
printf("Try it out and have fun!\n");
printf("Helmut Dersch  der@fh-furtwangen.de\n");
