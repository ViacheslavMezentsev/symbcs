public class StringFmt
{
	internal static string Compact(string s)
	{
		while (s.Length > 0 && s[0] == '(' && s[s.Length - 1] == ')')
		{
			var r = s.Substring(1, s.Length - 1 - 1);

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

		foreach (char t in r)
		{
		    switch (t)
		    {
		        case '(':
		            nopen++;
		            break;

		        case ')':
		            nopen--;

		            if ( nopen < 0 )
		            {
		                return false;
		            }
		            break;
		    }
		}

		return nopen == 0;
	}
}
