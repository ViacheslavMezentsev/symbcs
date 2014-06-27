using System;

internal class ParseException : Exception
{
	public ParseException(string s) : base(s)
	{
	}
}
internal class JasymcaException : Exception
{
	public JasymcaException(string s) : base(s)
	{
	}
}