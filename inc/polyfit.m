function p=polyfit(x,y,n)
l=length(x);

if (n>l)
	error("Not enough data to fit polynomial.\n");
end

if (l ~= length(y))
	error("x and y must have the same size.\n");
end

X=(x'*ones(1,n+1)).^(ones(l,1)*(n:-1:0));
p=(X\y')';