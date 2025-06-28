using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class PaymentRepository:Repository<Payment>, IpaymentRepository
    {
        public PaymentRepository(AppDbContext _dbcontext):base(_dbcontext)
        {
        }
        

    }
}
