﻿using Entities.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryStockChange:IRepositoryBase<StockChange>
    {
        IQueryable<object> GetStockChange();
    }
}
