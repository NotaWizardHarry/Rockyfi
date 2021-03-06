using System; 
using System.Text;

namespace Rockyfi.Expr
{
    static class ErrorFormator 
    {
        internal static string FormatError(Error code, int pos)
        {
            return string.Format(SR.ErrorFormat, code.ToString() , pos, (int)code);
        }
    }
}
