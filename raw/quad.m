function s=quad(fnc, a, b)
   % Function quad
   % Autor: Helmut Dersch
   % Nachbildung der Octave Bibliotheksfunktion
   % Argumente: func - die zu integrierende Funktion 
   %                   als String, untere und obere Grenzen a,b

   % Stuetzstellen   
   n=200;
   dx = (b - a)/ n;
 
   x = a:dx:b;
 
   % a == b ist rundungsabhaengig  
   if length(x) ~= n+1
      x1 = x;
	  x  = zeros(1,n+1);
	  x(1:n) = x1(1:n);
	  x(n+1) = b;
   end

   y = eval(fnc);   

   % Simpson Reihe
   % length x, y = n+1
   
   s  = y(1) + y(n+1);
   
   for i=2:2:n
      s = s + 4*y(i);
   end
   
   for i=3:2:n
      s = s + 2*y(i);
   end
   
   s = s * dx / 3;
