using System;
using Linguini.Shared.Types;
using Linguini.Shared.Util;

namespace PluralRules.Generated.Static
{
    
    public static class RuleTable
    {
        private static Func<PluralOperands, PluralCategory>[] cardinalMap = 
        {
            _ => PluralCategory.Other,
            po =>
            {
                if (po.I == 0 || po.N == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.I == 0 || 
                    po.I == 1 ))
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I.InRange(0, 1) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1 && po.V == 0)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 0 || 
                    po.N == 1 ) || po.I == 0 && po.F == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N.InRange(0, 1) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N.InRange(0, 1)  || po.N.InRange(11, 99) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1 || po.T != 0 && ( po.I == 0 || 
                    po.I == 1 ))
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.T == 0 && po.I % 10 == 1 && !(po.I % 100 == 11)  || po.T != 0)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 10 == 1 && !(po.I % 100 == 11)  || po.F % 10 == 1 && !(po.F % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && ( po.I == 1 || 
                    po.I == 2 || 
                    po.I == 3 ) || po.V == 0 && !(po.I % 10 == 4
                    || po.I % 10 == 6
                    || po.I % 10 == 9)  || po.V != 0 && !(po.F % 10 == 4
                    || po.F % 10 == 6
                    || po.F % 10 == 9) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 0 || (po.N % 100).InRange(11, 19) || po.V == 2 && (po.F % 100).InRange(11, 19))
                {
                    return PluralCategory.Zero;
                }
                if (po.N % 10 == 1 && !(po.N % 100 == 11)  || po.V == 2 && po.F % 10 == 1 && !(po.F % 100 == 11)  || po.V != 2 && po.F % 10 == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 0)
                {
                    return PluralCategory.Zero;
                }
                if (( po.I == 0 || 
                    po.I == 1 ) && po.N != 0)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 0)
                {
                    return PluralCategory.Zero;
                }
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 0 || po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N.InRange(2, 10) )
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1 && po.V == 0)
                {
                    return PluralCategory.One;
                }
                if (po.V != 0 || po.N == 0 || (po.N % 100).InRange(2, 19))
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 10 == 1 && !(po.I % 100 == 11)  || po.F % 10 == 1 && !(po.F % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && (po.I % 10).InRange(2, 4) && !(po.I % 100).InRange(12, 14) || (po.F % 10).InRange(2, 4) && !(po.F % 100).InRange(12, 14))
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.I == 0 || 
                    po.I == 1 ))
                {
                    return PluralCategory.One;
                }
                if (po.Exp() == 0 && po.I != 0 && po.I % 1000000 == 0 && po.V == 0 || !po.Exp().InRange(0, 5) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 11 ))
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 12 ))
                {
                    return PluralCategory.Two;
                }
                if (( po.N.InRange(3, 10)  || 
                    po.N.InRange(13, 19)  ))
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 100 == 1)
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && po.I % 100 == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.V == 0 && (po.I % 100).InRange(3, 4) || po.V != 0)
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 100 == 1 || po.F % 100 == 1)
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && po.I % 100 == 2 || po.F % 100 == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.V == 0 && (po.I % 100).InRange(3, 4) || (po.F % 100).InRange(3, 4))
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1 && po.V == 0)
                {
                    return PluralCategory.One;
                }
                if (po.I == 2 && po.V == 0)
                {
                    return PluralCategory.Two;
                }
                if (po.V == 0 && !po.N.InRange(0, 10)  && po.N % 10 == 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1 && po.V == 0)
                {
                    return PluralCategory.One;
                }
                if (po.I.InRange(2, 4)  && po.V == 0)
                {
                    return PluralCategory.Few;
                }
                if (po.V != 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1 && po.V == 0)
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && (po.I % 10).InRange(2, 4) && !(po.I % 100).InRange(12, 14))
                {
                    return PluralCategory.Few;
                }
                if (po.V == 0 && po.I != 1 && (po.I % 10).InRange(0, 1) || po.V == 0 && (po.I % 10).InRange(5, 9) || po.V == 0 && (po.I % 100).InRange(12, 14))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 1 && !(po.N % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                if ((po.N % 10).InRange(2, 4) && !(po.N % 100).InRange(12, 14))
                {
                    return PluralCategory.Few;
                }
                if (po.N % 10 == 0 || (po.N % 10).InRange(5, 9) || (po.N % 100).InRange(11, 14))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 1 && !(po.N % 100).InRange(11, 19))
                {
                    return PluralCategory.One;
                }
                if ((po.N % 10).InRange(2, 9) && !(po.N % 100).InRange(11, 19))
                {
                    return PluralCategory.Few;
                }
                if (po.F != 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 0 || (po.N % 100).InRange(2, 10))
                {
                    return PluralCategory.Few;
                }
                if ((po.N % 100).InRange(11, 19))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 10 == 1 && !(po.I % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && (po.I % 10).InRange(2, 4) && !(po.I % 100).InRange(12, 14))
                {
                    return PluralCategory.Few;
                }
                if (po.V == 0 && po.I % 10 == 0 || po.V == 0 && (po.I % 10).InRange(5, 9) || po.V == 0 && (po.I % 100).InRange(11, 14))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 1 && !(po.N % 100 == 11
                    || po.N % 100 == 71
                    || po.N % 100 == 91) )
                {
                    return PluralCategory.One;
                }
                if (po.N % 10 == 2 && !(po.N % 100 == 12
                    || po.N % 100 == 72
                    || po.N % 100 == 92) )
                {
                    return PluralCategory.Two;
                }
                if (((po.N % 10).InRange(3, 4)
                    || po.N % 10 == 9)  && !((po.N % 100).InRange(10, 19)
                    || (po.N % 100).InRange(70, 79)
                    || (po.N % 100).InRange(90, 99)) )
                {
                    return PluralCategory.Few;
                }
                if (po.N != 0 && po.N % 1000000 == 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.N.InRange(3, 6) )
                {
                    return PluralCategory.Few;
                }
                if (po.N.InRange(7, 10) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.V == 0 && po.I % 10 == 1)
                {
                    return PluralCategory.One;
                }
                if (po.V == 0 && po.I % 10 == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.V == 0 && (po.I % 100 == 0
                    || po.I % 100 == 20
                    || po.I % 100 == 40
                    || po.I % 100 == 60
                    || po.I % 100 == 80) )
                {
                    return PluralCategory.Few;
                }
                if (po.V != 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 0)
                {
                    return PluralCategory.Zero;
                }
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if ((po.N % 100 == 2
                    || po.N % 100 == 22
                    || po.N % 100 == 42
                    || po.N % 100 == 62
                    || po.N % 100 == 82)  || po.N % 1000 == 0 && ((po.N % 100000).InRange(1000, 20000)
                    || po.N % 100000 == 40000
                    || po.N % 100000 == 60000
                    || po.N % 100000 == 80000)  || po.N != 0 && po.N % 1000000 == 100000)
                {
                    return PluralCategory.Two;
                }
                if ((po.N % 100 == 3
                    || po.N % 100 == 23
                    || po.N % 100 == 43
                    || po.N % 100 == 63
                    || po.N % 100 == 83) )
                {
                    return PluralCategory.Few;
                }
                if (po.N != 1 && (po.N % 100 == 1
                    || po.N % 100 == 21
                    || po.N % 100 == 41
                    || po.N % 100 == 61
                    || po.N % 100 == 81) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 0)
                {
                    return PluralCategory.Zero;
                }
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                if ((po.N % 100).InRange(3, 10))
                {
                    return PluralCategory.Few;
                }
                if ((po.N % 100).InRange(11, 99))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 0)
                {
                    return PluralCategory.Zero;
                }
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.N == 3)
                {
                    return PluralCategory.Few;
                }
                if (po.N == 6)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
        };

        private static Func<PluralOperands, PluralCategory>[] ordinalMap = 
        {
            _ => PluralCategory.Other,
            po =>
            {
                if ((po.N % 10 == 1
                    || po.N % 10 == 2)  && !(po.N % 100 == 11
                    || po.N % 100 == 12) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 5 ))
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N.InRange(1, 4) )
                {
                    return PluralCategory.One;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if ((po.N % 10 == 2
                    || po.N % 10 == 3)  && !(po.N % 100 == 12
                    || po.N % 100 == 13) )
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 3 && !(po.N % 100 == 13) )
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if ((po.N % 10 == 6
                    || po.N % 10 == 9)  || po.N == 10)
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 6 || po.N % 10 == 9 || po.N % 10 == 0 && po.N != 0)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 11 || 
                    po.N == 8 || 
                    po.N == 80 || 
                    po.N == 800 ))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 11 || 
                    po.N == 8 || 
                    po.N.InRange(80, 89)  || 
                    po.N.InRange(800, 899)  ))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I == 1)
                {
                    return PluralCategory.One;
                }
                if (po.I == 0 || ((po.I % 100).InRange(2, 20)
                    || po.I % 100 == 40
                    || po.I % 100 == 60
                    || po.I % 100 == 80) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N % 10 == 4 && !(po.N % 100 == 14) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N.InRange(1, 4)  || ((po.N % 100).InRange(1, 4)
                    || (po.N % 100).InRange(21, 24)
                    || (po.N % 100).InRange(41, 44)
                    || (po.N % 100).InRange(61, 64)
                    || (po.N % 100).InRange(81, 84)) )
                {
                    return PluralCategory.One;
                }
                if (po.N == 5 || po.N % 100 == 5)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N % 10 == 1 && !(po.N % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                if (po.N % 10 == 2 && !(po.N % 100 == 12) )
                {
                    return PluralCategory.Two;
                }
                if (po.N % 10 == 3 && !(po.N % 100 == 13) )
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 3 ))
                {
                    return PluralCategory.Two;
                }
                if (po.N == 4)
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 11 ))
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 12 ))
                {
                    return PluralCategory.Two;
                }
                if (( po.N == 3 || 
                    po.N == 13 ))
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 3 ))
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                if (po.N == 4)
                {
                    return PluralCategory.Few;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.I % 10 == 1 && !(po.I % 100 == 11) )
                {
                    return PluralCategory.One;
                }
                if (po.I % 10 == 2 && !(po.I % 100 == 12) )
                {
                    return PluralCategory.Two;
                }
                if ((po.I % 10 == 7
                    || po.I % 10 == 8)  && !(po.I % 100 == 17
                    || po.I % 100 == 18) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if ((po.I % 10 == 1
                    || po.I % 10 == 2
                    || po.I % 10 == 5
                    || po.I % 10 == 7
                    || po.I % 10 == 8)  || (po.I % 100 == 20
                    || po.I % 100 == 50
                    || po.I % 100 == 70
                    || po.I % 100 == 80) )
                {
                    return PluralCategory.One;
                }
                if ((po.I % 10 == 3
                    || po.I % 10 == 4)  || (po.I % 1000 == 100
                    || po.I % 1000 == 200
                    || po.I % 1000 == 300
                    || po.I % 1000 == 400
                    || po.I % 1000 == 500
                    || po.I % 1000 == 600
                    || po.I % 1000 == 700
                    || po.I % 1000 == 800
                    || po.I % 1000 == 900) )
                {
                    return PluralCategory.Few;
                }
                if (po.I == 0 || po.I % 10 == 6 || (po.I % 100 == 40
                    || po.I % 100 == 60
                    || po.I % 100 == 90) )
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 3 ))
                {
                    return PluralCategory.Two;
                }
                if (po.N == 4)
                {
                    return PluralCategory.Few;
                }
                if (po.N == 6)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 5 || 
                    po.N == 7 || 
                    po.N == 8 || 
                    po.N == 9 || 
                    po.N == 10 ))
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 3 ))
                {
                    return PluralCategory.Two;
                }
                if (po.N == 4)
                {
                    return PluralCategory.Few;
                }
                if (po.N == 6)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 1 || 
                    po.N == 5 || 
                    po.N.InRange(7, 9)  ))
                {
                    return PluralCategory.One;
                }
                if (( po.N == 2 || 
                    po.N == 3 ))
                {
                    return PluralCategory.Two;
                }
                if (po.N == 4)
                {
                    return PluralCategory.Few;
                }
                if (po.N == 6)
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
            po =>
            {
                if (( po.N == 0 || 
                    po.N == 7 || 
                    po.N == 8 || 
                    po.N == 9 ))
                {
                    return PluralCategory.Zero;
                }
                if (po.N == 1)
                {
                    return PluralCategory.One;
                }
                if (po.N == 2)
                {
                    return PluralCategory.Two;
                }
                if (( po.N == 3 || 
                    po.N == 4 ))
                {
                    return PluralCategory.Few;
                }
                if (( po.N == 5 || 
                    po.N == 6 ))
                {
                    return PluralCategory.Many;
                }
                return PluralCategory.Other;
            },
        };
        
        private static int GetCardinalIndex(string culture)
        {
            switch (culture)
            {
                case "bm":
                case "bo":
                case "dz":
                case "id":
                case "ig":
                case "ii":
                case "in":
                case "ja":
                case "jbo":
                case "jv":
                case "jw":
                case "kde":
                case "kea":
                case "km":
                case "ko":
                case "lkt":
                case "lo":
                case "ms":
                case "my":
                case "nqo":
                case "osa":
                case "root":
                case "sah":
                case "ses":
                case "sg":
                case "su":
                case "th":
                case "to":
                case "vi":
                case "wo":
                case "yo":
                case "yue":
                case "zh":
                    return 0;
                case "am":
                case "as":
                case "bn":
                case "doi":
                case "fa":
                case "gu":
                case "hi":
                case "kn":
                case "pcm":
                case "zu":
                    return 1;
                case "ff":
                case "hy":
                case "kab":
                    return 2;
                case "pt":
                    return 3;
                case "ast":
                case "ca":
                case "de":
                case "en":
                case "et":
                case "fi":
                case "fy":
                case "gl":
                case "ia":
                case "io":
                case "it":
                case "ji":
                case "lij":
                case "nl":
                case "pt_PT":
                case "sc":
                case "scn":
                case "sv":
                case "sw":
                case "ur":
                case "yi":
                    return 4;
                case "si":
                    return 5;
                case "ak":
                case "bho":
                case "guw":
                case "ln":
                case "mg":
                case "nso":
                case "pa":
                case "ti":
                case "wa":
                    return 6;
                case "tzm":
                    return 7;
                case "af":
                case "an":
                case "asa":
                case "az":
                case "bem":
                case "bez":
                case "bg":
                case "brx":
                case "ce":
                case "cgg":
                case "chr":
                case "ckb":
                case "dv":
                case "ee":
                case "el":
                case "eo":
                case "es":
                case "eu":
                case "fo":
                case "fur":
                case "gsw":
                case "ha":
                case "haw":
                case "hu":
                case "jgo":
                case "jmc":
                case "ka":
                case "kaj":
                case "kcg":
                case "kk":
                case "kkj":
                case "kl":
                case "ks":
                case "ksb":
                case "ku":
                case "ky":
                case "lb":
                case "lg":
                case "mas":
                case "mgo":
                case "ml":
                case "mn":
                case "mr":
                case "nah":
                case "nb":
                case "nd":
                case "ne":
                case "nn":
                case "nnh":
                case "no":
                case "nr":
                case "ny":
                case "nyn":
                case "om":
                case "or":
                case "os":
                case "pap":
                case "ps":
                case "rm":
                case "rof":
                case "rwk":
                case "saq":
                case "sd":
                case "sdh":
                case "seh":
                case "sn":
                case "so":
                case "sq":
                case "ss":
                case "ssy":
                case "st":
                case "syr":
                case "ta":
                case "te":
                case "teo":
                case "tig":
                case "tk":
                case "tn":
                case "tr":
                case "ts":
                case "ug":
                case "uz":
                case "ve":
                case "vo":
                case "vun":
                case "wae":
                case "xh":
                case "xog":
                    return 8;
                case "da":
                    return 9;
                case "is":
                    return 10;
                case "mk":
                    return 11;
                case "ceb":
                case "fil":
                case "tl":
                    return 12;
                case "lv":
                case "prg":
                    return 13;
                case "lag":
                    return 14;
                case "ksh":
                    return 15;
                case "iu":
                case "naq":
                case "sat":
                case "se":
                case "sma":
                case "smi":
                case "smj":
                case "smn":
                case "sms":
                    return 16;
                case "shi":
                    return 17;
                case "mo":
                case "ro":
                    return 18;
                case "bs":
                case "hr":
                case "sh":
                case "sr":
                    return 19;
                case "fr":
                    return 20;
                case "gd":
                    return 21;
                case "sl":
                    return 22;
                case "dsb":
                case "hsb":
                    return 23;
                case "he":
                case "iw":
                    return 24;
                case "cs":
                case "sk":
                    return 25;
                case "pl":
                    return 26;
                case "be":
                    return 27;
                case "lt":
                    return 28;
                case "mt":
                    return 29;
                case "ru":
                case "uk":
                    return 30;
                case "br":
                    return 31;
                case "ga":
                    return 32;
                case "gv":
                    return 33;
                case "kw":
                    return 34;
                case "ar":
                case "ars":
                    return 35;
                case "cy":
                    return 36;
            }
            return -1;
        }

        private static int GetOrdinalIndex(string culture)
        {
            switch (culture)
            {
                case "af":
                case "am":
                case "an":
                case "ar":
                case "bg":
                case "bs":
                case "ce":
                case "cs":
                case "da":
                case "de":
                case "dsb":
                case "el":
                case "es":
                case "et":
                case "eu":
                case "fa":
                case "fi":
                case "fy":
                case "gl":
                case "gsw":
                case "he":
                case "hr":
                case "hsb":
                case "ia":
                case "id":
                case "in":
                case "is":
                case "iw":
                case "ja":
                case "km":
                case "kn":
                case "ko":
                case "ky":
                case "lt":
                case "lv":
                case "ml":
                case "mn":
                case "my":
                case "nb":
                case "nl":
                case "no":
                case "pa":
                case "pl":
                case "prg":
                case "ps":
                case "pt":
                case "root":
                case "ru":
                case "sd":
                case "sh":
                case "si":
                case "sk":
                case "sl":
                case "sr":
                case "sw":
                case "ta":
                case "te":
                case "th":
                case "tr":
                case "ur":
                case "uz":
                case "yue":
                case "zh":
                case "zu":
                    return 0;
                case "sv":
                    return 1;
                case "fil":
                case "fr":
                case "ga":
                case "hy":
                case "lo":
                case "mo":
                case "ms":
                case "ro":
                case "tl":
                case "vi":
                    return 2;
                case "hu":
                    return 3;
                case "ne":
                    return 4;
                case "be":
                    return 5;
                case "uk":
                    return 6;
                case "tk":
                    return 7;
                case "kk":
                    return 8;
                case "it":
                case "sc":
                case "scn":
                    return 9;
                case "lij":
                    return 10;
                case "ka":
                    return 11;
                case "sq":
                    return 12;
                case "kw":
                    return 13;
                case "en":
                    return 14;
                case "mr":
                    return 15;
                case "gd":
                    return 16;
                case "ca":
                    return 17;
                case "mk":
                    return 18;
                case "az":
                    return 19;
                case "gu":
                case "hi":
                    return 20;
                case "as":
                case "bn":
                    return 21;
                case "or":
                    return 22;
                case "cy":
                    return 23;
            }
            return -1;
        }

        public static Func<PluralOperands, PluralCategory> GetPluralFunc(string culture, RuleType type)
        {
            switch (type)
            {
                case RuleType.Cardinal:
                    return cardinalMap[GetCardinalIndex(culture)];
                case RuleType.Ordinal:
                    return ordinalMap[GetOrdinalIndex(culture)];
            }

            return _ => PluralCategory.Other;
        }

        public static string[] SpecialCaseCardinal = {
            "pt_PT",
            "pt-PT",
        };

        public static string[] SpecialCaseOrdinal = {
        };
        
    }
}
