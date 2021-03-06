﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.ServiceProxy;

namespace Bam.Net.Data.Repositories
{
    public abstract class RepositoryResolver : IRepositoryResolver 
    {
        public abstract IRepository GetRepository(IHttpContext context);

        public T GetRepository<T>(IHttpContext context) where T : IRepository
        {
            return (T)GetRepository(context);
        }
    }
}
