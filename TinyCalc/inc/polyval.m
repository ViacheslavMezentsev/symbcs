function y = polyval (c, x)
  n = length (c);
  y = c (1);
  for index = 2:n
    y = c (index) + x .* y;
  end
