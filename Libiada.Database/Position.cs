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
    
    public partial class Position
    {
        public long Id { get; set; }
        public long SubsequenceId { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
    
        public virtual Subsequence Subsequence { get; set; }
    }
}
