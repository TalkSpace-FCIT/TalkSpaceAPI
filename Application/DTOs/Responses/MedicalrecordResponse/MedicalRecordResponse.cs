namespace Application.DTOs.Responses.MedicalrecordResponse
{
    public record MedicalRecordResponse(
        int Id,
        DateTime VisitDate,
        string? Notes,
        string Prescriptions,
        int AppointmentId,
        DateTime AppointmentDate,
        string PatientName,
        string DoctorName,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

}
