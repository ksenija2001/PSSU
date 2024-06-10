using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator {

    public enum ActiveWhen {
        BELOW,
        ABOVE,
        EQUALS
    }

    public enum ScanState {
        ON,
        OFF
    }

    public class DBModel {

        public class Alarm {

            [Key]
            public int Id { get; set; }

            [Required]
            public double Value { get; set; }

            [Required]
            [EnumDataType(typeof(ActiveWhen))]
            private ActiveWhen Activate { get; set; }

            [Required]
            public DateTime AlarmTime { get; set; }

            [Required]
            private string Message { get; set; }

            [ForeignKey("Tag")]
            public string TagId { get; set; }
            public virtual Tag Tag { get; set; }

        }

        public class Tag {

            [Key]
            public string Name { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            [RegularExpression(@"ADDR[0-9]{3}")]
            public string IOAddress { get; set; }

            [Required]
            [Range(0, 1)]
            public byte Connected { get; set; }

        }

        public class DI : Tag {

            [Required]
            public double ScanTime { get; set; }

            [Required]
            [EnumDataType(typeof(ScanState))]
            public ScanState ScanState { get; set; }

            public virtual List<Alarm> Alarms { get; set; }
        }

        public class DO : Tag {

            [Required]
            [Range(0, 1)]
            public byte InitialValue { get; set; }
        }

        public class AI : Tag {

            [Required]
            public double ScanTime { get; set; }

            [Required]
            public ScanState ScanState { get; set; }

            [Required]
            public double LowLimit { get; set; }

            [Required]
            public double HighLimit { get; set; }

            [Required]
            [StringLength(5, MinimumLength = 1)]
            public string Units { get; set; }

            public virtual List<Alarm> Alarms { get; set; }
        }

        public class AO : Tag {

            [Required]
            public bool InitialValue { get; set; }

            [Required]
            public double LowLimit { get; set; }

            [Required]
            public double HighLimit { get; set; }

            [Required]
            [StringLength(5, MinimumLength = 1)]
            public string Units { get; set; }
        }

        public class  IOContext : DbContext {
            public DbSet<Alarm> Alarms { get; set; }
            public DbSet<DI> DIs { get; set; }
            public DbSet<DO> DOs { get; set; }
            public DbSet<AI> AIs { get; set; }
            public DbSet<AO> AOs { get; set; }
        }
    }
}
