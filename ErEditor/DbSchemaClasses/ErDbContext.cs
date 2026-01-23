using ErEditor.ErSchemaClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    public class ErDbContext : DbContext
    {
        public DbSet<DbErElementWithAttributes> ErElementsWithAttributes { get; set; }
        public DbSet<DbEntitySet> EntitySets { get; set; }
        public DbSet<DbRelationshipSet> RelationshipSets { get; set; }
        public DbSet<DbValueSet> ValueSets { get; set; }
        public DbSet<DbDiagram> Diagrams { get; set; }
        private readonly string dbFullPath = "";

        public ErDbContext(string dbFullPath)
        {
            this.dbFullPath = dbFullPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + dbFullPath);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbErElementWithAttributes>()
                .HasDiscriminator<string>("Element Type")
                .HasValue<DbEntitySet>("Entity Set")
                .HasValue<DbRelationshipSet>("Relationship Set");


            modelBuilder.Entity<DbAttribute>()
                .Property(attribute => attribute.IsKey)
                .HasDefaultValue(false);
            modelBuilder.Entity<DbRole>()
                .Property(link => link.IsKeyEntitySet)
                .HasDefaultValue(false);
            modelBuilder.Entity<DbRole>()
                .Property(link => link.IsIdDependant)
                .HasDefaultValue(false);

            modelBuilder.Entity<DbRole>().HasAlternateKey(c => new { c.EntitySetId, c.RelationshipSetId });

            /*
            modelBuilder.Entity<DbPrimitive>()
                .HasDiscriminator<string>("Primitive Type")
                .HasValue<DbShape>("Shape")
                .HasValue<DbAssociation>("Association");
            */
            modelBuilder.Entity<DbPrimitive>()
                .HasDiscriminator(primitive => primitive.Type)
                .HasValue<DbRectangle>("Rectangle")
                .HasValue<DbDiamond>("Diamond")
                .HasValue<DbEdge>("Edge");
        }
    }
}
