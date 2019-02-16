% essential
function y = acosh(x)
	y = log(x+sign(x)*sqrt(x*x-1));
	
