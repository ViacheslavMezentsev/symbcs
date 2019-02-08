% Demo Jasymca Octave Mode
p = 5000;
printf("Willkommen zur Live-Demonstration von Jasymca\n");
pause(200);
printf("\n\n");
pause(2000);
printf("Jasymca ist ein interaktives Rechenprogramm fuer Java Plattformen\n");
pause(2000);
printf("\n\n\n");
printf("Berechnen Sie beliebige mathematische Formeln\n");
printf(">>2^75-1\n");
format short;
2^75-1,
pause(p);
printf("\n\n\n");
printf("Gerechnet wird mit ca. 16 Dezimalstellen Genauigkeit\n");
printf(">>format long\n");
format long;
printf(">>2^75-1\n");
2^75-1,
pause(p);
printf("\n\n\n");
printf("Oder exakt mit der Funktion 'rat':\n");
printf(">>rat(2)^75-1\n");
rat(2)^75-1,
pause(p);

format short;
printf("\n\n\n");
printf("Finden von gemeinsamen Teilern:\n");
printf(">>gcd(2891951703861, 289169475515193915)\n");
gcd(2891951703861, 289169475515193915),
pause(p);
printf("\n\n\n");
printf("Auch bei symbolischen Ausdruecken:\n");
printf(">>syms x,z\n");
syms x,z
printf(">>gcd(z*x^5-z, x^2-2*x+1)\n");
gcd(z*x^5-z,x^2-2*x+1),
pause(p);
printf("\n\n\n");
printf("Rechnen mit Polynomen;\n");
printf(">> syms x\n");
syms x
printf(">> y=(x-1)*(x+2)*(x-1/3)*(x+25)\n");
y=(x-1)*(x+2)*(x-1/3)*(x+25),
pause(p);
printf("\n\n\n");
printf("Bestimmung der Nullstellen;\n");
printf(">> allroots( y )\n");
allroots( y ),
pause(p);
printf("\n\n\n");
printf("Zerlegung von komplexen Ausdruecken:\n");
printf(">> syms x;\n");
syms x;
printf(">> y=(3+i*x)/(2-i*x)\n");
y=(3+i*x)/(2-i*x),
pause(p);
printf("\n\n\n");
printf("In Realteil:\n");
printf(">> realpart(y)\n");
realpart(y),
pause(p);
printf("\n\n\n");
printf("und in Imaginaerteil:\n");
printf(">> imagpart(y)\n");
imagpart(y),
pause(p);
printf("\n\n\n");
printf("Rechnen mit Matrizen:\n");
printf(">> A = hilb(4)\n");
A = hilb(4),
pause(p);
printf("\n\n\n");
printf("Inverse:\n");
printf(">> B = inv(A)\n");
B = inv(A),
pause(p);
printf("\n\n\n");
printf("Multiplikation:\n");
printf(">> B * A\n");
B * A,
pause(p);
printf("\n\n\n");
printf("Determinante:\n");
printf(">> det( B )\n");
det( B ),
pause(p);
printf("\n\n\n");
printf("Symbolische Lineare Gleichungssysteme:\n");
printf(">> syms x,y;\n");
syms x,y;
printf(">> A=[x,-2,0;1 3*y 4;1 2 0]\n");
A=[x,-2,0;1 3*y 4;1 2 0],
pause(p);
printf("\n");
printf("Die Matrix A des Gleichungssystems haengt von Variablen x und y ab.\n");
pause(p);
printf("\n");
printf("Die rechte Seite des Gleichungssystems:\n");
printf(">>b = [1 -2  2 ]\n");
b = [1 -2  2 ],
pause(p);
printf("\n");
printf("\n\nUnd schliesslich die 3 Loesungen:\n");
printf(">> trigrat( linsolve( rat(A), b) )\n ");
trigrat( linsolve( rat(A), b) ),

pause(p);
pause(p);
printf("\n\n\n");
printf("Symbolisch integrieren:\n");
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
printf("und differenzieren:\n");
printf(">>  diff(ans,x)\n");
diff(ans,x),
pause(p);

printf("\n\n\n");
printf("Trigonometrische Vereinfachungen:\n");
printf(">> syms x;\n");
syms x;
printf(">> sin(x)^2+sin(x+2*pi/3)^2+sin(x+4*pi/3)^2\n");
sin(x)^2+sin(x+2*pi/3)^2+sin(x+4*pi/3)^2,
printf("\n\n\n");
pause(p);
printf("Deshalb benutzt man Drehstrom zur Energieuebertragung:\n");
printf(">> trigrat(ans)\n");
trigrat(ans),
pause(p);

printf("\n\n\n");
printf("Programmierbar, zum Beispiel mit Schleifen:\n");
pause(p);
printf("\n");
printf("Wir berechnen die ersten 5 Taylorpolynome von log(1+x):\n");
pause(p);
printf("\n");
printf(">> syms x;\n");
pause(p);
printf(">>for (k=1:5)\n>");
pause(p);
printf("y = rat( taylor( log(1+x),x,0,k ));\n>" );
pause(p);
printf("printf('Taylorpolynom Grad %f: %f',k,y)\n>");
pause(p);
printf("end\n\n");
pause(p);
syms x; 
for (k=1:5) y = rat(taylor( log(1+x),x,0,k )); printf('Grad %f: %f\n',k,y); end,
pause(p);
printf("\n\n\n");
printf("Und viele weitere Moeglichkeiten.\n");
printf("Viel Spass beim Ausprobieren!\n");
printf("Helmut Dersch  der@fh-furtwangen.de\n");

