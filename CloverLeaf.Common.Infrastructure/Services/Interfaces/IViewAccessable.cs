using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Services.Interfaces
{
    public interface IViewAccessable
    {
        IView View { get; set; }
    }
}
