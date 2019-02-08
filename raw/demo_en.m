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
printf("\n\n\n");
printf("Decompose into primes:\n");
printf(">>primes( ans )\n");
primes(ans),
pause(p);
printf("\n\n\n");
printf("Control: multiply primes:\n");
printf(">>prod( ans )\n");
prod( ans ),
pause(p);
format short;
printf("\n\n\n");
printf("Greatest common denominator:\n");
printf(">>gcd(289169475515192951703861, 2891694755151939166450249891702715)\n");
gcd(289169475515192951703861, 2891694755151939166450249891702715),
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
printf(">> A=[x,1,-2,-2,0;1 2 3*y 4 5;1 2 2 0 1;9 1 6 0 -1;0 0 1 0]\n");
A=[x,1,-2,-2,0;1 2 3*y 4 5;1 2 2 0 1;9 1 6 0 -1;0 0 1 0],
pause(p);
printf("\n");
printf("The system of equation's matrix A depends on symbolic variables x and y.\n");
pause(p);
printf("\n");
printf("The right-hand-side of the equations:\n");
printf(">>b = [1 -2  3  2  4 ]\n");
b = [1 -2  3  2  4 ],
pause(p);
printf("\n");
printf("\n\nFinally, the five solutions:\n");
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
pause(p);
printf("\n\n\n");
printf("Plots:\n");
printf(">> t=0:0.1:4*pi; x=sin(0.5*t+1); y=cos(1.5*t);\n");
t=0:0.1:4*pi; x=sin(0.5*t+1); y=cos(1.5*t);
printf(">> plot(x,y)\n");
plot(x,y)
pause(p);
printf(">> hold on\n");
hold on;
pause(1000);
printf(">> x=sin(0.5*t+2);plot(x,y,'r');\n");
x=sin(0.5*t+2);plot(x,y,'r');
pause(1000);
printf(">> x=sin(0.5*t+3);plot(x,y,'g');\n");
x=sin(0.5*t+3);plot(x,y,'g');
pause(1000);
printf(">> x=sin(0.5*t+4);plot(x,y,'c');\n");
x=sin(0.5*t+4);plot(x,y,'c');
pause(1000);
printf(">> x=sin(0.5*t+5);plot(x,y,'m');\n");
x=sin(0.5*t+5);plot(x,y,'m');
pause(1000);
printf(">> x=sin(0.5*t+6);plot(x,y,'y');\n");
x=sin(0.5*t+6);plot(x,y,'y');
pause(1000);
printf(">> x=sin(0.5*t+7);plot(x,y,'k');\n");
x=sin(0.5*t+7);plot(x,y,'k');
pause(1000);
printf(">> x=sin(0.5*t+8);plot(x,y,'w');\n");
x=sin(0.5*t+8);plot(x,y,'w');
pause(p);
pause(p);
printf(">> hold off\n");
hold off;

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
printf("We calculate the first 10 Taylorpolynomials of log(1+x):\n");
pause(p);
printf("\n");
printf(">> syms x;\n");
pause(p);
printf(">>for (k=1:10)\n>");
pause(p);
printf("y = rat( taylor( log(1+x),x,0,k ));\n>" );
pause(p);
printf("printf('degree %f: %f',k,y)\n>");
pause(p);
printf("end\n\n");
pause(p);
syms x; 
for (k=1:10) y = rat(taylor( log(1+x),x,0,k )); printf('degree %f: %f\n',k,y); end,
pause(p);
printf("\n\n\n");
printf("And many more options.\n");
printf("Try it out and have fun!\n");
printf("Helmut Dersch  der@fh-furtwangen.de\n");

