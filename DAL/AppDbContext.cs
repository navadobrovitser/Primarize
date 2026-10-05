using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostComment> PostComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // הגדרת מפתח מורכב עבור הטבלה המקשרת
            modelBuilder.Entity<PostComment>()
                .HasKey(pc => new { pc.PostId, pc.CommentId });

            // הגדרת קשר ל-Post עם ציון ה-Navigation Property המתאים
            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.PostComments)
                .HasForeignKey(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            // הגדרת קשר ל-Comment עם ציון ה-Navigation Property המתאים
            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.Comment)
                .WithMany(c => c.PostComments)
                .HasForeignKey(pc => pc.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // מניעת מחיקה בשרשרת עבור תגובות משתמש
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
   

