using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext context;
        private DbSet<Appointment> _dbset;

        public AppointmentRepository(AppDbContext context)
        {
            this.context = context;
            _dbset = context.Set<Appointment>();
        }
        public async Task AddAsync(Appointment entity)
        {
          await _dbset.AddAsync(entity);
           await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Appointment> entities)
        {
            await _dbset.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public Task<int> CountAsync(Expression<Func<Appointment, bool>>? predicate = null)
        {
            return predicate == null
                ? _dbset.CountAsync()
                : _dbset.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Appointment, bool>> predicate)
        {
            return await _dbset.AnyAsync(predicate);
        }


        public async Task<IEnumerable<Appointment>> FindAsync(
            Expression<Func<Appointment, bool>> predicate,
            Func<IQueryable<Appointment>, IIncludableQueryable<Appointment, object>>? include = null)
        {
            IQueryable<Appointment> query = _dbset.Where(predicate);

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Appointment>> GetAllAsync(Func<IQueryable<Appointment>, IIncludableQueryable<Appointment, object>>? include = null)
        {
            IQueryable<Appointment> query = _dbset;

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(string DoctorID)
        {
            return await _dbset.Where(a=>a.DoctorId==DoctorID)
            .Include(a => a.Patient)
            .Include(a => a.MedicalRecord)
            .ToListAsync();

        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _dbset.Where(a=>a.PatientId == patientId)
                .Include(a=>a.Doctor)
                .Include(a => a.MedicalRecord)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync<TId>(TId id) where TId : notnull
        {
            return await _dbset.FindAsync(id);
        }

        public void Remove(Appointment entity)
        {
            _dbset.Remove(entity);
            context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Appointment> entities)
        {
            _dbset.RemoveRange(entities);
            context.SaveChanges();  
        }

        public async Task ScheduleAppointmentAsync(Appointment appointment)
        {
             await _dbset.AddAsync(appointment);
            await context.SaveChangesAsync();
        }

        public void Update(Appointment entity)
        {
           _dbset.Update(entity);
            context.SaveChanges();
        }


    }

}
