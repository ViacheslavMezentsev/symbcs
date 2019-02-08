% least square curve fit using Gauss-Newton 
% example xi=0:0.1:20; for i=1:length(xi), yi=sin(xi)+rand(1)/10; end

function y=jacobi(f,x)
	for i=1:length(f),
		for k=1:length(x),
			y(i,k) = diff( f(i), x(k) );
		end
	end
end

function r = substvec0(v,x,y)
	r = 0;
	for i=1:length(v)
		r(i) = subst(v(i),x,y);
	end
end

function r = substvec1(v,x,y)
	r = y;
	for i=1:length(v)
		r = subst(v(i),x(i),r);
	end
end

function y = norm(x)
	y = sqrt( sum(x.*x) );
end

function a1=gauss(f,a,a0,x,xi,yi)
	a1 = a0;
	F = substvec0( xi, x, f) - yi;
	J = jacobi( F, a );
	for i=1:200
		Ja = substvec1(a1, a, J);
		Jat= Ja.';
		Fa = substvec1(a1, a, F).';
		M  = Jat*Ja;
		if(det(M)==0)
			error("Equations unsolvable. Use different initial values.\n");
		end
		s  = linsolve( M, Jat*Fa );
		a2 = a1 - s.';
		if( norm(a2-a1)==0 )
			break;
		end
		a1 = a2;
	end
end

function y=lsqcurvefit(f,x0,xdata,ydata)
	syms( x,x1,x2,x3,x4,x5,x6,x7,x8,x9,x10 );
	var_in =[x1,x2,x3,x4,x5,x6,x7,x8,x9,x10];
	if (length(x0)>10)
		error("Cannot handle more than 10 variables.\n");
	end
	if (length(xdata) ~= length(ydata) )
		error("xdata and ydata must be equal sized.\n");
	end
	
	for i=1:length(x0),
		var(i) = var_in(i);
	end
	F = f(var,x);
	y = gauss(F,var,x0,x,xdata,ydata);
end
	
