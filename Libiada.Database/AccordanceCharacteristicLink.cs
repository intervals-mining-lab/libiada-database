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
    
    public partial class AccordanceCharacteristicLink
    {
        public short Id { get; set; }
        public LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators.AccordanceCharacteristic AccordanceCharacteristic { get; set; }
        public LibiadaCore.Core.Link Link { get; set; }
    }
}
