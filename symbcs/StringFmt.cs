public class StringFmt
{
	internal static string compact(string s)
	{
		while (s.Length > 0 && s[0] == '(' && s[s.Length - 1] == ')')
		{
			string r = s.Substring(1, s.Length - 1 - 1);
			if (!balanced(r))
			{
				break;
			}
			s = r;
		}
		return s;
	}
	internal static bool balanced(string r)
	{
		int nopen = 0;
		for (int i = 0; i < r.Length; i++)
		{
			switch (r[i])
			{
				case '(':
					nopen++;
					break;
				case ')':
					nopen--;
					if (nopen < 0)
					{
						return false;
					}
				    break;
                default:
                    break;
			}
		}
		return nopen == 0;
	}
}