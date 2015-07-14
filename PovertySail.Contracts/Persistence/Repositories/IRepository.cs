using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts.Persistence.Repositories
{
    public interface IRepository
    {
        void Save<T>(T t);
    }
}
