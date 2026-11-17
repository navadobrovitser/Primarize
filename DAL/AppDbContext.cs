using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // Existing tables
        // ==========================================

        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<AnonymousVote> AnonymousVotes { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // ==========================================
        // RAG - Regulation tables
        // ==========================================

        public DbSet<RegulationVersion> RegulationVersions { get; set; }
        public DbSet<RegulationChunk> RegulationChunks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // Existing configurations
            // ==========================================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Id)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.UserId, pl.PostId })
                .IsUnique();

            modelBuilder.Entity<UserVote>()
                .HasIndex(uv => uv.UserId)
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.PostLikes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // RAG configurations
            // ==========================================

            // One regulation version contains many chunks
            modelBuilder.Entity<RegulationChunk>()
                .HasOne(c => c.Version)
                .WithMany(v => v.Chunks)
                .HasForeignKey(c => c.VersionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A section number can appear only once
            // within the same regulation version
            modelBuilder.Entity<RegulationChunk>()
                .HasIndex(c => new
                {
                    c.VersionId,
                    c.SectionNumber
                })
                .IsUnique();

            // Regulation version numbers must be unique
            modelBuilder.Entity<RegulationVersion>()
                .HasIndex(v => v.VersionNumber)
                .IsUnique();
        }
    }
}