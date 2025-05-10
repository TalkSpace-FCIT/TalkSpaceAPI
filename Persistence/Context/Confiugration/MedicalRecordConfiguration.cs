using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Context.Confiugration
{
    public class MedicalRecordConfiguration:IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.Property(m=>m.Prescriptions)
                .IsRequired()
                .HasMaxLength(maxLength: 1000);
        }
    }

}
