using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtOfHassan
{
    public static class RegExClass
    {
        public static readonly string IO             = @"[^IO]";

        public static readonly string Sex            = @"[^FMO]";

        public static readonly string Number         = @"[^0-9]";

        public static readonly string Numbers        = @"[^0-9;]";

        public static readonly string NumberDot      = @"[^0-9.]";

        public static readonly string Word           = @"[^\w]";

        public static readonly string WordBar        = @"[^\w-]";

        public static readonly string AlphabetCap    = @"[^A-Z]";

        public static readonly string Alphabet       = @"[^a-zA-Z]";

        public static readonly string AlphabetSlash  = @"[^a-zA-Z/]";

        public static readonly string AlphabetNumber = @"[^a-zA-Z0-9_-]";

        public static readonly string Name           = @"[^a-zA-Zㄱ-힣-]";

        public static readonly string NameDot        = @"[^a-zA-Zㄱ-힣-.]";

        public static readonly string FullName       = @"[^a-zA-Zㄱ-힣\s-.]";

        public static readonly string Address        = @"[^\w\:/.-]";

        public static readonly string File           = @"[^\w\:\\/.-]";

        public static readonly string Location       = @"[^\w\s-.()#%&]";

        public static readonly string Physician      = @"[^\w\s()-]";

        public static readonly string Code           = @"[^\w-%]";

        public static readonly string Description    = @"[^\w\s-()%]";

        public static readonly string Email          = @"[^\w-.@]";

        public static readonly string HtmlColor      = @"[^a-zA-Z0-9#;]";
    }
}
