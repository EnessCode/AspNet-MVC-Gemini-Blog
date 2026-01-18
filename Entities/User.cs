using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class User : BaseEntity
	{
		[StringLength(50)]
		public string Username { get; set; }

		[Required]
		[StringLength(100)]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(200)]
		public string PasswordHash { get; set; }
		public bool IsActive { get; set; }
		public int RoleId { get; set; }
		public virtual Role Role { get; set; }

		public string ResetToken { get; set; }
		public DateTime? ResetTokenExpires { get; set; }

		public int FailedLoginAttempts { get; set; } = 0;
		public DateTime? LockoutEnd { get; set; }

		public virtual ICollection<Post> Posts { get; set; }
		public virtual ICollection<Comment> Comments { get; set; }
		public virtual ICollection<Like> Likes { get; set; }
		public virtual ICollection<Session> Sessions { get; set; }
		public virtual ICollection<Notification> Notifications { get; set; }
		public virtual ICollection<SavedPost> SavedPosts { get; set; }

		public User()
		{
			IsActive = true;
			Posts = new List<Post>();
			Comments = new List<Comment>();
			Likes = new List<Like>();
			Sessions = new List<Session>();
			Notifications = new List<Notification>();
			SavedPosts = new List<SavedPost>();
		}
	}
}