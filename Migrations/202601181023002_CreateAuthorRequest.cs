namespace BlogProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateAuthorRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthorRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        IsProcessed = c.Boolean(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuthorRequests", "UserId", "dbo.Users");
            DropIndex("dbo.AuthorRequests", new[] { "UserId" });
            DropTable("dbo.AuthorRequests");
        }
    }
}
