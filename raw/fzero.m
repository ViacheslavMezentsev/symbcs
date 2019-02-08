function x=fzero(G, x1, x2)
% Loese die Gleichung G=0 mittels Intervallschachtelung
% im Intervall x1...x2. 
% G ist String mit Variable x
% Beispielaufruf: fzero('x^2-3',1,2) --> ans = 1.7320
% Rueckgabewert: x
% (1) Pruefen der Argumente
if (x2==x1)
  error('Zero interval.\n');
end

x=x1; y1=eval(G);
if (y1==0)
	return;
end
x=x2; y2=eval(G);
if (y2==0)
	return;
end

if (y1*y2>0)
  error('f(a) and f(b) must have opposite signs.\n');
end

% Stopping criteria
% 
abs_err_max = 1.0e-15;
max_iter  	= 1000;

iter 		= 1;
abs_err		= abs(x2-x1);

while (iter<=max_iter & abs_err>abs_err_max)
  x=(x1+x2)/2; y=eval(G);
  if (y==0)
    return;
  end
  if (y1*y<0)
    x2=x; y2=y;
  else
    x1=x; y1=y;
  end
  iter 		++;
  abs_err	= abs(x2-x1);
end 
