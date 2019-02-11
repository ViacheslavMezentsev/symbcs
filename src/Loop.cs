using System;
using System.Collections;
using System.Text;
using System.Threading;

internal class LambdaERROR : Lambda
{
    public override int Eval( Stack stack )
    {
        LambdaPRINTF.printf( stack );

        return Processor.ERROR;
    }
}

internal class LambdaEVAL : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var s_in = stack.Pop();

        if ( !( s_in is string ) )
        {
            throw new JasymcaException( "Argument to EVAL must be string." );
        }

        var pgm = pr.compile( ( string ) s_in );

        return pc.process_list( pgm, true );
    }
}

internal class LambdaBLOCK : Lambda
{
	public override int Eval(Stack stack)
	{
        int narg = GetNarg( stack );

        var local = pc.env.copy();
        var code = GetList( stack );

        var ups = new Stack();

	    int ret = UserProgram.process_block( code, ups, local, false );

		pc.env.update(local);

	    if ( ret != Processor.ERROR && ups.Count > 0 )
		{
			var y = ups.Pop();

			stack.Push(y);
		}
		else
		{
		    throw new JasymcaException( "Error processing block." );
		}

		return 0;
	}
}

internal class LambdaBRANCH : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack ), sel;

        List cond, b_true, b_false;

        switch ( narg )
        {
            case 2:
                cond = GetList( stack );
                b_true = GetList( stack );
                pc.process_list( cond, true );
                sel = GetInteger( pc.stack );

                if ( sel == 1 )
                {
                    return pc.process_list( b_true, true );
                }
                else if ( sel != 0 )
                {
                    throw new JasymcaException( "Branch requires boolean type." );
                }

                break;

            case 3:

                cond = GetList( stack );
                b_true = GetList( stack );
                b_false = GetList( stack );

                pc.process_list( cond, true );

                sel = GetInteger( pc.stack );

                if ( sel == 1 )
                {
                    return pc.process_list( b_true, true );
                }
                else if ( sel == 0 )
                {
                    return pc.process_list( b_false, true );
                }
                else
                {
                    throw new JasymcaException( "Branch requires boolean type, got " + sel );
                }
            default:
                throw new JasymcaException( "Wrong number of arguments to branch." );
        }

        return 0;
    }
}

internal class LambdaFOR : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var cond = GetList( stack );
        var body = GetList( stack );

        pc.process_list( cond, true );

        if ( pc.stack.Count == 0 || !( pc.stack.Peek() is Vector ) || ( ( Algebraic ) pc.stack.Peek() ).Name == null )
        {
            throw new ParseException( "Wrong format in for-loop." );
        }

        var vals = ( Vector ) pc.stack.Pop();

        for ( int i = 0; i < vals.Length(); i++ )
        {
            pc.env.putValue( vals.Name, vals[i] );

            int ret = pc.process_list( body, true );

            switch ( ret )
            {
                case Processor.BREAK:
                    return 0;

                case Processor.RETURN:
                case Processor.EXIT:
                case Processor.ERROR:
                    return ret;

                case Processor.CONTINUE: break;
            }
        }

        return 0;
    }
}

internal class LambdaXFOR : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var cond = GetList( stack );
        var step_in = GetList( stack );
        var thru_in = GetList( stack );
        var body = GetList( stack );

        pc.process_list( cond, true );

        if ( pc.stack.Count == 0 || !( pc.stack.Peek() is Symbolic ) || ( ( Algebraic ) pc.stack.Peek() ).Name == null )
        {
            throw new ParseException( "Non-constant initializer in for loop." );
        }

        var x = ( Symbolic ) pc.stack.Pop();

        var xname = x.Name;

        pc.process_list( step_in, true );

        if ( pc.stack.Count == 0 || !( pc.stack.Peek() is Symbolic ) )
        {
            throw new ParseException( "Step size must be constant." );
        }

        var step = ( Symbolic ) pc.stack.Pop();

        pc.process_list( thru_in, true );

        if ( pc.stack.Count == 0 || !( pc.stack.Peek() is Symbolic ) )
        {
            throw new ParseException( "Wrong format in for-loop." );
        }

        var thru = ( Symbolic ) pc.stack.Pop();

        var pos = !step.Smaller( Symbolic.ZERO );

        while ( true )
        {
            if ( ( pos ? thru.Smaller( x ) : x.Smaller( thru ) ) )
            {
                break;
            }

            pc.env.putValue( xname, x );

            int ret = pc.process_list( body, true );

            switch ( ret )
            {
                case Processor.BREAK:
                    return 0;

                case Processor.RETURN:
                case Processor.EXIT:
                case Processor.ERROR:
                    return ret;

                case Processor.CONTINUE: break;
            }

            x = ( Symbolic ) ( x + step );
        }

        return 0;
    }
}

internal class LambdaWHILE : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var cond = GetList( stack );
        var body = GetList( stack );

        while ( true )
        {
            pc.process_list( cond, true );

            var c = ( Symbolic ) pc.stack.Pop();

            if ( Equals( c, Symbolic.ZERO ) )
            {
                break;
            }
            else if ( !Equals( c, Symbolic.ONE ) )
            {
                throw new JasymcaException( "Not boolean: " + c );
            }

            int ret = pc.process_list( body, true );

            switch ( ret )
            {
                case Processor.BREAK:
                    return 0;

                case Processor.RETURN:
                case Processor.EXIT:
                case Processor.ERROR:
                    return ret;

                case Processor.CONTINUE: break;
            }
        }

        return 0;
    }
}

internal class LambdaPRINTF : Lambda
{
	public override int Eval(Stack st)
	{
		printf(st);
		return 0;
	}

	internal static void printf( Stack st )
	{
        int narg = GetNarg( st );

        var s_in = st.Pop();

        if ( !( s_in is string ) )
        {
            throw new JasymcaException( "Argument to PRINTF must be string." );
        }

        string fmt = ( string ) s_in;

        int idx, i = 1;
        var cs = "%f";

        while ( ( idx = fmt.IndexOf( cs, StringComparison.Ordinal ) ) != -1 && st.Count > 0 && narg-- > 1 )
        {
            var n = st.Pop();

            if ( n != null )
            {
                var sb = new StringBuilder( fmt );

                sb.Remove( idx, idx + cs.Length - idx );
                sb.Insert( idx, n.ToString() );

                fmt = sb.ToString();
            }
            else
            {
                break;
            }
        } 
        
        while ( ( idx = fmt.IndexOf( "\\n", StringComparison.Ordinal ) ) != -1 )
        {
            var sb = new StringBuilder( fmt );

            sb.Remove( idx, idx + "\\n".Length - idx );
            sb.Insert( idx, "\n" );

            fmt = sb.ToString();
        }

        if ( pc.ps != null )
        {
            pc.ps.print( fmt );
        }
    }
}

internal class LambdaPAUSE : Lambda
{
	public override int Eval( Stack st )
	{
		int narg = GetNarg(st);

	    int millis = Math.Abs( GetInteger( st ) );

		try
		{
			Thread.Sleep( millis );
		}
		catch (Exception)
		{
		}

		return 0;
	}
}
