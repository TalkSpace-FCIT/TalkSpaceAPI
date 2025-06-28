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
    public class BillingRepository:Repository<Billing>,IBillingRepository
    {
        private DbSet<Billing> _dbset;
        public BillingRepository(AppDbContext _dbcontext) : base(_dbcontext)
        {
            _dbset = _dbcontext.Set<Billing>();
        }

        public Task<Billing> GetbyAppointmentID(int appointmentid)
        {
            return _dbset.FirstOrDefaultAsync(b=>b.AppointmentId == appointmentid);
        }
    }
}
