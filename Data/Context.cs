using BlogProject.Entities;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace BlogProject.Data
{
	public class Context : DbContext
	{
		public Context() : base("name=Context")
		{
			Database.SetInitializer<Context>(null);
		}

		public DbSet<Role> Roles { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<PostTag> PostTags { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Like> Likes { get; set; }
		public DbSet<Session> Sessions { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<SavedPost> SavedPosts { get; set; }
		public DbSet<About> Abouts { get; set; }
		public DbSet<AuthorRequest> AuthorRequests { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PostTag>()
				.HasKey(pt => new { pt.PostId, pt.TagId });

			modelBuilder.Entity<PostTag>()
				.HasRequired(pt => pt.Post)
				.WithMany(p => p.PostTags)
				.HasForeignKey(pt => pt.PostId);

			modelBuilder.Entity<PostTag>()
				.HasRequired(pt => pt.Tag)
				.WithMany(t => t.PostTags)
				.HasForeignKey(pt => pt.TagId);

			modelBuilder.Entity<User>()
			.Property(u => u.Email)
			.HasMaxLength(256);

			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();

			modelBuilder.Entity<SavedPost>()
				.HasRequired(sp => sp.User)
				.WithMany(u => u.SavedPosts)
				.HasForeignKey(sp => sp.UserId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<SavedPost>()
				.HasRequired(sp => sp.Post)
				.WithMany(p => p.SavedPosts)
				.HasForeignKey(sp => sp.PostId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Like>()
				.HasRequired(l => l.User)
				.WithMany(u => u.Likes)
				.HasForeignKey(l => l.UserId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Comment>()
			   .HasRequired(c => c.User)
			   .WithMany(u => u.Comments)
			   .HasForeignKey(c => c.UserId)
			   .WillCascadeOnDelete(false);

			base.OnModelCreating(modelBuilder);
		}
	}
}