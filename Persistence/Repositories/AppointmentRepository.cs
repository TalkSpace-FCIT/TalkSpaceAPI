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
    public class AppointmentRepository : Repository<Appointment>,IAppointmentRepository
    {
        private readonly AppDbContext context;
        private DbSet<Appointment> _dbset;

        public AppointmentRepository(AppDbContext context):base(context)
        {
            this.context = context;
            _dbset = context.Set<Appointment>();
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

        public async Task ScheduleAppointmentAsync(Appointment appointment)
        {
             await _dbset.AddAsync(appointment);
           
        }



    }

}
