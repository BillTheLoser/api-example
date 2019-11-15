namespace Pnnl.Api.Approvals
{
    /// <summary>
    /// This data class is used to map a persons identifications to assist in normalizing a persons identification to the hanford id
    /// </summary>
    public class PersonIdentification
    {
        /// <summary>
        /// The persons BMI Employee Identification number
        /// </summary>
        public string EmployeeId { get; set; }
            
        /// <summary>
        /// The persons Hanford site Identification number
        /// </summary>
        public string HanfordId { get; set; }
    
        /// <summary>
        /// The domain for the person's primary network account
        /// </summary>
        public string Domain { get; set; }
    
        /// <summary>
        /// The network identificaiton for the person's primary account
        /// </summary>
        public string NetworkId { get; set; }
    
        /// <summary>
        /// Whether or not the person is currently active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
