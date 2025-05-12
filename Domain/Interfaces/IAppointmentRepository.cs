using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAppointmentRepository:IRepository<Appointment>
    {
        // Get Appointment /patient/{patientId}
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(string patientId);

        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(string DoctorID);

        Task ScheduleAppointmentAsync(Appointment appointment);// we can use Add async as well

    }

}
