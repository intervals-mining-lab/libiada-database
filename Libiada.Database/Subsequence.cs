//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Libiada.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Subsequence
    {
        public Subsequence()
        {
            SequenceAttribute = new HashSet<SequenceAttribute>();
            Position = new HashSet<Position>();
        }
    
        public long Id { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public System.DateTimeOffset Modified { get; set; }
        public long SequenceId { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Feature Feature { get; set; }
        public string RemoteId { get; set; }
        public bool Partial { get; set; }
    
        public virtual DnaSequence DnaSequence { get; set; }
        public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; }
        public virtual ICollection<Position> Position { get; set; }
    }
}
