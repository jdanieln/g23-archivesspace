namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class ResourceSubject
    {
        public int ResourceId { get; set; }
        public virtual Resource? Resource { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }
    }
}
