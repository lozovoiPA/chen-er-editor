using ErEditor.ErSchemaClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Routing.ConstrainedDelaunayTriangulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    public class ErDbContext : DbContext
    {
        public DbSet<DbErElement> Elements { get; set; }
        public DbSet<DbEntitySet> EntitySets { get; set; }
        public DbSet<DbRelationshipSet> RelationshipSets { get; set; }
        public DbSet<DbValueSet> ValueSets { get; set; }
        public DbSet<DbAttribute> Attributes { get; set; }
        public DbSet<DbRole> Roles { get; set; }
        public DbSet<DbMapping> Mappings { get; set; }
        public DbSet<DbDiagram> Diagrams { get; set; }

        private readonly string dbFullPath = "";

        public ErDbContext(string dbFullPath)
        {
            this.dbFullPath = dbFullPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=" + dbFullPath)
                .EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<DbErElement>()
                .HasDiscriminator<string>("Element Type")
                .HasValue<DbEntitySet>("Entity Set")
                .HasValue<DbRelationshipSet>("Relationship Set")
                .HasValue<DbValueSet>("Value Set")
                .HasValue<DbAttribute>("Attribute")
                .HasValue<DbRole>("Role")
                .HasValue<DbMapping>("Mapping");
            */
            modelBuilder.Entity<DbErElement>().UseTptMappingStrategy();

            modelBuilder.Entity<DbErElementWithAttributes>().ToTable("ElementsWithAttributes");

            modelBuilder.Entity<DbValueSet>()
                .HasMany(d => d.Attributes)
                .WithMany(o => o.ValueSets)
                .UsingEntity(j => j.ToTable("AttributeValueSet"));

            modelBuilder.Entity<DbAttribute>()
                .Property(attribute => attribute.IsKey)
                .HasDefaultValue(false);
            modelBuilder.Entity<DbRole>()
                .Property(link => link.IsKeyEntitySet)
                .HasDefaultValue(false);
            modelBuilder.Entity<DbRole>()
                .Property(link => link.IsIdDependant)
                .HasDefaultValue(false);

            //modelBuilder.Entity<DbRole>().HasAlternateKey(c => new { c.EntitySetId, c.RelationshipSetId });

            modelBuilder.Entity<DbPrimitive>()
                .HasDiscriminator(primitive => primitive.Type)
                .HasValue<DbRectangle>("Rectangle")
                .HasValue<DbDiamond>("Diamond")
                .HasValue<DbEdge>("Edge");
        }
    }
}
