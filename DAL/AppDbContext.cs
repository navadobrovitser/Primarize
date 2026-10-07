using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<AnonymousVote> AnonymousVotes { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }
        public DbSet<Post> Posts { get; set; } // מתוקן ל-Post (או Posts לפי איך שהגדרת את המחלקה עצמה)
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // מניעת מחיקה בשרשרת עבור תגובות משתמש
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // הגדרת קשר ישיר בין תגובה לפוסט
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // מניעת מחיקה בשרשרת עבור לייקים
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // אינדקסים ייחודיים
            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.UserId, pl.PostId })
                .IsUnique();

            modelBuilder.Entity<UserVote>()
                .HasIndex(uv => uv.UserId)
                .IsUnique();
        }
    }
}