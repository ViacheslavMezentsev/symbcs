function y=nchoosek(n,k)
   if (k > n)
        y = 0;
        return;
    end

    if (k > n/2)
        k = n-k;
    end
    
	if (k==0)
		y = 1;
	else
    	y = prod( (n-k+(1:k))./(1:k) );
    end

