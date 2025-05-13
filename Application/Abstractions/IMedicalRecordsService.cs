using Application.DTOs.Requests.MedicalrecordRequest;
using Application.DTOs.Responses.MedicalrecordResponse;
using Domain.Results;

namespace Application.Abstractions
{
    /// <summary>
    /// Defines the contract for managing medical records within the application.
    /// </summary>
    public interface IMedicalRecordsService
    {
        /// <summary>
        /// Retrieves a medical record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the medical record.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the medical record if found, or an error if not.
        /// </returns>
        Task<Result<MedicalRecordResponse>> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all medical records associated with a specific patient.
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a collection of medical records if found, or an error if not.
        /// </returns>
        Task<Result<IEnumerable<MedicalRecordResponse>>> GetByPatientIdAsync(string patientId);

        /// <summary>
        /// Creates a new medical record based on the provided request data.
        /// </summary>
        /// <param name="request">The data required to create a new medical record.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the created medical record, or an error if the creation fails.
        /// </returns>
        Task<Result<MedicalRecordResponse>> CreateAsync(CreateMedicalRecordRequest request);


        /// <summary>
        /// Retrieves all medical records available in the system.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a collection of all medical records if found, or an error if not.
        /// </returns>
        Task<Result<IEnumerable<MedicalRecordResponse>>> GetAllAsync();
    }
}
