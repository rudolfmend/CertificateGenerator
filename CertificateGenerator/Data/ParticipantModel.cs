using System;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Model pre účastníka
    /// </summary>
    public class ParticipantModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        // Pre časté použitie
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
    }

    /// <summary>
    /// Model pre organizátora
    /// </summary>
    public class OrganizerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
    }

    /// <summary>
    /// Model pre tému podujatia
    /// </summary>
    public class EventTopicModel
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
    }

    /// <summary>
    /// Model pre vygenerovaný certifikát (história)
    /// </summary>
    public class CertificateModel
    {
        public int Id { get; set; }
        // FK na organizátora
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; }
        // FK na účastníka
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public DateTime? ParticipantBirthDate { get; set; }
        public string ParticipantRegistrationNumber { get; set; }
        // FK na tému
        public int EventTopicId { get; set; }
        public string EventTopicName { get; set; }
        public DateTime? EventDate { get; set; }
        public string Notes { get; set; }
        public string PaperFormat { get; set; } // A3, A4, A5
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}