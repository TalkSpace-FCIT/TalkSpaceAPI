namespace Application.DTOs.Requests.MedicalrecordRequest
{
    public record CreateMedicalRecordRequest(
        DateTime VisitDate,
        string? Notes,
        string Prescriptions,
        int AppointmentId,
        string PatientId,
        string DoctorId);
}
