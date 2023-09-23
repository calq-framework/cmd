using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;

// TODO extend TextWriter (requires statefull shells) - because of that for now all Shell classes (not static utils) are internal
// stateful bash could be done maybe with subshells ( script ) > file ?
internal interface IShell
{
    string CMD(string script);
}
