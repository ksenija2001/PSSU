using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Xml.Serialization;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Windows;


namespace DataConcentrator {

    public enum ActiveWhen {
        BELOW,
        ABOVE,
        EQUALS
    }

    public class DBModel 
    {

        [XmlInclude(typeof(LogAlarm))]
        public class LogAlarm
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]

            public int Id { get; set; }

            [Required]
            public DateTime AlarmTime { get; set; }

            [Required]
            public string Message { get; set; }

            [ForeignKey("Tag")]
            public string TagId { get; set; }
            public virtual DBModel.Tag Tag { get; set; }

        }

        [XmlInclude(typeof(Alarm))]
        public class Alarm {


            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]

            public int Id {  get; set; }

            [Required]
            public double Value { get; set; }

            [Required]
            [EnumDataType(typeof(ActiveWhen))]
            public ActiveWhen Activate { get; set; }

            [Required]
            public string Message { get; set; }

            [ForeignKey("Tag")]
            public string TagId { get; set; }
            public virtual Tag Tag { get; set; }
        }

        public abstract class Tag
        {


            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]

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

        [XmlInclude(typeof(DI))]
        public class DI : Tag {


            [Required]
            public double ScanTime { get; set; }

            [Required]
            [Range(0, 1)]
            public byte ScanState { get; set; }

            public virtual List<Alarm> Alarms { get; set; }



        }

        [XmlInclude(typeof(DO))]

        public class DO : Tag {

            [Required]
            [Range(0, 1)]
            public byte InitialValue { get; set; }
        }

        [XmlInclude(typeof(AI))]

        public class AI : Tag {

            [Required]
            public double ScanTime { get; set; }

            [Required]
            [Range(0, 1)]
            public byte ScanState { get; set; }

            [Required]
            public double LowLimit { get; set; }

            [Required]
            public double HighLimit { get; set; }

            [Required]
            [StringLength(5, MinimumLength = 1)]
            public string Units { get; set; }

            public virtual List<Alarm> Alarms { get; set; }
        }

        [XmlInclude(typeof(AO))]

        public class AO : Tag {

            [Required]
            public double InitialValue { get; set; }

            [Required]
            public double LowLimit { get; set; }

            [Required]
            public double HighLimit { get; set; }

            [Required]
            [StringLength(5, MinimumLength = 1)]
            public string Units { get; set; }
        }

        public class  IOContext : DbContext {
            public DbSet<Tag> Tags { get; set; }
            public DbSet<LogAlarm> LogAlarms { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Conventions.Remove<StoreGeneratedIdentityKeyConvention>();


                modelBuilder.Entity<DI>().Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("DIs");
                });

                modelBuilder.Entity<AI>().Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("AIs");
                });

                modelBuilder.Entity<DO>().Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("DOs");
                });

                modelBuilder.Entity<AO>().Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("AOs");
                });

                modelBuilder.Entity<Alarm>().Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("Alarms");
                });
            }
        }

    }
}

