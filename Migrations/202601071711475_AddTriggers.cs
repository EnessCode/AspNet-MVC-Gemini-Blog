namespace BlogProject.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class AddTriggers : DbMigration
	{
		public override void Up()
		{
			Sql(@"
                CREATE TRIGGER trg_PostInsert_NotifyAdmin
                ON Posts
                AFTER INSERT
                AS
                BEGIN
                    -- Admin Rol ID'sini bul (Role tablosunda Name='Admin' olduğunu varsayıyoruz)
                    DECLARE @AdminRoleId INT;
                    SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Admin';

                    -- Tüm Adminlere bildirim bas
                    INSERT INTO Notifications (Type, Message, CreatedAt, IsRead, RelatedId, UserId)
                    SELECT 
                        'NewPost', 
                        u.Username + ' yeni bir yazı paylaştı: ' + LEFT(i.Title, 20) + '...',
                        GETDATE(), 
                        0, 
                        i.Id, 
                        adm.Id
                    FROM inserted i
                    INNER JOIN Users u ON i.UserId = u.Id 
                    CROSS JOIN Users adm WHERE adm.RoleId = @AdminRoleId
                END
            ");

			Sql(@"
                CREATE TRIGGER trg_CommentInsert_Notify
                ON Comments
                AFTER INSERT
                AS
                BEGIN
                    DECLARE @AdminRoleId INT;
                    SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Admin';

                    -- A) YAZARA BİLDİRİM (Kendi postuna kendi yorum yapmadıysa)
                    INSERT INTO Notifications (Type, Message, CreatedAt, IsRead, RelatedId, UserId)
                    SELECT 
                        'NewComment', 
                        commenter.Username + ' yazınıza yorum yaptı.',
                        GETDATE(), 
                        0, 
                        i.Id, 
                        p.UserId 
                    FROM inserted i
                    INNER JOIN Posts p ON i.PostId = p.Id
                    INNER JOIN Users commenter ON i.UserId = commenter.Id
                    WHERE p.UserId != i.UserId; 

                    -- B) ADMİNLERE BİLDİRİM (Yazar Admin değilse, Adminlere de bilgi gitsin)
                    INSERT INTO Notifications (Type, Message, CreatedAt, IsRead, RelatedId, UserId)
                    SELECT 
                        'NewComment', 
                        commenter.Username + ', ' + LEFT(p.Title, 15) + '... yazısına yorum yaptı.',
                        GETDATE(), 
                        0, 
                        i.Id, 
                        adm.Id
                    FROM inserted i
                    INNER JOIN Posts p ON i.PostId = p.Id
                    INNER JOIN Users commenter ON i.UserId = commenter.Id
                    CROSS JOIN Users adm WHERE adm.RoleId = @AdminRoleId
                    -- Post sahibi zaten adminse yukarıda aldı, tekrar almasın:
                    AND adm.Id != p.UserId 
                END
            ");

			Sql(@"
                CREATE TRIGGER trg_CommentDelete_Cleanup
                ON Comments
                AFTER DELETE
                AS
                BEGIN
                    DELETE FROM Notifications
                    WHERE RelatedId IN (SELECT Id FROM deleted) 
                    AND Type = 'NewComment'
                END
            ");

			Sql(@"
                CREATE TRIGGER trg_PostDelete_Cleanup
                ON Posts
                AFTER DELETE
                AS
                BEGIN
                    DELETE FROM Notifications
                    WHERE RelatedId IN (SELECT Id FROM deleted) 
                    AND Type = 'NewPost'
                END
            ");
		}

		public override void Down()
		{
			Sql("DROP TRIGGER trg_PostInsert_NotifyAdmin");
			Sql("DROP TRIGGER trg_CommentInsert_Notify");
			Sql("DROP TRIGGER trg_CommentDelete_Cleanup");
			Sql("DROP TRIGGER trg_PostDelete_Cleanup");
		}
	}
}
