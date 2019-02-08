function H=invhilb(n)
n=rat(n);
H=zeros(n);
for (i=1:n)
   i=rat(i);
   for (j=1:n)
      j=rat(j);
      x = nchoosek(i+j-2,i-1);
      H(i,j) = (-1)^(i+j)*(i+j-1)*nchoosek(n+i-1,n-j)*nchoosek(n+j-1,n-i)*x*x;
   end
end
H;
end
